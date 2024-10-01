using System.Collections;
using System.Collections.Generic;
using StellaCircles.Data;
using StellaCircles.Localize;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace StellaCircles.Title
{
    /// <summary>
    /// Help��ʊ֘A�̏������܂Ƃ߂��N���X
    /// </summary>
    public class HelpManager : MonoBehaviour
    {
        [SerializeField] Button helpButton;
        [SerializeField] Button helpOkButton;
        [SerializeField] Button nextPageButton;
        [SerializeField] Button backPageButton;
        
        [SerializeField] GameObject helpPanel;

        [SerializeField] HelpPanel[] helpPagePanels;
        [SerializeField] Text pageLabel; //�V�ѕ�(m/n)
        [SerializeField] Text themeLabel; //������@/�Q�[�����[�� �Ȃǌ��o��

        int currentPage;

        private void Start()
        {
            helpButton.onClick.AddListener(OnHelpButton);
            helpOkButton.onClick.AddListener(OnClickOK);
            nextPageButton.onClick.AddListener(() => OnClickArrow(1));
            backPageButton.onClick.AddListener(() => OnClickArrow(-1));
            
            helpPanel.SetActive(false);
        }

        private void HelpStart()
        {
            currentPage = 0;
            DisplayPage(currentPage);
        }

        void DisplayPage(int page)
        {

            if (page < 0 || page >= helpPagePanels.Length)
            {
                Debug.Log("Errrrrrrrrrr");
                return;
            }

            Debug.Log($"�y�[�W{page}��\������");

            foreach (var hp in helpPagePanels)
            {
                hp.gameObject.SetActive(false);
            }

            helpPagePanels[page].gameObject.SetActive(true);
            pageLabel.text = $"�V�ѕ�({page + 1}/{helpPagePanels.Length})";
            if (GameSetting.gameSettingValue.languageType == LanguageType.JP)
            {
                pageLabel.text = $"�V�ѕ�({page + 1}/{helpPagePanels.Length})";
            }
            else if (GameSetting.gameSettingValue.languageType == LanguageType.EN)
            {
                pageLabel.text = $"How to play({page + 1}/{helpPagePanels.Length})";
            }

            themeLabel.text = helpPagePanels[page].GetLangText();
        }


        private void OnClickArrow(int i)
        {
            //���E�̖�󂪉����ꂽ�̂ŁA�y�[�W��ύX����
            Debug.Log($"PageChange :{currentPage} -> {(currentPage + i) % helpPagePanels.Length}");
            currentPage = (currentPage + helpPagePanels.Length + i) % helpPagePanels.Length;
            DisplayPage(currentPage);
        }

        private void OnClickOK()
        {
            //�p�l�������
            AudioManager.Instance.ShotSE(AudioManager.Instance.SE_tap);
            CloseHelpPanel();
        }

        private void CloseHelpPanel()
        {
            helpPanel.SetActive(false);
        }

        private void OnHelpButton()
        {
            //�p�l�����J��
            AudioManager.Instance.ShotSE(AudioManager.Instance.SE_tap);
            OpenHelpPanel();
        }

        private void OpenHelpPanel()
        {
            HelpStart();
            helpPanel.SetActive(true);
        }
    }
}