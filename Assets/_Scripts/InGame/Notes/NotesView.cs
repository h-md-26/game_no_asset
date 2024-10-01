using System.Collections;
using System.Collections.Generic;
using StellaCircles.Data;
using UnityEngine;
using UnityEngine.Serialization;

namespace StellaCircles.InGame
{

    public class NotesView : MonoBehaviour
    {
        /// <summary>
        /// ノーツの配置、出現などの視覚的な管理
        /// </summary>

        GameAudioTimer audioTimer;
        
        NotesModel notesModel;
        
        [SerializeField] Transform[] nodeTransforms; 
        Vector2[] nodePositions;
        
        [SerializeField] GameObject[] notesPrefabs;

        [SerializeField] private GameObject[] noteObjs;
        private NoteManager[] noteManagers;
        int notesObjCount;

        private string[] fumenMeasureStrings;
        private int[] nodeDestinations;
        private int allNotesCount;
        private NoteType[] noteTypes;
        
        // 前のJudgingNoteId, JudgingNoteIdが変更されたときに元のJudgingNoteの状態を戻すために使用する
        private int preJudgingNoteId = -1;
        
        public void GameStart(FumenData fumen, GameAudioTimer audioTimer, NotesModel notesModel)
        {
            fumenMeasureStrings = fumen.FumenMeasureStrings;
            nodeDestinations = fumen.NodeDestinations;
            allNotesCount = fumen.AllNotesCount;
            noteTypes = fumen.NoteTypes;
            
            this.audioTimer = audioTimer;
            this.notesModel = notesModel;
            
            //ノードの位置登録
            nodePositions = new Vector2[6];
            for (int i = 0; i < 6; i++)
            {
                nodePositions[i] = nodeTransforms[i].transform.position;
            }

            //ノーツの設置
            notesObjCount = 0;
            noteObjs = new GameObject[allNotesCount];
            noteManagers = new NoteManager[allNotesCount];
            for (int i = 0; i < fumenMeasureStrings.Length; i++)
            {
                SetNotes(i);
            }
            
            // イベントの登録
            notesModel.onUnappearNote += OnUnappearNote;
            notesModel.onAppearNote += OnAppearNote;
            notesModel.onJudgingNote += OnJudgingNote;
            notesModel.onJudgedSuccessNote += OnJudgedSuccessNote;
            notesModel.onJudgedFailureNote += OnJudgedFailureNote;
            notesModel.onPassedNote += OnPassedNote;
        }
        
        //ノーツを配置
        void SetNotes(int measureId)
        {
            char[] noteChars = fumenMeasureStrings[measureId].ToCharArray();

            //0,のように最後に , がつくためlen-1
            for (int i = 0; i < noteChars.Length - 1; i++)
            {
                if (noteChars[i] == (char)NoteType.EMPTY) continue;

                // Lerpに置き換える
                Vector2 x = 
                    Vector2.Lerp(
                        nodePositions[nodeDestinations[measureId]], 
                        nodePositions[nodeDestinations[measureId + 1]],
                        (float)i / (noteChars.Length - 1));
                // Vector2 x = node_P[nodeDestinations[measureId]] + (node_P[nodeDestinations[(measureId + 1)]] - node_P[nodeDestinations[measureId]]) * i / (noteChars.Length - 1);
                noteObjs[notesObjCount] = Instantiate(notesPrefabs[noteChars[i] - '0'], x, Quaternion.identity);

                noteObjs[notesObjCount].transform.localScale = new Vector2(1.5f, 1.5f);

                if (noteChars[i] == (char)NoteType.HOLD_MID)
                {
                    noteObjs[notesObjCount].GetComponent<SpriteRenderer>().sortingOrder = -5000;
                }
                else
                {
                    noteObjs[notesObjCount].GetComponent<SpriteRenderer>().sortingOrder = 1000 - notesObjCount;
                }

                noteManagers[notesObjCount] = noteObjs[notesObjCount].GetComponent<NoteManager>();
                
                noteManagers[notesObjCount].NoteStart();
                noteObjs[notesObjCount].SetActive(false);
                
                notesObjCount++;
            }
        }

        // ノーツが出現した時の演出 Modelのイベント経由で発動
        private void OnUnappearNote(int noteId)
        {
            noteManagers[noteId].NoteStart();
            noteObjs[noteId].SetActive(false);
        }
        // ノーツが出現した時の演出 Modelのイベント経由で発動
        private void OnAppearNote(int noteId, NoteState preState)
        {
            // Debug.Log($"Note Appear {noteId}");
            noteObjs[noteId].SetActive(true);
            if (GameSetting.gameSettingValue.isEffect)
            {
                noteManagers[noteId].Appear(preState);
            }
        }
        // ノーツを判定準備状態にした時の演出 Modelのイベント経由で発動
        private void OnJudgingNote(int noteId)
        {
            if (preJudgingNoteId == noteId) return;
            
            // preJudgingNoteId が -1 , noteId が 0なら preJudgeNoteId を 0 にする
            if(preJudgingNoteId == -1 && noteId == 0) preJudgingNoteId = noteId;
            
            // Debug.Log($"判定するノーツの変更 {preJudgingNoteId} -> {noteId}");
            
            noteManagers[noteId].Judging();
            
            // 前のノーツが出現中なら、Judging状態を解除する
            if (!notesModel.GetNoteState(preJudgingNoteId).IsJudged() && notesModel.GetNoteState(preJudgingNoteId) != NoteState.PASSED )
            {
                noteManagers[preJudgingNoteId].Idle();
            }

            preJudgingNoteId = noteId;
        }
        // ノーツが判定された時の演出 Modelのイベント経由で発動
        private void OnJudgedSuccessNote(int noteId)
        {
            if (GameSetting.gameSettingValue.isEffect)
            {
                noteManagers[noteId].Judged();
            }
            else
            {
                noteObjs[noteId].SetActive(false);
            }
        }
        // ノーツが判定された時の演出 Modelのイベント経由で発動
        private void OnJudgedFailureNote(int noteId)
        {
            // Debug.Log($"失敗ノーツの非表示化 : {noteId}");
            
            if (GameSetting.gameSettingValue.isEffect)
            {
                noteManagers[noteId].Disappear();
            }
            else
            {
                noteObjs[noteId].SetActive(false);
            }
        }
        // ノーツが通過した状態にした時の演出 Modelのイベント経由で発動
        private void OnPassedNote(int noteId)
        {
            if (GameSetting.gameSettingValue.isEffect)
            {
                noteManagers[noteId].Disappear();
            }
            else
            {
                noteObjs[noteId].SetActive(false);
            }
        }
    }
}
