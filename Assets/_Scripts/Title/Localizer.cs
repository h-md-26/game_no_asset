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

            // �擾�����R���|�[�l���g������GameObject������
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