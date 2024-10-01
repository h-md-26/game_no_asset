using UnityEngine;

namespace StellaCircles.InGame
{
    /// <summary>
    /// ���ʂ̃f�[�^������
    /// FumenAnalyzer��ʂ��ĉ�͂������̂��g�p����
    /// </summary>
    public class FumenData
    {
        /// <summary>
        /// ��͂̂���Ȃ������Z�b�g
        /// </summary>
        private void SetMapInfo(FumenAnalyzer.MapInfo mapInfo)
        {
            bgm = mapInfo.bgm;
            offset = mapInfo.offset;
            mapDifficulty = mapInfo.mapDifficulty;
        }
        
        /// <summary>
        /// ��͂������ʏ����Z�b�g
        /// measureBPMs, fumenMeasureStrings, nodeDestinations��fumenData�ɑ������
        /// </summary>
        private void SetFumenInfo(FumenAnalyzer.FumenInfo fumenInfo)
        {
            measureBPMs = fumenInfo.measureBPMs;
            fumenMeasureStrings = fumenInfo.fumenMeasureStrings;
            nodeDestinations = fumenInfo.nodeDestinations;
        }
        
        /// <summary>
        /// ��͂����m�[�c�����Z�b�g
        /// </summary>
        private void SetNotesInfo(FumenAnalyzer.NotesInfo notesInfo)
        {
            notesCountThisMeasure = notesInfo.notesCountThisMeasure;
            notesCountUpToMeasure = notesInfo.notesCountUpToMeasure;
            allNotesCount = notesInfo.allNotesCount;
            scoreNotesCount = notesInfo.scoreNotesCount;
            measureStartTimes = notesInfo.measureStartTimes;
            measureDurations = notesInfo.measureDurations;
            noteBeatTimes = notesInfo.notesBeatTimes;
            noteTypes = notesInfo.noteTypes;
        }

        /// <summary>
        /// FumenAnalyzer����̂݌Ă�
        /// </summary>
        public FumenData(FumenAnalyzer.MapInfo mapInfo, FumenAnalyzer.FumenInfo fumenInfo, FumenAnalyzer.NotesInfo notesInfo)
        {
            SetMapInfo(mapInfo);
            SetFumenInfo(fumenInfo);
            SetNotesInfo(notesInfo);
        } 
        
        private AudioClip bgm;
        private float offset;
        // private int[] courseLevels;
        private int mapDifficulty;
        
        
        private float[] measureBPMs; //i���߂�BPM
        private string[] fumenMeasureStrings; //i���ߖڂ̕���
        private int[] nodeDestinations; //���̃m�[�h�̔ԍ��̔z��(0?5)
        
        
        private int[] notesCountThisMeasure; //i���ߖڂ̃m�[�c�̐�
        private int[] notesCountUpToMeasure; //i���ߖڂ܂ł̃m�[�c�̍��v [0]:0  notesCountUpToMeasure
        private int allNotesCount; // �m�[�c�̑���
        private int scoreNotesCount; // �X�R�A�ɉ�����m�[�c�̑���(HM���܂܂Ȃ��m�[�c�̐�)
        private float[] measureStartTimes; //i���߂̎n�܂鎞��
        private float[] measureDurations; //i���߂���(i+1)���߂܂ł̎���
        private float[] noteBeatTimes; //i�Ԗڂ̃m�[�c�̂������ׂ�����
        private NoteType[] noteTypes; //i�Ԗڂ̃m�[�c�̎��
        
        
        /// <summary>
        /// ���̕��ʂŎg�p����BGM
        /// </summary>
        public AudioClip BGM => bgm;
        /// <summary>
        /// ���ʂ̃I�t�Z�b�g
        /// </summary>
        public  float Offset => offset;
        
        /// <summary>
        /// �O������̒l�擾�p�v���p�e�B(����N���[������Ƃ�΂��̂Ōďo���ŃL���b�V������)
        /// </summary>
        public float[] MeasureBPMs => (float[])measureBPMs.Clone();
        /// <summary>
        /// �O������̒l�擾�p�v���p�e�B(����N���[������Ƃ�΂��̂Ōďo���ŃL���b�V������)
        /// </summary>
        public string[] FumenMeasureStrings => (string[])fumenMeasureStrings.Clone();
        /// <summary>
        /// �O������̒l�擾�p�v���p�e�B(����N���[������Ƃ�΂��̂Ōďo���ŃL���b�V������)
        /// </summary>
        public int[] NodeDestinations => (int[])nodeDestinations.Clone();
        
        /// <summary>
        /// �m�[�c�̑���
        /// </summary>
        public int AllNotesCount => allNotesCount;
        /// <summary>
        /// �X�R�A�ɉ�����m�[�c�̑���(HM���܂܂Ȃ��m�[�c�̐�)
        /// </summary>
        public int ScoreNotesCount => scoreNotesCount;
        /// <summary>
        /// �O������̒l�擾�p�v���p�e�B(����N���[������Ƃ�΂��̂Ōďo���ŃL���b�V������)
        /// </summary>
        public float[] MeasureStartTimes => (float[])measureStartTimes.Clone();
        /// <summary>
        /// �O������̒l�擾�p�v���p�e�B(����N���[������Ƃ�΂��̂Ōďo���ŃL���b�V������)
        /// </summary>
        public float[] MeasureDurations => (float[])measureDurations.Clone();

        /// <summary>
        /// �O������̒l�擾�p�v���p�e�B(����N���[������Ƃ�΂��̂Ōďo���ŃL���b�V������)
        /// </summary>
        public float[] NoteBeatTimes => (float[])noteBeatTimes.Clone();
        /// <summary>
        /// �O������̒l�擾�p�v���p�e�B(����N���[������Ƃ�΂��̂Ōďo���ŃL���b�V������)
        /// </summary>
        public NoteType[] NoteTypes => (NoteType[])noteTypes.Clone();
    }

    public enum NoteType
    {
        EMPTY = '0',
        TAP = '1',
        FLICK = '2',
        HOLD_START = '3',
        HOLD_END = '4',
        HOLD_MID = '5'
    }

}