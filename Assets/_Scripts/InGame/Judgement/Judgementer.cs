using System;
using StellaCircles.Data;
using UnityEngine;

namespace StellaCircles.InGame
{
    /// <summary>
    /// Inputter���炤�������͂ɑ΂��Ĕ��肷��
    /// ���肷��m�[�c��Updater�����Ŗ��t���[���X�V����
    /// </summary>
    public class Judgementer
    {
        public const float BAD_AREA = 0.180f;
        public const float NICE_AREA = 0.080f;
        public const float GREAT_AREA = 0.050f;
        
        // ��������Tap��Flick�����肳���o�O���m�F;
        
        /*
         TODO
        ����邱��
            Judgementer�́u����m�[�c���Z�b�g���鏈���v�Ɓu���͂ɑ΂��Ĕ��肷�鏈���v��ʃN���X�ɕ����邱�Ƃ���������
            [SerializeField]���Ċm�F����
            ����Εϐ������Ȃ����m�F����
                �Ƃ肠����AutoPlay�ɂ͂������̂Œ���
            ���Ȃ����Select�̃��t�@�N�^�����O���s��
        */
        
        /// <summary>
        /// SE�𑀍쎞�ɂȂ炷�ꍇtrue�A���莞�ɂȂ炷�ꍇfalse
        /// </summary>
        readonly bool isSeTimingOperation;

        readonly GameAudioTimer audioTimer;
        readonly NotesModel notesModel;
        private readonly JudgeNoteDecider judgeNoteDecision;

        private readonly float[] notesBeatTimes;
        private readonly NoteType[] noteTypes;
        
        // ���莞�ɌĂԃC�x���g ������o�R���ăX�R�A�┻��̃r���[���s��
        public event Action<JudgeResultType> onJudged;

        public Judgementer(FumenData fumenData, GameScoreModel scoreModel, GameAudioTimer  audioTimer, NotesModel notesModel)
        {
            notesBeatTimes = fumenData.NoteBeatTimes;
            noteTypes = fumenData.NoteTypes;
            
            // ���莞�ɃX�R�A��OnJudged���Ă�
            onJudged += scoreModel.OnJudged;
            
            this.audioTimer = audioTimer;
            this.notesModel = notesModel;
            judgeNoteDecision = new(fumenData, notesModel, audioTimer);
            
            isSeTimingOperation = GameSetting.gameSettingValue.isSETimingOperation;
            
            holdingFinId = -1;
        }

        // �z�[���h���Ă���w��id �z�[���h���Ă��Ȃ��Ƃ��͕��̒l�ɂ���
        int holdingFinId;

        /// <summary>
        /// Hold�����ǂ�����holdingFinId���m�F���邱�ƂŕԂ��v���p�e�B
        /// </summary>
        bool IsHolding => holdingFinId != -1;

        /// <summary>
        /// GameUpdater ���疈�t���[���Ăяo��
        /// </summary>
        public void JudgementerUpdate()
        {
            judgeNoteDecision.JudgementerUpdate();
        }

