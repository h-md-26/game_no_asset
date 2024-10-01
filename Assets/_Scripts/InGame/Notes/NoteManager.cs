using System.Collections;
using System.Collections.Generic;
using StellaCircles.Data;
using UnityEngine;

namespace StellaCircles.InGame
{
    public class NoteManager : MonoBehaviour
    {
        /// <summary>
        /// �m�[�c�̏o������A���莞�̓����𐧌�
        /// </summary>

        [SerializeField] bool isScale;

        [SerializeField] float scalingSpeed;

        [SerializeField] GameObject[] child;

        Vector3 _scale;

        
        // �J�ڃp�����[�^���L���b�V��
        private readonly int appearParameterHash = Animator.StringToHash("Appear");
        private readonly int idleParameterHash= Animator.StringToHash("Idle");
        private readonly int judgingParameterHash= Animator.StringToHash("Judging");
        private readonly int judgedParameterHash = Animator.StringToHash("Judged");
        private readonly int disappearParameterHash= Animator.StringToHash("Disappear");

        Animator animator;

        // Start is called before the first frame update
        public void NoteStart()
        {
            //isScale = false;
            animator = GetComponent<Animator>();
            
            // �J�ڃp�����[�^���n�b�V���ŃL���b�V������

            if (!GameSetting.gameSettingValue.isEffect)
            {
                transform.localScale = new Vector3(1.5f, 1.5f, 1f);
                foreach (GameObject g in child)
                {
                    g.SetActive(false);
                }
            }
        }

        public void Appear(NoteState preState)
        {
            //����UNAPPEAR�łȂ��m�[�c�͕\�����Ȃ�
            if (preState != NoteState.UNAPPEAR)
            {
                // Debug.Log($"����UNAPPEAR�łȂ��m�[�c�͕\�����Ȃ� preState: {preState}");
                return;
            }

            animator.SetTrigger(appearParameterHash);

            /*
            isScale=true;
            _scale = Vector3.zero;
            */
        }

        public void Judged()
        {
            animator.SetTrigger(judgedParameterHash);
        }

        
        /// <summary>
        /// Animation�̃g���K�[�ɐݒ�
        /// </summary>
        public void ActiveFalse()
        {
            gameObject.SetActive(false);
        }

        public void Judging()
        {
            // Debug.Log($"Judging : {name}");
            animator.SetTrigger(judgingParameterHash);
        }

        public void Idle()
        {
            animator.SetTrigger(idleParameterHash);
        }

        // ��\���ɂ���Ƃ��Ɏg�p
        public void Disappear()
        {
            animator.SetTrigger(disappearParameterHash);
        }
    }
}