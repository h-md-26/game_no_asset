using System.Collections;
using System.Collections.Generic;
using StellaCircles.Localize;
using UnityEngine;


namespace StellaCircles.Data
{
    public static class GameSetting
    {
        static GameSetting()
        {
            _selectAgeIndex = 0;
            _selectMapIndex = new int[] { 0, 0, 0, 0, 0 };
            _selectDifficultIndex = 0;

            _isBackSelect = false;
        }
        
        public static GameSettingValue gameSettingValue;

        // 選択番号を保存
        public static int _selectAgeIndex;
        public static int[] _selectMapIndex;
        public static int _selectDifficultIndex;

        // GameSceneから戻ってきたか
        public static bool _isBackSelect;
    }

    public class GameSettingValue
    {
        /*
        // 選択番号を保存
        public int _selectAgeIndex;
        public int[] _selectMapIndex;
        public int _selectDifficultIndex;

        // GameSceneから戻ってきたか
        public bool _isBackSelect;
        */

        // 設定
        public float masterVol = 0;
        public float bgmVol = -5;
        public float seVol = -10;
        public bool isSETimingOperation = true;
        public int flamelate = 60;
        public int flamelateIndex = 2;
        public bool isEffect = true;
        public float gosaOffset = 0;
        public int frickSensitivity = 500;
        public LanguageType languageType = LanguageType.JP;

        public void ValueInitialize()
        {
            Debug.Log("設定の初期化");

            //_selectAgeIndex = 0;
            //_selectMapIndex = new int[] { 0, 0, 0, 0, 0 };
            //_selectDifficultIndex = 0;

            //_isBackSelect = false;

            masterVol = 0;
            bgmVol = -5;
            seVol = -10;
            isSETimingOperation = true;
            flamelate = 60;
            flamelateIndex = 2;
            isEffect = true;
            gosaOffset = 0;
            frickSensitivity = 500;
            languageType = LanguageType.JP;
        }

    }

    /// <summary>
    /// GameSettingValueについての関数をまとめるクラス
    /// </summary>
    public static class GSVUtility
    {
        /// <summary>
        /// gsv がnullのとき、デフォルト値をセットする
        /// </summary>
        /// <param name="gsv"></param>
        /// <returns></returns>
        public static GameSettingValue SetNullGSV(GameSettingValue gsv)
        {
            if (gsv == null)
            {
                gsv = new();
            }

            if (gsv.masterVol < -80 || gsv.masterVol > 0)
            {
                gsv.masterVol = 0;
            }

            if (gsv.bgmVol < -80 || gsv.bgmVol > 0)
            {
                gsv.bgmVol = -5;
            }

            if (gsv.seVol < -80 || gsv.seVol > 0)
            {
                gsv.seVol = -10;
            }

            if (gsv.flamelate < 10 || gsv.flamelate > 200)
            {
                gsv.flamelate = 60;
                gsv.flamelateIndex = 2;
            }

            if (gsv.gosaOffset < -20 || gsv.gosaOffset > 20)
            {
                gsv.gosaOffset = 0;
            }

            if (gsv.frickSensitivity <= 0 || gsv.frickSensitivity > 1000)
            {
                gsv.frickSensitivity = 500;
            }

            return gsv;
        }
    }
}