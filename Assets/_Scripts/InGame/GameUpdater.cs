using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using StellaCircles.AssetBundleManagement;
using StellaCircles.Data;
using StellaCircles.Localize;
using StellaCircles.Select;
using StellaCircles.ViewUtils;
using UnityEngine.Serialization;

namespace StellaCircles.InGame
{
    /// <summary>
    /// Updateの順序を管理する
    /// ゲームのループを管理する
    /// </summary>
    public class GameUpdater : MonoBehaviour
    {
        [SerializeField] Localizer localizer;

        FumenAnalyzer fumenAnalyser;
        FumenData fumenData;
        
        GameScoreModel gameScoreModel; // ゲームスコア Judgementer経由で更新し、成績表示で使用する
        [SerializeField] GameScoreView gameScoreView;
        
        // 入力を管理する
        InputManager inputer;
        // ゲーム中の時間管理
        GameAudioTimer audioTimer;

        NotesModel notesModel;
        [SerializeField] NotesView notesView;
        [SerializeField] JudgeFrame judgeFrame;
        [SerializeField] GuideFrame guideFrame;
        AutoPlayer autoPlayer;
        [SerializeField] bool isAutoPlay = false;
        Judgementer judgementer;
        [SerializeField] JudgementView judgementView;
        [SerializeField] GradeManager gradeManager;

        [SerializeField] Text readyStartText;
        
        // ボタン登録
        [SerializeField] private Button gameMenuButton;
        [SerializeField] private Button menuBackGameButton;
        [SerializeField] private Button menuRestartButton;
        [SerializeField] private Button menuBackSelectButton;
        [SerializeField] private Button gradeBackSelectButton;
        

        bool isGame;

        SelectItemData mapData;
        int difficulty;

        void Start()
        {
            AudioSettings.OnAudioConfigurationChanged += OnAudioConfigurationChanged;

            // ボタン登録
            gameMenuButton.onClick.AddListener(OnGameMenu);
            menuBackGameButton.onClick.AddListener(OnMenuBackGame);
            menuRestartButton.onClick.AddListener(OnMenuRestartGame);
            menuBackSelectButton.onClick.AddListener(OnMenuBackSelect);
            gradeBackSelectButton.onClick.AddListener(OnGradeBackSelect);
            
            StartAsync(this.GetCancellationTokenOnDestroy()).Forget();
        }

        private async UniTaskVoid StartAsync(CancellationToken ct)
        {
            //設定の読み込み
            GameSetting.gameSettingValue = SaveManager.LoadGameSetting();
            Debug.Log($"FS : {GameSetting.gameSettingValue.frickSensitivity}");
            // MapDataBaseにデータをロードする
            MapDataBase.Instance.GradeDataLoad();

            localizer.AllLocalize();

            readyStartText.text = "Ready?";

            // 譜面読み込み
            (mapData, difficulty) = MapAgent.Instance.GetMapData();
            
            // 譜面の解析
            fumenAnalyser = new FumenAnalyzer();
            fumenData = await fumenAnalyser.AnalyzeMapData(mapData, difficulty, ct);
            
            // スコアの準備
            gameScoreModel = new GameScoreModel(fumenData.ScoreNotesCount);
            gameScoreView.GameStart(gameScoreModel);

            // 時間管理の準備
            audioTimer = new(fumenData);
            
            // ノーツの準備
            notesModel = new(fumenData, audioTimer);
            notesView.GameStart(fumenData, audioTimer, notesModel);
            
            // 判定の準備
            judgementer = new(fumenData, gameScoreModel, audioTimer, notesModel);
            judgementView.GameStart(judgementer);

            // 入力の準備
            inputer = new(judgementer);
            inputer.IsAcceptInput = true;
            
            isGame = false;
            isAutoPlay = false;
            AudioManager.Instance.AudioStart(fumenData.BGM);
            autoPlayer = new(fumenData, inputer, audioTimer);
            judgeFrame.GameStart(fumenData, audioTimer, ChangeStartText, ChangeEmptyText, GameEnd);
            guideFrame.GuideStart(fumenData, audioTimer);

            gradeManager.onClickBackToSelect = OnGradeBackSelect;
            gradeManager.ActiveFalsePanel();

            SceneOpen(ct).Forget();

            GameStart();
        }

        private async UniTaskVoid SceneOpen(CancellationToken ct)
        {
            await  UniTask.Delay((int)(0.3f * 1000), cancellationToken: ct);
            // yield return new WaitForSeconds(0.3f);
            SceneChangeAnimation.Instance.PlayOpenAnim();
        }

        
        void Update()
        {
            /*
            if (!isGame && GameStartInput())
            {
                GameStart();
            }
            */

            if (!isGame) return;
            audioTimer.MusicUpdate();
            judgementer.JudgementerUpdate();

            #region AutoPlay

            if (Application.isEditor)
            {
                if (Input.GetKeyUp(KeyCode.T))
                {
                    Debug.Log("オートモード切替");
                    isAutoPlay = !isAutoPlay;
                }

                if (isAutoPlay)
                {
                    autoPlayer.APUpdate();
                }
            }

            #endregion

            inputer.GetInput();
            notesModel.NotesModelUpdate();
            // notesView.VNUpdate();
            judgeFrame.JFUpdate();
            guideFrame.GuideUpdate();
        }

        // JudgeFrame側で呼ぶ
        private void ChangeStartText()
        {
            readyStartText.text = "Start!";
        }

        // JudgeFrame側で呼ぶ
        private void ChangeEmptyText()
        {
            readyStartText.text = "";
        }

