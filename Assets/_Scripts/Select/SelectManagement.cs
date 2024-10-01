using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using System.Threading;
using StellaCircles.AssetBundleManagement;
using StellaCircles.Data;
using StellaCircles.Localize;
using StellaCircles.ScrollView;
using StellaCircles.ViewUtils;


namespace StellaCircles.Select
{
    public class SelectManagement : MonoBehaviour
    {
        private static readonly int pressedHash = Animator.StringToHash("Pressed");
        [SerializeField] TestItemViewer itemViewer;
        [SerializeField] ItemScrollView scrollView;

        [SerializeField] Localizer localizer;

        #region �\���n

        [SerializeField] GameObject selectAgePanel;
        [SerializeField] GameObject selectMapPanel;
        [SerializeField] GameObject viewPanel;
        [SerializeField] GameObject scrollGardPanel;
        [SerializeField] GameObject selectDifficultPanel;

        [SerializeField] Text[] ageButtonRatioTexts;
        [SerializeField] GameObject[] ageButtonBacks;
        [SerializeField] GameObject[] difficultButtonBacks;

        [SerializeField] Text ageGradeLabelText;
        [SerializeField] Text[] ageGradeTexts;

        [SerializeField] Image[] mapGradeImgs;
        [SerializeField] GameObject selectMapSelectButtonObj;

        [SerializeField] Button selectCommonSelectButton;
        [SerializeField] Animator[] selectRotateAnims;
        [SerializeField] Text selectCommonSumText;

        [SerializeField] GameObject mapLoadingTextObj;
        [SerializeField] Text mapTitleText;
        [SerializeField] Text mapGenreText;
        [SerializeField] Text mapDateText;
        [SerializeField] Text mapScoreText;
        [SerializeField] Text[] mapJudgeTexts;
        [SerializeField] Text[] mapLevelTexts;
        [SerializeField] Image mapRankImage;

        [SerializeField] Sprite[] rankSprites;
        [SerializeField] Sprite hyphenSprite;

        #endregion

        [SerializeField] private int selectAgeIndex;
        [SerializeField] private int[] selectMapIndex;
        [SerializeField] private int selectDifficultIndex;

        GameObject[] selectDifficultJacketObjs;
        
        SelectItemData selectMap;
        
        void Start()
        {
            Debug.Log("�ݒ�̓ǂݍ���");
            //�ݒ�̓ǂݍ���
            GameSetting.gameSettingValue = SaveManager.LoadGameSetting();
            // MapDataBase�Ƀf�[�^�����[�h����
            MapDataBase.Instance.GradeDataLoad();

            selectAgeIndex = GameSetting._selectAgeIndex;
            selectMapIndex = GameSetting._selectMapIndex;
            selectDifficultIndex = GameSetting._selectDifficultIndex;

            localizer.AllLocalize();

            /*
            selectAgeIndex = 1;
            selectMapIndex = new int[] {0,0,0,0,0};
            selectDifficultIndex = 0;
            */
            mapLoadingTextObj.SetActive(false);

            SetSelectAgePanel(!GameSetting._isBackSelect);


            // Game����߂��Ă����Ƃ���
            if (GameSetting._isBackSelect)
            {
                Debug.Log("Game����߂��Ă���");
                BackFromGame(this.GetCancellationTokenOnDestroy()).Forget();
            }
            else
            {
                Debug.Log("Title���炫��");
                GoFromTitle(this.GetCancellationTokenOnDestroy()).Forget();
            }
        }

        private void Update()
        {
            if (SelectStateManager.selectState == SelectState.Map)
            {
                selectCommonSelectButton.enabled = false;

                selectDifficultJacketObjs = GameObject.FindGameObjectsWithTag("ItemCell");
                foreach (GameObject g in selectDifficultJacketObjs)
                {
                    if (g.GetComponent<ItemCell>().isSelectedCenterCell)
                    {
                        selectCommonSelectButton.enabled = true;
                    }
                }
            }
        }

