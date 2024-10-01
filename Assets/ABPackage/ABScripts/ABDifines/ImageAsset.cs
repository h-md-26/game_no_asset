using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StellaCircles.AssetBundleManagement
{
    [CreateAssetMenu(menuName = "AB/ImageAsset")]
    public class ImageAsset : ScriptableObject, IReturnMonoAsset<Sprite>
    {
        public int id;
        public Sprite sprite;

        public Sprite GetMonoAsset()
        {
            return sprite;
        }
    }
}