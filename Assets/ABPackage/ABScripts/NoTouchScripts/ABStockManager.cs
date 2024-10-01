using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using System.IO;
using System.Threading;
using StellaCircles.Utils;

namespace StellaCircles.AssetBundleManagement
{
    public static class ABStockManager
    {
        /// 
        /// 
        /// Hoge class がAssetBundleを要求
        /// ↓
        /// Stockを探索し、既にロード済みならこれを返し、参照数を1増やす
        ///                未ロードならロードし、Stockに追加し、参照数を1増やす
        /// 
        /// ＜AssetBundleのロード方法＞
        /// 　必要なAssetBundleはゲーム開始時に最新バージョンをダウンロードし、ローカルに保存しておく
        /// 　 保存場所は persistantDataPath/AssetBundles/にする
        /// 　そのため、ロードするときはローカルからやればよい
        /// 　 ロードに必要な情報はアセバンの名前なので、music0 等、idと紐づいた名前ならよさそう
        /// 
        /// Hoge class がAssetBundleを返却
        /// ↓
        /// 参照数を1減らし、参照がなくなったらStock,ReferCountからKeyをRemoveする
        /// 
        /// 
        /// ＜Assetのロード方法＞
        /// 　必要な情報はアセバンのインスタンスとアセットの名前なので、music0 等、idと紐づいた名前ならよさそう
        /// 
        /// 
        /// 
        /// 

        public static Dictionary<string, ABInfo> abDict;

        private static Dictionary<string, AssetBundle> bundleStock = new(); // ロード中のアセバンをもっておく
        private static Dictionary<string, int> bundleReferCount = new(); // ロード中のアセバンの参照数を持っておく

        // IReturnMonoAssetを実装したScriptableObjectのABからTReturnのアセットを取り出す
        public static async UniTask<TReturn> LoadMonoABAndAsset<TObj, TReturn>(int id, CancellationToken ct)
            where TObj : UnityEngine.Object, IReturnMonoAsset<TReturn>
            where TReturn : UnityEngine.Object
        {
            //Debug.Log($"load {typeof(TObj)}{id} > {typeof(TReturn)}");

            string assetName = GamePath.GetAssetName<TObj>(id);
            await GetAB(assetName, ct);
            TObj _asset = await LoadAssetAsync<TObj>(assetName, ct);
            TReturn _monoAsset = _asset.GetMonoAsset();
            return _monoAsset;
        }

        // アセバンの型からbundleNameを取得してリリースする
        public static async UniTask ReleaseMonoAB<T>(int id, CancellationToken ct) where T : UnityEngine.Object
        {
            string bundleName = GamePath.GetAssetName<T>(id);
            await ReleaseAB(bundleName, ct);
        }

        // アセバンのGet
        private static async UniTask<AssetBundle> GetAB(string bundleName, CancellationToken ct)
        {
            if (bundleStock.ContainsKey(bundleName))
            {
                //Debug.Log("StockからAssetbundleを渡し、参照数を1増やす");
                //StockからAssetbundleを渡し、参照数を1増やす
                bundleReferCount[bundleName]++;
                return bundleStock[bundleName];
            }
            else
            {
                //Debug.Log("Stockにないので、ローカルからロードする");
                //ローカルからロードする
                //string assetPath = await GetABPath(bundleName);
                string assetPath = GetABPath(bundleName);
                AssetBundle result = await LoadABAsync(assetPath, ct);
                //resultをStockに渡し、参照数を1増やす
                if (result != null)
                {
                    bundleStock.Add(bundleName, result);
                    bundleReferCount.Add(bundleName, 0);
                    bundleReferCount[bundleName]++;
                    return result;
                }
                else
                {
                    //ロードに失敗
                    Debug.Log($"AssetBundle{bundleName}のロードに失敗");
                    return null;
                }
            }

        }

        // アセバンのRelease
        private static async UniTask ReleaseAB(string bundleName, CancellationToken ct)
        {
            //Debug.Log($"{bundleName}のリリース");
            //Debug.Log($"{bundleReferCount[bundleName]}->{bundleReferCount[bundleName]-1}");
            //参照数を減らす
            if (bundleReferCount.ContainsKey(bundleName))
            {
                bundleReferCount[bundleName]--;
                if (bundleReferCount[bundleName] <= 0)
                {
                    //Debug.Log($"参照がなくなったのでUnload");
                    // 参照がなくなったので Unload(true) する
                    await UnloadAsync(bundleStock[bundleName], true, ct);
                    bundleStock.Remove(bundleName);
                    bundleReferCount.Remove(bundleName);
                }
            }
        }

        // アセットをロード AssetBundleとAssetの名前を同じにしている場合
        private static async UniTask<T> LoadAssetAsync<T>(string assetName, CancellationToken ct) where T : UnityEngine.Object
        {
            if (!bundleStock.ContainsKey(assetName))
            {
                await GetAB(assetName, ct);
            }

            if (ct.IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }

            var assetRequest = bundleStock[assetName].LoadAssetAsync<T>(assetName);
            await assetRequest;
            return (T)assetRequest.asset;
        }

        //ABのパス(ファイル名が{platform}_{name}_{ver})を返す
        //awsが解決するまで{platform}_{name})に変更
        private static string GetABPath(string name)
            //public static async UniTask<string> GetABPath(string name)
        {
            string plat_name = GamePath.GetFileNameThisPlatform(name, Application.platform);

            // 通信の文字情報ずれ問題を解決するまでなし！！
            //if (abDict == null)
            //{
            //    Debug.Log("abdictをロードする");
            //    abDict = await ABDictJson.LoadLocalABDictJson();
            //}

            //string plat_name_ver = $"{plat_name}_{abDict[plat_name].ver}";
            string plat_name_ver = $"{plat_name}";

            return GamePath.bundleDirPath + "/" + plat_name_ver;
        }

        #region private関数

        // AssetBundleをLoadする
        private static async UniTask<AssetBundle> LoadABAsync(string assetPath, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }
            
            return await AssetBundle.LoadFromFileAsync(assetPath);
        }

        // AssetBundleをUnloadする(true でアセットごと)
        private static async UniTask UnloadAsync(AssetBundle assetBundle, bool unloadAllAssets, CancellationToken ct)
        {
            if (assetBundle == null) return;

            if (ct.IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }
            
            await assetBundle.UnloadAsync(unloadAllAssets);
        }

        #endregion
    }
}