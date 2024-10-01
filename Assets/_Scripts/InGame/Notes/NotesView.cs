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
        /// �m�[�c�̔z�u�A�o���Ȃǂ̎��o�I�ȊǗ�
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
        
        // �O��JudgingNoteId, JudgingNoteId���ύX���ꂽ�Ƃ��Ɍ���JudgingNote�̏�Ԃ�߂����߂Ɏg�p����
        private int preJudgingNoteId = -1;
        
        public void GameStart(FumenData fumen, GameAudioTimer audioTimer, NotesModel notesModel)
        {
            fumenMeasureStrings = fumen.FumenMeasureStrings;
            nodeDestinations = fumen.NodeDestinations;
            allNotesCount = fumen.AllNotesCount;
            noteTypes = fumen.NoteTypes;
            
            this.audioTimer = audioTimer;
            this.notesModel = notesModel;
            
            //�m�[�h�̈ʒu�o�^
            nodePositions = new Vector2[6];
            for (int i = 0; i < 6; i++)
            {
                nodePositions[i] = nodeTransforms[i].transform.position;
            }

            //�m�[�c�̐ݒu
            notesObjCount = 0;
            noteObjs = new GameObject[allNotesCount];
            noteManagers = new NoteManager[allNotesCount];
            for (int i = 0; i < fumenMeasureStrings.Length; i++)
            {
                SetNotes(i);
            }
            
            // �C�x���g�̓o�^
            notesModel.onUnappearNote += OnUnappearNote;
            notesModel.onAppearNote += OnAppearNote;
            notesModel.onJudgingNote += OnJudgingNote;
            notesModel.onJudgedSuccessNote += OnJudgedSuccessNote;
            notesModel.onJudgedFailureNote += OnJudgedFailureNote;
            notesModel.onPassedNote += OnPassedNote;
        }
        
        //�m�[�c��z�u
        void SetNotes(int measureId)
        {
            char[] noteChars = fumenMeasureStrings[measureId].ToCharArray();

            //0,�̂悤�ɍŌ�� , ��������len-1
            for (int i = 0; i < noteChars.Length - 1; i++)
            {
                if (noteChars[i] == (char)NoteType.EMPTY) continue;

                // Lerp�ɒu��������
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

        // �m�[�c���o���������̉��o Model�̃C�x���g�o�R�Ŕ���
        private void OnUnappearNote(int noteId)
        {
            noteManagers[noteId].NoteStart();
            noteObjs[noteId].SetActive(false);
        }
        // �m�[�c���o���������̉��o Model�̃C�x���g�o�R�Ŕ���
        private void OnAppearNote(int noteId, NoteState preState)
        {
            // Debug.Log($"Note Appear {noteId}");
            noteObjs[noteId].SetActive(true);
            if (GameSetting.gameSettingValue.isEffect)
            {
                noteManagers[noteId].Appear(preState);
            }
        }
        // �m�[�c�𔻒菀����Ԃɂ������̉��o Model�̃C�x���g�o�R�Ŕ���
        private void OnJudgingNote(int noteId)
        {
            if (preJudgingNoteId == noteId) return;
            
            // preJudgingNoteId �� -1 , noteId �� 0�Ȃ� preJudgeNoteId �� 0 �ɂ���
            if(preJudgingNoteId == -1 && noteId == 0) preJudgingNoteId = noteId;
            
            // Debug.Log($"���肷��m�[�c�̕ύX {preJudgingNoteId} -> {noteId}");
            
            noteManagers[noteId].Judging();
            
            // �O�̃m�[�c���o�����Ȃ�AJudging��Ԃ���������
            if (!notesModel.GetNoteState(preJudgingNoteId).IsJudged() && notesModel.GetNoteState(preJudgingNoteId) != NoteState.PASSED )
            {
                noteManagers[preJudgingNoteId].Idle();
            }

            preJudgingNoteId = noteId;
        }
        // �m�[�c�����肳�ꂽ���̉��o Model�̃C�x���g�o�R�Ŕ���
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
        // �m�[�c�����肳�ꂽ���̉��o Model�̃C�x���g�o�R�Ŕ���
        private void OnJudgedFailureNote(int noteId)
        {
            // Debug.Log($"���s�m�[�c�̔�\���� : {noteId}");
            
            if (GameSetting.gameSettingValue.isEffect)
            {
                noteManagers[noteId].Disappear();
            }
            else
            {
                noteObjs[noteId].SetActive(false);
            }
        }
        // �m�[�c���ʉ߂�����Ԃɂ������̉��o Model�̃C�x���g�o�R�Ŕ���
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
