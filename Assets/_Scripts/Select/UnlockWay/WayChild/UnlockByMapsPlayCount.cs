using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace StellaCircles.Data
{
    [CreateAssetMenu(menuName = "UnLockWays/UnlockByMapsPlayCount")]
    public class UnlockByMapsPlayCount : UnlockWay
    {
        [SerializeField] SelectItemData[] referItemDatas;
        [SerializeField] int[] referItemDataIds;
        [SerializeField] int needCount;

        public override bool IsUnLocked()
        {
            int playCount = 0;
            /*
            for (int i = 0; i < referItemDatas.Length; i++)
            {
                Debug.Log($"{referItemDatas[i].itemName}のプレイ回数 EZ:{referItemDatas[i].gradeData.playCounts[0]}, HD:{referItemDatas[i].gradeData.playCounts[1]}");
                playCount += referItemDatas[i].gradeData.playCounts[0] + referItemDatas[i].gradeData.playCounts[1];
            }
            */
            foreach (var id in referItemDataIds)
            {
                //Debug.Log($"{MapDataBase.Instance.mapDict[id].infoData.itemName}のプレイ回数 EZ:{MapDataBase.Instance.mapDict[id].gradeData.playCounts[0]}, HD:{MapDataBase.Instance.mapDict[id].gradeData.playCounts[1]}");
                playCount += MapDataBase.Instance.mapDict[id].gradeData.playCounts[0] +
                             MapDataBase.Instance.mapDict[id].gradeData.playCounts[1];
            }

            if (playCount >= needCount)
            {
                return true;
            }
            else
            {
                Debug.Log($"現在のプレイ回数：{playCount}/{needCount}");
                return false;
            }
        }
    }
}
