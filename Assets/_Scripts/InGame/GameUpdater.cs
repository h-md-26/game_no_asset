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
    /// Update�̏������Ǘ�����
    /// �Q�[���̃��[�v���Ǘ�����
    /// </summary>
    public class GameUpdater : MonoBehaviour
    {
        [SerializeField] Localizer localizer;

        FumenAnalyzer fumenAnalyser;
        FumenData fumenData;
        
        GameScoreModel gameScoreModel; // �Q�[���X�R�A Judgementer�o�R�ōX�V���A���ѕ\���Ŏg�p����
        [SerializeField] GameScoreView gameScoreView;
        
        // ���͂��Ǘ�����
        InputManager inputer;
        // �Q�[�����̎��ԊǗ�
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
        
        // �{�^���o�^
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

            // �{�^���o�^
            gameMenuButton.onClick.AddListener(OnGameMenu);
            menuBackGameButton.onClick.AddListener(OnMenuBackGame);
            menuRestartButton.onClick.AddListener(OnMenuRestartGame);
            menuBackSelectButton.onClick.AddListener(OnMenuBackSelect);
            gradeBackSelectButton.onClick.AddListener(OnGradeBackSelect);
            
            StartAsync(this.GetCancellationTokenOnDestroy()).Forget();
        }

        private async UniTaskVoid StartAsync(CancellationToken ct)
        {
            //�ݒ�̓ǂݍ���
            GameSetting.gameSettingValue = SaveManager.LoadGameSetting();
            Debug.Log($"FS : {GameSetting.gameSettingValue.frickSensitivity}");
            // MapDataBase�Ƀf�[�^�����[�h����
            MapDataBase.Instance.GradeDataLoad();

            localizer.AllLocalize();

            readyStartText.text = "Ready?";

            // ���ʓǂݍ���
            (mapData, difficulty) = MapAgent.Instance.GetMapData();
            
            // ���ʂ̉��
            fumenAnalyser = new FumenAnalyzer();
            fumenData = await fumenAnalyser.AnalyzeMapData(mapData, difficulty, ct);
            
            // �X�R�A�̏���
            gameScoreModel = new GameScoreModel(fumenData.ScoreNotesCount);
            gameScoreView.GameStart(gameScoreModel);

            // ���ԊǗ��̏���
            audioTimer = new(fumenData);
            
            // �m�[�c�̏���
            notesModel = new(fumenData, audioTimer);
            notesView.GameStart(fumenData, audioTimer, notesModel);
            
            // ����̏���
            judgementer = new(fumenData, gameScoreModel, audioTimer, notesModel);
            judgementView.GameStart(judgementer);

            // ���͂̏���
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
                    Debug.Log("�I�[�g���[�h�ؑ�");
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

        // JudgeFrame���ŌĂ�
        private void ChangeStartText()
        {
            readyStartText.text = "Start!";
        }

        // JudgeFrame���ŌĂ�
        private void ChangeEmptyText()
        {
            readyStartText.text = "";
        }

        private void GameStart()
        {
            isGame = true;
            audioTimer.MusicGameStart();
        }

        //�Q�[�����f���j���[
        [SerializeField] GameObject GameMenuPanel;

        /// <summary>
        /// �����̃��j���[�{�^�����������Ƃ�
        /// </summary>
        private void OnGameMenu()
        {
            //���y������n�߂�O�ł́ATime.time���g���Ĕ���g�𓮂����Ă��邽�߁A
            //���̂܂܂��ƁA�Q�[���J�n���ɐݒ肵���@tm1_real ����|�[�Y���Ă��鎞�Ԃ�������Ă��܂�
            //��Ŏ����Ƃ��āA�Ƃ肠����isBGMStart�̑O�̓{�^���������Ȃ��悤�ɂ���
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
                // BGM���~����
                AudioManager.Instance.TryPouseBGM();
            }

            GameMenuPanel.SetActive(true);
        }

        /// <summary>
        /// ���j���[�p�l���́u�Q�[���ɖ߂�v���������Ƃ�
        /// </summary>
        private void OnMenuBackGame()
        {
            AudioManager.Instance.ShotSE(AudioManager.Instance.SE_tap);
            audioTimer.isOpenMenu = false;
            inputer.IsAcceptInput = true;
            if (audioTimer.isBGMStart)
            {
                // BGM���ĊJ
                AudioManager.Instance.TryUnpouseBGM();
            }

            GameMenuPanel.SetActive(false);
        }
        
        /// <summary>
        /// ���j���[�p�l���́u�͂��߂���v���������Ƃ�
        /// </summary>
        private void OnMenuRestartGame()
        {
            //ABAssetLoader.ReleaseMusic(mapData.itemId).Forget();
            mapData.ReleaseItemMusic(this.GetCancellationTokenOnDestroy()).Forget();
            AudioManager.Instance.ShotSE(AudioManager.Instance.SE_tap);
            RestartAsync(this.GetCancellationTokenOnDestroy()).Forget();
        }

        // �Q�[�����͂��߂���V�Ԃ��߂Ƀ��[�h���Ȃ���
        private async UniTaskVoid RestartAsync(CancellationToken ct)
        {
            NoTapCanvas.Instance.NoTap1Second();
            SceneChangeAnimation.Instance.PlayCloseAnim();
            await UniTask.Delay((int)(1f * 1000), cancellationToken: ct);
            SceneManager.LoadScene(Constant.SceneName.GAME);
        }

        /// <summary>
        /// ���j���[�p�l���́u�ȑI���ɖ߂�v���������Ƃ�
        /// </summary>
        private void OnMenuBackSelect()
        {
            AudioManager.Instance.ShotSE(AudioManager.Instance.SE_tap);
            GameSetting._isBackSelect = true;
            NoTapCanvas.Instance.NoTap1Second();
            
            BackSelectAsync(this.GetCancellationTokenOnDestroy()).Forget();
        }


        //�Q�[���I����̏���
        private void GameEnd()
        {
            isGame = false;
            GameEndAsync(this.GetCancellationTokenOnDestroy()).Forget();
        }

        private async UniTaskVoid GameEndAsync(CancellationToken ct)
        {
            // �Ȃ̃t�F�[�h�A�E�g
            AudioManager.Instance.FadeOutBGM();

            SceneChangeAnimation.Instance.PlayCloseAnim();
            await UniTask.Delay((int)(0.5f * 1000), cancellationToken: ct);

            await gradeManager.SetGradePanel(mapData, (Difficulty)difficulty, gameScoreModel, this.GetCancellationTokenOnDestroy());

            await UniTask.Delay((int)(1f * 1000), cancellationToken: ct);

            // ���щ�ʋ�
            AudioManager.Instance.PlayBGMOnSelect(AudioManager.Instance.BGM_grade, 0f);

            SceneChangeAnimation.Instance.PlayOpenAnim();
            await UniTask.Delay((int)(0.5f * 1000), cancellationToken: ct);
        }

        // ���щ�ʂ̃{�^�����������Ƃ�
        private void OnGradeBackSelect()
        {
            AudioManager.Instance.ShotSE(AudioManager.Instance.SE_tap);
            GameSetting._isBackSelect = true;
            NoTapCanvas.Instance.NoTap1Second();
            
            BackSelectAsync(this.GetCancellationTokenOnDestroy()).Forget();
        }

        // Select�V�[���ɖ߂鏈��
        private async UniTaskVoid BackSelectAsync(CancellationToken ct)
        {
            
            // await UniTask.Delay((int)(0.2f * 1000), cancellationToken: ct);
            
            SceneChangeAnimation.Instance.PlayCloseAnim();

            await UniTask.Delay((int)(0.6f * 1000), cancellationToken: ct);
            
            //��asset�̃����[�X
            mapData.ReleaseItemMusic(this.GetCancellationTokenOnDestroy()).Forget();
            mapData.ReleaseItemSprite(this.GetCancellationTokenOnDestroy()).Forget();

            await UniTask.Delay((int)(0.4f * 1000), cancellationToken: ct);

            
            SceneManager.LoadScene(Constant.SceneName.SELECT);
        }


        //�I�[�f�B�I�@�킪�ύX���ꂽ���ɌĂ΂��
        void OnAudioConfigurationChanged(bool deviceWasChanged)
        {
            //�Q�[�����Ȃ烁�j���[��ʂ��J��
            if (isGame)
            {
                OnGameMenu();
            }
        }
    }

}