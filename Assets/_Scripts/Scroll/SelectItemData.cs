using System.Collections;
using System.Collections.Generic;
using StellaCircles.Localize;
using UnityEngine;

namespace StellaCircles.Data
{
    [CreateAssetMenu]
    public class SelectItemData : ScriptableObject
    {
        public int itemId;
        public string itemName;
        public string itemNameEN;
        public string itemGenre;

        public string itemGenreEN;

        //public Sprite itemSprite;
        //public AudioClip itemMusic;
        public float itemDemoTime;
        public int[] mapLevels = new int[2];
        public float itemBPM;
        public float itemOffset;
        public TextAsset[] itemMaps;
        public string itemDate;
        public UnlockWay itemUnLockWay;
        [TextArea] public string itemUnLockWayText;
        [TextArea] public string itemUnLockWayTextEN;
        public Age itemAge;
        //public MapGradeData gradeData;

        public MapGradeData GetGradeData()
        {
            return MapDataBase.Instance.mapDict[itemId].gradeData;
        }

        public string GetItemName()
        {
            if (GameSetting.gameSettingValue.languageType == LanguageType.JP)
            {
                return itemName;
            }
            else if (GameSetting.gameSettingValue.languageType == LanguageType.EN)
            {
                return itemNameEN;
            }
            else
            {
                return itemName;
            }
        }

        public string GetItemGenre()
        {
            if (GameSetting.gameSettingValue.languageType == LanguageType.JP)
            {
                return itemGenre;
            }
            else if (GameSetting.gameSettingValue.languageType == LanguageType.EN)
            {
                return itemGenreEN;
            }
            else
            {
                return itemGenre;
            }
        }

        public string GetUnLockWayText()
        {
            if (GameSetting.gameSettingValue.languageType == LanguageType.JP)
            {
                return itemUnLockWayText;
            }
            else if (GameSetting.gameSettingValue.languageType == LanguageType.EN)
            {
                return itemUnLockWayTextEN;
            }
            else
            {
                return itemUnLockWayText;
            }
        }
    }

    public enum Age
    {
        _2019 = 0,
        _2020 = 1,
        _2021 = 2,
        _2022 = 3,
        _2023 = 4
    }
}