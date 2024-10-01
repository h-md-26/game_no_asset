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

        #region 表示系

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
            Debug.Log("設定の読み込み");
            //設定の読み込み
            GameSetting.gameSettingValue = SaveManager.LoadGameSetting();
            // MapDataBaseにデータをロードする
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


            // Gameから戻ってきたときは
            if (GameSetting._isBackSelect)
            {
                Debug.Log("Gameから戻ってきた");
                BackFromGame(this.GetCancellationTokenOnDestroy()).Forget();
            }
            else
            {
                Debug.Log("Titleからきた");
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
                selectCommonSumText.text = "年代の選択";
            }
            else if (GameSetting.gameSettingValue.languageType == LanguageType.EN)
            {
                selectCommonSumText.text = "Select the year";
            }
            else
            {
                selectCommonSumText.text = "年代の選択";
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
            // i = 0 : 2019 (enum Age のとおり)
            selectAgeIndex = i;


            foreach (var g in ageButtonBacks)
            {
                g.SetActive(false);
            }

            ageButtonBacks[i].SetActive(true);

            // 成績表示欄の変更
            ageGradeLabelText.text = (i + 2019).ToString();
            for (int j = 0; j < ageGradeTexts.Length; j++)
            {
                ageGradeTexts[j].text = MapDataBase.Instance.mapDBA.rankPerAges[i, j].ToString();
            }
        }

        public void OnClickAgeButton(int i)
        {
            // i = 0 : 2019 (enum Age の通り)
            selectAgeIndex = i;

            AudioManager.Instance.ShotSE(AudioManager.Instance.SE_tap);

            foreach (var g in ageButtonBacks)
            {
                g.SetActive(false);
            }

            ageButtonBacks[i].SetActive(true);

            //成績表示欄の変更
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

        // Age選択画面のセレクトが押された
        private void OnClickAgeSelectButton()
        {
            AudioManager.Instance.ShotSE(AudioManager.Instance.SE_decide);

            NoTapCanvas.Instance.NoTapHalfSeconds();

            //スクロールで用いるデータの登録
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
                selectCommonSumText.text = "曲の選択";
            }
            else if (GameSetting.gameSettingValue.languageType == LanguageType.EN)
            {
                selectCommonSumText.text = "Select the song";
            }
            else
            {
                selectCommonSumText.text = "曲の選択";
            }

            mapGradeImgs[0].sprite = hyphenSprite;
            mapGradeImgs[1].sprite = hyphenSprite;

            await ImageABStocker.Instance.GetJacketImagesOfAge(selectAgeIndex, ct);

            SetSelectMapPanel();
        }

        private void SetScrollData()
        {
            //スクロールで用いるデータの登録
            //試しにデータがひとつのところに同じデータを複数入れてみる

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
            //BGMの変更
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

        // 左下の成績ランク表示をセットする
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

        //曲選択画面をセット 選択中の曲を変更
        private void SetSelectMapPanel()
        {
            mapLoadingTextObj.SetActive(false);
            
            SelectStateManager.selectState = SelectState.Map;
            if (GameSetting.gameSettingValue.languageType == LanguageType.JP)
            {
                selectCommonSumText.text = "曲の選択";
            }
            else if (GameSetting.gameSettingValue.languageType == LanguageType.EN)
            {
                selectCommonSumText.text = "Select the song";
            }
            else
            {
                selectCommonSumText.text = "曲の選択";
            }


            Debug.Log("SelectMapに移動したことをCellに伝える");

            if (selectDifficultJacketObjs != null)
            {
                foreach (GameObject g in selectDifficultJacketObjs)
                {
                    g.SetActive(true);
                    g.GetComponent<ItemCell>().isSelectDiff = false;
                }
            }

            Debug.Log($"選択中の曲を{selectMapIndex[selectAgeIndex]}番目の要素に変更");

            scrollView.SelectCellExtend(selectMapIndex[selectAgeIndex], selectAgeIndex);

            selectDifficultPanel.SetActive(false);
            selectAgePanel.SetActive(false);
            scrollGardPanel.SetActive(false);
            selectMapPanel.SetActive(true);
            viewPanel.SetActive(true);
        }

        // 曲選択画面のバックが押された
        private void OnClickBackFromSelectMap()
        {
            AudioManager.Instance.ShotSE(AudioManager.Instance.SE_tap);

            selectCommonSelectButton.enabled = true;
            SetSelectAgePanel(true);
            selectMapPanel.SetActive(false);
            viewPanel.SetActive(false);
        }

        // 曲選択画面のセレクトが押された
        public void OnClickMapSelectButton(int _selectMapIndex)
        {
            //解放条件を満たしている場合
            if (itemViewer.selectItems[_selectMapIndex].itemUnLockWay.IsUnLocked())
            {
                PlaySelectRotateAnim();
                AudioManager.Instance.ShotSE(AudioManager.Instance.SE_decide);
                selectMapIndex[selectAgeIndex] = _selectMapIndex;
                selectMap = itemViewer.selectItems[_selectMapIndex];


                //ItemCellタグで検索し、pos.xが最も0に近いジャケットのみを表示
                Debug.Log("1つを除いて非表示");
                selectDifficultJacketObjs = GameObject.FindGameObjectsWithTag("ItemCell");
                Debug.Log($"Cellの数 {GameObject.FindGameObjectsWithTag("ItemCell").Length}");
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
                //解放条件を満たしていない場合スルー
                AudioManager.Instance.ShotSE(AudioManager.Instance.SE_tapmiss);
                Debug.Log("Locked!");
            }
        }

        // 難易度選択画面のパネルセット
        private void SetSelectDifficultPanel(SelectItemData _map)
        {
            SelectStateManager.selectState = SelectState.Difficult;

            if (GameSetting.gameSettingValue.languageType == LanguageType.JP)
            {
                selectCommonSumText.text = "難易度の選択";
            }
            else if (GameSetting.gameSettingValue.languageType == LanguageType.EN)
            {
                selectCommonSumText.text = "Select the difficulty";
            }
            else
            {
                selectCommonSumText.text = "難易度の選択";
            }

            SetGradePanel(_map);

            SwitchDifferent(selectDifficultIndex);

            viewPanel.SetActive(true);
            scrollGardPanel.SetActive(true);
            selectMapPanel.SetActive(false);
            selectAgePanel.SetActive(false);
            selectDifficultPanel.SetActive(true);


            Debug.Log("SelectDiffに移動したことをCellに伝える");
            selectDifficultJacketObjs = GameObject.FindGameObjectsWithTag("ItemCell");
            foreach (GameObject g in selectDifficultJacketObjs)
            {
                g.GetComponent<ItemCell>().isSelectDiff = true;
            }
        }

        // 難易度選択画面の成績パネルセット
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
        
        // 難易度を切り替える
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

        // 難易度選択画面の難易度切替が押された
        public void OnClickDifficultButton(int i)
        {
            AudioManager.Instance.ShotSE(AudioManager.Instance.SE_tap);
            SwitchDifferent(i);
        }

        // 難易度選択画面のバックが押された
        public void OnClickBackFromSelectDifficult()
        {
            AudioManager.Instance.ShotSE(AudioManager.Instance.SE_tap);
            SelectStateManager.selectState = SelectState.Map;
            SetSelectMapPanel();
        }

        // 難易度選択画面のセレクトが押された
        public void OnClickDifficultSelectButton()
        {
            AudioManager.Instance.FadeOutBGM();
            AudioManager.Instance.ShotSE(AudioManager.Instance.SE_gameStart);

            GameSetting._selectAgeIndex = selectAgeIndex;
            GameSetting._selectMapIndex = selectMapIndex;
            GameSetting._selectDifficultIndex = selectDifficultIndex;

            // Game に曲情報を持ち越す
            Debug.Log(itemViewer.selectItems[selectMapIndex[selectAgeIndex]].itemName);
            MapAgent.Instance.SetMapData(itemViewer.selectItems[selectMapIndex[selectAgeIndex]], selectDifficultIndex);

            //フェードアウト → Gameシーンへ
            //音も引き継げるように音が鳴りやむまで音Objを消さない
            NoTapCanvas.Instance.NoTap1Second();
            GoGame().Forget();
        }

        //SelectButton の統一
        public void OnClickCommonSelectButton()
        {
            //Buttonのクリック

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

        // セレクトボタンを押したときのアニメーション一括再生
        private void PlaySelectRotateAnim()
        {
            foreach (Animator anim in selectRotateAnims)
            {
                anim.SetTrigger(pressedHash);
            }
        }

        //BackButton の統一
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

        // ゲームに進む非同期処理
        async UniTask GoGame()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.2f));
            SceneChangeAnimation.Instance.PlayCloseAnim();

            await UniTask.Delay(TimeSpan.FromSeconds(0.6f));

            SceneManager.LoadScene(Constant.SceneName.GAME);
        }

        // ゲームを中断して戻ってきた非同期処理
        async UniTask BackFromGame(CancellationToken ct)
        {

            SetScrollData();
            await ImageABStocker.Instance.GetJacketImagesOfAge(selectAgeIndex, ct);
            SetSelectMapPanel();

            selectMap = itemViewer.selectItems[selectMapIndex[selectAgeIndex]];

            OnClickCommonSelectButton();

            await UniTask.Delay(TimeSpan.FromSeconds(0.3f), cancellationToken: ct);
            Debug.Log("OpenScene");

            Debug.Log("SelectMapに移動したことをCellに伝える");

            selectDifficultJacketObjs = GameObject.FindGameObjectsWithTag("ItemCell");
            foreach (GameObject g in selectDifficultJacketObjs)
            {
                g.GetComponent<ItemCell>().isSelectDiff = true;
            }

            await UniTask.Delay(TimeSpan.FromSeconds(0.1f), cancellationToken: ct);

            SceneChangeAnimation.Instance.PlayOpenAnim();
        }

        // タイトル画面からセレクト画面に来た時の非同期処理
        async UniTask GoFromTitle(CancellationToken ct)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.3f), cancellationToken: ct);
            Debug.Log("OpenScene");
            SceneChangeAnimation.Instance.PlayOpenAnim();
        }

        // タイトルに戻るときの非同期処理
        async UniTask BackToTitle(CancellationToken ct)
        {
            SceneChangeAnimation.Instance.PlayCloseAnim();
            await UniTask.Delay(TimeSpan.FromSeconds(0.7f), cancellationToken: ct);

            // Titleシーンへ
            Debug.Log("タイトルへ");
            SceneManager.LoadScene(Constant.SceneName.TITLE);
        }


        /*
         * SelectAge
         *  MapDataBaseから、年代ごとの全曲数と解放数(遊べる曲数、解放された曲数)を取得し、AgeButtonに表示する
         *  AgeButtonをタップすると照準が合い、左下に詳細と成績を表示する
         *  いまは19?23なのでボタンが5個で足りているが足りなくなったら、矢印ボタンを追加する
         *  照準の合った状態でSelectButtonを押すと、選択された年代を渡してSelectMapに進む
         *
         * SelectMap
         *  渡された年代の曲データをスクロールに渡す
         *  スクロールは渡された曲が解放されているか否かを調べ、表示をLock/Unlockで入れ替える
         *  上の方のボタンを押すと 渡された年代の曲データの内、解放されているだけ表示するように切り替える
         *  BackButtonが押されたらSelectAgeに戻る、Androidの戻るボタンも同様
         *  照準が合った状態の曲の成績を左下に表示する
         *  照準が合った状態で0.3秒程度経過したら、選択された曲を流す
         *  照準の合った状態でSelectButtonを押すと、Lockなら、ダメSEを鳴らし、UnLockなら、選択された曲IDを渡してSelectDifficultに進む
         *
         * SelectDifficult
         *  渡された曲の成績、譜面情報などをパネルに乗せ、難易度ボタンにもランクのバッジを貼る
         *  難易度ボタンEasyに照準を合わせておき、選択された難易度に照準を合わせなおす
         *  難易度変更とともに成績情報などを変更する
         *  SelectButtonが押されたら、曲IDと選択された難易度情報を渡してGameに進む
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