using UnityEngine;

namespace StellaCircles.InGame
{
    public class JudgeNoteDecider
    {
        public JudgeNoteDecider(FumenData fumenData, NotesModel notesModel, GameAudioTimer audioTimer)
        {
            notesBeatTimes = fumenData.NoteBeatTimes;
            noteTypes = fumenData.NoteTypes;
            
            this.notesModel = notesModel;
            
            this.audioTimer = audioTimer;

            judgingNoteId = 0;
            isJudgingHoldStart = false;
            
            SetJudgingNoteId(0);
        }

        private readonly GameAudioTimer audioTimer;

        private readonly NotesModel notesModel;
        private readonly float[] notesBeatTimes;
        private readonly NoteType[] noteTypes;
        
        
        // ���肷��m�[�c ������A���t���[���X�V�����
        private int judgingNoteId;
        public int JudgingNoteId => judgingNoteId;
        
        /// <summary>
        /// HoldStart �� judgingNoteId �ɐݒ肵���t���O�B
        /// HoldStart�������莞�� isJudgingHoldEnd �� true�ɂ���Ƃ�������
        /// </summary>
        bool isJudgingHoldStart;
        public bool IsJudgingHoldStart => isJudgingHoldStart;
        /// <summary>
        /// HoldEnd �� judgingNoteId �ɐݒ肵���t���O�B
        /// ���ꂪtrue�̂Ƃ��̂�HoldMid�̏������s���Ƃ�������
        /// </summary>
        bool isJudgingHoldEnd;
        public bool IsJudgingHoldEnd => isJudgingHoldEnd;
        
        /// <summary>
        /// ���݂�Hold�� HoldStart �� Id�BHold���Ă��Ȃ��Ƃ��͕��̒l
        /// </summary>
        int currentHoldStartId;
        /// <summary>
        /// ���݂�Hold�� HoldStart �� Id�BHold���Ă��Ȃ��Ƃ��͕��̒l
        /// </summary>
        public int CurrentHoldStartId => currentHoldStartId;
        /// <summary>
        /// ���݂�Hold�� HoldEnd �� Id�BHold���Ă��Ȃ��Ƃ��͕��̒l
        /// </summary>
        int currentHoldEndId;
        /// <summary>
        /// ���݂�Hold�� HoldEnd �� Id�BHold���Ă��Ȃ��Ƃ��͕��̒l
        /// </summary>
        public int CurrentHoldEndId => currentHoldEndId;

        /// <summary>
        /// Judgementer �o�R�Ŗ��t���[���Ăяo��
        /// </summary>
        public void JudgementerUpdate()
        {
            if (judgingNoteId >= notesBeatTimes.Length - 1) return;

            //��ԋ߂��m�[�c�����𔻒�
            UpdateJudgingNoteId(judgingNoteId);
        }
        
        /// <summary>
        /// ���ݎ����ƈ�ԋ߂��m�[�c���Z�b�g����
        /// �m�[�c�̒T���� currentJudgingNoteId �̎���ōs��
        /// </summary>
        public void UpdateJudgingNoteId(int currentJudgingNoteId)
        {
            SetJudgingNoteId(GetNewJudgingId(currentJudgingNoteId));
        }

        // ���߂̃m�[�c����x�ł�JUDGING�ɕύX������
        private bool isChangeJudgingFirstNote;
        
