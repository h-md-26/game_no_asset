using UnityEngine;
using UnityEngine.UI;

namespace StellaCircles.ViewUtils
{

    public class ImageScroller : MonoBehaviour
    {
        public Image image; // �X�N���[��������Image
        public float scrollSpeed = 0.1f; // �X�N���[�����x

        private RectTransform rectTransform;
        private float currentPosition = 0f;

        void Start()
        {
            // Image��RectTransform���擾
            rectTransform = image.GetComponent<RectTransform>();
        }

        void Update()
        {
            // �X�N���[�����x�Ɋ�Â��ĉ摜���ړ�
            currentPosition += scrollSpeed * Time.deltaTime;

            // ���[�v������
            if (currentPosition >= 1.0f)
            {
                currentPosition -= 1.0f;
            }

            // Image��UV���W��ݒ�
            image.material.mainTextureOffset = new Vector2(currentPosition, 0);
        }
    }
}