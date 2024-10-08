using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StellaCircles.DebugUtils
{
    public class ShowFPS : MonoBehaviour
    {
        [SerializeField] Text fps_text;
        int frameCount;
        float prevTime;
        float fps;

        // 初期化処理
        void Start()
        {
            frameCount = 0;
            prevTime = 0.0f;
        }

        // 更新処理
        void Update()
        {
            frameCount++;
            float time = Time.realtimeSinceStartup - prevTime;

            if (time >= 0.5f)
            {
                fps = frameCount / time;
                fps_text.text = "FPS:" + fps;

                frameCount = 0;
                prevTime = Time.realtimeSinceStartup;
            }
        }
    }

}