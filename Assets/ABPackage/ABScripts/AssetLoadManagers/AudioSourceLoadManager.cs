using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace StellaCircles.AssetBundleManagement
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioSourceLoadManager : MonoBehaviour
    {
        public AudioSource audioSource;

        public bool isMusicLoading = false; //���[�h�t���O
        public bool isMusicStacking = false; //���[�h�̃X�^�b�N�t���O
        public int musicStackId = -1; //�X�^�b�N���Ă���ID
        public int musicUsingId = -1; //���Ƀ��[�h����ID

        void Start()
        {
            audioSource = GetComponent<AudioSource>();
        }
    }
}