using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;


namespace StellaCircles.AssetBundleManagement
{
    public static class ABFileUtilities
    {
        public static async UniTask WriteByteData(string name, int ver, byte[] data, string path)
        {
            await FileUtilities.WriteByteData($"{name}_{ver}", data, path);
            //await FileUtilities.WriteByteData($"{name}", data, path);
        }
    }

}