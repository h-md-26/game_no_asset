using System;
using StellaCircles.Data;
using UnityEngine;

namespace StellaCircles.InGame
{
    /// <summary>
    /// Inputterからうけた入力に対して判定する
    /// 判定するノーツをUpdater快癒で毎フレーム更新する
    /// </summary>
    public class Judgementer
    {
        public const float BAD_AREA = 0.180f;
        public const float NICE_AREA = 0.080f;
        public const float GREAT_AREA = 0.050f;
        
        // 長押しでTapもFlickも判定されるバグを確認;
        
        /*
         TODO
        次やること
            Judgementerの「判定ノーツをセットする処理」と「入力に対して判定する処理」を別クラスに分けることを検討する
            [SerializeField]を再確認する
            激やば変数名がないか確認する
                とりあえずAutoPlayにはあったので直す
            問題なければSelectのリファクタリングを行う
        */
        
        /// <summary>
        /// SEを操作時にならす場合true、判定時にならす場合false
        /// </summary>
        readonly bool isSeTimingOperation;

        readonly GameAudioTimer audioTimer;
        readonly NotesModel notesModel;
        private readonly JudgeNoteDecider judgeNoteDecision;

        private readonly float[] notesBeatTimes;
        private readonly NoteType[] noteTypes;
        
        // 判定時に呼ぶイベント これを経由してスコアや判定のビューを行う
        public event Action<JudgeResultType> onJudged;

        public Judgementer(FumenData fumenData, GameScoreModel scoreModel, GameAudioTimer  audioTimer, NotesModel notesModel)
        {
            notesBeatTimes = fumenData.NoteBeatTimes;
            noteTypes = fumenData.NoteTypes;
            
            // 判定時にスコアのOnJudgedを呼ぶ
            onJudged += scoreModel.OnJudged;
            
            this.audioTimer = audioTimer;
            this.notesModel = notesModel;
            judgeNoteDecision = new(fumenData, notesModel, audioTimer);
            
            isSeTimingOperation = GameSetting.gameSettingValue.isSETimingOperation;
            
            holdingFinId = -1;
        }

        // ホールドしている指のid ホールドしていないときは負の値にする
        int holdingFinId;

        /// <summary>
        /// Hold中かどうかをholdingFinIdを確認することで返すプロパティ
        /// </summary>
        bool IsHolding => holdingFinId != -1;

        /// <summary>
        /// GameUpdater から毎フレーム呼び出す
        /// </summary>
        public void JudgementerUpdate()
        {
            judgeNoteDecision.JudgementerUpdate();
        }

