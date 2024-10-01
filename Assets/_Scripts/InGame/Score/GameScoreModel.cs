using System;
using System.Collections;
using System.Collections.Generic;
using StellaCircles.Data;
using UnityEngine;

namespace StellaCircles.InGame
{
    /// <summary>
    /// ゲームの判定やスコアなどの情報を持つ
    /// </summary>
    public class GameScoreModel
    {
        // 最高スコア
        const float MAX_SCORE = 1000000f;
        
        // JudgeResultTypeごとの回数を記録する辞書
        public Dictionary<JudgeResultType, int> judgeCountDictionary = new Dictionary<JudgeResultType, int>();
        
        // HOLD_MIDノーツを除いたノーツの総数
        public int scoreNotesCount;
        
        // JudgeResultTypeごとに入る得点の辞書
        private readonly Dictionary<JudgeResultType, int> scoreValueDictionary = new Dictionary<JudgeResultType, int>();
        
        // 現在のスコア
        public int currentScore;
        // スコアランク
        public PlayRank playRank;
        
        // スコアが変更された際に呼ばれる ビューはここから登録する
        public event Action<int> onScoreChanged;
        
        
        public GameScoreModel(int scoreNotesCount)
        {
            this.scoreNotesCount = scoreNotesCount;
            
            // カウントの初期化・登録
            judgeCountDictionary = new Dictionary<JudgeResultType, int>
            {
                { JudgeResultType.GREAT, 0 },
                { JudgeResultType.NICE, 0 },
                { JudgeResultType.BAD, 0 },
                { JudgeResultType.MISS, 0 }
            };

            // 判定ごとの得点を計算して登録
            // スコアはすべてGREATの時に100万点になる
            scoreValueDictionary = new Dictionary<JudgeResultType, int>
            {
                { JudgeResultType.GREAT, Mathf.FloorToInt(MAX_SCORE / scoreNotesCount) },
                { JudgeResultType.NICE, Mathf.FloorToInt(MAX_SCORE / scoreNotesCount * 2 / 3) },
                { JudgeResultType.BAD, Mathf.FloorToInt(MAX_SCORE / scoreNotesCount / 3) },
                { JudgeResultType.MISS, 0 }
            };
        }

        private void AddJudgeCount(JudgeResultType judgeResultType)
        {
            if (judgeResultType == JudgeResultType.NONE)
            {
                throw new NotImplementedException();
            }
            
            judgeCountDictionary[judgeResultType]++;
        }
        private void AddScore(JudgeResultType judgeResultType)
        {
            if (judgeResultType == JudgeResultType.NONE)
            {
                throw new NotImplementedException();
            }
            
            currentScore += scoreValueDictionary[judgeResultType];

            if (judgeResultType != JudgeResultType.MISS)
            {
                onScoreChanged?.Invoke(currentScore);
            }
        }

        /// <summary>
        /// 判定時にイベント経由で呼ぶ
        /// </summary>
        /// <param name="judgeResultType"></param>
        public void OnJudged(JudgeResultType judgeResultType)
        {
            if (judgeResultType == JudgeResultType.NONE)
            {
                throw new NotImplementedException();
            }
            
            AddJudgeCount(judgeResultType);
            AddScore(judgeResultType);
        }


        /// <summary>
        /// MISSを計算し、最終的なスコアを出す
        /// </summary>
        public void GameEnd()
        {
            // MISSを計算
            judgeCountDictionary[JudgeResultType.MISS] =
                scoreNotesCount
                - judgeCountDictionary[JudgeResultType.GREAT]
                - judgeCountDictionary[JudgeResultType.NICE]
                - judgeCountDictionary[JudgeResultType.BAD];
            
            // AllGreatなら currentScore を MAX_SCORE にする
            if (judgeCountDictionary[JudgeResultType.GREAT] == scoreNotesCount)
            {
                Debug.Log($"All Great : {MAX_SCORE}");
                currentScore = (int)MAX_SCORE;
            }

            playRank = Score2Rank(currentScore);
        }
        
        /// <summary>
        /// スコアからRankを算出
        /// </summary>
        private PlayRank Score2Rank(int score)
        {
            if (score == 1000000)
            {
                return PlayRank.T;
            }
            else if (score >= 900000)
            {
                return PlayRank.S;
            }
            else if (score >= 800000)
            {
                return PlayRank.A;
            }
            else if (score >= 600000)
            {
                return PlayRank.B;
            }
            else if (score >= 400000)
            {
                return PlayRank.C;
            }
            else if (score >= 200000)
            {
                return PlayRank.D;
            }
            else
            {
                return PlayRank.E;
            }
        }
    }
}
