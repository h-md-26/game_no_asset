using System.Collections;
using System.Collections.Generic;
using StellaCircles.AssetBundleManagement;
using UnityEngine;

namespace StellaCircles.Utils
{
    public static class GamePath
    {
        #region Pathes

        // アセバン・ablistのAPI
        public static string abApiUrl = "https://*****";

        // アセバン・ablistの保存場所
        // awsが解決するまでStreamingAssets/abfilesに変更
        //public static string bundleDirPath = Application.dataPath + "/abfiles";
        public static string bundleDirPath = Application.streamingAssetsPath + "/abfiles";

        // アセバンのビルド出力先
        public static string buildPath = "Assets/ABPackage/ABBuilds";

        // ABListのファイル名
        public const string abdict = "abdict";

        // MusicAssetのアセバンの名前共通部分
        private const string musicAssetName = "musicid";

        // ImageAssetのアセバンの名前共通部分
        private const string imageAssetName = "imageid";

        // MusicInfoAssetのアセバンの名前共通部分
        private const string musicInfoAssetName = "musicinfoid";

        // MusicAssetのEditor上の置き場所
        public const string musicAssetEditorPath = "Assets/ABPackage/ABAssets/MusicAssets/";

        // ImageAssetのEditor上の置き場所
        public const string imageAssetEditorPath = "Assets/ABPackage/ABAssets/ImageAssets/";

        // MusicInfoAssetのEditor上の置き場所
        public const string musicInfoAssetEditorPath = "Assets/ABPackage/ABAssets/MusicInfoAssets/";

        // プラットホーム別のファイル名部分
        private const string androidNamePart = "a_";
        private const string iosNamePart = "i_";
        private const string windowsNamePart = "w_";

        #endregion

        // ファイル名の頭にプラットフォーム識別用の文字列をつける　ABファイルのコンパイル用に使ってるだけ
        public static string GetFileNameThisPlatform(string commonPart, RuntimePlatform platform)
        {
            switch (platform)
            {
                case RuntimePlatform.Android:
                    return androidNamePart + commonPart;
                case RuntimePlatform.IPhonePlayer:
                    return iosNamePart + commonPart;
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    return windowsNamePart + commonPart;
                default:
                    return windowsNamePart + commonPart;
            }
        }

        // アセットの型からアセバンの名前を取得
        public static string GetAssetName<T>(int id) where T : UnityEngine.Object
        {
            if (typeof(T) == typeof(AudioClip) || typeof(T) == typeof(MusicAsset))
            {
                return musicAssetName + id;
            }
            else if (typeof(T) == typeof(Sprite) || typeof(T) == typeof(ImageAsset))
            {
                return imageAssetName + id;
            }
            else
            {
                Debug.Log("想定されていない型");
                return null;
            }
        }

        // エディター上の圧縮前ファイルの場所を返す ビルド用
        public static string GetAssetEditorPath<T>()
        {
            if (typeof(T) == typeof(AudioClip) || typeof(T) == typeof(MusicAsset))
            {
                return musicAssetEditorPath;
            }
            else if (typeof(T) == typeof(Sprite) || typeof(T) == typeof(ImageAsset))
            {
                return imageAssetEditorPath;
            }
            else
            {
                Debug.Log("想定されていない型");
                return null;
            }
        }

        // プラットフォームによって取得するabdictを分けるためにサーバー上にあるabdictの名前をもらう
        public static string GetABDictNameThisPlatform()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    return androidNamePart + abdict;
                case RuntimePlatform.IPhonePlayer:
                    return iosNamePart + abdict;
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    return windowsNamePart + abdict;
                default:
                    return windowsNamePart + abdict;
            }
        }
    }
}