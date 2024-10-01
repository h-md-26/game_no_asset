using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace StellaCircles.AssetBundleManagement.Web
{
    public class WebUtilities
    {
        // WebからByte配列として取得する
        public static async UniTask<byte[]> ByteFromWebRequest(string url)
        {
            using (UnityWebRequest uwr = UnityWebRequest.Get(url))
            {
                await uwr.SendWebRequest();

                // エラーチェック
                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(uwr.error);
                    return default;
                }

                // bytedataを返す
                return uwr.downloadHandler.data;
            }
        }

        // Webからtextとして取得する
        public static async UniTask<string> TextFromWebRequest(string url)
        {
            using (UnityWebRequest uwr = UnityWebRequest.Get(url))
            {
                await uwr.SendWebRequest();

                // エラーチェック
                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(uwr.error);
                    return default;
                }

                // bytedataを返す
                return uwr.downloadHandler.text;
            }
        }

        // WebからJSONとして取得する
        public static async UniTask<T> JsonFromWebRequest<T>(string url)
        {
            using (UnityWebRequest uwr = UnityWebRequest.Get(url))
            {
                await uwr.SendWebRequest();

                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(uwr.error);
                    return default(T);
                }

                string json = uwr.downloadHandler.text;
                Debug.Log(json);
                T data = default;

                await UniTask.RunOnThreadPool(() => { data = JsonUtility.FromJson<T>(json); });

                return data;
            }
        }
    }
}