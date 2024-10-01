using Cysharp.Threading.Tasks;
using FancyScrollView;
using StellaCircles.AssetBundleManagement;
using StellaCircles.Data;
using StellaCircles.Localize;
using StellaCircles.Select;
using UnityEngine;
using UnityEngine.UI;

namespace StellaCircles.ScrollView
{

    class ItemCell : FancyCell<SelectItemData, ItemContext>
    {
        [SerializeField] Animator animator = default;

        static class AnimatorHash
        {
            public static readonly int Scroll = Animator.StringToHash("scroll");
        }

        [SerializeField] Text _txtName;
        [SerializeField] Text _lockedText;
        [SerializeField] public Image _image;
        [SerializeField] GameObject _lockImage;
        [SerializeField] Text _unlockWayText;
        [SerializeField] GameObject _backLight;
        [SerializeField] Button _button;

        RectTransform rT;

        public override void Initialize()
        {
            rT = GetComponent<RectTransform>();

            //_button.onClick.AddListener(() => Context.OnCellClicked?.Invoke(Index));

        }

        public override void UpdateContent(SelectItemData itemData)
        {

            //_txtName.text = itemData.itemName;
            _txtName.text = itemData.GetItemName();
            // imageのロード完了まで別の画像を入れておく
            //_image.sprite = itemData.itemSprite;
            //Debug.Log("ジャケット画像を更新");
            _image.sprite = ImageABStocker.Instance.GetJacketImage(itemData.itemId);

            if (itemData.itemUnLockWay.IsUnLocked())
            {
                _lockImage.SetActive(false);
            }
            else
            {
                //_unlockWayText.text = itemData.itemUnLockWayText;
                if (GameSetting.gameSettingValue.languageType == LanguageType.JP)
                {
                    _lockedText.text = "未開放";
                }
                else if (GameSetting.gameSettingValue.languageType == LanguageType.EN)
                {
                    _lockedText.text = "Locked!";
                }
                else
                {
                    _lockedText.text = "未開放";
                }

                _unlockWayText.text = itemData.GetUnLockWayText();
                _lockImage.SetActive(true);
            }

            if (Context.SelectedIndex == Index)
            {
                //Debug.Log($"SelectedIndex:{Context.SelectedIndex}");
                _backLight.gameObject.SetActive(true);
            }
            else
            {
                _backLight.gameObject.SetActive(false);

            }
        }


        public bool isSelectedCenterCell = false;
        public bool isSelectDiff = false;

        private void Update()
        {
            /*
            if (Context.SelectedIndex == Index)
            {
                //Debug.Log($"{Context.SelectedIndex} == {Index} : rt = {rT.position}");

                // 真ん中に来たらフラグを立てて
                if (rT.position.x < 0.5f && rT.position.x > -0.5f)
                {
                    isSelectedCenterCell = true;
                }
                else
                {
                    isSelectedCenterCell = false;
                }
            }
            else
            {
                isSelectedCenterCell = false;
            }
            */

            isSelectedCenterCell = CheckIsSelectedCenterCell();

            //if (isSelectDiff && !isSelectedCenterCell)
            //{
            //    Debug.Log("自粛");
            //    gameObject.SetActive(false);
            //}

            //if(isSelectDiff && Mathf.Abs( transform.position.x) > 1)
            //{
            //    gameObject.SetActive(false);
            //}
            if (SelectStateManager.selectState == SelectState.Difficult && Mathf.Abs(transform.position.x) > 1)
            {
                gameObject.SetActive(false);
            }
        }

        public bool CheckIsSelectedCenterCell()
        {
            if (Context.SelectedIndex == Index)
            {
                //Debug.Log($"{Context.SelectedIndex} == {Index} : rt = {rT.position}");

                // 真ん中に来たらフラグを立てて
                if (rT.position.x < 0.5f && rT.position.x > -0.5f)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public override void UpdatePosition(float position)
        {
            currentPosition = position;
            if (animator.isActiveAndEnabled)
            {
                animator.Play(AnimatorHash.Scroll, -1, position);
            }

            animator.speed = 0;
        }

        // GameObject が非アクティブになると Animator がリセットされてしまうため
        // 現在位置を保持しておいて OnEnable のタイミングで現在位置を再設定します
        float currentPosition = 0;

        void OnEnable() => UpdatePosition(currentPosition);
    }
}