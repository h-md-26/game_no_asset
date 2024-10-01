using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace StellaCircles.InGame
{
    public class GuideFrame : MonoBehaviour
    {
        GameAudioTimer audioTimer;
        
        [SerializeField] Transform[] nodeTransforms;
        Vector2[] nodePositions;
        
        Vector2 jfPos;
        int currentMeasureId = 0;
        private int[] nodeDestinations;

        public void GuideStart(FumenData fumen, GameAudioTimer audioTimer)
        {
            // fumenのmeasureStartTimeのコピーを取得
            measureStartTimes = fumen.MeasureStartTimes;
            measureDurations = fumen.MeasureDurations;
            nodeDestinations = fumen.NodeDestinations;
            
            this.audioTimer = audioTimer;
            
            nodePositions = new Vector2[6];
            for (int i = 0; i < 6; i++)
            {
                nodePositions[i] = nodeTransforms[i].transform.position;
            }

            currentMeasureId = 0;

            transform.position = nodePositions[nodeDestinations[currentMeasureId]];
        }

        private float[] measureStartTimes;
        private float[] measureDurations;

        public void GuideUpdate()
        {
            //用意された小節数を超えたらリターン
            if (currentMeasureId >= measureStartTimes.Length) return;

            if (audioTimer.AudioOffsettedTime + 1.5f - measureStartTimes[currentMeasureId] > measureDurations[currentMeasureId])
            {
                jfPos = nodePositions[nodeDestinations[currentMeasureId + 1]];
            }
            else
            {
                // Lerpに置き換える
                float deltaTime = audioTimer.AudioOffsettedTime + 1.5f - measureStartTimes[currentMeasureId];
                jfPos = Vector2.Lerp(nodePositions[nodeDestinations[currentMeasureId]], nodePositions[nodeDestinations[(currentMeasureId + 1)]], deltaTime / measureDurations[currentMeasureId]);
            }

            transform.position = jfPos;

            if (audioTimer.AudioOffsettedTime + 1.5f - measureStartTimes[currentMeasureId] >= measureDurations[currentMeasureId])
            {
                currentMeasureId++;
            }
        }
    }
}