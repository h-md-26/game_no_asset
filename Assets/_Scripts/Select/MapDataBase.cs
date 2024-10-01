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
                Debug.Log("�V���O���g�����̂��߂Ɏ����ŌĂ�ł��");
                // �V���O���g�����̂��߂Ɏ����ŌĂ�ł��
                MapDataBase.Instance.GradeDataLoad();
            }
        }

        // MapData�̓C���X�y�N�^�[�ォ��ݒ肷��
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
            // ���уf�[�^�̃��[�h(�f�[�^���Ȃ��ꍇ�͏�����)
            // i�Ԗڂ̃f�[�^��itemid���L�[�Ƃ��ăf�[�^�����[�h����
            
            if(!isMapDictLoaded) CreateMapDataDict();
            
            foreach (var m in mapDict)
            {
                //Debug.Log($"id:{m.Value.infoData.itemId} = dictKey{m.Key} �̐��т̃��[�h");
                mapDict[m.Key].gradeData = SaveManager.LoadGradeData(m.Value.infoData.itemId.ToString());
            }
        }

        public void ResetGradeData()
        {
            // MapDatas�ɓo�^����Ă���id���L�[�Ƃ��āA���v���C�̐��уf�[�^��������

            if(!isMapDictLoaded) CreateMapDataDict();
            
            foreach (var m in mapDict)
            {
                Debug.Log($"id:{m.Value.infoData.itemId} = dictKey{m.Key} �̐��т̃��Z�b�g");
                mapDict[m.Key].gradeData.GradeInitialize();
                SaveManager.SaveGradeData(m.Value.infoData.itemId.ToString(), m.Value.gradeData);
            }
        }

        public void CountUpPlayCount(SelectItemData itemData, Difficulty difficulty)
        {
            Debug.Log(
                $"{itemData.itemName}�̃v���C��Up {itemData.GetGradeData().playCounts[(int)difficulty]}��{itemData.GetGradeData().playCounts[(int)difficulty] + 1}");
            itemData.GetGradeData().playCounts[(int)difficulty]++;

            SaveManager.SaveGradeData(itemData.itemId.ToString(), itemData.GetGradeData());
        }

        //�X�R�A�X�V�����s
        public void UpdateGradeData(RecordType recordType, SelectItemData itemData, Difficulty difficulty,
            GameScoreModel gameScoreModel)
        {
            itemData.GetGradeData().playScores[(int)difficulty] = gameScoreModel.currentScore;
            
            // GREAT ~ MISS ���Z�b�g
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