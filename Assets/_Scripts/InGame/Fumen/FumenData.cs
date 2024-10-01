using UnityEngine;

namespace StellaCircles.InGame
{
    /// <summary>
    /// 譜面のデータを持つ
    /// FumenAnalyzerを通して解析したものを使用する
    /// </summary>
    public class FumenData
    {
        /// <summary>
        /// 解析のいらない情報をセット
        /// </summary>
        private void SetMapInfo(FumenAnalyzer.MapInfo mapInfo)
        {
            bgm = mapInfo.bgm;
            offset = mapInfo.offset;
            mapDifficulty = mapInfo.mapDifficulty;
        }
        
        /// <summary>
        /// 解析した譜面情報をセット
        /// measureBPMs, fumenMeasureStrings, nodeDestinationsをfumenDataに代入する
        /// </summary>
        private void SetFumenInfo(FumenAnalyzer.FumenInfo fumenInfo)
        {
            measureBPMs = fumenInfo.measureBPMs;
            fumenMeasureStrings = fumenInfo.fumenMeasureStrings;
            nodeDestinations = fumenInfo.nodeDestinations;
        }
        
        /// <summary>
        /// 解析したノーツ情報をセット
        /// </summary>
        private void SetNotesInfo(FumenAnalyzer.NotesInfo notesInfo)
        {
            notesCountThisMeasure = notesInfo.notesCountThisMeasure;
            notesCountUpToMeasure = notesInfo.notesCountUpToMeasure;
            allNotesCount = notesInfo.allNotesCount;
            scoreNotesCount = notesInfo.scoreNotesCount;
            measureStartTimes = notesInfo.measureStartTimes;
            measureDurations = notesInfo.measureDurations;
            noteBeatTimes = notesInfo.notesBeatTimes;
            noteTypes = notesInfo.noteTypes;
        }

        /// <summary>
        /// FumenAnalyzerからのみ呼ぶ
        /// </summary>
        public FumenData(FumenAnalyzer.MapInfo mapInfo, FumenAnalyzer.FumenInfo fumenInfo, FumenAnalyzer.NotesInfo notesInfo)
        {
            SetMapInfo(mapInfo);
            SetFumenInfo(fumenInfo);
            SetNotesInfo(notesInfo);
        } 
        
        private AudioClip bgm;
        private float offset;
        // private int[] courseLevels;
        private int mapDifficulty;
        
        
        private float[] measureBPMs; //i小節のBPM
        private string[] fumenMeasureStrings; //i小節目の譜面
        private int[] nodeDestinations; //次のノードの番号の配列(0?5)
        
        
        private int[] notesCountThisMeasure; //i小節目のノーツの数
        private int[] notesCountUpToMeasure; //i小節目までのノーツの合計 [0]:0  notesCountUpToMeasure
        private int allNotesCount; // ノーツの総数
        private int scoreNotesCount; // スコアに加えるノーツの総数(HMを含まないノーツの数)
        private float[] measureStartTimes; //i小節の始まる時間
        private float[] measureDurations; //i小節から(i+1)小節までの時間
        private float[] noteBeatTimes; //i番目のノーツのたたくべき時間
        private NoteType[] noteTypes; //i番目のノーツの種類
        
        
        /// <summary>
        /// この譜面で使用するBGM
        /// </summary>
        public AudioClip BGM => bgm;
        /// <summary>
        /// 譜面のオフセット
        /// </summary>
        public  float Offset => offset;
        
        /// <summary>
        /// 外部からの値取得用プロパティ(毎回クローンするとやばいので呼出側でキャッシュする)
        /// </summary>
        public float[] MeasureBPMs => (float[])measureBPMs.Clone();
        /// <summary>
        /// 外部からの値取得用プロパティ(毎回クローンするとやばいので呼出側でキャッシュする)
        /// </summary>
        public string[] FumenMeasureStrings => (string[])fumenMeasureStrings.Clone();
        /// <summary>
        /// 外部からの値取得用プロパティ(毎回クローンするとやばいので呼出側でキャッシュする)
        /// </summary>
        public int[] NodeDestinations => (int[])nodeDestinations.Clone();
        
        /// <summary>
        /// ノーツの総数
        /// </summary>
        public int AllNotesCount => allNotesCount;
        /// <summary>
        /// スコアに加えるノーツの総数(HMを含まないノーツの数)
        /// </summary>
        public int ScoreNotesCount => scoreNotesCount;
        /// <summary>
        /// 外部からの値取得用プロパティ(毎回クローンするとやばいので呼出側でキャッシュする)
        /// </summary>
        public float[] MeasureStartTimes => (float[])measureStartTimes.Clone();
        /// <summary>
        /// 外部からの値取得用プロパティ(毎回クローンするとやばいので呼出側でキャッシュする)
        /// </summary>
        public float[] MeasureDurations => (float[])measureDurations.Clone();

        /// <summary>
        /// 外部からの値取得用プロパティ(毎回クローンするとやばいので呼出側でキャッシュする)
        /// </summary>
        public float[] NoteBeatTimes => (float[])noteBeatTimes.Clone();
        /// <summary>
        /// 外部からの値取得用プロパティ(毎回クローンするとやばいので呼出側でキャッシュする)
        /// </summary>
        public NoteType[] NoteTypes => (NoteType[])noteTypes.Clone();
    }

    public enum NoteType
    {
        EMPTY = '0',
        TAP = '1',
        FLICK = '2',
        HOLD_START = '3',
        HOLD_END = '4',
        HOLD_MID = '5'
    }

}