using System;
using System.Collections;
using System.Collections.Generic;
using StellaCircles.Data;
using StellaCircles.InGame;
using StellaCircles.Select;
using StellaCircles.Utils;
using UnityEngine;

namespace StellaCircles.Data
{
    public class MapDataBase : Singleton<MapDataBase>
    {
        public override void Awake()
        {
            base.Awake();

            mapDBA = new MapDataBaseAnalyse(this);

            if (MapDataBase.Instance == null)
            {
                Debug.Log("シングルトン化のために自分で呼んでやる");
                // シングルトン化のために自分で呼んでやる
                MapDataBase.Instance.GradeDataLoad();
            }
        }

        // MapDataはインスペクター上から設定する
        public MapData[] mapDatas;

        public Dictionary<int, MapData> mapDict = new Dictionary<int, MapData>();

        //public SelectItemData[] mapDatas;
        public MapDataBaseAnalyse mapDBA;
        
        private bool isMapDictLoaded = false;
        
        public void CreateMapDataDict()
        {
            mapDict.Clear();
            for (int i = 0; i < mapDatas.Length; i++)
            {
                mapDict.Add(mapDatas[i].infoData.itemId, mapDatas[i]);
            }
            
            isMapDictLoaded = true;
        }

        public void GradeDataLoad()
        {
            // 成績データのロード(データがない場合は初期化)
            // i番目のデータのitemidをキーとしてデータをロードする
            
            if(!isMapDictLoaded) CreateMapDataDict();
            
            foreach (var m in mapDict)
            {
                //Debug.Log($"id:{m.Value.infoData.itemId} = dictKey{m.Key} の成績のロード");
                mapDict[m.Key].gradeData = SaveManager.LoadGradeData(m.Value.infoData.itemId.ToString());
            }
        }

        public void ResetGradeData()
        {
            // MapDatasに登録されているidをキーとして、未プレイの成績データを代入する

            if(!isMapDictLoaded) CreateMapDataDict();
            
            foreach (var m in mapDict)
            {
                Debug.Log($"id:{m.Value.infoData.itemId} = dictKey{m.Key} の成績のリセット");
                mapDict[m.Key].gradeData.GradeInitialize();
                SaveManager.SaveGradeData(m.Value.infoData.itemId.ToString(), m.Value.gradeData);
            }
        }

        public void CountUpPlayCount(SelectItemData itemData, Difficulty difficulty)
        {
            Debug.Log(
                $"{itemData.itemName}のプレイ回数Up {itemData.GetGradeData().playCounts[(int)difficulty]}→{itemData.GetGradeData().playCounts[(int)difficulty] + 1}");
            itemData.GetGradeData().playCounts[(int)difficulty]++;

            SaveManager.SaveGradeData(itemData.itemId.ToString(), itemData.GetGradeData());
        }

        //スコア更新時実行
        public void UpdateGradeData(RecordType recordType, SelectItemData itemData, Difficulty difficulty,
            GameScoreModel gameScoreModel)
        {
            itemData.GetGradeData().playScores[(int)difficulty] = gameScoreModel.currentScore;
            
            // GREAT ~ MISS をセット
            for (int j = (int)JudgeResultType.GREAT; j <= (int)JudgeResultType.MISS; j++)
            {
                itemData.GetGradeData().gradeJudgeResultMaxMins[(int)difficulty, (int)recordType * 4 + j] = gameScoreModel.judgeCountDictionary[(JudgeResultType)j];
            }

            itemData.GetGradeData().PlayRankMaxMin[(int)difficulty, (int)recordType] = gameScoreModel.playRank;

            SaveManager.SaveGradeData(itemData.itemId.ToString(), itemData.GetGradeData());
        }
    }

    public enum RecordType
    {
        Best = 0,
        Worst = 1
    }
}