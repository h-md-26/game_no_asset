using System.Collections;
using System.Collections.Generic;
using StellaCircles.Data;
using UnityEngine;

namespace StellaCircles.InGame
{
    public class NoteManager : MonoBehaviour
    {
        /// <summary>
        /// ノーツの出現時や、判定時の動きを制御
        /// </summary>

        [SerializeField] bool isScale;

        [SerializeField] float scalingSpeed;

        [SerializeField] GameObject[] child;

        Vector3 _scale;

        
        // 遷移パラメータをキャッシュ
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
            
            // 遷移パラメータをハッシュでキャッシュする

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
            //既にUNAPPEARでないノーツは表示しない
            if (preState != NoteState.UNAPPEAR)
            {
                // Debug.Log($"既にUNAPPEARでないノーツは表示しない preState: {preState}");
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
        /// Animationのトリガーに設定
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

        // 非表示にするときに使用
        public void Disappear()
        {
            animator.SetTrigger(disappearParameterHash);
        }
    }
}