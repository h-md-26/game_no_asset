using System;
using System.Collections;
using System.Collections.Generic;
using StellaCircles.InGame;
using UnityEngine;
using UnityEngine.UI;

namespace StellaCircles.InGame
{
    public class JudgementView : MonoBehaviour
    {
        // Judgementer�̌��ʂ����ƂɌ����ڂ�ύX����

        /*

        �����ł�
        ����e�L�X�g

        ��S��

        �X�R�A�e�L�X�g
        �m�[�c
        �͂��ꂼ��̃r���[�ōs��

        */

        [SerializeField] public Text judgeResultText;
        [SerializeField] string judgeResultStateName = "UIScalling";
        Animator judgeResultTxAm;
        int judgeResultHash;

        public void GameStart(Judgementer judgementer)
        {
            judgeResultText.text = string.Empty;
            judgeResultTxAm = judgeResultText.GetComponent<Animator>();
            judgeResultHash = Animator.StringToHash(judgeResultStateName);

            judgementer.onJudged += PlayJudgeTextAnimation;
        }

        void PlayJudgeTextAnimation(JudgeResultType judgeResultType)
        {
            if(judgeResultType == JudgeResultType.NONE) return;
            
            judgeResultText.text = GetResultString(judgeResultType);
            judgeResultTxAm.Play(judgeResultHash);
        }

        private string GetResultString(JudgeResultType judgeResultType)
        {
            switch (judgeResultType)
            {
                case JudgeResultType.NONE:
                    return "";
                case JudgeResultType.GREAT:
                    return "Great!";
                case JudgeResultType.NICE:
                    return "Nice!";
                case JudgeResultType.BAD:
                    return "Bad";
                case JudgeResultType.MISS:
                    return "Miss";
                default:
                    throw new NotImplementedException();
            }
        }

    }
}