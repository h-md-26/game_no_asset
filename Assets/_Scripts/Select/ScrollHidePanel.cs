using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace StellaCircles.ViewUtils
{
    public class ScrollHidePanel : MonoBehaviour
    {
        [SerializeField] RectTransform rectT;
        [SerializeField] RectTransform screenSizeRT;
        float JacketSize = 550;
        [SerializeField] bool isLeftHider;

        float w = 0;
        float ad;
        bool isAdjusted;

        void Update()
        {
            if (Mathf.Abs(w - screenSizeRT.rect.width) > 1)
            {
                Debug.Log("Width変更より、Hideの大きさを調整する");
                isAdjusted = false;
            }


            if (isAdjusted) return;

            w = screenSizeRT.rect.width;
            ad = (w + JacketSize) / 2;
            if (isLeftHider)
            {
                // Rightをadにする
                gameObject.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
                gameObject.GetComponent<RectTransform>().offsetMax = new Vector2(-ad, 0);
            }
            else
            {
                // Leftをadにする
                gameObject.GetComponent<RectTransform>().offsetMin = new Vector2(ad, 0);
                gameObject.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
            }

            isAdjusted = true;
        }
    }
}