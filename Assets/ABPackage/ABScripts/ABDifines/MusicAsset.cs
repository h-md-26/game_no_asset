using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StellaCircles.AssetBundleManagement
{
    [CreateAssetMenu(menuName = "AB/MusicAsset")]
    public class MusicAsset : ScriptableObject, IReturnMonoAsset<AudioClip>
    {
        public int id;
        public AudioClip clip;

        public AudioClip GetMonoAsset()
        {
            return clip;
        }
    }
}