using UnityEngine;
using System.Linq;
using StellaCircles.Data;
using StellaCircles.Select;
using UnityEngine.UI;

namespace StellaCircles.ScrollView
{
    public class TestItemViewer : MonoBehaviour
    {
        public SelectItemData[] selectItems;
        [SerializeField] private ItemScrollView _itemScrollView;
        [SerializeField] private SelectManagement selectManagement;

        [SerializeField] Button selectButton;
        /*
        void Start()
        {
            //selectButton.onClick.AddListener(OnSelectButton);

            _itemScrollView.OnSelectionChanged(OnSelectionChanged);

            _itemScrollView.UpdateData(selectItems);
            _itemScrollView.SelectCell(0);
        }
        */

        public void ViwerStart()
        {
            //selectButton.onClick.AddListener(OnSelectButton);

            _itemScrollView.OnSelectionChanged(OnSelectionChanged);

            _itemScrollView.UpdateData(selectItems);
            //_itemScrollView.SelectCell(0);
        }



        void OnSelectionChanged(int index)
        {
            //Debug.Log($"Selected item info: index {index}");
        }

        public void OnSelectButton()
        {
            //Debug.Log($"Buttoned item info: index {_itemScrollView._selectedIndex}");
            selectManagement.OnClickMapSelectButton(_itemScrollView._selectedIndex);
        }
    }
}