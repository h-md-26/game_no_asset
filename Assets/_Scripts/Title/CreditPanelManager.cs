using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StellaCircles.Title
{
    public class CreditPanelManager : MonoBehaviour
    {
        [SerializeField] private Button creditButton;
        [SerializeField] private Button closeCreditButton;
        
        [SerializeField] GameObject creditPanel;

        void Start()
        {
            creditButton.onClick.AddListener(OnCreditButton);
            closeCreditButton.onClick.AddListener(OnCloseCreditButton);
            
            creditPanel.SetActive(false);
        }

        public void OnCreditButton()
        {
            Debug.Log("Credit");
            AudioManager.Instance.ShotSE(AudioManager.Instance.SE_tap);
            OpenCreditPanel();
        }

        public void OnCloseCreditButton()
        {
            Debug.Log("Credit Close");
            AudioManager.Instance.ShotSE(AudioManager.Instance.SE_tap);
            CloseCreditPanel();
        }

        private void OpenCreditPanel()
        {
            creditPanel.SetActive(true);
        }

        private void CloseCreditPanel()
        {
            creditPanel.SetActive(false);
        }

    }
}