using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace StellaCircles.InGame
{
    /// <summary>
    /// 判定枠の動きを制御する
    /// </summary>
    public class JudgeFrame : MonoBehaviour
    {
        GameAudioTimer audioTimer;
        
        [SerializeField] Transform[] nodeTransforms;
        private Vector2[] nodePositions;
        
        private Vector2 jfPos;
        private int currentMeasureId = 0;
        private int[] nodeDestinations;

        private event Action changeStartText;
        private event Action changeEmptyText;
        private event Action gameEnd;

        public void GameStart(FumenData fumen,GameAudioTimer audioTimer, Action changeStartText, Action changeEmptyText, Action gameEnd)
        {
            // fumenのmeasureStartTimeのコピーを取得
            measureStartTimes = fumen.MeasureStartTimes;
            measureDurations = fumen.MeasureDurations;
            nodeDestinations = fumen.NodeDestinations;
            
            this.audioTimer = audioTimer;
            
            this.changeStartText += changeStartText;
            this.changeEmptyText += changeEmptyText;
            this.gameEnd += gameEnd;
            
            // nodeの座標を登録
            nodePositions = new Vector2[nodeTransforms.Length];
            for (int i = 0; i < 6; i++)
            {
                nodePositions[i] = nodeTransforms[i].position;
            }

            currentMeasureId = 0;

            transform.position = nodePositions[nodeDestinations[currentMeasureId]];
            //tm1 = -240f / fumen.measureBPMs[0] * 6;

        }

        private float[] measureStartTimes;
        private float[] measureDurations;
        
        public void JFUpdate()
        {
            //用意された小節数を超えたらリターン
            if (currentMeasureId >= measureStartTimes.Length) return;

            // 判定枠の移動
            if (audioTimer.AudioOffsettedTime - measureStartTimes[currentMeasureId] > measureDurations[currentMeasureId])
            {
                jfPos = nodePositions[nodeDestinations[currentMeasureId + 1]];
            }
            else
            {
                // Lerpに置き換える
                float deltaTime = audioTimer.AudioOffsettedTime - measureStartTimes[currentMeasureId];
                jfPos = Vector2.Lerp(nodePositions[nodeDestinations[currentMeasureId]], nodePositions[nodeDestinations[(currentMeasureId + 1)]], deltaTime / measureDurations[currentMeasureId]);
            }

            transform.position = jfPos;


            // 判定枠の位置に応じた処理
            // 分離した方がいいかも
            if (audioTimer.AudioOffsettedTime - measureStartTimes[currentMeasureId] >= measureDurations[currentMeasureId])
            {
                currentMeasureId++;

                if (currentMeasureId == 2)
                {
                    changeStartText?.Invoke();
                }

                if (currentMeasureId == 3)
                {
                    changeEmptyText?.Invoke();
                }

                //
                if (currentMeasureId >= measureStartTimes.Length)
                {
                    Debug.Log("演奏終了");
                    gameEnd?.Invoke();
                    return;
                }
            }
        }
    }
}