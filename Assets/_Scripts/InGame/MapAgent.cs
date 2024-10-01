using System.Collections;
using System.Collections.Generic;
using StellaCircles.Data;
using StellaCircles.Utils;
using UnityEngine;

namespace StellaCircles
{
    public class MapAgent : Singleton<MapAgent>
    {
        [SerializeField] public SelectItemData mapData;
        [SerializeField] public int difficult;

        public void SetMapData(SelectItemData _mapData, int _difficult)
        {
            mapData = _mapData;
            difficult = _difficult;
        }

        public (SelectItemData, int) GetMapData()
        {
            return (mapData, difficult);
        }
    }
}