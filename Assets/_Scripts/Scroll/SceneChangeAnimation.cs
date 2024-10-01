using System.Collections;
using System.Collections.Generic;
using StellaCircles.Utils;
using UnityEngine;

namespace StellaCircles.ViewUtils
{

    public class SceneChangeAnimation : Singleton<SceneChangeAnimation>
    {
        [SerializeField] Animator animator;

        readonly int IdleHash = Animator.StringToHash("CloseIdle");
        readonly int CloseHash = Animator.StringToHash("Close");

        public void PlayCloseAnim()
        {
            if (animator.GetCurrentAnimatorStateInfo(0).shortNameHash == IdleHash)
            {
                animator.SetTrigger("Close");
            }
        }

        public void PlayOpenAnim()
        {
            if (animator.GetCurrentAnimatorStateInfo(0).shortNameHash == CloseHash)
            {
                animator.SetTrigger("Open");
            }
        }
    }

}