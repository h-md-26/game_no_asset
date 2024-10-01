using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StellaCircles.Data
{
    [CreateAssetMenu]
    public class MapData : ScriptableObject
    {
        public SelectItemData infoData;

        public MapGradeData gradeData;
    }
}