        private void GameStart()
        {
            isGame = true;
            audioTimer.MusicGameStart();
        }

        //ゲーム中断メニュー
        [SerializeField] GameObject GameMenuPanel;

        /// <summary>
        /// 左下のメニューボタンを押したとき
        /// </summary>
        private void OnGameMenu()
        {
            //音楽が流れ始める前では、Time.timeを使って判定枠を動かしているため、
            //今のままだと、ゲーム開始時に設定した　tm1_real からポーズしている時間だけずれてしまう
            //後で治すとして、とりあえずisBGMStartの前はボタンを押せないようにする
            if (!isGame) return;
            if (!audioTimer.isBGMStart)
            {
                AudioManager.Instance.ShotSE(AudioManager.Instance.SE_tapmiss);
                return;
            }

            AudioManager.Instance.ShotSE(AudioManager.Instance.SE_tap);
            audioTimer.isOpenMenu = true;
            inputer.IsAcceptInput = false;
            if (audioTimer.isBGMStart)
            {
                // BGMを停止する
                AudioManager.Instance.TryPouseBGM();
            }

            GameMenuPanel.SetActive(true);
        }

        /// <summary>
        /// メニューパネルの「ゲームに戻る」を押したとき
        /// </summary>
        private void OnMenuBackGame()
        {
            AudioManager.Instance.ShotSE(AudioManager.Instance.SE_tap);
            audioTimer.isOpenMenu = false;
            inputer.IsAcceptInput = true;
            if (audioTimer.isBGMStart)
            {
                // BGMを再開
                AudioManager.Instance.TryUnpouseBGM();
            }

            GameMenuPanel.SetActive(false);
        }
        
        /// <summary>
        /// メニューパネルの「はじめから」を押したとき
        /// </summary>
        private void OnMenuRestartGame()
        {
            //ABAssetLoader.ReleaseMusic(mapData.itemId).Forget();
            mapData.ReleaseItemMusic(this.GetCancellationTokenOnDestroy()).Forget();
            AudioManager.Instance.ShotSE(AudioManager.Instance.SE_tap);
            RestartAsync(this.GetCancellationTokenOnDestroy()).Forget();
        }

        // ゲームをはじめから遊ぶためにロードしなおす
        private async UniTaskVoid RestartAsync(CancellationToken ct)
        {
            NoTapCanvas.Instance.NoTap1Second();
            SceneChangeAnimation.Instance.PlayCloseAnim();
            await UniTask.Delay((int)(1f * 1000), cancellationToken: ct);
            SceneManager.LoadScene(Constant.SceneName.GAME);
        }

        /// <summary>
        /// メニューパネルの「曲選択に戻る」を押したとき
        /// </summary>
        private void OnMenuBackSelect()
        {
            AudioManager.Instance.ShotSE(AudioManager.Instance.SE_tap);
            GameSetting._isBackSelect = true;
            NoTapCanvas.Instance.NoTap1Second();
            
            BackSelectAsync(this.GetCancellationTokenOnDestroy()).Forget();
        }


        //ゲーム終了後の処理
        private void GameEnd()
        {
            isGame = false;
            GameEndAsync(this.GetCancellationTokenOnDestroy()).Forget();
        }

        private async UniTaskVoid GameEndAsync(CancellationToken ct)
        {
            // 曲のフェードアウト
            AudioManager.Instance.FadeOutBGM();

            SceneChangeAnimation.Instance.PlayCloseAnim();
            await UniTask.Delay((int)(0.5f * 1000), cancellationToken: ct);

            await gradeManager.SetGradePanel(mapData, (Difficulty)difficulty, gameScoreModel, this.GetCancellationTokenOnDestroy());

            await UniTask.Delay((int)(1f * 1000), cancellationToken: ct);

            // 成績画面曲
            AudioManager.Instance.PlayBGMOnSelect(AudioManager.Instance.BGM_grade, 0f);

            SceneChangeAnimation.Instance.PlayOpenAnim();
            await UniTask.Delay((int)(0.5f * 1000), cancellationToken: ct);
        }

        // 成績画面のボタンを押したとき
        private void OnGradeBackSelect()
        {
            AudioManager.Instance.ShotSE(AudioManager.Instance.SE_tap);
            GameSetting._isBackSelect = true;
            NoTapCanvas.Instance.NoTap1Second();
            
            BackSelectAsync(this.GetCancellationTokenOnDestroy()).Forget();
        }

        // Selectシーンに戻る処理
        private async UniTaskVoid BackSelectAsync(CancellationToken ct)
        {
            
            // await UniTask.Delay((int)(0.2f * 1000), cancellationToken: ct);
            
            SceneChangeAnimation.Instance.PlayCloseAnim();

            await UniTask.Delay((int)(0.6f * 1000), cancellationToken: ct);
            
            //曲assetのリリース
            mapData.ReleaseItemMusic(this.GetCancellationTokenOnDestroy()).Forget();
            mapData.ReleaseItemSprite(this.GetCancellationTokenOnDestroy()).Forget();

            await UniTask.Delay((int)(0.4f * 1000), cancellationToken: ct);

            
            SceneManager.LoadScene(Constant.SceneName.SELECT);
        }


        //オーディオ機器が変更された時に呼ばれる
        void OnAudioConfigurationChanged(bool deviceWasChanged)
        {
            //ゲーム中ならメニュー画面を開く
            if (isGame)
            {
                OnGameMenu();
            }
        }
    }

}