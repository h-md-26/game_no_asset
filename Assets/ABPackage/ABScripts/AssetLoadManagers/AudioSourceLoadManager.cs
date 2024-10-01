using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace StellaCircles.AssetBundleManagement
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioSourceLoadManager : MonoBehaviour
    {
        public AudioSource audioSource;

        public bool isMusicLoading = false; //ロードフラグ
        public bool isMusicStacking = false; //ロードのスタックフラグ
        public int musicStackId = -1; //スタックしているID
        public int musicUsingId = -1; //既にロードしたID

        void Start()
        {
            audioSource = GetComponent<AudioSource>();
        }
    }
}