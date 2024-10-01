namespace StellaCircles.InGame
{
    public static class NoteStateExtensions
    {
        /// <summary>
        /// JUDGED_SUCCESS or JUDGED_FAILURE �̎� true
        /// </summary>
        public static bool IsJudged(this NoteState noteState)
        {
            return noteState is NoteState.JUDGED_SUCCESS or NoteState.JUDGED_FAILURE;
        }
        
        
        /// <summary>
        /// JUDGED_SUCCESS or JUDGED_FAILURE or PASSED �̎� true�B
        /// �m�[�c�������A�j���[�V�������Ȃ�A�j���[�V�������㏑���������Ȃ��̂ł�����g���ďꍇ��������
        /// </summary>
        public static bool IsUnactiveAnimationed(this NoteState noteState)
        {
            return noteState is NoteState.JUDGED_SUCCESS or NoteState.JUDGED_FAILURE or NoteState.PASSED;
        }
    }
}

