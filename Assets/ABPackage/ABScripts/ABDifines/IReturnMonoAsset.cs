using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StellaCircles.AssetBundleManagement
{
    public interface IReturnMonoAsset<T> where T : UnityEngine.Object
    {
        T GetMonoAsset();
    }
}