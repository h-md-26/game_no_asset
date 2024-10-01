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
        
        
        // 判定するノーツ 判定後や、毎フレーム更新される
        private int judgingNoteId;
        public int JudgingNoteId => judgingNoteId;
        
        /// <summary>
        /// HoldStart を judgingNoteId に設定したフラグ。
        /// HoldStart成功判定時に isJudgingHoldEnd を trueにするといいかも
        /// </summary>
        bool isJudgingHoldStart;
        public bool IsJudgingHoldStart => isJudgingHoldStart;
        /// <summary>
        /// HoldEnd を judgingNoteId に設定したフラグ。
        /// これがtrueのときのみHoldMidの処理を行うといいかも
        /// </summary>
        bool isJudgingHoldEnd;
        public bool IsJudgingHoldEnd => isJudgingHoldEnd;
        
        /// <summary>
        /// 現在のHoldの HoldStart の Id。Holdしていないときは負の値
        /// </summary>
        int currentHoldStartId;
        /// <summary>
        /// 現在のHoldの HoldStart の Id。Holdしていないときは負の値
        /// </summary>
        public int CurrentHoldStartId => currentHoldStartId;
        /// <summary>
        /// 現在のHoldの HoldEnd の Id。Holdしていないときは負の値
        /// </summary>
        int currentHoldEndId;
        /// <summary>
        /// 現在のHoldの HoldEnd の Id。Holdしていないときは負の値
        /// </summary>
        public int CurrentHoldEndId => currentHoldEndId;

        /// <summary>
        /// Judgementer 経由で毎フレーム呼び出す
        /// </summary>
        public void JudgementerUpdate()
        {
            if (judgingNoteId >= notesBeatTimes.Length - 1) return;

            //一番近いノーツだけを判定
            UpdateJudgingNoteId(judgingNoteId);
        }
        
        /// <summary>
        /// 現在時刻と一番近いノーツをセットする
        /// ノーツの探索を currentJudgingNoteId の周りで行う
        /// </summary>
        public void UpdateJudgingNoteId(int currentJudgingNoteId)
        {
            SetJudgingNoteId(GetNewJudgingId(currentJudgingNoteId));
        }

        // 初めのノーツを一度でもJUDGINGに変更したか
        private bool isChangeJudgingFirstNote;
        
        //judgingNoteId に noteId をセットする
        private void SetJudgingNoteId(int noteId)
        {
            // 範囲内に収める
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

            // 初めのノーツの出現後に初めのノーツが判定範囲内に入り、
            // 初めのノーツをJudgingにしたことが無ければJudgingにする
            if (noteId == 0 
                && notesModel.IsFirstNoteAppeared 
                && !isChangeJudgingFirstNote 
                && Mathf.Abs(notesBeatTimes[noteId] - audioTimer.AudioOffsettedTime) <= Judgementer.BAD_AREA)
            {
                // Debug.Log($"初めのノーツの状態をJUDGINGに変更する {notesModel.GetNoteState(noteId)}");
                isChangeJudgingFirstNote = true;
                notesModel.ChangeNoteState(noteId, NoteState.JUDGING);
            }
            // 判定準備ノーツが変更されたら、notesModel経由でノーツを判定準備状態にする
            else if (noteId != judgingNoteId)
            {
                notesModel.ChangeNoteState(noteId, NoteState.JUDGING);
                // 判定済みでなく、通過してもいないなら APPEARに戻す
                if(!notesModel.GetNoteState(judgingNoteId).IsJudged() && notesModel.GetNoteState(judgingNoteId) != NoteState.PASSED)
                    notesModel.ChangeNoteState(judgingNoteId, NoteState.APPEAR);
            }

            // 判定ノーツの更新
            judgingNoteId = noteId;
        }

        //現在時刻に一番近いノーツを返す
        private int GetNewJudgingId(int currentJudgingNoteId)
        {
            /*
            判定するノーツが HoldStart or HoldEndなら、
            HoldMidは特殊な判定を行うので HoldStart, HoldEndの判定範囲外に出るまでは判定ノーツをロックする
            
            それ以外なら、一番近いノーツを現在の判定ノーツの位置から線形探索する
            毎フレーム呼ばれるため、2分探索よりも速くなる
            */
            
            // HoldStartのロック処理
            // HoldStartノーツが次判定するノーツなら、HoldStartノーツの許容範囲内から出るまではJudgingNoteIdを変更しない
            if (isJudgingHoldStart)
            {
                // HSノーツと現在時刻の差が前の BadArea を超えたら
                // JudgingNoteIdのHoldStartへのロックを解除する
                if (notesBeatTimes[currentHoldStartId] - audioTimer.AudioOffsettedTime < -Judgementer.BAD_AREA)
                {
                    isJudgingHoldStart = false;
                    currentHoldStartId = -1;
                }
                else
                {
                    // HoldStartIdでロックする
                    return currentHoldStartId;
                }
            }
            
            // HoldEndのロック処理
            // HoldEndノーツが次判定するノーツなら、HoldEndノーツの許容範囲の後ろにいくまではJudgingNoteIdを変更しない
            if (isJudgingHoldEnd)
            {
                // HoldEndのBadAreaを後ろ側で越えた or HoldEndの次のノーツの方が現在時刻に近いノーツになった
                // なら、ロックを解除して次のノーツを探索する
                
                // Debug.Log("isJudgingHoldEnd : " +isJudgingHoldEnd + ", currentHoldEndId : " +currentHoldEndId);
                
                if (notesBeatTimes[currentHoldEndId] - audioTimer.AudioOffsettedTime >= -Judgementer.BAD_AREA
                    || Mathf.Abs(notesBeatTimes[currentHoldEndId] - audioTimer.AudioOffsettedTime)
                    < Mathf.Abs(notesBeatTimes[GetNextUnJudgedNote(currentHoldEndId + 1, SearchDirection.Positive)] - audioTimer.AudioOffsettedTime))
                {
                    return currentHoldEndId;
                }
                else
                {
                    Debug.Log("HoldEndのロック解除");
                }
            }

            
            // HoldStart, HoldEndでないなら、現在時刻に一番近いノーツを返す
            // この時HoldMid, HoldEndは選択しない
            if(currentJudgingNoteId < 0) currentJudgingNoteId = 0;
            if (currentJudgingNoteId >= notesModel.allNotesCount) currentJudgingNoteId = notesModel.allNotesCount - 1;
            int searchingNoteId = GetNextUnJudgedNote(currentJudgingNoteId + 1, SearchDirection.Positive);
            // currentJudgingNoteId よりも 1つ後ろのノーツ の方が現在時刻に近い場合、最も近いノーツは正方向にある
            if (Mathf.Abs(notesBeatTimes[searchingNoteId] - audioTimer.AudioOffsettedTime)
                < Mathf.Abs(notesBeatTimes[currentJudgingNoteId] - audioTimer.AudioOffsettedTime))
            {
                // 正方向に線形探索をして最も近いノーツを探す
                // 毎フレーム行うため、2分探索よりも計算量が小さくなる
                searchingNoteId = GetNearestJudgedNoteThisDirection(searchingNoteId, SearchDirection.Positive);
            }
            // currentJudgingNoteIdの1つ後ろのノーツ よりも currentJudgingNoteId の方が現在時刻に近い場合、最も近いノーツは負方向 or 現在のノーツ
            else
            {
                // 負方向に線形探索をして最も近いノーツを探す
                // 毎フレーム行うため、2分探索よりも計算量が小さくなる
                searchingNoteId = GetNearestJudgedNoteThisDirection(currentJudgingNoteId, SearchDirection.Negative);
            }
            
            
            // 一番近いノーツがHoldStartに変更されたら、フラグを立て、HoldEndを探す
            // if (!isJudgingHoldStart && !isHolding && noteTypes[searchingNoteId] == NoteType.HOLD_START)
            if (!isJudgingHoldStart && noteTypes[searchingNoteId] == NoteType.HOLD_START)
            {
                isJudgingHoldStart = true;
                currentHoldStartId = searchingNoteId;
                
                // HoldEndを探す
                currentHoldEndId = SearchHoldEndId(currentHoldStartId);
            }

            if (noteTypes[searchingNoteId] == NoteType.HOLD_MID)
            {
                Debug.LogWarning("HoldMIDが判定ノーツになった");
                LogHoldingValue();
            }
            
            return searchingNoteId;
        }

        /// <summary>
        /// デバッグ用にホールド関連のフラグを吐き出す
        /// </summary>
        void LogHoldingValue()
        {
            Debug.Log(
                $"IsJudgingHoldStart: {IsJudgingHoldStart}, IsJudgingHoldEnd: {IsJudgingHoldEnd}, CurrentHoldStartId: {CurrentHoldStartId}, CurrentHoldEndId: {CurrentHoldEndId}");
        }

        /// <summary>
        /// HoldStartIdに対応するHoldEndIdを返す
        /// </summary>
        /// <param name="holdStartId"></param>
        /// <returns></returns>
        private int SearchHoldEndId(int holdStartId)
        {
            int holdEndId = holdStartId + 1;
                
            // HoldEndを探す
            while (noteTypes[holdEndId] != NoteType.HOLD_END)
            {
                holdEndId++;
            }
            
            return holdEndId;
        }

        /// <summary>
        /// 指定方向の現在地点から一番近い未判定ノーツを取得
        /// HoldEnd, HoldMid は取得しない
        /// </summary>
        private int GetNextUnJudgedNote(int startNoteId, SearchDirection dir)
        {
            int searchingNoteId = startNoteId;

            while (true)
            {
                // 範囲内に収める処理
                if (searchingNoteId <= 0)
                {
                    return 0;
                }
                if (searchingNoteId >= notesModel.NoteStateLength - 1)
                {
                    return notesModel.NoteStateLength - 1;
                }
                
                // 判定済みでなく、通過していないノーツなら終了する
                if (!notesModel.GetNoteState(searchingNoteId).IsJudged() &&
                    noteTypes[searchingNoteId] != NoteType.HOLD_END &&
                    noteTypes[searchingNoteId] != NoteType.HOLD_MID &&
                    notesModel.GetNoteState(searchingNoteId) != NoteState.PASSED)
                {
                    return searchingNoteId;
                }

                //次を確認
                searchingNoteId += (int)dir;
            }
        }
        
        /// <summary>
        /// 指定方向の現在時刻に一番近い未判定ノーツを取得
        /// </summary>
        private int GetNearestJudgedNoteThisDirection(int startNoteId, SearchDirection dir)
        {
            int preNoteId = startNoteId;
            int searchingNoteId = startNoteId;

            while (true)
            {
                searchingNoteId = GetNextUnJudgedNote(preNoteId, dir);
                
                // これ以上その方向に進めない場合、その方向の最後の値を返す
                if (searchingNoteId == preNoteId)
                {
                    return searchingNoteId;
                }
                
                // 時刻を比較
                if (Mathf.Abs(notesBeatTimes[preNoteId] - audioTimer.AudioOffsettedTime)
                    <= Mathf.Abs(notesBeatTimes[searchingNoteId] - audioTimer.AudioOffsettedTime))
                {
                    // searchingNoteId よりも preNoteId の方が現在時刻に近いので preNoteId を返す
                    return preNoteId;
                }

                //次を確認
                preNoteId = searchingNoteId;
            }
            
        }

        /// <summary>
        /// HoldStart判定時に呼ぶ
        /// </summary>
        public void JudgedHoldStart()
        {
            isJudgingHoldStart = false;
            isJudgingHoldEnd = true;

            if (currentHoldEndId < 0)
            {
                // 前のHoldの判定が終わる前に次のHoldの判定が始まるとここにきてしまう
                Debug.Log($"judgedHoldStart : startId : {currentHoldStartId}, endId : {currentHoldEndId}");
                currentHoldEndId = SearchHoldEndId(currentHoldStartId);
            }

        }
        
        /// <summary>
        /// HoldEnd判定時に呼ぶ
        /// </summary>
        public void JudgedHoldEnd()
        {
            isJudgingHoldStart = false;
            isJudgingHoldEnd = false;
            currentHoldEndId = -1;
            currentHoldStartId = -1;
        }

        /// <summary>
        /// HoldEndが判定範囲内かどうか
        /// </summary>
        public bool IsHoldEndInJudgeArea
        {
            get
            {
                if (!isJudgingHoldEnd) return false;
                
                // HoldEndが現在時刻と離れていなければTrue
                return (Mathf.Abs(notesBeatTimes[CurrentHoldEndId] - audioTimer.AudioOffsettedTime) <= Judgementer.BAD_AREA);
            }
        }
        
        /// <summary>
        /// HoldEndが判定範囲を通り過ぎたかどうか
        /// </summary>
        public bool IsPassedHoldEndJudgeArea
        {
            get
            {
                if (!isJudgingHoldEnd) return false;
                
                // HoldEndが現在時刻と離れていなければTrue
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
