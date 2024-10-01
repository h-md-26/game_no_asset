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
        /// Hoge class ��AssetBundle��v��
        /// ��
        /// Stock��T�����A���Ƀ��[�h�ς݂Ȃ炱���Ԃ��A�Q�Ɛ���1���₷
        ///                �����[�h�Ȃ烍�[�h���AStock�ɒǉ����A�Q�Ɛ���1���₷
        /// 
        /// ��AssetBundle�̃��[�h���@��
        /// �@�K�v��AssetBundle�̓Q�[���J�n���ɍŐV�o�[�W�������_�E�����[�h���A���[�J���ɕۑ����Ă���
        /// �@ �ۑ��ꏊ�� persistantDataPath/AssetBundles/�ɂ���
        /// �@���̂��߁A���[�h����Ƃ��̓��[�J��������΂悢
        /// �@ ���[�h�ɕK�v�ȏ��̓A�Z�o���̖��O�Ȃ̂ŁAmusic0 ���Aid�ƕR�Â������O�Ȃ�悳����
        /// 
        /// Hoge class ��AssetBundle��ԋp
        /// ��
        /// �Q�Ɛ���1���炵�A�Q�Ƃ��Ȃ��Ȃ�����Stock,ReferCount����Key��Remove����
        /// 
        /// 
        /// ��Asset�̃��[�h���@��
        /// �@�K�v�ȏ��̓A�Z�o���̃C���X�^���X�ƃA�Z�b�g�̖��O�Ȃ̂ŁAmusic0 ���Aid�ƕR�Â������O�Ȃ�悳����
        /// 
        /// 
        /// 
        /// 

        public static Dictionary<string, ABInfo> abDict;

        private static Dictionary<string, AssetBundle> bundleStock = new(); // ���[�h���̃A�Z�o���������Ă���
        private static Dictionary<string, int> bundleReferCount = new(); // ���[�h���̃A�Z�o���̎Q�Ɛ��������Ă���

        // IReturnMonoAsset����������ScriptableObject��AB����TReturn�̃A�Z�b�g�����o��
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

        // �A�Z�o���̌^����bundleName���擾���ă����[�X����
        public static async UniTask ReleaseMonoAB<T>(int id, CancellationToken ct) where T : UnityEngine.Object
        {
            string bundleName = GamePath.GetAssetName<T>(id);
            await ReleaseAB(bundleName, ct);
        }

        // �A�Z�o����Get
        private static async UniTask<AssetBundle> GetAB(string bundleName, CancellationToken ct)
        {
            if (bundleStock.ContainsKey(bundleName))
            {
                //Debug.Log("Stock����Assetbundle��n���A�Q�Ɛ���1���₷");
                //Stock����Assetbundle��n���A�Q�Ɛ���1���₷
                bundleReferCount[bundleName]++;
                return bundleStock[bundleName];
            }
            else
            {
                //Debug.Log("Stock�ɂȂ��̂ŁA���[�J�����烍�[�h����");
                //���[�J�����烍�[�h����
                //string assetPath = await GetABPath(bundleName);
                string assetPath = GetABPath(bundleName);
                AssetBundle result = await LoadABAsync(assetPath, ct);
                //result��Stock�ɓn���A�Q�Ɛ���1���₷
                if (result != null)
                {
                    bundleStock.Add(bundleName, result);
                    bundleReferCount.Add(bundleName, 0);
                    bundleReferCount[bundleName]++;
                    return result;
                }
                else
                {
                    //���[�h�Ɏ��s
                    Debug.Log($"AssetBundle{bundleName}�̃��[�h�Ɏ��s");
                    return null;
                }
            }

        }

        // �A�Z�o����Release
        private static async UniTask ReleaseAB(string bundleName, CancellationToken ct)
        {
            //Debug.Log($"{bundleName}�̃����[�X");
            //Debug.Log($"{bundleReferCount[bundleName]}->{bundleReferCount[bundleName]-1}");
            //�Q�Ɛ������炷
            if (bundleReferCount.ContainsKey(bundleName))
            {
                bundleReferCount[bundleName]--;
                if (bundleReferCount[bundleName] <= 0)
                {
                    //Debug.Log($"�Q�Ƃ��Ȃ��Ȃ����̂�Unload");
                    // �Q�Ƃ��Ȃ��Ȃ����̂� Unload(true) ����
                    await UnloadAsync(bundleStock[bundleName], true, ct);
                    bundleStock.Remove(bundleName);
                    bundleReferCount.Remove(bundleName);
                }
            }
        }

        // �A�Z�b�g�����[�h AssetBundle��Asset�̖��O�𓯂��ɂ��Ă���ꍇ
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

        //AB�̃p�X(�t�@�C������{platform}_{name}_{ver})��Ԃ�
        //aws����������܂�{platform}_{name})�ɕύX
        private static string GetABPath(string name)
            //public static async UniTask<string> GetABPath(string name)
        {
            string plat_name = GamePath.GetFileNameThisPlatform(name, Application.platform);

            // �ʐM�̕�����񂸂������������܂łȂ��I�I
            //if (abDict == null)
            //{
            //    Debug.Log("abdict�����[�h����");
            //    abDict = await ABDictJson.LoadLocalABDictJson();
            //}

            //string plat_name_ver = $"{plat_name}_{abDict[plat_name].ver}";
            string plat_name_ver = $"{plat_name}";

            return GamePath.bundleDirPath + "/" + plat_name_ver;
        }

        #region private�֐�

        // AssetBundle��Load����
        private static async UniTask<AssetBundle> LoadABAsync(string assetPath, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }
            
            return await AssetBundle.LoadFromFileAsync(assetPath);
        }

        // AssetBundle��Unload����(true �ŃA�Z�b�g����)
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