        private void SetSelectAgePanel(bool isAudio)
        {
            SelectStateManager.selectState = SelectState.Age;


            if (GameSetting.gameSettingValue.languageType == LanguageType.JP)
            {
                selectCommonSumText.text = "�N��̑I��";
            }
            else if (GameSetting.gameSettingValue.languageType == LanguageType.EN)
            {
                selectCommonSumText.text = "Select the year";
            }
            else
            {
                selectCommonSumText.text = "�N��̑I��";
            }

            MapDataBase.Instance.mapDBA.SetInfos();

            for (int i = 0; i < ageButtonRatioTexts.Length; i++)
            {
                //Debug.Log("Text Change");
                ageButtonRatioTexts[i].text =
                    $"{MapDataBase.Instance.mapDBA.countAgeUnlocks[i]} / {MapDataBase.Instance.mapDBA.countAges[i]}";
            }

            OnClickAgeButtonMute(selectAgeIndex);
            if (isAudio)
            {
                AudioManager.Instance.PlayBGMOnSelect(AudioManager.Instance.BGM_selectAge, 0f);
            }

            selectMapPanel.SetActive(false);
            viewPanel.SetActive(false);
            scrollGardPanel.SetActive(false);
            selectDifficultPanel.SetActive(false);
            selectAgePanel.SetActive(true);
        }

        private void OnClickAgeButtonMute(int i)
        {
            // i = 0 : 2019 (enum Age �̂Ƃ���)
            selectAgeIndex = i;


            foreach (var g in ageButtonBacks)
            {
                g.SetActive(false);
            }

            ageButtonBacks[i].SetActive(true);

            // ���ѕ\�����̕ύX
            ageGradeLabelText.text = (i + 2019).ToString();
            for (int j = 0; j < ageGradeTexts.Length; j++)
            {
                ageGradeTexts[j].text = MapDataBase.Instance.mapDBA.rankPerAges[i, j].ToString();
            }
        }

        public void OnClickAgeButton(int i)
        {
            // i = 0 : 2019 (enum Age �̒ʂ�)
            selectAgeIndex = i;

            AudioManager.Instance.ShotSE(AudioManager.Instance.SE_tap);

            foreach (var g in ageButtonBacks)
            {
                g.SetActive(false);
            }

            ageButtonBacks[i].SetActive(true);

            //���ѕ\�����̕ύX
            ageGradeLabelText.text = (i + 2019).ToString();
            for (int j = 0; j < ageGradeTexts.Length; j++)
            {
                ageGradeTexts[j].text = MapDataBase.Instance.mapDBA.rankPerAges[i, j].ToString();
            }
        }

        private void OnClickBackFromSelectAge()
        {
            AudioManager.Instance.ShotSE(AudioManager.Instance.SE_tap);

            NoTapCanvas.Instance.NoTap1Second();
            BackToTitle(this.GetCancellationTokenOnDestroy()).Forget();
        }

        // Age�I����ʂ̃Z���N�g�������ꂽ
        private void OnClickAgeSelectButton()
        {
            AudioManager.Instance.ShotSE(AudioManager.Instance.SE_decide);

            NoTapCanvas.Instance.NoTapHalfSeconds();

            //�X�N���[���ŗp����f�[�^�̓o�^
            SetScrollData();

            SetMapSelectAsync(this.GetCancellationTokenOnDestroy()).Forget();
        }

        private async UniTask SetMapSelectAsync(CancellationToken ct)
        {
            mapLoadingTextObj.SetActive(true);
            selectDifficultPanel.SetActive(false);
            selectAgePanel.SetActive(false);
            scrollGardPanel.SetActive(false);
            selectMapPanel.SetActive(true);
            if (GameSetting.gameSettingValue.languageType == LanguageType.JP)
            {
                selectCommonSumText.text = "�Ȃ̑I��";
            }
            else if (GameSetting.gameSettingValue.languageType == LanguageType.EN)
            {
                selectCommonSumText.text = "Select the song";
            }
            else
            {
                selectCommonSumText.text = "�Ȃ̑I��";
            }

            mapGradeImgs[0].sprite = hyphenSprite;
            mapGradeImgs[1].sprite = hyphenSprite;

            await ImageABStocker.Instance.GetJacketImagesOfAge(selectAgeIndex, ct);

            SetSelectMapPanel();
        }

