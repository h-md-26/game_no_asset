using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StellaCircles.Data
{
    public abstract class UnlockWay : ScriptableObject
    {
        public abstract bool IsUnLocked();
    }
}