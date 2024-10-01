using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using System.Text.RegularExpressions;
using StellaCircles.Utils;

namespace StellaCircles.AssetBundleManagement.Web
{
    public static class DownloadAsset
    {
        #region WebRequest

        /*
         * �A�v���N�����ɃT�[�o�[����
         *   �EAssetBundle�̖��O
         * �@�EAssetBundle�̍ŐV�o�[�W�������X�g
         * �@�EAssetBundle�̋��LURL���X�g
         * �@�̓��������X�g��JSON�t�@�C�����󂯎��
         *
         * Save���Ă���AssetBundle�̌��݂̃o�[�W�������X�g�����[�h����
         *
         * 2�̃o�[�W�������X�g���r���āA
         * �@�E�V�����o�[�W�����̂���AssetBundle
         * �@�E�V���ɒǉ����ꂽAssetBundle�@�@�@�@��URL���X�g���g���ă_�E�����[�h���A
         * �@�E�ŐV���X�g�ŏ�����AssetBundle�@�@�@���폜����
         *
         * �_�E�����[�h����AssetBundle��(bundleDirPath)�ɔz�u���A�Â��o�[�W������AssetBundle���폜����
         * �@���O�͂ǂ��Ȃ邩�m�F����
         *
         */

        public static async UniTask UpdateAB()
        {
            (Dictionary<string, ABInfo> rdict, Dictionary<string, ABInfo> ldict) = await GetBothABDict();
            await CompareABDict(rdict, ldict);
        }

        private static async UniTask CompareABDict(Dictionary<string, ABInfo> rdict, Dictionary<string, ABInfo> ldict)
        {
            /*
            rdict�̗v�fremote�ɑ΂��� ldict���� Key(fileName) ����v������̂�T��
            ��v������̂��������ꍇ�Aver���r���A
            ver�������ꍇ�A�������Ȃ�
            ver���قȂ�ꍇ�Aremote �� url ����ŐV��AssetBundle���擾����
            ��v������̂��Ȃ������ꍇ�Aremote �� url ����ŐV��AssetBundle���擾����

            ldict�ɂ�����rdict�ɂȂ����̂̏����͍��͍l���Ȃ����Ƃɂ���

             */
            List<(string name, int ver)> dlFileList = new();
            List<UniTask> dlTaskList = new();

            foreach (var r in rdict)
            {
                // rdict�Ɋ܂܂��Key�������Ă��Ȃ� or ���̃o�[�W�������قȂ�
                if (!ldict.ContainsKey(r.Key) || ldict[r.Key].ver != r.Value.ver)
                {
                    // r url ����ŐV�o�[�W������AssetBundle ���_�E�����[�h��(�����ł̓��������A��ł܂Ƃ߂ĕ��񏈗�����)�Aldict���X�V����
                    Debug.Log($"local��abdict�� {r.Key}���Ȃ� or Ver���Â�");
                    dlFileList.Add((r.Key, r.Value.ver));
                }
            }

            // dlFileList��Key�Ƃ��� rdict�̗v�f�̒��� url ����ŐV�o�[�W������AssetBundle ���_�E�����[�h(���񏈗�)
            foreach (var k in dlFileList)
            {
                var dlTask = WriteFileFromWebRequest(k.name, k.ver, GamePath.abApiUrl + k.name, GamePath.bundleDirPath);
                Debug.Log($"{GamePath.abApiUrl + k.name}���烊�N�G�X�g");
                dlTaskList.Add(dlTask);
            }

            Debug.Log("AB�_�E�����[�h/�������݊J�n");
            // �擾����DL�^�X�N���܂Ƃ߂Ď��s���A�S�ďI���܂ő҂�
            await UniTask.WhenAll(dlTaskList);
            Debug.Log("AB�_�E�����[�h/�������݊���");

            /*
             * �_�E�����[�h�������������m�F���A�����������̂�localabdict�ɏ�������
             * ���s�������̂��������Ƃ��A���s�������Ƃ�m�点�ďI������
             *
             * �����������ǂ����̊m�F
             * �@idea1�F�ۑ�����t�@�C�����Ƀo�[�W���������Ă����A�i$"{name}_{ver}"�j
             * �@       rdict�ƃt�@�C�����{�o�[�W��������v�����ꍇ�����Ƃ��A
             * �@       �Â��o�[�W����(ldict����擾)�̃t�@�C���͍폜����
             *
             * �����������̂�ldict�ɏ������ށA���s�������͎̂����rdict�Ƃ̔�r�ł�����x�_�E�����[�h�����
             *
             * ldict��json�ŕۑ�����
             */
            Dictionary<string, ABInfo> dldict = GetDictsFromFilesContainName_Ver(GamePath.bundleDirPath);
            if (dldict != null)
            {
                foreach (var r in rdict)
                {
                    //�_�E�����[�h����(rdict��dldict��name,ver����v)�����Ȃ�
                    if (dldict.ContainsKey(r.Key) && dldict[r.Key].ver == r.Value.ver)
                    {
                        //Debug.Log($"{GamePath.bundleDirPath}/{r.Key}_{r.Value.ver} �̃_�E�����[�h�ɐ���");
                        if (ldict.ContainsKey(r.Key))
                        {
                            //�Â��o�[�W����(ldict��ver����擾)���폜
                            //Debug.Log($"{GamePath.bundleDirPath}/{r.Key}_{ldict[r.Key].ver} ���폜");
                        }
                    }
                }
            }

            Debug.Log("abdict�㏑���J�n");
            // ldict��dldict�ŏ㏑���ۑ�����
            await ABDictJson.WriteABDictJson(dldict);
            Debug.Log("abdict�㏑������");

            //ABStockmanager�ɂ�����
            ABStockManager.abDict = dldict;
        }