        //judgingNoteId �� noteId ���Z�b�g����
        private void SetJudgingNoteId(int noteId)
        {
            // �͈͓��Ɏ��߂�
            if (noteId < 0)
            {
                Debug.LogError("Judging note id is negative");
                noteId = 0;
            }
            else if (noteId >= notesModel.allNotesCount)
            {
                Debug.LogError("Judging note id is too large");
                noteId = notesModel.allNotesCount - 1;
            }

            // ���߂̃m�[�c�̏o����ɏ��߂̃m�[�c������͈͓��ɓ���A
            // ���߂̃m�[�c��Judging�ɂ������Ƃ��������Judging�ɂ���
            if (noteId == 0 
                && notesModel.IsFirstNoteAppeared 
                && !isChangeJudgingFirstNote 
                && Mathf.Abs(notesBeatTimes[noteId] - audioTimer.AudioOffsettedTime) <= Judgementer.BAD_AREA)
            {
                // Debug.Log($"���߂̃m�[�c�̏�Ԃ�JUDGING�ɕύX���� {notesModel.GetNoteState(noteId)}");
                isChangeJudgingFirstNote = true;
                notesModel.ChangeNoteState(noteId, NoteState.JUDGING);
            }
            // ���菀���m�[�c���ύX���ꂽ��AnotesModel�o�R�Ńm�[�c�𔻒菀����Ԃɂ���
            else if (noteId != judgingNoteId)
            {
                notesModel.ChangeNoteState(noteId, NoteState.JUDGING);
                // ����ς݂łȂ��A�ʉ߂��Ă����Ȃ��Ȃ� APPEAR�ɖ߂�
                if(!notesModel.GetNoteState(judgingNoteId).IsJudged() && notesModel.GetNoteState(judgingNoteId) != NoteState.PASSED)
                    notesModel.ChangeNoteState(judgingNoteId, NoteState.APPEAR);
            }

            // ����m�[�c�̍X�V
            judgingNoteId = noteId;
        }

        //���ݎ����Ɉ�ԋ߂��m�[�c��Ԃ�
        private int GetNewJudgingId(int currentJudgingNoteId)
        {
            /*
            ���肷��m�[�c�� HoldStart or HoldEnd�Ȃ�A
            HoldMid�͓���Ȕ�����s���̂� HoldStart, HoldEnd�̔���͈͊O�ɏo��܂ł͔���m�[�c�����b�N����
            
            ����ȊO�Ȃ�A��ԋ߂��m�[�c�����݂̔���m�[�c�̈ʒu������`�T������
            ���t���[���Ă΂�邽�߁A2���T�����������Ȃ�
            */
            
            // HoldStart�̃��b�N����
            // HoldStart�m�[�c�������肷��m�[�c�Ȃ�AHoldStart�m�[�c�̋��e�͈͓�����o��܂ł�JudgingNoteId��ύX���Ȃ�
            if (isJudgingHoldStart)
            {
                // HS�m�[�c�ƌ��ݎ����̍����O�� BadArea �𒴂�����
                // JudgingNoteId��HoldStart�ւ̃��b�N����������
                if (notesBeatTimes[currentHoldStartId] - audioTimer.AudioOffsettedTime < -Judgementer.BAD_AREA)
                {
                    isJudgingHoldStart = false;
                    currentHoldStartId = -1;
                }
                else
                {
                    // HoldStartId�Ń��b�N����
                    return currentHoldStartId;
                }
            }
            
            // HoldEnd�̃��b�N����
            // HoldEnd�m�[�c�������肷��m�[�c�Ȃ�AHoldEnd�m�[�c�̋��e�͈͂̌��ɂ����܂ł�JudgingNoteId��ύX���Ȃ�
            if (isJudgingHoldEnd)
            {
                // HoldEnd��BadArea����둤�ŉz���� or HoldEnd�̎��̃m�[�c�̕������ݎ����ɋ߂��m�[�c�ɂȂ���
                // �Ȃ�A���b�N���������Ď��̃m�[�c��T������
                
                // Debug.Log("isJudgingHoldEnd : " +isJudgingHoldEnd + ", currentHoldEndId : " +currentHoldEndId);
                
                if (notesBeatTimes[currentHoldEndId] - audioTimer.AudioOffsettedTime >= -Judgementer.BAD_AREA
                    || Mathf.Abs(notesBeatTimes[currentHoldEndId] - audioTimer.AudioOffsettedTime)
                    < Mathf.Abs(notesBeatTimes[GetNextUnJudgedNote(currentHoldEndId + 1, SearchDirection.Positive)] - audioTimer.AudioOffsettedTime))
                {
                    return currentHoldEndId;
                }
                else
                {
                    Debug.Log("HoldEnd�̃��b�N����");
                }
            }

            
            // HoldStart, HoldEnd�łȂ��Ȃ�A���ݎ����Ɉ�ԋ߂��m�[�c��Ԃ�
            // ���̎�HoldMid, HoldEnd�͑I�����Ȃ�
            if(currentJudgingNoteId < 0) currentJudgingNoteId = 0;
            if (currentJudgingNoteId >= notesModel.allNotesCount) currentJudgingNoteId = notesModel.allNotesCount - 1;
            int searchingNoteId = GetNextUnJudgedNote(currentJudgingNoteId + 1, SearchDirection.Positive);
            // currentJudgingNoteId ���� 1���̃m�[�c �̕������ݎ����ɋ߂��ꍇ�A�ł��߂��m�[�c�͐������ɂ���
            if (Mathf.Abs(notesBeatTimes[searchingNoteId] - audioTimer.AudioOffsettedTime)
                < Mathf.Abs(notesBeatTimes[currentJudgingNoteId] - audioTimer.AudioOffsettedTime))
            {
                // �������ɐ��`�T�������čł��߂��m�[�c��T��
                // ���t���[���s�����߁A2���T�������v�Z�ʂ��������Ȃ�
                searchingNoteId = GetNearestJudgedNoteThisDirection(searchingNoteId, SearchDirection.Positive);
            }
            // currentJudgingNoteId��1���̃m�[�c ���� currentJudgingNoteId �̕������ݎ����ɋ߂��ꍇ�A�ł��߂��m�[�c�͕����� or ���݂̃m�[�c
            else
            {
                // �������ɐ��`�T�������čł��߂��m�[�c��T��
                // ���t���[���s�����߁A2���T�������v�Z�ʂ��������Ȃ�
                searchingNoteId = GetNearestJudgedNoteThisDirection(currentJudgingNoteId, SearchDirection.Negative);
            }
            
            
            // ��ԋ߂��m�[�c��HoldStart�ɕύX���ꂽ��A�t���O�𗧂āAHoldEnd��T��
            // if (!isJudgingHoldStart && !isHolding && noteTypes[searchingNoteId] == NoteType.HOLD_START)
            if (!isJudgingHoldStart && noteTypes[searchingNoteId] == NoteType.HOLD_START)
            {
                isJudgingHoldStart = true;
                currentHoldStartId = searchingNoteId;
                
                // HoldEnd��T��
                currentHoldEndId = SearchHoldEndId(currentHoldStartId);
            }

            if (noteTypes[searchingNoteId] == NoteType.HOLD_MID)
            {
                Debug.LogWarning("HoldMID������m�[�c�ɂȂ���");
                LogHoldingValue();
            }
            
            return searchingNoteId;
        }

