using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StellaCircles.InGame
{

    public class AutoPlayer
    {
        const float greatArea = 0.040f;
        float timeDif;
        int ju_i = 0;

        readonly GameAudioTimer audioTimer;
        readonly InputManager inputer;

        private float[] notesBeatTimes;
        private NoteType[] noteTypes;
        
        public AutoPlayer(FumenData fumen, InputManager inputer, GameAudioTimer audioTimer)
        {
            this.inputer = inputer;
            this.audioTimer = audioTimer;
            
            ju_i = 0;
            
            notesBeatTimes = fumen.NoteBeatTimes;
            noteTypes = fumen.NoteTypes;
        }

        public void APUpdate()
        {
            if (notesBeatTimes.Length <= ju_i) return;

            // 現在時刻と判定ノーツの差の絶対値
            timeDif = notesBeatTimes[ju_i] - audioTimer.AudioOffsettedTime;

            if (timeDif <= 0)
            {
                if (timeDif < -1f)
                {
                    while (timeDif > 0f)
                    {
                        ju_i++;
                        if (ju_i >= notesBeatTimes.Length) return;
                        timeDif = notesBeatTimes[ju_i] - audioTimer.AudioOffsettedTime;
                    }
                }

                if (noteTypes[ju_i] == NoteType.TAP)
                {
                    inputer.InputTap(1);
                }
                else if (noteTypes[ju_i] == NoteType.FLICK)
                {
                    inputer.InputFlick(1);
                }
                else if (noteTypes[ju_i] == NoteType.HOLD_START)
                {
                    inputer.InputTap(1);
                }
                else if (noteTypes[ju_i] == NoteType.HOLD_MID)
                {
                    inputer.InputHolding(1);
                }
                else if (noteTypes[ju_i] == NoteType.HOLD_END)
                {
                    inputer.InputRelease(1);
                }

                ju_i++;
            }
            // time と notetimeを参照してInputerの関数を呼び出す
        }
    }

}