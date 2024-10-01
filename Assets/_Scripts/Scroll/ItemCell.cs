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
            // image�̃��[�h�����܂ŕʂ̉摜�����Ă���
            //_image.sprite = itemData.itemSprite;
            //Debug.Log("�W���P�b�g�摜���X�V");
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
                    _lockedText.text = "���J��";
                }
                else if (GameSetting.gameSettingValue.languageType == LanguageType.EN)
                {
                    _lockedText.text = "Locked!";
                }
                else
                {
                    _lockedText.text = "���J��";
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

                // �^�񒆂ɗ�����t���O�𗧂Ă�
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
            //    Debug.Log("���l");
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

                // �^�񒆂ɗ�����t���O�𗧂Ă�
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

        // GameObject ����A�N�e�B�u�ɂȂ�� Animator �����Z�b�g����Ă��܂�����
        // ���݈ʒu��ێ����Ă����� OnEnable �̃^�C�~���O�Ō��݈ʒu���Đݒ肵�܂�
        float currentPosition = 0;

        void OnEnable() => UpdatePosition(currentPosition);
    }
}