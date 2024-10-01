using System.Collections;
using System.Collections.Generic;
using StellaCircles.Data;
using StellaCircles.Localize;
using UnityEngine;

namespace StellaCircles.Title
{
    public class HelpPanel : MonoBehaviour
    {
        [SerializeField] string pageTheme;
        [SerializeField] string pageThemeEN;


        public string GetLangText()
        {
            if (GameSetting.gameSettingValue.languageType == LanguageType.JP)
            {
                return pageTheme;
            }
            else if (GameSetting.gameSettingValue.languageType == LanguageType.EN)
            {
                return pageThemeEN;
            }


            return pageTheme;
        }
    }
}