        // HM��HoldEnd�t���O���g���ď������������܂������ĂȂ�;
        
        
        /// <summary>
        /// ����̋N�_
        /// ���͂̎�ނƎwId���󂯎���Ĕ��肷��
        /// </summary>
        public void JudgeNotes(InputType inputType, int finId)
        {
            var currentJudgeNoteId = judgeNoteDecision.JudgingNoteId;
            // Debug.Log($"Judge!! type: {inputType}, id: {currentJudgeNoteId}");

            // Hold���Ɏw�𗣂�����AHold���s�ɂ���
            if (inputType == InputType.Release && finId == holdingFinId && !judgeNoteDecision.IsHoldEndInJudgeArea)
            {
                Debug.Log("Hold���Ɏw�𗣂���");
                FailHold(currentJudgeNoteId);
            }
            
            // Hold��HoldEnd���߂�����������Ă�����Hold���s�ɂ���
            if (judgeNoteDecision.IsJudgingHoldEnd && inputType == InputType.Holding &&
                judgeNoteDecision.IsPassedHoldEndJudgeArea)
            {
                Debug.Log("HoldEnd�𒴂�����Hold�𑱂��Ă���");
                FailHold(currentJudgeNoteId);
            }

            // ���菀������Ă��Ȃ��m�[�c�͔��肵�Ȃ�
            if (notesModel.GetNoteState(currentJudgeNoteId) != NoteState.JUDGING) return;

            // ���ݎ����Ɣ���m�[�c�̍��̐�Βl
            float timeDif = Mathf.Abs(notesBeatTimes[currentJudgeNoteId] - audioTimer.AudioOffsettedTime);
            
            // isJudgingHoldEnd �̂Ƃ��AHold���͂Ȃ�HoldMid�𔻒肷��
            if (judgeNoteDecision.IsJudgingHoldEnd && inputType == InputType.Holding) // ����������
            {
                // Debug.Log("HoldMid�̔���");
                
                // ���݂̎����܂ł̖������HoldMid�𔻒肷��
                JudgeHoldMid(judgeNoteDecision.CurrentHoldStartId, finId);
            }
            else //�������ȊO�̓���
            {
                // ���͂ƑΉ�����m�[�c�łȂ����return
                // Hold�̎��s�������s��
                switch (inputType)
                {
                    // Tap����
                    case InputType.Tap:
                    {
                        // Hold����Tap�����Ȃ�AHold���s�ɂ���
                        // if (judgeNoteDecision.IsJudgingHoldEnd && !judgeNoteDecision.IsHoldEndInJudgeArea)
                        if(IsHolding)
                        {
                            // Debug.Log("Hold����Tap����");
                            FailHold(currentJudgeNoteId);
                        }

                        // Tap���͂ł́ATap,HS�m�[�c�݂̂𔻒�
                        if (noteTypes[currentJudgeNoteId] is not (NoteType.TAP or NoteType.HOLD_START))
                        {
                            // Debug.Log($"Tap���͂ł́ATap,HS�m�[�c�݂̂𔻒肷��̂� return : {noteTypes[currentJudgeNoteId]}");
                            return;
                        }

                        break;
                    }
                    // �t���b�N�ł́AFlick�m�[�c�̂ݔ���
                    case InputType.Flick:
                        // Debug.Log("Flick���͂���");
                        if (noteTypes[currentJudgeNoteId] != NoteType.FLICK)
                        {
                            // Debug.Log($"�t���b�N�ł́AFlick�m�[�c�̂ݔ��肷��̂� return : {noteTypes[currentJudgeNoteId]}");
                            return;   
                        }
                        break;
                    // �w�����ł́AHoldEnd�m�[�c�̂ݔ���
                    case InputType.Release:
                        // Debug.Log($"�w�����ł́AHoldEnd�m�[�c�̂ݔ���, noteTypes[currentJudgeNoteId] : {noteTypes[currentJudgeNoteId]}");
                        if(noteTypes[currentJudgeNoteId] != NoteType.HOLD_END) return;
                        if (finId != holdingFinId) return; // HoldEnd�̂Ƃ��A finId�������Ă��鎞��������
                        break;
                    default:
                        // Debug.Log("���̓��͔͂��肵�Ȃ�");
                        return;
                }

                // ����͈͊O�Ȃ� return
                if (timeDif >= BAD_AREA) return;
                
                // ���͂Ɣ��肷��m�[�c�̎�ނ������Ă���΂����ɂ��ǂ蒅���A���肳���
                BandJudge(TimeDif2JudgeResultType(timeDif), currentJudgeNoteId, finId);
            }
        }
        