        /// <summary>
        /// �f�o�b�O�p�Ƀz�[���h�֘A�̃t���O��f���o��
        /// </summary>
        void LogHoldingValue()
        {
            Debug.Log(
                $"IsJudgingHoldStart: {IsJudgingHoldStart}, IsJudgingHoldEnd: {IsJudgingHoldEnd}, CurrentHoldStartId: {CurrentHoldStartId}, CurrentHoldEndId: {CurrentHoldEndId}");
        }

        /// <summary>
        /// HoldStartId�ɑΉ�����HoldEndId��Ԃ�
        /// </summary>
        /// <param name="holdStartId"></param>
        /// <returns></returns>
        private int SearchHoldEndId(int holdStartId)
        {
            int holdEndId = holdStartId + 1;
                
            // HoldEnd��T��
            while (noteTypes[holdEndId] != NoteType.HOLD_END)
            {
                holdEndId++;
            }
            
            return holdEndId;
        }

        /// <summary>
        /// �w������̌��ݒn�_�����ԋ߂�������m�[�c���擾
        /// HoldEnd, HoldMid �͎擾���Ȃ�
        /// </summary>
        private int GetNextUnJudgedNote(int startNoteId, SearchDirection dir)
        {
            int searchingNoteId = startNoteId;

            while (true)
            {
                // �͈͓��Ɏ��߂鏈��
                if (searchingNoteId <= 0)
                {
                    return 0;
                }
                if (searchingNoteId >= notesModel.NoteStateLength - 1)
                {
                    return notesModel.NoteStateLength - 1;
                }
                
                // ����ς݂łȂ��A�ʉ߂��Ă��Ȃ��m�[�c�Ȃ�I������
                if (!notesModel.GetNoteState(searchingNoteId).IsJudged() &&
                    noteTypes[searchingNoteId] != NoteType.HOLD_END &&
                    noteTypes[searchingNoteId] != NoteType.HOLD_MID &&
                    notesModel.GetNoteState(searchingNoteId) != NoteState.PASSED)
                {
                    return searchingNoteId;
                }

                //�����m�F
                searchingNoteId += (int)dir;
            }
        }
        
