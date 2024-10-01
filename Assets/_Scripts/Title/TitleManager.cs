using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using StellaCircles.Data;
using StellaCircles.Localize;
using StellaCircles.ViewUtils;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


namespace StellaCircles.Title
{
    public class TitleManager : MonoBehaviour
    {
        [SerializeField] AudioClip BGM_Thema;

        [SerializeField] LanguageButton languageButton;
        [SerializeField] Localizer localizer;
        [SerializeField] SettingPanelManager settingPanelManager;
        [SerializeField] AudioSettingPanelManager audioSettingPanelManager;

        [SerializeField] private Button startButton;
        
        void Start()
        {
            startButton.onClick.AddListener(OnStartButton);
            TitleStartLoad(this.GetCancellationTokenOnDestroy()).Forget();
        }

        private async UniTaskVoid TitleStartLoad(CancellationToken ct)
        {
            Debug.Log("読み込み中");

            await UniTask.Yield(ct);

            MapDataBase.Instance.CreateMapDataDict();

            //設定の読み込み

            GameSetting._isBackSelect = false;
            GameSetting.gameSettingValue = SaveManager.LoadGameSetting();
            languageButton.SetLangButton();
            localizer.AllLocalize();
            settingPanelManager.SetSettingValue();
            audioSettingPanelManager.SetAudioSettingValue();

            // MapDataBaseにデータをロードする
            MapDataBase.Instance.GradeDataLoad();

            Debug.Log("読み込み完了");

            AudioManager.Instance.PlayBGMOnSelect(BGM_Thema, 0f);

            //SettingPanel等の非アクティブ化は各クラスのStartで行う
            
            SceneChangeAnimation.Instance.PlayOpenAnim();
        }


        //Title画面のボタン
        private void OnStartButton()
        {
            Debug.Log("Start");
            AudioManager.Instance.ShotSE(AudioManager.Instance.SE_gameStart);
            NoTapCanvas.Instance.NoTap1Second();
            GoSelect(this.GetCancellationTokenOnDestroy()).Forget();
        }

        private async UniTaskVoid GoSelect(CancellationToken ct)
        {
            await UniTask.Delay((int)(0.2f * 1000), cancellationToken: ct);
            SceneChangeAnimation.Instance.PlayCloseAnim();

            await UniTask.Delay((int)(0.6f * 1000), cancellationToken: ct);

            SceneManager.LoadScene(Constant.SceneName.SELECT);
        }

        //Title右上ボタン
        //未実装
        private void OnShareButton()
        {
            Debug.Log("Share");
            AudioManager.Instance.ShotSE(AudioManager.Instance.SE_tap);

        }

    }
}