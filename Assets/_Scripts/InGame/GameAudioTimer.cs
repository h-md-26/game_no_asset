using System.Collections;
using System.Collections.Generic;
using StellaCircles.Data;
using UnityEngine;

namespace StellaCircles.InGame
{

    public class GameAudioTimer
    {
        // 判定の補正に使う
        readonly float gosa;

        float tm1;
        private float c_time;
        private float gosaTime;
        /// <summary>
        /// 判定のオフセット補正を考慮した譜面の再生時間
        /// </summary>
        public float AudioOffsettedTime => gosaTime;
        float tm1_real;

        public GameAudioTimer(FumenData fumen)
        {
            isOpenMenu = false;
            isBGMStart = false;
            tm1 = -240f / fumen.MeasureBPMs[0] * 3 - fumen.Offset;

            gosa = GameSetting.gameSettingValue.gosaOffset * 0.001f;
        }

        public void MusicGameStart()
        {
            tm1_real = Time.time;
            c_time = tm1;
            //Debug.Log($"c_time {c_time}");

        }

        public void MusicStart()
        {
            isBGMStart = true;
            AudioManager.Instance.PlayBGMnoloop();
            //audioManager.PlayBGM();
        }

        public bool isBGMStart;
        public bool isOpenMenu;

        public void MusicUpdate()
        {
            //Debug・譜面制作用
            if (Application.isEditor)
            {
                if (Input.GetKeyUp(KeyCode.RightArrow))
                {
                    AudioManager.Instance.PlusBGMtime(2f);
                }
                else if (Input.GetKeyUp(KeyCode.LeftArrow))
                {
                    AudioManager.Instance.PlusBGMtime(-2f);
                }
                else if (Input.GetKeyUp(KeyCode.UpArrow))
                {
                    AudioManager.Instance.PlusBGMtime(10f);
                }
                else if (Input.GetKeyUp(KeyCode.DownArrow))
                {
                    AudioManager.Instance.PlusBGMtime(-10f);
                }

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    AudioManager.Instance.SwitchPouseUnpouseBGM();
                }
            }

            if (isOpenMenu) return;

            // c_time < 0 の時
            // c_time = tm1 - tm1_real + Time.time
            if (c_time < 0)
            {
                c_time = tm1 - tm1_real + Time.time;
                if (c_time >= 0)
                {
                    MusicStart();
                }
            }

            // c_time >= 0 の時
            // c_time = audioTimeTester.sudioSource.time
            if (c_time >= 0)
            {
                // c_time = AudioManager.Instance.GetBGMtime();
                c_time = AudioManager.Instance.BGMtime;
            }

            gosaTime = c_time + gosa;
        }
    }

}