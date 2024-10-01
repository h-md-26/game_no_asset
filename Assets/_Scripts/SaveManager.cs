using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CI.QuickSave;
using CI.QuickSave.Core.Storage;
using System;
using UnityEngine.Events;


namespace StellaCircles.Data
{
    public static class SaveManager
    {
        static QuickSaveWriter gradeWriter;
        static QuickSaveReader gradeReader;
        static QuickSaveWriter settingWriter;
        static QuickSaveReader settingReader;
        static QuickSaveSettings qsSettings;

        // callback
        static UnityAction onCompleteLoad;
        static UnityAction onCompleteSave;

        static SaveManager()
        {
            //QuickSaveGlobalSettings.StorageLocation = Application.dataPath;
            QuickSaveGlobalSettings.StorageLocation = Application.persistentDataPath;
            Debug.Log("保存場所：" + QuickSaveGlobalSettings.StorageLocation);

            // QuickSaveSettingsのインスタンスを作成
            qsSettings = new QuickSaveSettings();
            // 暗号化の方法 
            qsSettings.SecurityMode = SecurityMode.Aes;
            // Aesの暗号化キー
            qsSettings.Password = "awCharge89JudgmentBurst";
            // 圧縮の方法
            qsSettings.CompressionMode = CompressionMode.Gzip;

            // QuickSaveWriterのインスタンスを作成(Save)
            //gradeWriter = QuickSaveWriter.Create("GradeData",qsSettings);
            gradeWriter = QuickSaveWriter.Create("GradeData");
            settingWriter = QuickSaveWriter.Create("GameSetting");
            // ファイル生成用
            gradeWriter.Write("Error", 0);
            gradeWriter.Commit();
            settingWriter.Write("Error", 0);
            settingWriter.Commit();

            // QuickSaveReaderのインスタンスを作成(Load)
            //gradeReader = QuickSaveReader.Create("GradeData", qsSettings);
            gradeReader = QuickSaveReader.Create("GradeData");
            settingReader = QuickSaveReader.Create("GameSetting");

            onCompleteLoad = OnCompleteLoad;
            onCompleteSave = OnCompleteSave;

        }

        public static void SaveGradeData(string keyID, MapGradeData gradeData)
        {
            Debug.Log("セーブ開始:" + Time.time);

            Debug.Log("保存場所：" + QuickSaveGlobalSettings.StorageLocation);

            // QuickSaveWriterのインスタンスを作成(Save)
            //gradeWriter = QuickSaveWriter.Create("GradeData", qsSettings);
            gradeWriter = QuickSaveWriter.Create("GradeData");

            // データを書き込む
            gradeWriter.Write(keyID, gradeData);
            // 変更を反映
            gradeWriter.Commit();

            onCompleteSave?.Invoke();
        }

        public static MapGradeData LoadGradeData(string keyID)
        {
            //Debug.Log("ロード開始:" + Time.time);

            // QuickSaveReaderのインスタンスを作成(Load)
            //gradeReader = QuickSaveReader.Create("GradeData", qsSettings);
            gradeReader = QuickSaveReader.Create("GradeData");

            // リーダーのリロード
            gradeReader.Reload();
            /*
            // ファイルが見つからない場合無視
            if (FileAccess.Exists("GradeData", false))
            {
                Debug.Log("ファイルが見つかりません");
                var d = new MapGradeData();
                d.GradeInitialize();
                return d;
            }
            */

            MapGradeData gradeData;
            if (gradeReader.TryRead(keyID, out MapGradeData data))
            {
                // データを読み込む
                gradeData = gradeReader.Read<MapGradeData>(keyID);
            }
            else
            {
                // データが存在しないので初期化する
                Debug.Log("データが存在しないため、初期化する");
                var d = new MapGradeData();
                d.GradeInitialize();
                SaveGradeData(keyID, d);
                gradeData = d;
            }

            onCompleteLoad?.Invoke();
            return gradeData;
        }

        public static void SaveGameSetting(GameSettingValue gsv)
        {
            settingWriter = QuickSaveWriter.Create("GameSetting");

            // データを書き込む
            settingWriter.Write("GameSetting", gsv);
            // 変更を反映
            settingWriter.Commit();

            onCompleteSave?.Invoke();
        }

        public static GameSettingValue LoadGameSetting()
        {
            // QuickSaveReaderのインスタンスを作成(Load)
            settingReader = QuickSaveReader.Create("GameSetting");

            // リーダーのリロード
            settingReader.Reload();

            GameSettingValue gsv;
            if (settingReader.TryRead("GameSetting", out GameSettingValue data))
            {
                // データを読み込む
                gsv = settingReader.Read<GameSettingValue>("GameSetting");
            }
            else
            {
                // データが存在しないので初期化する
                Debug.Log("設定データが存在しないため、初期化する");
                gsv = new();
                gsv.ValueInitialize();

                SaveGameSetting(gsv);
            }

            onCompleteLoad?.Invoke();
            return GSVUtility.SetNullGSV(gsv);
        }


        public static GameSettingValue LoadGameSetting(Action[] callbacks)
        {
            var gsv = LoadGameSetting();

            Debug.Log("指定のコールバックを実行");
            foreach (var c in callbacks)
            {
                c.Invoke();
            }

            return gsv;
        }


        private static void OnCompleteLoad()
        {
            //Debug.Log("ロード完了:" + Time.time);
        }

        private static void OnCompleteSave()
        {
            Debug.Log("セーブ完了:" + Time.time);
        }
    }
}