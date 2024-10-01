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
            Debug.Log("�ۑ��ꏊ�F" + QuickSaveGlobalSettings.StorageLocation);

            // QuickSaveSettings�̃C���X�^���X���쐬
            qsSettings = new QuickSaveSettings();
            // �Í����̕��@ 
            qsSettings.SecurityMode = SecurityMode.Aes;
            // Aes�̈Í����L�[
            qsSettings.Password = "awCharge89JudgmentBurst";
            // ���k�̕��@
            qsSettings.CompressionMode = CompressionMode.Gzip;

            // QuickSaveWriter�̃C���X�^���X���쐬(Save)
            //gradeWriter = QuickSaveWriter.Create("GradeData",qsSettings);
            gradeWriter = QuickSaveWriter.Create("GradeData");
            settingWriter = QuickSaveWriter.Create("GameSetting");
            // �t�@�C�������p
            gradeWriter.Write("Error", 0);
            gradeWriter.Commit();
            settingWriter.Write("Error", 0);
            settingWriter.Commit();

            // QuickSaveReader�̃C���X�^���X���쐬(Load)
            //gradeReader = QuickSaveReader.Create("GradeData", qsSettings);
            gradeReader = QuickSaveReader.Create("GradeData");
            settingReader = QuickSaveReader.Create("GameSetting");

            onCompleteLoad = OnCompleteLoad;
            onCompleteSave = OnCompleteSave;

        }

        public static void SaveGradeData(string keyID, MapGradeData gradeData)
        {
            Debug.Log("�Z�[�u�J�n:" + Time.time);

            Debug.Log("�ۑ��ꏊ�F" + QuickSaveGlobalSettings.StorageLocation);

            // QuickSaveWriter�̃C���X�^���X���쐬(Save)
            //gradeWriter = QuickSaveWriter.Create("GradeData", qsSettings);
            gradeWriter = QuickSaveWriter.Create("GradeData");

            // �f�[�^����������
            gradeWriter.Write(keyID, gradeData);
            // �ύX�𔽉f
            gradeWriter.Commit();

            onCompleteSave?.Invoke();
        }

        public static MapGradeData LoadGradeData(string keyID)
        {
            //Debug.Log("���[�h�J�n:" + Time.time);

            // QuickSaveReader�̃C���X�^���X���쐬(Load)
            //gradeReader = QuickSaveReader.Create("GradeData", qsSettings);
            gradeReader = QuickSaveReader.Create("GradeData");

            // ���[�_�[�̃����[�h
            gradeReader.Reload();
            /*
            // �t�@�C����������Ȃ��ꍇ����
            if (FileAccess.Exists("GradeData", false))
            {
                Debug.Log("�t�@�C����������܂���");
                var d = new MapGradeData();
                d.GradeInitialize();
                return d;
            }
            */

            MapGradeData gradeData;
            if (gradeReader.TryRead(keyID, out MapGradeData data))
            {
                // �f�[�^��ǂݍ���
                gradeData = gradeReader.Read<MapGradeData>(keyID);
            }
            else
            {
                // �f�[�^�����݂��Ȃ��̂ŏ���������
                Debug.Log("�f�[�^�����݂��Ȃ����߁A����������");
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

            // �f�[�^����������
            settingWriter.Write("GameSetting", gsv);
            // �ύX�𔽉f
            settingWriter.Commit();

            onCompleteSave?.Invoke();
        }

        public static GameSettingValue LoadGameSetting()
        {
            // QuickSaveReader�̃C���X�^���X���쐬(Load)
            settingReader = QuickSaveReader.Create("GameSetting");

            // ���[�_�[�̃����[�h
            settingReader.Reload();

            GameSettingValue gsv;
            if (settingReader.TryRead("GameSetting", out GameSettingValue data))
            {
                // �f�[�^��ǂݍ���
                gsv = settingReader.Read<GameSettingValue>("GameSetting");
            }
            else
            {
                // �f�[�^�����݂��Ȃ��̂ŏ���������
                Debug.Log("�ݒ�f�[�^�����݂��Ȃ����߁A����������");
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

            Debug.Log("�w��̃R�[���o�b�N�����s");
            foreach (var c in callbacks)
            {
                c.Invoke();
            }

            return gsv;
        }


        private static void OnCompleteLoad()
        {
            //Debug.Log("���[�h����:" + Time.time);
        }

        private static void OnCompleteSave()
        {
            Debug.Log("�Z�[�u����:" + Time.time);
        }
    }
}