        // HMをHoldEndフラグを使って消す処理がうまくいってない;
        
        
        /// <summary>
        /// 判定の起点
        /// 入力の種類と指Idを受け取って判定する
        /// </summary>
        public void JudgeNotes(InputType inputType, int finId)
        {
            var currentJudgeNoteId = judgeNoteDecision.JudgingNoteId;
            // Debug.Log($"Judge!! type: {inputType}, id: {currentJudgeNoteId}");

            // Hold中に指を離したら、Hold失敗にする
            if (inputType == InputType.Release && finId == holdingFinId && !judgeNoteDecision.IsHoldEndInJudgeArea)
            {
                Debug.Log("Hold中に指を離した");
                FailHold(currentJudgeNoteId);
            }
            
            // HoldをHoldEndを過ぎた後も続けていたらHold失敗にする
            if (judgeNoteDecision.IsJudgingHoldEnd && inputType == InputType.Holding &&
                judgeNoteDecision.IsPassedHoldEndJudgeArea)
            {
                Debug.Log("HoldEndを超えたがHoldを続けている");
                FailHold(currentJudgeNoteId);
            }

            // 判定準備されていないノーツは判定しない
            if (notesModel.GetNoteState(currentJudgeNoteId) != NoteState.JUDGING) return;

            // 現在時刻と判定ノーツの差の絶対値
            float timeDif = Mathf.Abs(notesBeatTimes[currentJudgeNoteId] - audioTimer.AudioOffsettedTime);
            
            // isJudgingHoldEnd のとき、Hold入力ならHoldMidを判定する
            if (judgeNoteDecision.IsJudgingHoldEnd && inputType == InputType.Holding) // 長押し入力
            {
                // Debug.Log("HoldMidの判定");
                
                // 現在の時刻までの未判定のHoldMidを判定する
                JudgeHoldMid(judgeNoteDecision.CurrentHoldStartId, finId);
            }
            else //長押し以外の入力
            {
                // 入力と対応するノーツでなければreturn
                // Holdの失敗処理も行う
                switch (inputType)
                {
                    // Tap入力
                    case InputType.Tap:
                    {
                        // Hold中にTapしたなら、Hold失敗にする
                        // if (judgeNoteDecision.IsJudgingHoldEnd && !judgeNoteDecision.IsHoldEndInJudgeArea)
                        if(IsHolding)
                        {
                            // Debug.Log("Hold中にTapした");
                            FailHold(currentJudgeNoteId);
                        }

                        // Tap入力では、Tap,HSノーツのみを判定
                        if (noteTypes[currentJudgeNoteId] is not (NoteType.TAP or NoteType.HOLD_START))
                        {
                            // Debug.Log($"Tap入力では、Tap,HSノーツのみを判定するので return : {noteTypes[currentJudgeNoteId]}");
                            return;
                        }

                        break;
                    }
                    // フリックでは、Flickノーツのみ判定
                    case InputType.Flick:
                        // Debug.Log("Flick入力きた");
                        if (noteTypes[currentJudgeNoteId] != NoteType.FLICK)
                        {
                            // Debug.Log($"フリックでは、Flickノーツのみ判定するので return : {noteTypes[currentJudgeNoteId]}");
                            return;   
                        }
                        break;
                    // 指離しでは、HoldEndノーツのみ判定
                    case InputType.Release:
                        // Debug.Log($"指離しでは、HoldEndノーツのみ判定, noteTypes[currentJudgeNoteId] : {noteTypes[currentJudgeNoteId]}");
                        if(noteTypes[currentJudgeNoteId] != NoteType.HOLD_END) return;
                        if (finId != holdingFinId) return; // HoldEndのとき、 finIdがあっている時だけ判定
                        break;
                    default:
                        // Debug.Log("他の入力は判定しない");
                        return;
                }

                // 判定範囲外なら return
                if (timeDif >= BAD_AREA) return;
                
                // 入力と判定するノーツの種類があっていればここにたどり着き、判定される
                BandJudge(TimeDif2JudgeResultType(timeDif), currentJudgeNoteId, finId);
            }
        }
        
        /// <summary>
        /// Tap, Flick, HoldStart, HoldEnd 等の時間範囲で判定するノーツの成功時に呼ぶ
        /// 失敗判定や、対応していない操作は事前に除く
        /// </summary>
        private void BandJudge(JudgeResultType judgeResultType, int currentJudgeNoteId, int finId)
        {
            #region Hold仕様
            // HOLD_START のとき、Great or Good のときは指IDを記録しておき、
            //                    Bad のときはその先のHOLDをすべて消し、効果を出す
            //                    このノーツがとり過ぎた時も、消す
            //
            //                    以上の実装のため、HOLD_ENDから判定対象が変わった時に記録した指IDを -1 にしておく。
            //                                      Bad, 通過の時も念のため -1 にしておく
            //
            // HOLD_MID のとき、入力がHolding 指IDが記録されたものの時判定する
            //                         途中で指を話した場合 (入力がReleaseでNoteがLINE or CURVE)
            //                           その先のHOLDをすべて消し、効果を出す
            // HOLD_END のとき、       入力がRelease 指IDが記録されたものの時判定する
            //
            // ※ホールド失敗時に前の部分も消す
            #endregion
            
            var noteType = noteTypes[currentJudgeNoteId];
            
            // ノーツの判定時の特殊処理
            NoteJudgedSpecialProcess(currentJudgeNoteId, noteType, judgeResultType, finId);

            // SEの場合分け
            PlaySEOnJudged(noteType, judgeResultType);
                
            // Hold情報の取り消し
            if (noteType == NoteType.HOLD_END)
            {
                holdingFinId = -1;
            }

            //共通処理
            OnJudgedSuccess(currentJudgeNoteId);
        }

