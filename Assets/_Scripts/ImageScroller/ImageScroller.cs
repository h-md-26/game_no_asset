using UnityEngine;
using UnityEngine.UI;

namespace StellaCircles.ViewUtils
{

    public class ImageScroller : MonoBehaviour
    {
        public Image image; // スクロールさせるImage
        public float scrollSpeed = 0.1f; // スクロール速度

        private RectTransform rectTransform;
        private float currentPosition = 0f;

        void Start()
        {
            // ImageのRectTransformを取得
            rectTransform = image.GetComponent<RectTransform>();
        }

        void Update()
        {
            // スクロール速度に基づいて画像を移動
            currentPosition += scrollSpeed * Time.deltaTime;

            // ループさせる
            if (currentPosition >= 1.0f)
            {
                currentPosition -= 1.0f;
            }

            // ImageのUV座標を設定
            image.material.mainTextureOffset = new Vector2(currentPosition, 0);
        }
    }
}