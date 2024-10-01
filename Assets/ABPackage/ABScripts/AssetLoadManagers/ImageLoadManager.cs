using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace StellaCircles.AssetBundleManagement
{
    [RequireComponent(typeof(Image))]
    public class ImageLoadManager : MonoBehaviour
    {
        public Image image;
        public int usingId = -1;

        private void Start()
        {
            image = GetComponent<Image>();
        }
    }
}