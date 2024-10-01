using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FancyScrollView;
using System;
using EasingCore;
using Cysharp.Threading.Tasks;
using StellaCircles.Data;
using StellaCircles.Select;

namespace StellaCircles.ScrollView
{
    class ItemScrollView : FancyScrollView<SelectItemData, ItemContext>
    {
        [SerializeField] SelectManagement selectManagement;

        [SerializeField] private Scroller _scroller = default;
        [SerializeField] private GameObject _cellPrefab;
        public int _selectedIndex;

        Action<int> onSelectionChanged;

        private List<SelectItemData> items;

        protected override GameObject CellPrefab => _cellPrefab;

        protected override void Initialize()
        {
            base.Initialize();

            Context.OnCellClicked = SelectCell;

            _scroller.OnValueChanged(UpdatePosition);
            //_scroller.OnSelectionChanged(UpdateSelection);

            _scroller.OnSelectionChanged(SelectCell);

        }

        //private void UpdateSelection(int index)
        private void UpdateSelection(int mapIndex, int ageIndex)
        {
            //選択中のモノと同じものを選択し、かつAgeからでないなら成績を変更しない
            //Ageを変えた時に前のAgeの成績が乗らないようにするため？
            //if (Context.SelectedIndex == mapIndex  && ageIndex == Context.SelectedAgeIndex && selectManagement.selectState != SelectState.Age)
            if (Context.SelectedIndex == mapIndex && ageIndex == Context.SelectedAgeIndex &&
                SelectStateManager.selectState != SelectState.Age)
            {
                //Debug.Log($"map{mapIndex}, age{ageIndex}選択中のモノと同じものを選択し、SelectState：{selectManagement.selectState}");
                Debug.Log($"map{mapIndex}, age{ageIndex}選択中のモノと同じものを選択し、SelectState：{SelectStateManager.selectState}");
                return;
            }

            //tamesi
            SetBGM(mapIndex);
            //

            //selectManagement.selectState = SelectState.Map;
            SelectStateManager.selectState = SelectState.Map;

            selectManagement.SetMapGradePanel(mapIndex);

            Context.SelectedIndex = mapIndex;
            _selectedIndex = mapIndex;
            Refresh();

            onSelectionChanged?.Invoke(mapIndex);
        }

        public void UpdateData(IList<SelectItemData> items)
        {
            UpdateContents(items);
            _scroller.SetTotalCount(items.Count);
        }

        public void OnSelectionChanged(Action<int> callback)
        {
            onSelectionChanged = callback;
        }

        public void SelectCell(int index)
        {
            SelectCellExtend(index, -1);
        }

        public void SelectCellExtend(int selectMapIndex, int selectAgeIndex)
        {
            selectManagement.SetMapGradePanel(selectMapIndex);

            if (selectMapIndex < 0 || selectMapIndex >= ItemsSource.Count || (selectMapIndex == Context.SelectedIndex &&
                                                                              selectAgeIndex ==
                                                                              Context.SelectedAgeIndex))
            {
                return;
            }

            UpdateSelection(selectMapIndex, selectAgeIndex);
            _scroller.ScrollTo(selectMapIndex, 0.35f, Ease.OutCubic);
        }

        public void SetBGM(int index)
        {
            //Debug.Log($"{index}番が選ばれたので可能なら変更する");
            selectManagement.SetBGM(index, this.GetCancellationTokenOnDestroy()).Forget();
        }
    }
}