        //��������abdict���W�߂�
        private static async UniTask<(Dictionary<string, ABInfo>, Dictionary<string, ABInfo>)> GetBothABDict()
        {
            var rtask = GetRemoteABDict();
            var ltask = GetLocalABDict();
            return await UniTask.WhenAll(rtask, ltask);
        }

        //�T�[�o�[����abdict���擾����
        private static async UniTask<Dictionary<string, ABInfo>> GetRemoteABDict()
        {
            var r = await ABDictJson.LoadRemoteABDictJson();
            Debug.Log("�T�[�o�[����abdict�����[�h");
            return r;
        }

        //���[�J������abdict���擾����
        private static async UniTask<Dictionary<string, ABInfo>> GetLocalABDict()
        {
            var r = await ABDictJson.LoadLocalABDictJson();
            Debug.Log("���[�J������abdict�����[�h");
            return r;
        }

        //�o�[�W�����̌Â��t�@�C���̍ŐV�o�[�W������URL����擾���A�㏑������
        //�ʐM�Ɏ��s������㏑�����Ȃ�
        private static async UniTask WriteFileFromWebRequest(string name, int ver, string url, string savePath)
        {
            // url ���� byte[] �ɂ��A savePath �� name �Ƃ��ď㏑��
            byte[] data = await WebUtilities.ByteFromWebRequest(url);
            if (data == default)
            {
                Debug.Log("�f�[�^���擾�ł��܂���ł���");
                //�f�[�^���擾�ł��Ȃ������Ƃ��A��̏����Ń_�E�����[�h�σ��X�g�ɓ���Ȃ����߁A���Ȃ�
            }
            else
            {
                await ABFileUtilities.WriteByteData(name, ver, data, savePath);
            }
        }

        //path�ɂ���t�@�C���̓��A"{name}_{ver}"�̌`�̂��̂�name��Key, ABInfo.ver��Value�ɂ��ĕԂ�
        private static Dictionary<string, ABInfo> GetDictsFromFilesContainName_Ver(string path)
        {
            return Name_VerStrings2Dicts(GetFilesName_Ver(path));
        }

        //$"{name}_{ver}"�̌`�ɂ����t�@�C�����𕪉�����
        //���O��SelectFilesContainTailAnderScoreAndNumber(string[] files)�ōi�������̂�n��
        private static Dictionary<string, ABInfo> Name_VerStrings2Dicts(string[] strings)
        {
            Dictionary<string, ABInfo> _dict = new();
            for (int i = 0; i < strings.Length; i++)
            {
                string[] array = strings[i].Split('_');
                string[] newArray = array.Take(array.Length - 1).ToArray();

                string _name = string.Join('_', newArray);
                ABInfo _abinfo = new(int.Parse(array[^1]));

                //Debug.Log($"{strings[i]} -> {_name} {array[^1]}");
                _dict.Add(_name, _abinfo);
            }

            return _dict;
        }

        //������ *****_1 �̂悤�ɂȂ����t�@�C���݂̂�Ԃ�
        private static string[] SelectFilesName_Ver(string[] files)
        {
            List<string> resultlist = new();
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                if (System.Text.RegularExpressions.Regex.IsMatch(fileName, @"_\d+$"))
                {
                    resultlist.Add(fileName);
                }
            }

            var resultarray = resultlist.ToArray();

            return resultarray;
        }

        // path����ǂ݂������t�@�C���̓��A������ *****_1 �̂悤�ɂȂ����t�@�C���݂̂�Ԃ�
        private static string[] GetFilesName_Ver(string path)
        {
            return SelectFilesName_Ver(FileUtilities.GetFiles(path));
        }

        #endregion
    }

}