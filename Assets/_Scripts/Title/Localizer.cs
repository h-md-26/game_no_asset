using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StellaCircles.Localize
{
    public class Localizer : MonoBehaviour
    {
        [SerializeField] GameObject[] ltObjs;

        public void AllLocalize()
        {

            // 取得したコンポーネントを持つGameObjectを処理
            foreach (var ltObj in ltObjs)
            {
                var lts = ltObj.GetComponents<LocalizeText>();
                foreach (var lt in lts)
                {
                    lt.Localize();
                }
            }
        }
    }
}