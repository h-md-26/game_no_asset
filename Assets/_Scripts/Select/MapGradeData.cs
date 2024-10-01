using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace StellaCircles.Data
{
    public class MapGradeData
    {
        /*
         * 曲の成績を保存
         * 　この曲のプレイ回数
         * 　この曲の難易度ごとのプレイ回数
         * 　この曲の難易度ごとの最高成績/最低成績の詳細
         *
         * 成績情報
         * 　ランク
         * 　各判定の回数
         * 　スコア
         *
         */

        public int[] playCounts; // プレイ回数
        public int[] playScores; // プレイスコア
        public int[,] gradeJudgeResultMaxMins = new int[2, 8]; // 最高/最低成績内訳
        public PlayRank[,] PlayRankMaxMin = new PlayRank[2, 2]; //第1引数：難易度, 第2引数：0最高/1最低

        public void GradeInitialize()
        {
            playCounts = new int[2];
            playScores = new int[2];

            for (int i = 0; i < playCounts.Length; i++)
            {
                playCounts[i] = 0;
            }

            for (int i = 0; i < playScores.Length; i++)
            {
                playScores[i] = 0;
            }

            for (int i = 0; i < gradeJudgeResultMaxMins.GetLength(0); i++)
            {
                for (int j = 0; j < gradeJudgeResultMaxMins.GetLength(1); j++)
                {
                    gradeJudgeResultMaxMins[i, j] = 0;
                }
            }

            for (int i = 0; i < PlayRankMaxMin.GetLength(0); i++)
            {
                for (int j = 0; j < PlayRankMaxMin.GetLength(1); j++)
                {
                    PlayRankMaxMin[i, j] = PlayRank.YET;
                }
            }
        }
    }

    public enum PlayRank
    {
        T = 6,
        S = 5,
        A = 4,
        B = 3,
        C = 2,
        D = 1,
        E = 0,
        YET = -1
    }
}