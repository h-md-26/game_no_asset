using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StellaCircles.Data
{

    [CreateAssetMenu(menuName = "UnLockWays/UnlockByAllPlayCount")]
    public class UnlockByAllPlayCount : UnlockWay
    {

        [SerializeField] int needCount;

        public override bool IsUnLocked()
        {
            int playCount = 0;
            /*
            for (int i = 0; i < MapDataBase.Instance.mapDatas.Length; i++)
            {
                playCount += MapDataBase.Instance.mapDatas[i].gradeData.playCounts[0] + MapDataBase.Instance.mapDatas[i].gradeData.playCounts[1];
            }
            */
            foreach (var mapData in MapDataBase.Instance.mapDict)
            {
                playCount += mapData.Value.gradeData.playCounts[0] + mapData.Value.gradeData.playCounts[1];
            }

            if (playCount >= needCount)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

}