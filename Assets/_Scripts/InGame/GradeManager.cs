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
         * GameEnd�̗���
         *
         * ���t�I������ʑJ�ځ����щ�ʁ���ʑJ�ځ���Փx�I��
         *
         * ���щ��
         * �@���т��󂯎��ARank��Miss���v�Z����
         * �@�n�C�X�R�Aor���[�X�g�X�R�A�Ȃ琬�т�ۑ�����( Easy Save  ot  Quick save )
         * �@�n�C�X�R�A���ɂ̓L���L��������H
         * �@�V���ɉ��ւ��ꂽ�Ȃ�����������փA�j���[�V�����𗬂�
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
            // gameScoreModel�̍ŏI�I�Ȓl���W�v����
            gameScoreModel.GameEnd();
            
            
            titleText.text = itemData.GetItemName();
            difficultyText.text = $"<{difficulty}>";

            // AllGreat�Ȃ� ���o�����Ă���������
            

            // �����N��\��
            var rank = gameScoreModel.playRank;
            rankText.text = rank.ToString();
            if ((int)rank >= 0)
            {
                rankImage.sprite = rankSprites[(int)rank];
            }

            // �e�L�X�g�ɒl���Z�b�g
            scoreText.text = gameScoreModel.currentScore.ToString();
            greatText.text = gameScoreModel.judgeCountDictionary[JudgeResultType.GREAT].ToString();
            niceText.text = gameScoreModel.judgeCountDictionary[JudgeResultType.NICE].ToString();
            badText.text = gameScoreModel.judgeCountDictionary[JudgeResultType.BAD].ToString();
            missText.text = gameScoreModel.judgeCountDictionary[JudgeResultType.MISS].ToString();

            // ab imageid(id) �� load
            jacketImage.sprite = await itemData.LoadItemSprite(ct);

            // �f�[�^�̕ۑ� //
            MapDataBase.Instance.CountUpPlayCount(itemData, difficulty);
            if (itemData.GetGradeData().playScores[(int)difficulty] < gameScoreModel.currentScore)
            {
                Debug.Log("�X�R�A�X�V");
                MapDataBase.Instance.UpdateGradeData(RecordType.Best, itemData, difficulty, gameScoreModel);
            }
            
            // �{�^���̓o�^
            backToSelectButtoon.onClick.AddListener(OnClickBackToSelect);

            gradePanel.SetActive(true);
        }

        /// <summary>
        /// GameUpdater�ɂ���Z���N�g�ɖ߂鏈����o�^����
        /// </summary>
        public Action onClickBackToSelect;

        /// <summary>
        /// Back�{�^���ɂ��̊֐����Z�b�g����
        /// </summary>
        private void OnClickBackToSelect()
        {
            onClickBackToSelect?.Invoke();
        }
    }
}