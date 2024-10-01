using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StellaCircles.Data
{
    [CreateAssetMenu(menuName = "UnLockWays/UnlockByDefault")]
    public class UnlockByDefault : UnlockWay
    {
        public override bool IsUnLocked()
        {
            return true;
        }
    }
}