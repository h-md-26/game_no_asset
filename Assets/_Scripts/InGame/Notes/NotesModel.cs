using System;
using System.Collections;
using System.Collections.Generic;
using StellaCircles.Data;
using UnityEngine;

namespace StellaCircles.InGame
{

    public class NotesModel
    {
        /*
        現在NoteViewとJudgemanerにあるノーツの状態管理をこちらに移す

        */
        
        GameAudioTimer audioTimer;

        // ノーツの状態配列
        private NoteState[] noteStates;
        // noteStatesを外から取得するためのプロパティ・関数
        public int NoteStateLength => noteStates.Length;
        public NoteState GetNoteState(int index)
        {
            if (index < 0 || index >= noteStates.Length)
            {
                Debug.LogError("範囲外の index が指定された");
                return 0;
            }
            
            return noteStates[index];
        }
        
        public readonly int allNotesCount;
        private float[] notesBeatTimes;
        private readonly NoteType[] noteTypes;

        public NotesModel(FumenData fumen, GameAudioTimer audioTimer)
        {
            allNotesCount = fumen.AllNotesCount;
            notesBeatTimes = fumen.NoteBeatTimes;
            noteTypes = fumen.NoteTypes;

            this.audioTimer = audioTimer;
            
            //ノーツの表示切り換え変数の設定
            activeNotesCount = 0;
            unActiveNotesCount = 0;

            noteStates = new NoteState[allNotesCount];
            for (int i = 0; i < allNotesCount; i++)
            {
                noteStates[i] = NoteState.UNAPPEAR;
            }
        }

        int activeNotesCount;
        int unActiveNotesCount;
        private bool isFirstNoteAppeared = false;
        public  bool IsFirstNoteAppeared => isFirstNoteAppeared;
        
        public void NotesModelUpdate()
        {
            //判定枠の近くに来たノーツから表示
            if (activeNotesCount < allNotesCount && notesBeatTimes[activeNotesCount] < audioTimer.AudioOffsettedTime + 1.5f)
            {
                var preState = noteStates[activeNotesCount];
                if (!preState.IsJudged())
                {
                    // ノーツの出現処理：ビュー側
                    ChangeNoteState(activeNotesCount, NoteState.APPEAR);
                }

                if (!isFirstNoteAppeared && activeNotesCount == 0)
                {
                    isFirstNoteAppeared = true;
                }

                activeNotesCount++;
            }

            if (unActiveNotesCount < allNotesCount &&
                notesBeatTimes[unActiveNotesCount] + 0.500f < audioTimer.AudioOffsettedTime)
            {
                // このPASSEDが HOLD~~だった時、Hold失敗を呼ぶ
                var type = noteTypes[unActiveNotesCount];
                if (type is NoteType.HOLD_START or NoteType.HOLD_MID or NoteType.HOLD_END)
                {
                    if (!noteStates[unActiveNotesCount].IsJudged())
                    {
                        Debug.Log($"Holdが通過:{unActiveNotesCount}");
                        FailThisHold(unActiveNotesCount);
                    }
                }

                if (!noteStates[unActiveNotesCount].IsJudged())
                {
                    // 通り過ぎたのでMISS
                    // 何もしない仕様にしてある
                }

                // ノーツの消す処理：ビュー側
                // Debug.Log($"Passedに変更する {unactiveNotesCount}");
                ChangeNoteState(unActiveNotesCount, NoteState.PASSED);

                unActiveNotesCount++;
            }
        }
        
        
        /// <summary>
        /// (holdStartNoteId + 1) ~ 現在時刻 の HoldMid を消し、JUDGED_SUCCESS にする
        /// </summary>
        /// <param name="holdStartNoteId"></param>
        public void JudgeHoldMidsAtHoldStart(int holdStartNoteId)
        {
            int holdMidNoteId = holdStartNoteId;
            holdMidNoteId++;
            while (true)
            {
                if (noteTypes[holdMidNoteId] == NoteType.HOLD_MID
                    && (notesBeatTimes[holdMidNoteId] - audioTimer.AudioOffsettedTime) < 0.010f)
                {
                    // notesModel経由でノーツを判定状態にする
                    ChangeNoteState(holdMidNoteId, NoteState.JUDGED_SUCCESS);
                }
                else
                {
                    break;
                }

                holdMidNoteId++;
            }
        }

