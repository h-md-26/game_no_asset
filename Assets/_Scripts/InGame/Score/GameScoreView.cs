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
    
        // スコアアップ時にスコアテキストをアニメーションさせ、スコア表示を変更する
        void ScoreUp(int currentScore)
        {
            scoreText.text = currentScore.ToString();
        }
    }
}
