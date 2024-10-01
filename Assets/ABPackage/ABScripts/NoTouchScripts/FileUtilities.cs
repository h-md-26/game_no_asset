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
        //ByteDataの書き込み
        public static async UniTask WriteByteData(string name, byte[] data, string path)
        {
            string sp = path + "/" + name;

            await UniTask.RunOnThreadPool(() => CheckDir(path));

            //Debug.Log($"{name}をbyte[]で保存：{sp}");
            File.WriteAllBytes(sp, data);

            await File.WriteAllBytesAsync(sp, data);

            Debug.Log($"{name}がbyte[]で保存されました：{sp}");
        }

        //JSONの書き込み
        public static async UniTask WriteJson<T>(string name, T data, string path)
        {
            Debug.Log(data);

            string sp = path + "/" + name;

            await UniTask.RunOnThreadPool(() => CheckDir(path));

            //Debug.Log($"{name}をjsonで保存：{sp}");
            string json = null;
            await UniTask.RunOnThreadPool(() => { json = JsonUtility.ToJson(data); });

            Debug.Log($"json = {json}");
            await File.WriteAllTextAsync(sp, json);

            Debug.Log($"{name}がjsonで保存されました：{sp}");
        }

        //JSONの読み込み
        public static async UniTask<T> LoadJson<T>(string name, string path)
        {
            string json;
            string sp = path + "/" + name;

            T data = default;

            await UniTask.RunOnThreadPool(() => CheckDir(path));

            if (File.Exists(sp))
            {
                Debug.Log($"jsonデータ {name}をロード：{sp}");
                json = await File.ReadAllTextAsync(sp);

                Debug.Log($"{name}をロードしました：{sp}");
                Debug.Log(json);
            }
            else
            {
                json = "";
                await File.WriteAllTextAsync(sp, json);
                Debug.Log($"{name}を作成しました：{sp}");
            }

            await UniTask.RunOnThreadPool(() => { data = JsonUtility.FromJson<T>(json); });

            return data;
        }

        public static string[] GetFiles(string path)
        {
            try
            {
                // 指定されたディレクトリ内のすべてのファイルのパスを取得
                string[] files = null;
                files = Directory.GetFiles(path);

                // ファイル名を表示
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