        private void SetScrollData()
        {
            //�X�N���[���ŗp����f�[�^�̓o�^
            //�����Ƀf�[�^���ЂƂ̂Ƃ���ɓ����f�[�^�𕡐�����Ă݂�

            SelectItemData[] ar;
            if (MapDataBase.Instance.mapDBA.countAges[selectAgeIndex] == 1)
            {
                ar = new SelectItemData[3];

                for (int i = 0; i < ar.Length; i++)
                {
                    ar[i] = MapDataBase.Instance.mapDict[MapDataBase.Instance.mapDBA.mapIdPerAgeIDs[selectAgeIndex, 0]].infoData;
                }
            }
            else
            {
                ar = new SelectItemData[MapDataBase.Instance.mapDBA.countAges[selectAgeIndex]];

                for (int i = 0; i < ar.Length; i++)
                {
                    ar[i] = MapDataBase.Instance.mapDict[MapDataBase.Instance.mapDBA.mapIdPerAgeIDs[selectAgeIndex, i]].infoData;
                }
            }

            itemViewer.selectItems = ar;
            itemViewer.ViwerStart();
        }

        public async UniTask SetBGM(int index, CancellationToken ct)
        {
            //BGM�̕ύX
            AudioManager.Instance.StopBGM();

            await AudioManager.Instance.GetMusicStack(
                itemViewer.selectItems[index].itemId,
                (clip) =>
                {
                    AudioManager.Instance.BGM_scroll = clip;
                    AudioManager.Instance.isMusicLoading = false;
                }, ct);
            Debug.Log(
                $"index : {index}, itemId : {itemViewer.selectItems[index].itemId}, demoTime : {itemViewer.selectItems[index].itemDemoTime}");
            AudioManager.Instance.PlayBGMOnSelect(AudioManager.Instance.BGM_scroll, itemViewer.selectItems[index].itemDemoTime);
        }

        // �����̐��у����N�\�����Z�b�g����
        public void SetMapGradePanel(int index)
        {
            mapGradeImgs[0].sprite 
                = itemViewer.selectItems[index].GetGradeData().PlayRankMaxMin[0, 0] != PlayRank.YET 
                    ? rankSprites[(int)itemViewer.selectItems[index].GetGradeData().PlayRankMaxMin[0, 0]] 
                    : hyphenSprite;

            mapGradeImgs[1].sprite 
                = itemViewer.selectItems[index].GetGradeData().PlayRankMaxMin[1, 0] != PlayRank.YET 
                    ? rankSprites[(int)itemViewer.selectItems[index].GetGradeData().PlayRankMaxMin[1, 0]] 
                    : hyphenSprite;
        }

        //�ȑI����ʂ��Z�b�g �I�𒆂̋Ȃ�ύX
        private void SetSelectMapPanel()
        {
            mapLoadingTextObj.SetActive(false);
            
            SelectStateManager.selectState = SelectState.Map;
            if (GameSetting.gameSettingValue.languageType == LanguageType.JP)
            {
                selectCommonSumText.text = "�Ȃ̑I��";
            }
            else if (GameSetting.gameSettingValue.languageType == LanguageType.EN)
            {
                selectCommonSumText.text = "Select the song";
            }
            else
            {
                selectCommonSumText.text = "�Ȃ̑I��";
            }


            Debug.Log("SelectMap�Ɉړ��������Ƃ�Cell�ɓ`����");

            if (selectDifficultJacketObjs != null)
            {
                foreach (GameObject g in selectDifficultJacketObjs)
                {
                    g.SetActive(true);
                    g.GetComponent<ItemCell>().isSelectDiff = false;
                }
            }

            Debug.Log($"�I�𒆂̋Ȃ�{selectMapIndex[selectAgeIndex]}�Ԗڂ̗v�f�ɕύX");

            scrollView.SelectCellExtend(selectMapIndex[selectAgeIndex], selectAgeIndex);

            selectDifficultPanel.SetActive(false);
            selectAgePanel.SetActive(false);
            scrollGardPanel.SetActive(false);
            selectMapPanel.SetActive(true);
            viewPanel.SetActive(true);
        }

