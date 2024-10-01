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
        ����NoteView��Judgemaner�ɂ���m�[�c�̏�ԊǗ���������Ɉڂ�

        */
        
        GameAudioTimer audioTimer;

        // �m�[�c�̏�Ԕz��
        private NoteState[] noteStates;
        // noteStates���O����擾���邽�߂̃v���p�e�B�E�֐�
        public int NoteStateLength => noteStates.Length;
        public NoteState GetNoteState(int index)
        {
            if (index < 0 || index >= noteStates.Length)
            {
                Debug.LogError("�͈͊O�� index ���w�肳�ꂽ");
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
            
            //�m�[�c�̕\���؂芷���ϐ��̐ݒ�
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
            //����g�̋߂��ɗ����m�[�c����\��
            if (activeNotesCount < allNotesCount && notesBeatTimes[activeNotesCount] < audioTimer.AudioOffsettedTime + 1.5f)
            {
                var preState = noteStates[activeNotesCount];
                if (!preState.IsJudged())
                {
                    // �m�[�c�̏o�������F�r���[��
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
                // ����PASSED�� HOLD~~���������AHold���s���Ă�
                var type = noteTypes[unActiveNotesCount];
                if (type is NoteType.HOLD_START or NoteType.HOLD_MID or NoteType.HOLD_END)
                {
                    if (!noteStates[unActiveNotesCount].IsJudged())
                    {
                        Debug.Log($"Hold���ʉ�:{unActiveNotesCount}");
                        FailThisHold(unActiveNotesCount);
                    }
                }

                if (!noteStates[unActiveNotesCount].IsJudged())
                {
                    // �ʂ�߂����̂�MISS
                    // �������Ȃ��d�l�ɂ��Ă���
                }

                // �m�[�c�̏��������F�r���[��
                // Debug.Log($"Passed�ɕύX���� {unactiveNotesCount}");
                ChangeNoteState(unActiveNotesCount, NoteState.PASSED);

                unActiveNotesCount++;
            }
        }
        
        
        /// <summary>
        /// (holdStartNoteId + 1) ~ ���ݎ��� �� HoldMid �������AJUDGED_SUCCESS �ɂ���
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
                    // notesModel�o�R�Ńm�[�c�𔻒��Ԃɂ���
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
        /// ���ݎ��� ~ (holdEndNoteId - 1) �� HoldMid �������AJUDGED_SUCCESS �ɂ���
        /// </summary>
        /// <param name="holdEndNoteId"></param>
        public void JudgeHoldMidsAtHoldEnd(int holdEndNoteId)
        {
            // Debug.Log("HoldEnd����HoldMid��������");
            
            int holdMidNoteId = holdEndNoteId;
            holdMidNoteId--;

            while (true)
            {
                if (noteTypes[holdMidNoteId] == NoteType.HOLD_MID
                    && !GetNoteState(holdMidNoteId).IsJudged())
                {
                    // Debug.Log($"HM����(HE):{holdMidNoteId}");

                    // notesModel�o�R�Ńm�[�c�𔻒菀����Ԃɂ���
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
        /// failureNoteId �� �w�肳�ꂽ Hold ��JUDGE_FAILURE�ɂ���
        /// �߂�l�͂��� Hold �� HoldEnd �� Id
        /// </summary>
        /// <param name="failureNoteId"></param>
        public int FailThisHold(int failureNoteId)
        {
            // noteState �� JUDGED_FAILURE�ɂ���
            
            // JUDGED_FAILURE�ɂ��A���s��Ԃɂ���
            int holdStartSideNoteId = failureNoteId;
            int holdEndSideNoteId = failureNoteId;

            AudioManager.Instance.StopHold();
            //AudioManager.Instance.ShotSE(AudioManager.Instance.SE_holdfail);

            // HoldFail���ꂽ�ꏊ��HOLD�o�Ȃ���ΕԂ�
            var noteType = noteTypes[failureNoteId];
            if (noteType is not (NoteType.HOLD_START or NoteType.HOLD_MID or NoteType.HOLD_END))
            {
                Debug.LogError("Bug : Hold���łȂ��̂�FailThisHold���Ă΂ꂽ");
                return 0;
            }

            // HoldFail���ꂽ�ꏊ���O��HOLD�����s��Ԃɂ���
            noteType = noteTypes[holdStartSideNoteId];
            while (noteType != NoteType.HOLD_START)
            {
                noteType = noteTypes[holdStartSideNoteId];
                
                // Debug.Log("HoldFail���ꂽ�ꏊ���O��HOLD�����s��Ԃɂ���");
                ChangeNoteState(holdStartSideNoteId, NoteState.JUDGED_FAILURE);

                // HoldStart�܂�fail�ɂ�����I��
                if (noteType == NoteType.HOLD_START)
                {
                    break;
                }

                holdStartSideNoteId--;
            }

            noteType = noteTypes[holdEndSideNoteId];
            // HoldFail���ꂽ�ꏊ�����HOLD�����s��Ԃɂ���
            while (noteType != NoteType.TAP)
            {
                noteType = noteTypes[holdEndSideNoteId];
                
                // notesModel�o�R�Ńm�[�c�����s�����Ԃɂ���
                ChangeNoteState(holdEndSideNoteId, NoteState.JUDGED_FAILURE);

                // HoldEnd�܂�fail�ɂ�����I��
                if (noteType == NoteType.HOLD_END)
                {
                    break;
                }

                holdEndSideNoteId++;
            }

            return holdEndSideNoteId;
        }
        
        // �����Ԃ̕ύX
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
                    // ���ɏ����������s���Ă���ꍇ�̓X���[����
                    if (noteStates[noteId].IsUnactiveAnimationed())
                    {
                        return;
                    }
                    noteStates[noteId] = NoteState.JUDGED_FAILURE;
                    onJudgedFailureNote?.Invoke(noteId);
                    break;
                case NoteState.PASSED:
                    // ���ɏ����������s���Ă���ꍇ�̓X���[����
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
        /// NoteState�ɂ�APPEAR�ɕύX����O�̏�Ԃ�����
        /// </summary>
        public event Action<int, NoteState> onAppearNote; 
        public event Action<int> onPassedNote;
        
    }
}
