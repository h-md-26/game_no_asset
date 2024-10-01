using StellaCircles.Data;
using StellaCircles.Localize;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace StellaCircles.Title
{
    public class SettingPanelManager : MonoBehaviour
    {
        // TitleManager����SettingButton���̏����𕪗�
        [SerializeField] AudioSettingPanelManager aspm;
        [SerializeField] GameObject settingPanel;

        //Setting����UI
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

        //Color �����static�Ɉڂ���������������
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
            
            Debug.Log("Setting�̃{�^���o�^����");
            
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

        //SettingPanel�����Ƃ�
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

        //Setting��ʂ̃{�^��
        private void OnAudioSettingButton()
        {
            // AudioSetting���J�����Ƃ�TitleManager�ɒʒm
            AudioManager.Instance.ShotSE(AudioManager.Instance.SE_tap);

            CloseSettingPanel();
            aspm.OpenAudioSettingPanel();
        }

        private void OnFrameButton()
        {
            AudioManager.Instance.ShotSE(AudioManager.Instance.SE_tap);

            //�t���[�����[�glist��index��؂�ւ������
            int c_framerateIndex = GameSetting.gameSettingValue.flamelateIndex;

            //Array��1�i�߂�
            c_framerateIndex = (c_framerateIndex + 1) % framerateArray.Length;

            //���̃t���[�����[�g�ŃZ�b�g
            SetFramerateButton(c_framerateIndex);
        }

        private void OnEffectButton()
        {
            AudioManager.Instance.ShotSE(AudioManager.Instance.SE_tap);

            bool currentIsEffect = GameSetting.gameSettingValue.isEffect;

            //���]���ăZ�b�g
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
                resetButtonText.text = "���Z�b�g���܂�";
                if (GameSetting.gameSettingValue.languageType == LanguageType.JP)
                {
                    resetButtonText.text = "���Z�b�g���܂�";
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
            resetButtonText.text = $"{5 - resetCount}��^�b�v";

            if (GameSetting.gameSettingValue.languageType == LanguageType.JP)
            {
                resetButtonText.text = $"{5 - resetCount}��^�b�v";
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
                effectBtnText.text = "����";

                if (GameSetting.gameSettingValue.languageType == LanguageType.JP)
                {
                    effectBtnText.text = "����";
                }
                else if (GameSetting.gameSettingValue.languageType == LanguageType.EN)
                {
                    effectBtnText.text = "ON";
                }

                Debug.Log("Effect Setting : Effect");
            }
            else
            {
                effectBtnText.text = "�Ȃ�";

                if (GameSetting.gameSettingValue.languageType == LanguageType.JP)
                {
                    effectBtnText.text = "�Ȃ�";
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