        // �ȑI����ʂ̃o�b�N�������ꂽ
        private void OnClickBackFromSelectMap()
        {
            AudioManager.Instance.ShotSE(AudioManager.Instance.SE_tap);

            selectCommonSelectButton.enabled = true;
            SetSelectAgePanel(true);
            selectMapPanel.SetActive(false);
            viewPanel.SetActive(false);
        }

        // �ȑI����ʂ̃Z���N�g�������ꂽ
        public void OnClickMapSelectButton(int _selectMapIndex)
        {
            //��������𖞂����Ă���ꍇ
            if (itemViewer.selectItems[_selectMapIndex].itemUnLockWay.IsUnLocked())
            {
                PlaySelectRotateAnim();
                AudioManager.Instance.ShotSE(AudioManager.Instance.SE_decide);
                selectMapIndex[selectAgeIndex] = _selectMapIndex;
                selectMap = itemViewer.selectItems[_selectMapIndex];


                //ItemCell�^�O�Ō������Apos.x���ł�0�ɋ߂��W���P�b�g�݂̂�\��
                Debug.Log("1�������Ĕ�\��");
                selectDifficultJacketObjs = GameObject.FindGameObjectsWithTag("ItemCell");
                Debug.Log($"Cell�̐� {GameObject.FindGameObjectsWithTag("ItemCell").Length}");
                foreach (GameObject g in selectDifficultJacketObjs)
                {
                    if (g.GetComponent<ItemCell>().CheckIsSelectedCenterCell())
                    {
                        Debug.Log($"this Cell active");
                        g.SetActive(true);
                    }
                    else
                    {
                        Debug.Log($"this Cell inactive");
                        g.SetActive(false);
                    }
                }

                NoTapCanvas.Instance.NoTapHalfSeconds();

                SetSelectDifficultPanel(selectMap);
            }
            else
            {
                //��������𖞂����Ă��Ȃ��ꍇ�X���[
                AudioManager.Instance.ShotSE(AudioManager.Instance.SE_tapmiss);
                Debug.Log("Locked!");
            }
        }

        // ��Փx�I����ʂ̃p�l���Z�b�g
        private void SetSelectDifficultPanel(SelectItemData _map)
        {
            SelectStateManager.selectState = SelectState.Difficult;

            if (GameSetting.gameSettingValue.languageType == LanguageType.JP)
            {
                selectCommonSumText.text = "��Փx�̑I��";
            }
            else if (GameSetting.gameSettingValue.languageType == LanguageType.EN)
            {
                selectCommonSumText.text = "Select the difficulty";
            }
            else
            {
                selectCommonSumText.text = "��Փx�̑I��";
            }

            SetGradePanel(_map);

            SwitchDifferent(selectDifficultIndex);

            viewPanel.SetActive(true);
            scrollGardPanel.SetActive(true);
            selectMapPanel.SetActive(false);
            selectAgePanel.SetActive(false);
            selectDifficultPanel.SetActive(true);


            Debug.Log("SelectDiff�Ɉړ��������Ƃ�Cell�ɓ`����");
            selectDifficultJacketObjs = GameObject.FindGameObjectsWithTag("ItemCell");
            foreach (GameObject g in selectDifficultJacketObjs)
            {
                g.GetComponent<ItemCell>().isSelectDiff = true;
            }
        }

        // ��Փx�I����ʂ̐��уp�l���Z�b�g
        private void SetGradePanel(SelectItemData _map)
        {
            mapTitleText.text = _map.GetItemName();
            mapGenreText.text = _map.GetItemGenre();
            mapDateText.text = _map.itemDate;
            mapScoreText.text = _map.GetGradeData().playScores[selectDifficultIndex].ToString();

            for (int i = 0; i < mapJudgeTexts.Length; i++)
            {
                mapJudgeTexts[i].text = _map.GetGradeData().gradeJudgeResultMaxMins[selectDifficultIndex, i].ToString();
            }

            for (int i = 0; i < mapLevelTexts.Length; i++)
            {
                mapLevelTexts[i].text = _map.mapLevels[i].ToString();
            }

            if ((int)_map.GetGradeData().PlayRankMaxMin[selectDifficultIndex, 0] >= 0)
            {
                mapRankImage.enabled = true;
                mapRankImage.sprite = rankSprites[(int)_map.GetGradeData().PlayRankMaxMin[selectDifficultIndex, 0]];
            }
            else
            {
                mapRankImage.enabled = false;
            }
        }
        
