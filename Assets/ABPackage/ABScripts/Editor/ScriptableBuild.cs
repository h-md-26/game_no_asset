using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Cysharp.Threading.Tasks;
using StellaCircles.Utils;


namespace StellaCircles.AssetBundleManagement
{
    public class ScriptableBuild
    {
        static int[] buildIds;

        [MenuItem("Assets/Build ScriptableAB")]
        static void Build()
        {
            BuildAsync().Forget();

        }

        static async UniTask BuildAsync()
        {
            buildIds = new int[] { 0 };

            await BuildABLine<MusicAsset>(buildIds, RuntimePlatform.WindowsPlayer);
            await BuildABLine<MusicAsset>(buildIds, RuntimePlatform.Android);
            await BuildABLine<MusicAsset>(buildIds, RuntimePlatform.IPhonePlayer);
            await BuildABLine<ImageAsset>(buildIds, RuntimePlatform.WindowsPlayer);
            await BuildABLine<ImageAsset>(buildIds, RuntimePlatform.Android);
            await BuildABLine<ImageAsset>(buildIds, RuntimePlatform.IPhonePlayer);
        }

        // バージョンリストとファイルの種類からビルドリストを作成
        static List<AssetBundleBuild> CreateBuildList<T>(int[] buildIds, RuntimePlatform platform)
            where T : UnityEngine.Object
        {
            List<AssetBundleBuild> assetBundleBuilds = new List<AssetBundleBuild>();
            int i = 0;
            foreach (int id in buildIds)
            {
                Debug.Log(
                    $"{GamePath.GetFileNameThisPlatform(GamePath.GetAssetName<T>(id), platform)}, {GamePath.GetAssetEditorPath<T>()}{GamePath.GetAssetName<T>(id)}.asset");

                var bb = new AssetBundleBuild();
                bb.assetBundleName = GamePath.GetFileNameThisPlatform(GamePath.GetAssetName<T>(id), platform);
                if (File.Exists($"{GamePath.GetAssetEditorPath<T>()}{GamePath.GetAssetName<T>(id)}.asset"))
                {
                    bb.assetNames = new string[]
                        { $"{GamePath.GetAssetEditorPath<T>()}{GamePath.GetAssetName<T>(id)}.asset" };
                    Debug.Log($"CreateABB{i} : {bb.assetBundleName} , {string.Join(" ", bb.assetNames)}");
                    assetBundleBuilds.Add(bb);
                }
                else
                {
                    Debug.Log(
                        $"ファイルが存在しません:{GamePath.GetAssetEditorPath<T>()}{GamePath.GetAssetName<MusicAsset>(id)}.asset");
                }
            }

            return assetBundleBuilds;
        }

        // ビルドリストとplatformからビルド
        static void BuildAB(List<AssetBundleBuild> buildList, RuntimePlatform platform)
        {
            string platPath;
            BuildTarget buildTarget;

            switch (platform)
            {
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                    platPath = "/Windows";
                    buildTarget = BuildTarget.StandaloneWindows64;
                    break;
                case RuntimePlatform.Android:
                    platPath = "/Android";
                    buildTarget = BuildTarget.Android;
                    break;
                case RuntimePlatform.IPhonePlayer:
                    platPath = "/iOS";
                    buildTarget = BuildTarget.iOS;
                    break;
                default:
                    Debug.Log("platformについて実装されていません");
                    return;
            }

            //生成パスが存在するか確認し、なければ生成
            if (!Directory.Exists(GamePath.buildPath + platPath))
            {
                Debug.Log($"GamePath.buildPath + platPathが存在しないので生成");
                Directory.CreateDirectory(GamePath.buildPath + platPath);
            }

            //ビルド
            BuildPipeline.BuildAssetBundles(GamePath.buildPath + platPath,
                buildList.ToArray(),
                BuildAssetBundleOptions.None,
                buildTarget);
        }

        // リスト生成とビルドをまとめてやる
        static async UniTask BuildABLine<T>(int[] buildIds, RuntimePlatform platform) where T : UnityEngine.Object
        {
            string platPath;
            switch (platform)
            {
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                    platPath = "/Windows";
                    break;
                case RuntimePlatform.Android:
                    platPath = "/Android";
                    break;
                case RuntimePlatform.IPhonePlayer:
                    platPath = "/iOS";
                    break;
                default:
                    Debug.Log("platformについて実装されていません");
                    return;
            }

            //生成パスが存在するか確認し、なければ生成
            if (!Directory.Exists(GamePath.buildPath + platPath))
            {
                Debug.Log($"{GamePath.buildPath + platPath}が存在しないので生成");
                Directory.CreateDirectory(GamePath.buildPath + platPath);
            }

            var abdict = await LoadPlatABDictJson(platform);
            var list = CreateBuildList<T>(buildIds, platform);
            BuildAB(list, platform);
            UpdateABDict<T>(buildIds, abdict, platform);
            await SavePlatABDictJson(abdict, platform);
        }

        // プラットフォーム別のabdictを取得する
        public static async UniTask<Dictionary<string, ABInfo>> LoadPlatABDictJson(RuntimePlatform platform)
        {
            string platPath;
            switch (platform)
            {
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                    platPath = "/Windows";
                    break;
                case RuntimePlatform.Android:
                    platPath = "/Android";
                    break;
                case RuntimePlatform.IPhonePlayer:
                    platPath = "/iOS";
                    break;
                default:
                    Debug.Log("platformについて実装されていません");
                    return null;
            }

            var list =
                await FileUtilities.LoadJson<ABInfoWrapList>(
                    GamePath.GetFileNameThisPlatform(GamePath.abdict, platform),
                    GamePath.buildPath + platPath);
            return ABDictJson.ABWrapList2Dict(list);
        }

        // buildIdsに入っているidのバージョンを1上げる
        public static void UpdateABDict<T>(int[] buildIds, Dictionary<string, ABInfo> abdict, RuntimePlatform platform)
            where T : UnityEngine.Object
        {
            foreach (int id in buildIds)
            {
                string fileName = GamePath.GetFileNameThisPlatform(GamePath.GetAssetName<T>(id), platform);
                if (abdict.ContainsKey(fileName))
                {
                    abdict[fileName].ver++;
                }
                else
                {
                    abdict.Add(fileName, new ABInfo(0));
                }
            }
        }

        // プラットフォーム別のabdictを保存する
        public static async UniTask SavePlatABDictJson(Dictionary<string, ABInfo> dict, RuntimePlatform platform)
        {
            string platPath;
            switch (platform)
            {
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                    platPath = "/Windows";
                    break;
                case RuntimePlatform.Android:
                    platPath = "/Android";
                    break;
                case RuntimePlatform.IPhonePlayer:
                    platPath = "/iOS";
                    break;
                default:
                    Debug.Log("platformについて実装されていません");
                    return;
            }

            //DictionaryはJsonにできないので、リストに直してから渡す
            var list = ABDictJson.ABDict2WrapList(dict);

            await FileUtilities.WriteJson(GamePath.GetFileNameThisPlatform(GamePath.abdict, platform), list,
                GamePath.buildPath + platPath);
        }

        /*
         * アセバンビルドの流れ
         * ・ビルドするプラットフォームを選択する//
         * ・プラットフォームごとのabdictをロードする//
         * ・ビルドするアセバンのidとアセットの種類を指定//
         * ・ビルドするアセバンの旧ファイルを削除(もしかしたら自動でやってくれるかも)
         * ・ビルドし、今回ビルドしたモノについてabdictのverを更新する//
         * ・abdictを保存する//
         *
         */
    }
}