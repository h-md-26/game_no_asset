using System.Collections;
using System.Collections.Generic;
using StellaCircles.Data;
using UnityEngine;
using UnityEngine.UI;

namespace StellaCircles.Localize
{
    public class LocalizeText : MonoBehaviour
    {
        [TextArea] [SerializeField] string jpStatement;
        [TextArea] [SerializeField] string enStatement;

        [SerializeField] Text text;


        public void Localize()
        {
            if (GameSetting.gameSettingValue.languageType == LanguageType.JP)
            {
                if (text != null)
                {
                    text.text = jpStatement;

                }
            }
            else if (GameSetting.gameSettingValue.languageType == LanguageType.EN)
            {
                if (text != null)
                {
                    text.text = enStatement;

                }
            }
        }
    }

    public enum LanguageType
    {
        JP,
        EN
    }
}