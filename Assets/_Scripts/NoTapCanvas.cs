using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using StellaCircles.Utils;
using UnityEngine;


namespace StellaCircles.ViewUtils
{
    public class NoTapCanvas : Singleton<NoTapCanvas>
    {
        [SerializeField] GameObject noTapPanel;

        public override void Awake()
        {
            base.Awake();
            
            noTapPanel.SetActive(false);
        }
        
        /// <summary>
        /// 指定秒数だけ画面上に透明なパネルを表示することでタップできなくする
        /// </summary>
        /// <param name="s"></param>
        public void NoTapSeconds(float s)
        {
            NoTapSecondsCo(s, this.GetCancellationTokenOnDestroy()).Forget();
        }

        private async UniTaskVoid NoTapSecondsCo(float s, CancellationToken ct)
        {
            noTapPanel.SetActive(true);

            await UniTask.Delay((int)(s * 1000), cancellationToken: ct);

            noTapPanel.SetActive(false);
        }

        public void NoTapHalfSeconds()
        {
            NoTapSecondsCo(0.5f, this.GetCancellationTokenOnDestroy()).Forget();
        }

        public void NoTap1Second()
        {            
            NoTapSecondsCo(1f, this.GetCancellationTokenOnDestroy()).Forget();
        }

    }
}