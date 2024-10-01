using System;
using StellaCircles.Data;
using StellaCircles.Localize;
using UnityEngine;
using UnityEngine.UI;


namespace StellaCircles.Title
{
    public class AudioSettingPanelManager : MonoBehaviour
    {
        public Action onAudioOKButton;

        [SerializeField] GameObject audioSettingPanel;

        // [SerializeField] private Button audioSettingButton;
        [SerializeField] private Button audioOKButton;
        [SerializeField] private Button audioDefaultButton;
        [SerializeField] private Button audioTestButton;
        [SerializeField] private Button seTimingButton;
        
        //AudioSetting内のUI
        [SerializeField] Text seTimingText;
        [SerializeField] Slider masterSlider;
        [SerializeField] Slider bgmSlider;
        [SerializeField] Slider seSlider;

        private void Start()
        {
            // audioSettingButton.onClick.AddListener(OnAudioSettingButton);
            audioOKButton.onClick.AddListener(OnAudioOKButton);
            audioDefaultButton.onClick.AddListener(OnAudioDefaultButton);
            audioTestButton.onClick.AddListener(OnAudioTestButton);
            seTimingButton.onClick.AddListener(OnSeTimingButton);
            
            masterSlider.onValueChanged.AddListener(OnMasterSlider);
            bgmSlider.onValueChanged.AddListener(OnBGMSlider);
            seSlider.onValueChanged.AddListener(OnSESlider);
            
            Debug.Log("Audioのボタン登録完了");
            
            audioSettingPanel.SetActive(false);
        }

        // SettingPanelの方に書く
        // private void OnAudioSettingButton()
        // {
        //     Debug.Log("AudioSetting");
        //     AudioManager.Instance.ShotSE(AudioManager.Instance.SE_tap);
        //
        //     OpenAudioSettingPanel();
        // }

        //AudioPanelを開くとき
        public void OpenAudioSettingPanel()
        {
            SetAudioSettingValue();

            audioSettingPanel.SetActive(true);
        }

        public void SetAudioSettingValue()
        {
            SetSeTiming(GameSetting.gameSettingValue.isSETimingOperation);

            AudioManager.Instance.audioMixer.SetFloat("Master", GameSetting.gameSettingValue.masterVol);
            masterSlider.value = GameSetting.gameSettingValue.masterVol;

            AudioManager.Instance.audioMixer.SetFloat("BGM", GameSetting.gameSettingValue.bgmVol);
            bgmSlider.value = GameSetting.gameSettingValue.bgmVol;

            AudioManager.Instance.audioMixer.SetFloat("SE", GameSetting.gameSettingValue.seVol);
            seSlider.value = GameSetting.gameSettingValue.seVol;
        }

        //AudioPanelを閉じるとき(閉じる方法がボタンを押すのみなのでくっつけている)
        void CloseAudioSettingPanel()
        {
            Debug.Log("CompleteAudioSetting");
            AudioManager.Instance.ShotSE(AudioManager.Instance.SE_tap);
            audioSettingPanel.SetActive(false);
        }


        //AudioSetting画面のボタン
        private void OnAudioOKButton()
        {
            CloseAudioSettingPanel();

            //他クラス(SettingPanelManager)のAudioPanelを閉じた時の処理を実行
            onAudioOKButton.Invoke();
        }

        private void OnAudioDefaultButton()
        {
            Debug.Log("Audio:Default");
            AudioManager.Instance.ShotSE(AudioManager.Instance.SE_tap);

            float defaultMaster = 0;
            GameSetting.gameSettingValue.masterVol = defaultMaster;
            AudioManager.Instance.audioMixer.SetFloat("Master", defaultMaster);
            masterSlider.value = defaultMaster;

            float defaultBGM = -5;
            GameSetting.gameSettingValue.bgmVol = defaultBGM;
            AudioManager.Instance.audioMixer.SetFloat("BGM", defaultBGM);
            bgmSlider.value = defaultBGM;

            float defaultSE = -10;
            GameSetting.gameSettingValue.seVol = defaultSE;
            AudioManager.Instance.audioMixer.SetFloat("SE", defaultSE);
            seSlider.value = defaultSE;
        }

        private void OnAudioTestButton()
        {
            Debug.Log("Audio Test");
            AudioManager.Instance.ShotSE(AudioManager.Instance.ShotRandomSE());
        }

        private void OnSeTimingButton()
        {
            bool isSeTiming = GameSetting.gameSettingValue.isSETimingOperation;

            AudioManager.Instance.ShotSE(AudioManager.Instance.SE_tap);

            //現在と逆のSE方式をセット
            SetSeTiming(!isSeTiming);
        }

        //音量スライダー操作
        private void OnMasterSlider(float masterValue)
        {
            GameSetting.gameSettingValue.masterVol = masterValue;
            AudioManager.Instance.audioMixer.SetFloat("Master", masterValue);
        }

        private void OnBGMSlider(float bgmValue)
        {
            GameSetting.gameSettingValue.bgmVol = bgmValue;
            AudioManager.Instance.audioMixer.SetFloat("BGM", bgmValue);
        }

        private void OnSESlider(float seValue)
        {
            GameSetting.gameSettingValue.seVol = seValue;
            AudioManager.Instance.audioMixer.SetFloat("SE", seValue);
        }


        void SetSeTiming(bool isOperation)
        {
            GameSetting.gameSettingValue.isSETimingOperation = isOperation;
            if (isOperation)
            {
                seTimingText.text = "操作時";
                if (GameSetting.gameSettingValue.languageType == LanguageType.JP)
                {
                    seTimingText.text = "操作時";
                }
                else if (GameSetting.gameSettingValue.languageType == LanguageType.EN)
                {
                    seTimingText.text = "Operation";
                }

                Debug.Log("SETimng Setting : Operation");
            }
            else
            {
                seTimingText.text = "判定時";
                if (GameSetting.gameSettingValue.languageType == LanguageType.JP)
                {
                    seTimingText.text = "判定時";
                }
                else if (GameSetting.gameSettingValue.languageType == LanguageType.EN)
                {
                    seTimingText.text = "Decision";
                }

                Debug.Log("SETimng Setting : Judgement");
            }
        }
    }
}