        // ��Փx��؂�ւ���
        private void SwitchDifferent(int i)
        {
            selectDifficultIndex = i;

            SetGradePanel(selectMap);

            foreach (var g in difficultButtonBacks)
            {
                g.SetActive(false);
            }

            difficultButtonBacks[i].SetActive(true);
        }

        // ��Փx�I����ʂ̓�Փx�ؑւ������ꂽ
        public void OnClickDifficultButton(int i)
        {
            AudioManager.Instance.ShotSE(AudioManager.Instance.SE_tap);
            SwitchDifferent(i);
        }

        // ��Փx�I����ʂ̃o�b�N�������ꂽ
        public void OnClickBackFromSelectDifficult()
        {
            AudioManager.Instance.ShotSE(AudioManager.Instance.SE_tap);
            SelectStateManager.selectState = SelectState.Map;
            SetSelectMapPanel();
        }

        // ��Փx�I����ʂ̃Z���N�g�������ꂽ
        public void OnClickDifficultSelectButton()
        {
            AudioManager.Instance.FadeOutBGM();
            AudioManager.Instance.ShotSE(AudioManager.Instance.SE_gameStart);

            GameSetting._selectAgeIndex = selectAgeIndex;
            GameSetting._selectMapIndex = selectMapIndex;
            GameSetting._selectDifficultIndex = selectDifficultIndex;

            // Game �ɋȏ��������z��
            Debug.Log(itemViewer.selectItems[selectMapIndex[selectAgeIndex]].itemName);
            MapAgent.Instance.SetMapData(itemViewer.selectItems[selectMapIndex[selectAgeIndex]], selectDifficultIndex);

            //�t�F�[�h�A�E�g �� Game�V�[����
            //���������p����悤�ɉ������ނ܂ŉ�Obj�������Ȃ�
            NoTapCanvas.Instance.NoTap1Second();
            GoGame().Forget();
        }

        //SelectButton �̓���
        public void OnClickCommonSelectButton()
        {
            //Button�̃N���b�N

            switch (SelectStateManager.selectState)
            {
                case (SelectState.Age):
                    PlaySelectRotateAnim();
                    OnClickAgeSelectButton();
                    break;
                case (SelectState.Map):
                    itemViewer.OnSelectButton();
                    break;
                case (SelectState.Difficult):
                    PlaySelectRotateAnim();
                    OnClickDifficultSelectButton();
                    break;
            }
        }

        // �Z���N�g�{�^�����������Ƃ��̃A�j���[�V�����ꊇ�Đ�
        private void PlaySelectRotateAnim()
        {
            foreach (Animator anim in selectRotateAnims)
            {
                anim.SetTrigger(pressedHash);
            }
        }

        //BackButton �̓���
        public void OnClickCommonBackButton()
        {

            switch (SelectStateManager.selectState)
            {
                case (SelectState.Age):
                    OnClickBackFromSelectAge();
                    break;
                case (SelectState.Map):
                    OnClickBackFromSelectMap();
                    break;
                case (SelectState.Difficult):
                    OnClickBackFromSelectDifficult();
                    break;
            }
        }

