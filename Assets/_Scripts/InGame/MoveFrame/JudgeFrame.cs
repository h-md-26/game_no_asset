using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace StellaCircles.InGame
{
    /// <summary>
    /// ����g�̓����𐧌䂷��
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
            // fumen��measureStartTime�̃R�s�[���擾
            measureStartTimes = fumen.MeasureStartTimes;
            measureDurations = fumen.MeasureDurations;
            nodeDestinations = fumen.NodeDestinations;
            
            this.audioTimer = audioTimer;
            
            this.changeStartText += changeStartText;
            this.changeEmptyText += changeEmptyText;
            this.gameEnd += gameEnd;
            
            // node�̍��W��o�^
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
            //�p�ӂ��ꂽ���ߐ��𒴂����烊�^�[��
            if (currentMeasureId >= measureStartTimes.Length) return;

            // ����g�̈ړ�
            if (audioTimer.AudioOffsettedTime - measureStartTimes[currentMeasureId] > measureDurations[currentMeasureId])
            {
                jfPos = nodePositions[nodeDestinations[currentMeasureId + 1]];
            }
            else
            {
                // Lerp�ɒu��������
                float deltaTime = audioTimer.AudioOffsettedTime - measureStartTimes[currentMeasureId];
                jfPos = Vector2.Lerp(nodePositions[nodeDestinations[currentMeasureId]], nodePositions[nodeDestinations[(currentMeasureId + 1)]], deltaTime / measureDurations[currentMeasureId]);
            }

            transform.position = jfPos;


            // ����g�̈ʒu�ɉ���������
            // ��������������������
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
                    Debug.Log("���t�I��");
                    gameEnd?.Invoke();
                    return;
                }
            }
        }
    }
}