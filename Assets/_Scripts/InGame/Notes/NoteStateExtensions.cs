namespace StellaCircles.InGame
{
    public static class NoteStateExtensions
    {
        /// <summary>
        /// JUDGED_SUCCESS or JUDGED_FAILURE の時 true
        /// </summary>
        public static bool IsJudged(this NoteState noteState)
        {
            return noteState is NoteState.JUDGED_SUCCESS or NoteState.JUDGED_FAILURE;
        }
        
        
        /// <summary>
        /// JUDGED_SUCCESS or JUDGED_FAILURE or PASSED の時 true。
        /// ノーツを消すアニメーション中ならアニメーションを上書きしたくないのでこれを使って場合分けする
        /// </summary>
        public static bool IsUnactiveAnimationed(this NoteState noteState)
        {
            return noteState is NoteState.JUDGED_SUCCESS or NoteState.JUDGED_FAILURE or NoteState.PASSED;
        }
    }
}

