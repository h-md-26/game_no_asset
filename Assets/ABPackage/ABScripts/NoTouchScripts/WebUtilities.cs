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
        // Web����Byte�z��Ƃ��Ď擾����
        public static async UniTask<byte[]> ByteFromWebRequest(string url)
        {
            using (UnityWebRequest uwr = UnityWebRequest.Get(url))
            {
                await uwr.SendWebRequest();

                // �G���[�`�F�b�N
                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(uwr.error);
                    return default;
                }

                // bytedata��Ԃ�
                return uwr.downloadHandler.data;
            }
        }

        // Web����text�Ƃ��Ď擾����
        public static async UniTask<string> TextFromWebRequest(string url)
        {
            using (UnityWebRequest uwr = UnityWebRequest.Get(url))
            {
                await uwr.SendWebRequest();

                // �G���[�`�F�b�N
                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(uwr.error);
                    return default;
                }

                // bytedata��Ԃ�
                return uwr.downloadHandler.text;
            }
        }

        // Web����JSON�Ƃ��Ď擾����
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