        // �Q�[���ɐi�ޔ񓯊�����
        async UniTask GoGame()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.2f));
            SceneChangeAnimation.Instance.PlayCloseAnim();

            await UniTask.Delay(TimeSpan.FromSeconds(0.6f));

            SceneManager.LoadScene(Constant.SceneName.GAME);
        }

        // �Q�[���𒆒f���Ė߂��Ă����񓯊�����
        async UniTask BackFromGame(CancellationToken ct)
        {

            SetScrollData();
            await ImageABStocker.Instance.GetJacketImagesOfAge(selectAgeIndex, ct);
            SetSelectMapPanel();

            selectMap = itemViewer.selectItems[selectMapIndex[selectAgeIndex]];

            OnClickCommonSelectButton();

            await UniTask.Delay(TimeSpan.FromSeconds(0.3f), cancellationToken: ct);
            Debug.Log("OpenScene");

            Debug.Log("SelectMap�Ɉړ��������Ƃ�Cell�ɓ`����");

            selectDifficultJacketObjs = GameObject.FindGameObjectsWithTag("ItemCell");
            foreach (GameObject g in selectDifficultJacketObjs)
            {
                g.GetComponent<ItemCell>().isSelectDiff = true;
            }

            await UniTask.Delay(TimeSpan.FromSeconds(0.1f), cancellationToken: ct);

            SceneChangeAnimation.Instance.PlayOpenAnim();
        }

        // �^�C�g����ʂ���Z���N�g��ʂɗ������̔񓯊�����
        async UniTask GoFromTitle(CancellationToken ct)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.3f), cancellationToken: ct);
            Debug.Log("OpenScene");
            SceneChangeAnimation.Instance.PlayOpenAnim();
        }

        // �^�C�g���ɖ߂�Ƃ��̔񓯊�����
        async UniTask BackToTitle(CancellationToken ct)
        {
            SceneChangeAnimation.Instance.PlayCloseAnim();
            await UniTask.Delay(TimeSpan.FromSeconds(0.7f), cancellationToken: ct);

            // Title�V�[����
            Debug.Log("�^�C�g����");
            SceneManager.LoadScene(Constant.SceneName.TITLE);
        }


        /*
         * SelectAge
         *  MapDataBase����A�N�ゲ�Ƃ̑S�Ȑ��Ɖ����(�V�ׂ�Ȑ��A������ꂽ�Ȑ�)���擾���AAgeButton�ɕ\������
         *  AgeButton���^�b�v����ƏƏ��������A�����ɏڍׂƐ��т�\������
         *  ���܂�19?23�Ȃ̂Ń{�^����5�ő���Ă��邪����Ȃ��Ȃ�����A���{�^����ǉ�����
         *  �Ə��̍�������Ԃ�SelectButton�������ƁA�I�����ꂽ�N���n����SelectMap�ɐi��
         *
         * SelectMap
         *  �n���ꂽ�N��̋ȃf�[�^���X�N���[���ɓn��
         *  �X�N���[���͓n���ꂽ�Ȃ��������Ă��邩�ۂ��𒲂ׁA�\����Lock/Unlock�œ���ւ���
         *  ��̕��̃{�^���������� �n���ꂽ�N��̋ȃf�[�^�̓��A�������Ă��邾���\������悤�ɐ؂�ւ���
         *  BackButton�������ꂽ��SelectAge�ɖ߂�AAndroid�̖߂�{�^�������l
         *  �Ə�����������Ԃ̋Ȃ̐��т������ɕ\������
         *  �Ə�����������Ԃ�0.3�b���x�o�߂�����A�I�����ꂽ�Ȃ𗬂�
         *  �Ə��̍�������Ԃ�SelectButton�������ƁALock�Ȃ�A�_��SE��炵�AUnLock�Ȃ�A�I�����ꂽ��ID��n����SelectDifficult�ɐi��
         *
         * SelectDifficult
         *  �n���ꂽ�Ȃ̐��сA���ʏ��Ȃǂ��p�l���ɏ悹�A��Փx�{�^���ɂ������N�̃o�b�W��\��
         *  ��Փx�{�^��Easy�ɏƏ������킹�Ă����A�I�����ꂽ��Փx�ɏƏ������킹�Ȃ���
         *  ��Փx�ύX�ƂƂ��ɐ��я��Ȃǂ�ύX����
         *  SelectButton�������ꂽ��A��ID�ƑI�����ꂽ��Փx����n����Game�ɐi��
         *
         */
    }

    public enum SelectState
    {
        Age,
        Map,
        Difficult,
        Shop
    }

    public enum Difficulty
    {
        EASY = 0,
        HARD = 1
    }
}