        /// <summary>
        /// Tap, Flick, HoldStart, HoldEnd ���̎��Ԕ͈͂Ŕ��肷��m�[�c�̐������ɌĂ�
        /// ���s�����A�Ή����Ă��Ȃ�����͎��O�ɏ���
        /// </summary>
        private void BandJudge(JudgeResultType judgeResultType, int currentJudgeNoteId, int finId)
        {
            #region Hold�d�l
            // HOLD_START �̂Ƃ��AGreat or Good �̂Ƃ��͎wID���L�^���Ă����A
            //                    Bad �̂Ƃ��͂��̐��HOLD�����ׂď����A���ʂ��o��
            //                    ���̃m�[�c���Ƃ�߂��������A����
            //
            //                    �ȏ�̎����̂��߁AHOLD_END���画��Ώۂ��ς�������ɋL�^�����wID�� -1 �ɂ��Ă����B
            //                                      Bad, �ʉ߂̎����O�̂��� -1 �ɂ��Ă���
            //
            // HOLD_MID �̂Ƃ��A���͂�Holding �wID���L�^���ꂽ���̂̎����肷��
            //                         �r���Ŏw��b�����ꍇ (���͂�Release��Note��LINE or CURVE)
            //                           ���̐��HOLD�����ׂď����A���ʂ��o��
            // HOLD_END �̂Ƃ��A       ���͂�Release �wID���L�^���ꂽ���̂̎����肷��
            //
            // ���z�[���h���s���ɑO�̕���������
            #endregion
            
            var noteType = noteTypes[currentJudgeNoteId];
            
            // �m�[�c�̔��莞�̓��ꏈ��
            NoteJudgedSpecialProcess(currentJudgeNoteId, noteType, judgeResultType, finId);

            // SE�̏ꍇ����
            PlaySEOnJudged(noteType, judgeResultType);
                
            // Hold���̎�����
            if (noteType == NoteType.HOLD_END)
            {
                holdingFinId = -1;
            }

            //���ʏ���
            OnJudgedSuccess(currentJudgeNoteId);
        }

        /// <summary>
        /// �m�[�c���Ƃ̓��ꏈ��
        /// </summary>
        private void NoteJudgedSpecialProcess(int judgedNoteId, NoteType noteType, JudgeResultType judgeResultType, int finID)
        {
            // ���茋�ʂ��Ƃ̃A�j���[�V����������
            onJudged?.Invoke(judgeResultType);
            
            //�m�[�c�̎�ށE���茋�ʂɉ����ď����𕪂���
            // Tap, Flick �͓��ʂȏ����͂Ȃ�
            switch (noteType)
            {
                case NoteType.HOLD_START:
                    // Hold���n�߂��w���L��
                    holdingFinId = finID;
                    // Hold�J�n�t���O�𗧂āAHoldEnd�ɔ�����
                    judgeNoteDecision.JudgedHoldStart();
                    notesModel.JudgeHoldMidsAtHoldStart(judgedNoteId);
                    break;
                case NoteType.HOLD_END:
                    // Hold�֘A�̃t���O��܂�
                    holdingFinId = -1;
                    judgeNoteDecision.JudgedHoldEnd();
                    notesModel.JudgeHoldMidsAtHoldEnd(judgedNoteId);
                    break;
            }
        }
        
        // ���茋�ʂ�ASE�̃^�C�~���O�ݒ�ɍ��킹��SE��炷���E����炷����ύX����
        private void PlaySEOnJudged(NoteType noteType, JudgeResultType judgeResultType)
        {
            switch (noteType)
            {
                case NoteType.HOLD_START:
                    AudioManager.Instance.PlayHold();
                    break;
                case NoteType.HOLD_END:
                    AudioManager.Instance.StopHold();
                    AudioManager.Instance.ShotSE(AudioManager.Instance.SE_holdend);
                    break;
                case NoteType.TAP:
                {
                    // ���莞�ɖ炷�ݒ�Ȃ� BAD�̏ꍇ����������SE��炷
                    if (!isSeTimingOperation)
                    {
                        var tapSe = judgeResultType == JudgeResultType.BAD ? AudioManager.Instance.SE_tapmiss : AudioManager.Instance.SE_tap;
                        AudioManager.Instance.ShotSE(tapSe);
                    }
                    break;
                }
                case NoteType.FLICK:
                {
                    // ���莞�ɖ炷�ݒ�Ȃ� BAD�̏ꍇ����������SE��炷
                    if (!isSeTimingOperation)
                    {
                        var flickSe = judgeResultType == JudgeResultType.BAD ? AudioManager.Instance.SE_frickmiss : AudioManager.Instance.SE_frick;
                        AudioManager.Instance.ShotSE(flickSe);
                    }
                    break;
                }
            }
        }
        
