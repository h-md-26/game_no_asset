using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace StellaCircles.Data
{
    public class MapGradeData
    {
        /*
         * �Ȃ̐��т�ۑ�
         * �@���̋Ȃ̃v���C��
         * �@���̋Ȃ̓�Փx���Ƃ̃v���C��
         * �@���̋Ȃ̓�Փx���Ƃ̍ō�����/�Œᐬ�т̏ڍ�
         *
         * ���я��
         * �@�����N
         * �@�e����̉�
         * �@�X�R�A
         *
         */

        public int[] playCounts; // �v���C��
        public int[] playScores; // �v���C�X�R�A
        public int[,] gradeJudgeResultMaxMins = new int[2, 8]; // �ō�/�Œᐬ�ѓ���
        public PlayRank[,] PlayRankMaxMin = new PlayRank[2, 2]; //��1�����F��Փx, ��2�����F0�ō�/1�Œ�

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