using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using StellaCircles.AssetBundleManagement;
using StellaCircles.Data;
using StellaCircles.Select;
using UnityEngine;
using UnityEngine.UI;

namespace StellaCircles.InGame
{
    public class GradeManager : MonoBehaviour
    {
        /*
         * GameEndの流れ
         *
         * 演奏終了→画面遷移→成績画面→画面遷移→難易度選択
         *
         * 成績画面
         * 　成績を受け取り、RankやMissを計算する
         * 　ハイスコアorワーストスコアなら成績を保存する( Easy Save  ot  Quick save )
         * 　ハイスコア時にはキラキラさせる？
         * 　新たに解禁された曲があったら解禁アニメーションを流す
         *
         */

        [SerializeField] GameObject gradePanel;
        [SerializeField] Text titleText;
        [SerializeField] Text difficultyText;
        [SerializeField] Text rankText;
        [SerializeField] Image rankImage;
        [SerializeField] Text scoreText;
        [SerializeField] Text greatText;
        [SerializeField] Text niceText;
        [SerializeField] Text badText;
        [SerializeField] Text missText;
        [SerializeField] Image jacketImage;
        [SerializeField] Button backToSelectButtoon;

        [SerializeField] Sprite[] rankSprites;

        public void ActiveFalsePanel()
        {
            gradePanel.SetActive(false);
        }

        public async UniTask SetGradePanel(SelectItemData itemData, Difficulty difficulty, GameScoreModel gameScoreModel, CancellationToken ct)
        {
            // gameScoreModelの最終的な値を集計する
            gameScoreModel.GameEnd();
            
            
            titleText.text = itemData.GetItemName();
            difficultyText.text = $"<{difficulty}>";

            // AllGreatなら 演出を入れてもいいかも
            

            // ランクを表示
            var rank = gameScoreModel.playRank;
            rankText.text = rank.ToString();
            if ((int)rank >= 0)
            {
                rankImage.sprite = rankSprites[(int)rank];
            }

            // テキストに値をセット
            scoreText.text = gameScoreModel.currentScore.ToString();
            greatText.text = gameScoreModel.judgeCountDictionary[JudgeResultType.GREAT].ToString();
            niceText.text = gameScoreModel.judgeCountDictionary[JudgeResultType.NICE].ToString();
            badText.text = gameScoreModel.judgeCountDictionary[JudgeResultType.BAD].ToString();
            missText.text = gameScoreModel.judgeCountDictionary[JudgeResultType.MISS].ToString();

            // ab imageid(id) を load
            jacketImage.sprite = await itemData.LoadItemSprite(ct);

            // データの保存 //
            MapDataBase.Instance.CountUpPlayCount(itemData, difficulty);
            if (itemData.GetGradeData().playScores[(int)difficulty] < gameScoreModel.currentScore)
            {
                Debug.Log("スコア更新");
                MapDataBase.Instance.UpdateGradeData(RecordType.Best, itemData, difficulty, gameScoreModel);
            }
            
            // ボタンの登録
            backToSelectButtoon.onClick.AddListener(OnClickBackToSelect);

            gradePanel.SetActive(true);
        }

        /// <summary>
        /// GameUpdaterにあるセレクトに戻る処理を登録する
        /// </summary>
        public Action onClickBackToSelect;

        /// <summary>
        /// Backボタンにこの関数をセットする
        /// </summary>
        private void OnClickBackToSelect()
        {
            onClickBackToSelect?.Invoke();
        }
    }
}