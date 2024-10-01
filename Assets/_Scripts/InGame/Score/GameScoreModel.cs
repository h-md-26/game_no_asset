using System;
using System.Collections;
using System.Collections.Generic;
using StellaCircles.Data;
using UnityEngine;

namespace StellaCircles.InGame
{
    /// <summary>
    /// �Q�[���̔����X�R�A�Ȃǂ̏�������
    /// </summary>
    public class GameScoreModel
    {
        // �ō��X�R�A
        const float MAX_SCORE = 1000000f;
        
        // JudgeResultType���Ƃ̉񐔂��L�^���鎫��
        public Dictionary<JudgeResultType, int> judgeCountDictionary = new Dictionary<JudgeResultType, int>();
        
        // HOLD_MID�m�[�c���������m�[�c�̑���
        public int scoreNotesCount;
        
        // JudgeResultType���Ƃɓ��链�_�̎���
        private readonly Dictionary<JudgeResultType, int> scoreValueDictionary = new Dictionary<JudgeResultType, int>();
        
        // ���݂̃X�R�A
        public int currentScore;
        // �X�R�A�����N
        public PlayRank playRank;
        
        // �X�R�A���ύX���ꂽ�ۂɌĂ΂�� �r���[�͂�������o�^����
        public event Action<int> onScoreChanged;
        
        
        public GameScoreModel(int scoreNotesCount)
        {
            this.scoreNotesCount = scoreNotesCount;
            
            // �J�E���g�̏������E�o�^
            judgeCountDictionary = new Dictionary<JudgeResultType, int>
            {
                { JudgeResultType.GREAT, 0 },
                { JudgeResultType.NICE, 0 },
                { JudgeResultType.BAD, 0 },
                { JudgeResultType.MISS, 0 }
            };

            // ���育�Ƃ̓��_���v�Z���ēo�^
            // �X�R�A�͂��ׂ�GREAT�̎���100���_�ɂȂ�
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
        /// ���莞�ɃC�x���g�o�R�ŌĂ�
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
        /// MISS���v�Z���A�ŏI�I�ȃX�R�A���o��
        /// </summary>
        public void GameEnd()
        {
            // MISS���v�Z
            judgeCountDictionary[JudgeResultType.MISS] =
                scoreNotesCount
                - judgeCountDictionary[JudgeResultType.GREAT]
                - judgeCountDictionary[JudgeResultType.NICE]
                - judgeCountDictionary[JudgeResultType.BAD];
            
            // AllGreat�Ȃ� currentScore �� MAX_SCORE �ɂ���
            if (judgeCountDictionary[JudgeResultType.GREAT] == scoreNotesCount)
            {
                Debug.Log($"All Great : {MAX_SCORE}");
                currentScore = (int)MAX_SCORE;
            }

            playRank = Score2Rank(currentScore);
        }
        
        /// <summary>
        /// �X�R�A����Rank���Z�o
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