        /// <summary>
        /// ���݂̎����܂ł̖������HoldMid�𔻒肷��
        /// </summary>
        private void JudgeHoldMid(int holdStartNoteId, int finID)
        {
            if (!judgeNoteDecision.IsJudgingHoldEnd)
            {
                Debug.LogError("���̔���m�[�c��HoldEnd�łȂ��̂�JudgeHoldMid���Ă΂ꂽ");
                return;
            }
            if (finID != holdingFinId)
            {
                Debug.LogError("finId��HoldStart�ƈ�v���Ȃ��̂�JudgeHoldMid���Ă΂ꂽ");
                return;
            }

            // ���������Ă���w��HoldStart�̎w�Ȃ�A���ݎ����܂ł̖������HoldMid�𔻒肷��
            
            // holdStartNoteId ����̒T���ɂ��Ă��邪�A
            // ���̊֐����Ă΂��x�ɏ��߂���T�����邱�ƂɂȂ�̂ŁA
            // �O�̒T�����ʂ𗘗p����悤�ɂ���Ƃ�������
            int currentHoldMidId = holdStartNoteId + 1;
            while (true)
            {
                // holdStart���猻�ݎ��� or HoldEnd�܂ł�APPEAR��HoldMid�𔻒肷��

                // holdMid�łȂ��m�[�c�܂ł��� or
                // ���ݎ���������̃m�[�c�܂ł��� �Ȃ�I������
                if (noteTypes[currentHoldMidId] != NoteType.HOLD_MID
                    || notesBeatTimes[currentHoldMidId] - audioTimer.AudioOffsettedTime >= 0)
                {
                    // Debug.Log($"HoldMid�����I�� : id {currentHoldMidId}, time {notesBeatTimes[judgeNoteDecision.CurrentHoldEndId] - audioTimer.gosaTime}");
                    return;
                }

                // ���肳��Ă��Ȃ�HoldMid�𔻒肷��
                if (notesModel.GetNoteState(currentHoldMidId) == NoteState.APPEAR)
                {
                    OnJudgedSuccess(currentHoldMidId);
                }

                currentHoldMidId++;
            }
        }

        private void OnJudgedSuccess(int judgedNoteId)
        {
            // Debug.Log($"OnJudgedSuccess:{judgedNoteId}");
            
            //���莞�̃m�[�c�̓���
            notesModel.ChangeNoteState(judgedNoteId, NoteState.JUDGED_SUCCESS);
            
            //���肷��m�[�c�̍X�V
            judgeNoteDecision.UpdateJudgingNoteId(judgedNoteId);
        }
        
        /// <summary>
        /// Hold�Ɏ��s�����̂Ŏc����Hold������JUDGE_FAILURE�ɂ���
        /// </summary>
        private void FailHold(int failNoteId)
        {
            Debug.Log($"FailHold:{failNoteId}");
            
            // MISS�A�j���[�V����������
            onJudged?.Invoke(JudgeResultType.MISS);
            
            // se��炷
            AudioManager.Instance.ShotSE(AudioManager.Instance.SE_holdfail);
            
            // Hold��Ԃ̉���
            holdingFinId = -1;
            judgeNoteDecision.JudgedHoldEnd();
            
            // noteState �� JUDGED_FAILURE�ɂ���
            int holdEndNoteId = notesModel.FailThisHold(failNoteId);

            // HoldEnd����Ŕ���m�[�c��T�����A�Z�b�g����
            judgeNoteDecision.UpdateJudgingNoteId(holdEndNoteId);
        }

        private JudgeResultType TimeDif2JudgeResultType(float timeDif)
        {
            if (timeDif < GREAT_AREA) // Great����
            {
                return JudgeResultType.GREAT;
            }
            else if (timeDif < NICE_AREA) //Nice����
            {
                return JudgeResultType.NICE;
            }
            else //Bad����
            {
                return JudgeResultType.BAD;
            }
        }
    }
    
    public enum JudgeResultType
    {
        NONE = -1,
        GREAT = 0,
        NICE = 1,
        BAD = 2,
        MISS = 3,
    }
}