        /// <summary>
        /// ノーツごとの特殊処理
        /// </summary>
        private void NoteJudgedSpecialProcess(int judgedNoteId, NoteType noteType, JudgeResultType judgeResultType, int finID)
        {
            // 判定結果ごとのアニメーションをつける
            onJudged?.Invoke(judgeResultType);
            
            //ノーツの種類・判定結果に応じて処理を分ける
            // Tap, Flick は特別な処理はなし
            switch (noteType)
            {
                case NoteType.HOLD_START:
                    // Holdを始めた指を記憶
                    holdingFinId = finID;
                    // Hold開始フラグを立て、HoldEndに備える
                    judgeNoteDecision.JudgedHoldStart();
                    notesModel.JudgeHoldMidsAtHoldStart(judgedNoteId);
                    break;
                case NoteType.HOLD_END:
                    // Hold関連のフラグを折る
                    holdingFinId = -1;
                    judgeNoteDecision.JudgedHoldEnd();
                    notesModel.JudgeHoldMidsAtHoldEnd(judgedNoteId);
                    break;
            }
        }
        
        // 判定結果や、SEのタイミング設定に合わせてSEを鳴らすか・何を鳴らすかを変更する
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
                    // 判定時に鳴らす設定なら BADの場合分けをしてSEを鳴らす
                    if (!isSeTimingOperation)
                    {
                        var tapSe = judgeResultType == JudgeResultType.BAD ? AudioManager.Instance.SE_tapmiss : AudioManager.Instance.SE_tap;
                        AudioManager.Instance.ShotSE(tapSe);
                    }
                    break;
                }
                case NoteType.FLICK:
                {
                    // 判定時に鳴らす設定なら BADの場合分けをしてSEを鳴らす
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
        /// 現在の時刻までの未判定のHoldMidを判定する
        /// </summary>
        private void JudgeHoldMid(int holdStartNoteId, int finID)
        {
            if (!judgeNoteDecision.IsJudgingHoldEnd)
            {
                Debug.LogError("次の判定ノーツがHoldEndでないのにJudgeHoldMidが呼ばれた");
                return;
            }
            if (finID != holdingFinId)
            {
                Debug.LogError("finIdがHoldStartと一致しないのにJudgeHoldMidが呼ばれた");
                return;
            }

            // 長押ししている指がHoldStartの指なら、現在時刻までの未判定のHoldMidを判定する
            
            // holdStartNoteId からの探索にしているが、
            // この関数が呼ばれる度に初めから探索することになるので、
            // 前の探索結果を利用するようにするといいかも
            int currentHoldMidId = holdStartNoteId + 1;
            while (true)
            {
                // holdStartから現在時刻 or HoldEndまでのAPPEARなHoldMidを判定する

                // holdMidでないノーツまできた or
                // 現在時刻よりも後のノーツまできた なら終了する
                if (noteTypes[currentHoldMidId] != NoteType.HOLD_MID
                    || notesBeatTimes[currentHoldMidId] - audioTimer.AudioOffsettedTime >= 0)
                {
                    // Debug.Log($"HoldMid消し終了 : id {currentHoldMidId}, time {notesBeatTimes[judgeNoteDecision.CurrentHoldEndId] - audioTimer.gosaTime}");
                    return;
                }

                // 判定されていないHoldMidを判定する
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
            
            //判定時のノーツの動作
            notesModel.ChangeNoteState(judgedNoteId, NoteState.JUDGED_SUCCESS);
            
            //判定するノーツの更新
            judgeNoteDecision.UpdateJudgingNoteId(judgedNoteId);
        }
        
        /// <summary>
        /// Holdに失敗したので残ったHold部分をJUDGE_FAILUREにする
        /// </summary>
        private void FailHold(int failNoteId)
        {
            Debug.Log($"FailHold:{failNoteId}");
            
            // MISSアニメーションさせる
            onJudged?.Invoke(JudgeResultType.MISS);
            
            // seを鳴らす
            AudioManager.Instance.ShotSE(AudioManager.Instance.SE_holdfail);
            
            // Hold状態の解除
            holdingFinId = -1;
            judgeNoteDecision.JudgedHoldEnd();
            
            // noteState を JUDGED_FAILUREにする
            int holdEndNoteId = notesModel.FailThisHold(failNoteId);

            // HoldEnd周りで判定ノーツを探索し、セットする
            judgeNoteDecision.UpdateJudgingNoteId(holdEndNoteId);
        }

        private JudgeResultType TimeDif2JudgeResultType(float timeDif)
        {
            if (timeDif < GREAT_AREA) // Great判定
            {
                return JudgeResultType.GREAT;
            }
            else if (timeDif < NICE_AREA) //Nice判定
            {
                return JudgeResultType.NICE;
            }
            else //Bad判定
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
