using StellaCircles.Data;
using StellaCircles.Localize;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace StellaCircles.Title
{
    public class SettingPanelManager : MonoBehaviour
    {
        // TitleManagerからSettingButton内の処理を分離
        [SerializeField] AudioSettingPanelManager aspm;
        [SerializeField] GameObject settingPanel;

        //Setting内のUI
        [SerializeField] private Button settingButton;
        [SerializeField] private Button settingCloseButton;
        [SerializeField] private Button audioSettingButton;
        [SerializeField] private Button frameButton;
        [SerializeField] private Button effectButton;
        [SerializeField] private Button resetButton;
        [SerializeField] Slider offsetSlider;
        [SerializeField] Slider flickSlider;
        
        [SerializeField] Text framerateBtnText;
        [SerializeField] Text effectBtnText;
        [SerializeField] Text offsetSliderText;
        [SerializeField] Text flickSliderText;
        [SerializeField] Image resetButtonImage;
        [SerializeField] Text resetButtonText;

        //Color これはstaticに移した方がいいかも
        [SerializeField] Color[] resetColors;

        readonly int[] framerateArray = { 15, 30, 60, 120 };


        private void Start()
        {
            settingButton.onClick.AddListener(OnSettingButton);
            settingCloseButton.onClick.AddListener(OnSettingCloseButton);
            audioSettingButton.onClick.AddListener(OnAudioSettingButton);
            frameButton.onClick.AddListener(OnFrameButton);
            effectButton.onClick.AddListener(OnEffectButton);
            resetButton.onClick.AddListener(OnResetButton);
            offsetSlider.onValueChanged.AddListener(OnOffsetSlider);
            flickSlider.onValueChanged.AddListener(OnFlickSlider);
            
            Debug.Log("Settingのボタン登録完了");
            
            aspm.onAudioOKButton += OpenSettingPanel;

            settingPanel.SetActive(false);
        }

        private void OnSettingButton()
        {
            Debug.Log("Setting");
            AudioManager.Instance.ShotSE(AudioManager.Instance.SE_tap);
            OpenSettingPanel();
        }

        private void OpenSettingPanel()
        {
            SetSettingValue();
            settingPanel.SetActive(true);
        }

        public void SetSettingValue()
        {
            offsetSlider.value = GameSetting.gameSettingValue.gosaOffset;
            offsetSliderText.text = $"{GameSetting.gameSettingValue.gosaOffset}ms";
            //var fs = 9 * (100 - v) + 100;
            /*
             * fs-100 = 9(100-v)
             * (fs-100)/9 = 100-v
             * v = 100 - (fs-100)/9
             */
            var v = 100 - (GameSetting.gameSettingValue.frickSensitivity - 100) / 9;
            flickSlider.value = v;
            flickSliderText.text = v.ToString();
            SetEffectButton(GameSetting.gameSettingValue.isEffect);
            SetFramerateButton(GameSetting.gameSettingValue.flamelateIndex);
            resetCount = 0;
            SetResetButton();
        }

        //SettingPanelを閉じるとき
        void CloseSettingPanel()
        {
            Debug.Log("Close Setting");
            settingPanel.SetActive(false);
        }

        private void OnSettingCloseButton()
        {
            AudioManager.Instance.ShotSE(AudioManager.Instance.SE_tap);

            SaveManager.SaveGameSetting(GameSetting.gameSettingValue);

            CloseSettingPanel();
        }

        //Setting画面のボタン
        private void OnAudioSettingButton()
        {
            // AudioSettingを開くことをTitleManagerに通知
            AudioManager.Instance.ShotSE(AudioManager.Instance.SE_tap);

            CloseSettingPanel();
            aspm.OpenAudioSettingPanel();
        }

        private void OnFrameButton()
        {
            AudioManager.Instance.ShotSE(AudioManager.Instance.SE_tap);

            //フレームレートlistのindexを切り替える方式
            int c_framerateIndex = GameSetting.gameSettingValue.flamelateIndex;

            //Arrayを1つ進める
            c_framerateIndex = (c_framerateIndex + 1) % framerateArray.Length;

            //このフレームレートでセット
            SetFramerateButton(c_framerateIndex);
        }

        private void OnEffectButton()
        {
            AudioManager.Instance.ShotSE(AudioManager.Instance.SE_tap);

            bool currentIsEffect = GameSetting.gameSettingValue.isEffect;

            //反転してセット
            SetEffectButton(!currentIsEffect);
        }

        private void OnOffsetSlider(float offsetValue)
        {
            Debug.Log("OnOffsetSlider ValueChanged");
            
            var flooredValue = Mathf.FloorToInt(offsetValue);
            GameSetting.gameSettingValue.gosaOffset = flooredValue;
            offsetSlider.value = flooredValue;
            offsetSliderText.text = $"{flooredValue}ms";
            Debug.Log($"OffsetSlider : {flooredValue}");
        }

        private void OnFlickSlider(float flickValue)
        {
            var flooredValue = Mathf.FloorToInt(flickValue);
            //0~100 -> 1000~500~100  
            // 100 - v = 100~0
            // (100 - v)+100 = 200~100
            // 9(100 - v)+100 = 1000~100
            var fs = 9 * (100 - flooredValue) + 100;
            GameSetting.gameSettingValue.frickSensitivity = fs;
            flickSlider.value = flooredValue;
            flickSliderText.text = $"{flooredValue}";
            Debug.Log($"FlickSlider : {flooredValue} => FS : {fs}");
        }

        int resetCount;

        private void OnResetButton()
        {
            AudioManager.Instance.ShotSE(AudioManager.Instance.SE_tap);
            resetCount++;
            if (resetCount >= 5)
            {
                resetCount = 0;
                resetButtonImage.color = resetColors[resetCount];
                resetButtonText.color = resetColors[resetCount];
                resetButtonText.text = "リセットします";
                if (GameSetting.gameSettingValue.languageType == LanguageType.JP)
                {
                    resetButtonText.text = "リセットします";
                }
                else if (GameSetting.gameSettingValue.languageType == LanguageType.EN)
                {
                    resetButtonText.text = "Reset!!";
                }

                Debug.Log("Reset");
                MapDataBase.Instance.ResetGradeData();
            }
            else
            {
                SetResetButton();
            }
        }

        void SetResetButton()
        {
            resetButtonImage.color = resetColors[resetCount];
            resetButtonText.color = resetColors[resetCount];
            resetButtonText.text = $"{5 - resetCount}回タップ";

            if (GameSetting.gameSettingValue.languageType == LanguageType.JP)
            {
                resetButtonText.text = $"{5 - resetCount}回タップ";
            }
            else if (GameSetting.gameSettingValue.languageType == LanguageType.EN)
            {
                resetButtonText.text = $"Tap {5 - resetCount} times";
            }
        }

        void SetFramerateButton(int framerateIndex)
        {
            int framerate = framerateArray[framerateIndex];
            framerateBtnText.text = $"{framerate}fps";
            Debug.Log($"Framerate:{framerate}");

            Application.targetFrameRate = framerate;
            GameSetting.gameSettingValue.flamelateIndex = framerateIndex;
            GameSetting.gameSettingValue.flamelate = framerate;
        }

        void SetEffectButton(bool isEffect)
        {
            if (isEffect)
            {
                effectBtnText.text = "あり";

                if (GameSetting.gameSettingValue.languageType == LanguageType.JP)
                {
                    effectBtnText.text = "あり";
                }
                else if (GameSetting.gameSettingValue.languageType == LanguageType.EN)
                {
                    effectBtnText.text = "ON";
                }

                Debug.Log("Effect Setting : Effect");
            }
            else
            {
                effectBtnText.text = "なし";

                if (GameSetting.gameSettingValue.languageType == LanguageType.JP)
                {
                    effectBtnText.text = "なし";
                }
                else if (GameSetting.gameSettingValue.languageType == LanguageType.EN)
                {
                    effectBtnText.text = "OFF";
                }

                Debug.Log("Effect Setting : None Effect");
            }

            GameSetting.gameSettingValue.isEffect = isEffect;
        }

    }
}