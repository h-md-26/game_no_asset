using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StellaCircles.Select
{
    public class SelectAudioManager : MonoBehaviour
    {
        public AudioClip BGM_selectAge;
        public AudioClip BGM_scroll;

        public AudioClip SE_tap;
        public AudioClip SE_select;

        [SerializeField] private AudioSource BGMso;
        [SerializeField] private AudioSource SEso;

        public void ShotSE(AudioClip ac)
        {
            SEso.PlayOneShot(ac);
        }


        public void PlayBGM(AudioClip ac, float demoStart)
        {
            Debug.Log("SelectAudioÇ©ÇÁçƒê∂");

            BGMso.clip = ac;
            BGMso.time = demoStart;
            BGMso.Play();
        }
    }
}