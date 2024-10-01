using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StellaCircles.InGame
{
    public class GameScoreView : MonoBehaviour
    {
        [SerializeField] Text scoreText;

        public void GameStart(GameScoreModel gameScoreModel)
        {
            scoreText.text = "0";
            gameScoreModel.onScoreChanged += ScoreUp;
        }
    
        // �X�R�A�A�b�v���ɃX�R�A�e�L�X�g���A�j���[�V���������A�X�R�A�\����ύX����
        void ScoreUp(int currentScore)
        {
            scoreText.text = currentScore.ToString();
        }
    }
}
