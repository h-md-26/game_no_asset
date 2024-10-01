using System;
using System.Collections;
using System.Collections.Generic;
using StellaCircles.Data;
using StellaCircles.Localize;
using UnityEngine;
using UnityEngine.UI;

namespace StellaCircles.Title
{
    public class LanguageButton : MonoBehaviour
    {
        [SerializeField] Button langButton;
        [SerializeField] Text langText;
        [SerializeField] Localizer localizer;

        private void Start()
        {
            langButton.onClick.AddListener(OnLanguageButton);
        }

        public void SetLangButton()
        {
            if (GameSetting.gameSettingValue.languageType == LanguageType.JP)
            {
                langText.text = "JP";

            }
            else if (GameSetting.gameSettingValue.languageType == LanguageType.EN)
            {
                langText.text = "EN";
            }
        }

        public void OnLanguageButton()
        {
            if (GameSetting.gameSettingValue.languageType == LanguageType.JP)
            {
                GameSetting.gameSettingValue.languageType = LanguageType.EN;
            }
            else if (GameSetting.gameSettingValue.languageType == LanguageType.EN)
            {
                GameSetting.gameSettingValue.languageType = LanguageType.JP;
            }
            else
            {
                GameSetting.gameSettingValue.languageType = LanguageType.JP;
            }

            SetLangButton();
            localizer.AllLocalize();
            SaveManager.SaveGameSetting(GameSetting.gameSettingValue);
            AudioManager.Instance.ShotSE(AudioManager.Instance.SE_tap);
        }
    }
}
