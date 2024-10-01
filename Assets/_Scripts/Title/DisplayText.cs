using System.Collections;
using System.Collections.Generic;
using StellaCircles.Data;
using StellaCircles.Localize;
using UnityEngine;
using UnityEngine.UI;

namespace StellaCircles.Title
{
    public class DisplayText : MonoBehaviour
    {
        [SerializeField] Text contextText;

        [SerializeField] string[] contexts;
        [SerializeField] string[] contextsEN;

        public int displayCount;

        public void DisplayContextText()
        {
            contextText.text = contexts[displayCount % contexts.Length];

            if (GameSetting.gameSettingValue.languageType == LanguageType.JP)
            {
                contextText.text = contexts[displayCount % contexts.Length];
            }
            else if (GameSetting.gameSettingValue.languageType == LanguageType.EN)
            {
                contextText.text = contextsEN[displayCount % contexts.Length];
            }



            displayCount = (displayCount + 1) % contexts.Length;
        }

        public void CountReset()
        {
            displayCount = 0;
        }
    }
}