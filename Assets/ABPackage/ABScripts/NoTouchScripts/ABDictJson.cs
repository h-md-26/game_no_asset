using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Cysharp.Threading.Tasks;
using StellaCircles.AssetBundleManagement.Web;
using StellaCircles.Utils;


namespace StellaCircles.AssetBundleManagement
{

    /// �ǂݏ���������ABDict�ɓ������������m
    public static class ABDictJson
    {
        //Dictionary<string, ABInfo> abInfoDict;

        public static async UniTask WriteABDictJson(Dictionary<string, ABInfo> dict)
        {
            //Dictionary��Json�ɂł��Ȃ��̂ŁA���X�g�ɒ����Ă���n��
            var list = ABDict2WrapList(dict);

            await FileUtilities.WriteJson("abdict", list, GamePath.bundleDirPath);
        }

        public static async UniTask<Dictionary<string, ABInfo>> LoadLocalABDictJson()
        {
            var list = await FileUtilities.LoadJson<ABInfoWrapList>(GamePath.abdict, GamePath.bundleDirPath);
            return ABWrapList2Dict(list);
        }

        public static async UniTask<Dictionary<string, ABInfo>> LoadRemoteABDictJson()
        {
            var list = await WebUtilities.JsonFromWebRequest<ABInfoWrapList>(GamePath.abApiUrl +
                                                                             GamePath.GetABDictNameThisPlatform());
            return ABWrapList2Dict(list);
        }

        public static ABInfoWrapList ABDict2WrapList(Dictionary<string, ABInfo> dict)
        {
            List<ABInfoL> abinfoL = new();
            foreach (var d in dict)
            {
                abinfoL.Add(new ABInfoL(d.Key, d.Value));
            }

            return new ABInfoWrapList(abinfoL);
        }

        public static Dictionary<string, ABInfo> ABWrapList2Dict(ABInfoWrapList list)
        {
            Dictionary<string, ABInfo> abinfoD = new();

            if (list == null)
            {
                Debug.Log("abjson��null");
            }
            else
            {
                foreach (var l in list.abinfoL)
                {
                    abinfoD.Add(l.name, l.abinfo);
                }
            }

            return abinfoD;
        }
    }

    [System.Serializable]
    public class ABInfo
    {
        public int ver;

        public ABInfo(int ver)
        {
            this.ver = ver;
        }
    }

    [System.Serializable]
    public class ABInfoL
    {
        public string name;
        public ABInfo abinfo;

        public ABInfoL(string _name, ABInfo _abinfo)
        {
            name = _name;
            abinfo = _abinfo;
        }
    }

    [System.Serializable]
    public class ABInfoWrapList
    {
        public List<ABInfoL> abinfoL;

        public ABInfoWrapList(List<ABInfoL> _abinfoL)
        {
            abinfoL = _abinfoL;
        }
    }
}