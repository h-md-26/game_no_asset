using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace StellaCircles.Data
{
    /// <summary>
    /// MapDataBaseの情報からセレクト画面で必要な情報を計算する
    /// </summary>
    public class MapDataBaseAnalyse
    {
        public MapDataBaseAnalyse(MapDataBase _mapDB)
        {
            mapDB = _mapDB;
        }
        
        
        private const int AGE_LENGTH = 5;
        private const int RANK_LENGTH = 7;
        
        MapDataBase mapDB;

        // ageごとにどのmap(id)があるか
        //public int[,] mapPerAgeIDs;
        //public int[,] mapPerAgeUnlockIDs;
        public int[,] mapIdPerAgeIDs;
        public int[,] mapIdPerAgeUnlockIDs;
        public int[] countAges = new int[AGE_LENGTH];
        public int[] countAgeUnlocks = new int[AGE_LENGTH];

        // Ageごとの各ランク数  [ Age , Rank ] 
        public int[,] rankPerAges;

        public void SetInfos()
        {
            mapIdPerAgeIDs = new int[AGE_LENGTH, mapDB.mapDict.Count];
            mapIdPerAgeUnlockIDs = new int[AGE_LENGTH, mapDB.mapDict.Count];

            for (int i = 0; i < mapIdPerAgeIDs.GetLength(0); i++)
            {
                for (int j = 0; j < mapIdPerAgeIDs.GetLength(1); j++)
                {
                    mapIdPerAgeIDs[i, j] = -1;
                }
            }

            //年代ごとにどのidの曲があるか登録する
            int[] _countAges = new int[AGE_LENGTH];
            int[] _countAgeUnlocks = new int[AGE_LENGTH];

            foreach (var m in mapDB.mapDict)
            {
                int _age = (int)m.Value.infoData.itemAge;

                mapIdPerAgeIDs[_age, _countAges[_age]] = m.Value.infoData.itemId;
                _countAges[_age]++;

                if (m.Value.infoData.itemUnLockWay.IsUnLocked())
                {
                    //Debug.Log($"id ; { m.Value.infoData.itemId}");
                    mapIdPerAgeUnlockIDs[_age, _countAgeUnlocks[_age]] = m.Value.infoData.itemId;
                    _countAgeUnlocks[_age]++;
                }
            }



            countAges = _countAges;
            countAgeUnlocks = _countAgeUnlocks;

            // 各年代ごとの各Rank数を取得
            rankPerAges = new int[AGE_LENGTH, RANK_LENGTH];
            for (int i = 0; i < countAges.Length; i++)
            {
                for (int j = 0; j < countAges[i]; j++)
                {
                    if (mapDB.mapDict[mapIdPerAgeIDs[i, j]].gradeData.PlayRankMaxMin[0, 0] != PlayRank.YET)
                    {
                        rankPerAges[i, (int)mapDB.mapDict[mapIdPerAgeIDs[i, j]].gradeData.PlayRankMaxMin[0, 0]]++;
                    }

                    if (mapDB.mapDict[mapIdPerAgeIDs[i, j]].gradeData.PlayRankMaxMin[1, 0] != PlayRank.YET)
                    {
                        rankPerAges[i, (int)mapDB.mapDict[mapIdPerAgeIDs[i, j]].gradeData.PlayRankMaxMin[1, 0]]++;
                    }
                }
            }
        }

    }
}