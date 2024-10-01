using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace StellaCircles.Data
{
    [CreateAssetMenu(menuName = "UnLockWays/UnlockByUpperRankSpecificMaps")]
    public class UnlockByUpperRankSpecificMaps : UnlockWay
    {
        [SerializeField] SelectItemData[] referItemDatas;
        [SerializeField] int[] referItemDataIds;
        [SerializeField] PlayRank upperRank;

        public override bool IsUnLocked()
        {
            int k = 0;
            /*
            for (int i = 0; i < referItemDatas.Length; i++)
            {
                if ((int)referItemDatas[i].gradeData.PlayRankMaxMin[0, 0] >= (int)upperRank
                    || (int) referItemDatas[i].gradeData.PlayRankMaxMin[1, 0] >= (int)upperRank)
                {
                    k++;
                }
                //MapDataBase.Instance.mapDatas[];
                //referItemDatas[i].itemId
                if ((int)referItemDatas[i].gradeData.PlayRankMaxMin[0, 0] >= (int)upperRank
                    || (int) referItemDatas[i].gradeData.PlayRankMaxMin[1, 0] >= (int)upperRank)
                {
                    k++;
                }
            }
            */

            foreach (var id in referItemDataIds)
            {
                if ((int)MapDataBase.Instance.mapDict[id].gradeData.PlayRankMaxMin[0, 0] >= (int)upperRank
                    || (int)MapDataBase.Instance.mapDict[id].gradeData.PlayRankMaxMin[1, 0] >= (int)upperRank)
                {
                    k++;
                }

                if ((int)MapDataBase.Instance.mapDict[id].gradeData.PlayRankMaxMin[0, 0] >= (int)upperRank
                    || (int)MapDataBase.Instance.mapDict[id].gradeData.PlayRankMaxMin[1, 0] >= (int)upperRank)
                {
                    k++;
                }
            }

            if (k >= referItemDataIds.Length)
            {
                return true;
            }
            else return false;
        }
    }
}