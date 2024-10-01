using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;
using Cysharp.Threading.Tasks;

namespace StellaCircles.AssetBundleManagement
{
    [System.Serializable]
    public class FileUtilities
    {
        //ByteData�̏�������
        public static async UniTask WriteByteData(string name, byte[] data, string path)
        {
            string sp = path + "/" + name;

            await UniTask.RunOnThreadPool(() => CheckDir(path));

            //Debug.Log($"{name}��byte[]�ŕۑ��F{sp}");
            File.WriteAllBytes(sp, data);

            await File.WriteAllBytesAsync(sp, data);

            Debug.Log($"{name}��byte[]�ŕۑ�����܂����F{sp}");
        }

        //JSON�̏�������
        public static async UniTask WriteJson<T>(string name, T data, string path)
        {
            Debug.Log(data);

            string sp = path + "/" + name;

            await UniTask.RunOnThreadPool(() => CheckDir(path));

            //Debug.Log($"{name}��json�ŕۑ��F{sp}");
            string json = null;
            await UniTask.RunOnThreadPool(() => { json = JsonUtility.ToJson(data); });

            Debug.Log($"json = {json}");
            await File.WriteAllTextAsync(sp, json);

            Debug.Log($"{name}��json�ŕۑ�����܂����F{sp}");
        }

        //JSON�̓ǂݍ���
        public static async UniTask<T> LoadJson<T>(string name, string path)
        {
            string json;
            string sp = path + "/" + name;

            T data = default;

            await UniTask.RunOnThreadPool(() => CheckDir(path));

            if (File.Exists(sp))
            {
                Debug.Log($"json�f�[�^ {name}�����[�h�F{sp}");
                json = await File.ReadAllTextAsync(sp);

                Debug.Log($"{name}�����[�h���܂����F{sp}");
                Debug.Log(json);
            }
            else
            {
                json = "";
                await File.WriteAllTextAsync(sp, json);
                Debug.Log($"{name}���쐬���܂����F{sp}");
            }

            await UniTask.RunOnThreadPool(() => { data = JsonUtility.FromJson<T>(json); });

            return data;
        }

        public static string[] GetFiles(string path)
        {
            try
            {
                // �w�肳�ꂽ�f�B���N�g�����̂��ׂẴt�@�C���̃p�X���擾
                string[] files = null;
                files = Directory.GetFiles(path);

                // �t�@�C������\��
                Debug.Log(String.Join(' ', files));

                return files;
            }
            catch (Exception ex)
            {
                Debug.Log("Error: " + ex.Message);
                return null;
            }
        }

        private static void CheckDir(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}