        /// <summary>
        /// �w������̌��ݎ����Ɉ�ԋ߂�������m�[�c���擾
        /// </summary>
        private int GetNearestJudgedNoteThisDirection(int startNoteId, SearchDirection dir)
        {
            int preNoteId = startNoteId;
            int searchingNoteId = startNoteId;

            while (true)
            {
                searchingNoteId = GetNextUnJudgedNote(preNoteId, dir);
                
                // ����ȏセ�̕����ɐi�߂Ȃ��ꍇ�A���̕����̍Ō�̒l��Ԃ�
                if (searchingNoteId == preNoteId)
                {
                    return searchingNoteId;
                }
                
                // �������r
                if (Mathf.Abs(notesBeatTimes[preNoteId] - audioTimer.AudioOffsettedTime)
                    <= Mathf.Abs(notesBeatTimes[searchingNoteId] - audioTimer.AudioOffsettedTime))
                {
                    // searchingNoteId ���� preNoteId �̕������ݎ����ɋ߂��̂� preNoteId ��Ԃ�
                    return preNoteId;
                }

                //�����m�F
                preNoteId = searchingNoteId;
            }
            
        }

        /// <summary>
        /// HoldStart���莞�ɌĂ�
        /// </summary>
        public void JudgedHoldStart()
        {
            isJudgingHoldStart = false;
            isJudgingHoldEnd = true;

            if (currentHoldEndId < 0)
            {
                // �O��Hold�̔��肪�I���O�Ɏ���Hold�̔��肪�n�܂�Ƃ����ɂ��Ă��܂�
                Debug.Log($"judgedHoldStart : startId : {currentHoldStartId}, endId : {currentHoldEndId}");
                currentHoldEndId = SearchHoldEndId(currentHoldStartId);
            }

        }
        
        /// <summary>
        /// HoldEnd���莞�ɌĂ�
        /// </summary>
        public void JudgedHoldEnd()
        {
            isJudgingHoldStart = false;
            isJudgingHoldEnd = false;
            currentHoldEndId = -1;
            currentHoldStartId = -1;
        }

        /// <summary>
        /// HoldEnd������͈͓����ǂ���
        /// </summary>
        public bool IsHoldEndInJudgeArea
        {
            get
            {
                if (!isJudgingHoldEnd) return false;
                
                // HoldEnd�����ݎ����Ɨ���Ă��Ȃ����True
                return (Mathf.Abs(notesBeatTimes[CurrentHoldEndId] - audioTimer.AudioOffsettedTime) <= Judgementer.BAD_AREA);
            }
        }
        
        /// <summary>
        /// HoldEnd������͈͂�ʂ�߂������ǂ���
        /// </summary>
        public bool IsPassedHoldEndJudgeArea
        {
            get
            {
                if (!isJudgingHoldEnd) return false;
                
                // HoldEnd�����ݎ����Ɨ���Ă��Ȃ����True
                return (notesBeatTimes[CurrentHoldEndId] - audioTimer.AudioOffsettedTime) <= -Judgementer.BAD_AREA;
            }
        }
        
        private enum SearchDirection
        {
            Positive = 1,
            Negative = -1,
        }
        
    
    }
}