        /// <summary>
        /// 現在時刻 ~ (holdEndNoteId - 1) の HoldMid を消し、JUDGED_SUCCESS にする
        /// </summary>
        /// <param name="holdEndNoteId"></param>
        public void JudgeHoldMidsAtHoldEnd(int holdEndNoteId)
        {
            // Debug.Log("HoldEnd時のHoldMid同時けし");
            
            int holdMidNoteId = holdEndNoteId;
            holdMidNoteId--;

            while (true)
            {
                if (noteTypes[holdMidNoteId] == NoteType.HOLD_MID
                    && !GetNoteState(holdMidNoteId).IsJudged())
                {
                    // Debug.Log($"HM消す(HE):{holdMidNoteId}");

                    // notesModel経由でノーツを判定準備状態にする
                    ChangeNoteState(holdMidNoteId, NoteState.JUDGED_SUCCESS);
                }
                else
                {
                    // Debug.Log($"HM:{holdMidNoteId} , {noteTypes[holdMidNoteId].ToString()} , {GetNoteState(holdMidNoteId).ToString()}");
                    break;
                }

                holdMidNoteId--;
            }
        }
        
        
        /// <summary>
        /// failureNoteId で 指定された Hold をJUDGE_FAILUREにする
        /// 戻り値はこの Hold の HoldEnd の Id
        /// </summary>
        /// <param name="failureNoteId"></param>
        public int FailThisHold(int failureNoteId)
        {
            // noteState を JUDGED_FAILUREにする
            
            // JUDGED_FAILUREにし、失敗状態にする
            int holdStartSideNoteId = failureNoteId;
            int holdEndSideNoteId = failureNoteId;

            AudioManager.Instance.StopHold();
            //AudioManager.Instance.ShotSE(AudioManager.Instance.SE_holdfail);

            // HoldFailされた場所がHOLD出なければ返す
            var noteType = noteTypes[failureNoteId];
            if (noteType is not (NoteType.HOLD_START or NoteType.HOLD_MID or NoteType.HOLD_END))
            {
                Debug.LogError("Bug : Hold中でないのにFailThisHoldが呼ばれた");
                return 0;
            }

            // HoldFailされた場所より前のHOLDを失敗状態にする
            noteType = noteTypes[holdStartSideNoteId];
            while (noteType != NoteType.HOLD_START)
            {
                noteType = noteTypes[holdStartSideNoteId];
                
                // Debug.Log("HoldFailされた場所より前のHOLDを失敗状態にする");
                ChangeNoteState(holdStartSideNoteId, NoteState.JUDGED_FAILURE);

                // HoldStartまでfailにしたら終了
                if (noteType == NoteType.HOLD_START)
                {
                    break;
                }

                holdStartSideNoteId--;
            }

            noteType = noteTypes[holdEndSideNoteId];
            // HoldFailされた場所より後のHOLDを失敗状態にする
            while (noteType != NoteType.TAP)
            {
                noteType = noteTypes[holdEndSideNoteId];
                
                // notesModel経由でノーツを失敗判定状態にする
                ChangeNoteState(holdEndSideNoteId, NoteState.JUDGED_FAILURE);

                // HoldEndまでfailにしたら終了
                if (noteType == NoteType.HOLD_END)
                {
                    break;
                }

                holdEndSideNoteId++;
            }

            return holdEndSideNoteId;
        }
        
        // 判定状態の変更
        public void ChangeNoteState(int noteId, NoteState noteState)
        {
            switch (noteState)
            {
                case NoteState.UNAPPEAR:
                    noteStates[noteId] = NoteState.UNAPPEAR;
                    onUnappearNote?.Invoke(noteId);
                    break;
                case NoteState.APPEAR:
                    var preState = noteStates[noteId];
                    noteStates[noteId] = NoteState.APPEAR;
                    onAppearNote?.Invoke(noteId, preState);
                    break;
                case NoteState.JUDGING:
                    noteStates[noteId] = NoteState.JUDGING;
                    onJudgingNote?.Invoke(noteId);
                    break;
                case NoteState.JUDGED_SUCCESS: 
                    noteStates[noteId] = NoteState.JUDGED_SUCCESS;
                    onJudgedSuccessNote?.Invoke(noteId);
                    break;
                case NoteState.JUDGED_FAILURE: 
                    // 既に消す処理を行っている場合はスルーする
                    if (noteStates[noteId].IsUnactiveAnimationed())
                    {
                        return;
                    }
                    noteStates[noteId] = NoteState.JUDGED_FAILURE;
                    onJudgedFailureNote?.Invoke(noteId);
                    break;
                case NoteState.PASSED:
                    // 既に消す処理を行っている場合はスルーする
                    if (noteStates[noteId].IsUnactiveAnimationed())
                    {
                        return;
                    }
                    noteStates[noteId] = NoteState.PASSED;
                    onPassedNote?.Invoke(noteId);
                    break;
            }
        }

        public event Action<int> onUnappearNote; 
        public event Action<int> onJudgingNote;
        public event Action<int> onJudgedSuccessNote;
        public event Action<int> onJudgedFailureNote;
        /// <summary>
        /// NoteStateにはAPPEARに変更する前の状態を入れる
        /// </summary>
        public event Action<int, NoteState> onAppearNote; 
        public event Action<int> onPassedNote;
        
    }
}
