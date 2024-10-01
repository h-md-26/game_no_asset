using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using StellaCircles.AssetBundleManagement;
using StellaCircles.Data;
using StellaCircles.Utils;
using UnityEngine;

namespace StellaCircles.InGame
{
    /// <summary>
    /// 譜面を解析してFumenDataとして渡す
    /// </summary>
    public class FumenAnalyzer
    {
        private SelectItemData mapData; // mapData 初めのBPMにアクセスするために使用
        private string[] fumenDataStrings; //コマンド入り行別譜面データ
        private float offset; // 曲と譜面の開始位置のオフセット
        
        /// <summary>
        /// mapDataと難易度情報からFumenDataを作成して返す
        /// </summary>
        public async UniTask<FumenData> AnalyzeMapData(SelectItemData mapData, int mapDifficulty, CancellationToken ct)
        {
            this.mapData = mapData;
            offset = mapData.itemOffset;
            // mapDataのidから音楽をロードする
            var bgm = await mapData.LoadItemMusic(ct);
            
            fumenDataStrings = GetFumenDataStrings(this.mapData, mapDifficulty);
            
            // 解析のいらない情報を代入する
            var mapInfo = new MapInfo(bgm, offset, mapDifficulty);
            
            // 解析した譜面情報を代入する
            var fumenInfo = AnalyzeFumenInfo();
            
            // 解析したノーツ情報を代入する
            var notesInfo = AnalyzeNotesInfo(fumenInfo);
            
            return new FumenData(mapInfo, fumenInfo, notesInfo);
        }
        
        
        // 譜面情報を行ごとに分割した配列を返す
        private string[] GetFumenDataStrings(SelectItemData mapData, int mapDifficulty)
        {
            return mapData.itemMaps[mapDifficulty].text.Split('\r', '\n');
        }

        #region FumenInfoの解析
        
        private bool isHolding = false; // Hold中かどうか
        int currentNode = 0; // 現在居るノードの番号 0?5まで
        bool isClockwise = true; // 現在のまわり方が時計回りかどうか
        bool isNextClockwise = true; // CROSSした後のまわり方が時計回りかどうか
        bool isCross = false; // 次に 1, 4 ノードに来た時、CROSSするかどうか
        int isWarp = -1; // ワープ先のノード ワープしないなら負の数を入れる
        
        // 譜面情報を #コマンド と Hold を読み取って生成する
        private FumenInfo AnalyzeFumenInfo()
        {
            /*
            やること
                FumenInfo : measureBPMs, fumenMeasureStrings, nodeDestinations
                を fumenDataStringsから解析する
            
                measureBPMs は
                    #BPM を目印に生成する
                
                fumenMeasureStrings は
                    #コマンドがない行を入れる
                    HoldStartとHoldEndの間はHoldMidで埋める
                    
                nodeDestinations は
                    #WARP , #CROSS_CR , #CROSS_CCR を目印に生成する
            
            */

            float currentBPM = mapData.itemBPM; //小節ごとのBPMを入れる
            string notesStringStock = ""; // 行をまたぐ譜面情報の場合に目の行の文字列を保存しておくための変数
            isHolding = false; // Hold中かどうかを入れておく
            currentNode = 0; // 現在居るノードの番号 0?5まで
            isClockwise = true; // 現在のまわり方が時計回りかどうか
            isNextClockwise = true; // CROSSした後のまわり方が時計回りかどうか
            isCross = false; // 次に 1, 4 ノードに来た時、CROSSするかどうか
            isWarp = -1; // ワープ先のノード ワープしないなら負の数を入れる

            var measureBPMList = new List<float>();
            var fumenMeasureStringList = new List<string>();
            var nodeDestinationList = new List<int>();

            // 始まりの動きは ノード3 → 4 → 5 → 0 で固定
            for (int i = 3; i < 6; i++)
            {
                fumenMeasureStringList.Add("0,");
                measureBPMList.Add(currentBPM);
                nodeDestinationList.Add(i % 6);
            }

            for(int i = 0; i < fumenDataStrings.Length; i++)
            {
                if (fumenDataStrings[i].Length < 1) continue;
                char[] fumenLine = fumenDataStrings[i].ToCharArray();
                if (fumenLine[0] == '#') //#コマンドの処理
                {
                    string[] sharpCommand = fumenDataStrings[i].Split(' ');

                    switch (sharpCommand[0])
                    {
                        case "#BPM":
                            // BPMを sharpCommand[1] に変更する
                            currentBPM = float.Parse(sharpCommand[1]);
                            break;
                        case "#DELAY":
                            // sharpCommand[1] だけかけて１小節移動する
                            measureBPMList.Add(240 / float.Parse(sharpCommand[1]));
                            fumenMeasureStringList.Add("0");
                            nodeDestinationList.Add(currentNode);
                            currentNode = NextNode;
                            break;
                        case "#CROSS_CW":
                            //1→4,4→1 と渡って、時計回り
                            //crossフラグをたて、1 or 4 に来た時に nowNode + 3 に渡り、以降 時計回り
                            isCross = true;
                            isNextClockwise = true;
                            break;
                        case "#CROSS_CCW":
                            //1→4,4→1 と渡って、反時計回り
                            //crossフラグをたて、1 or 4 に来た時に nowNode + 3 に渡り、以降 反時計回り
                            isCross = true;
                            isNextClockwise = false;
                            break;
                        case "#WARP":
                            // sharpCommand[1] に渡り、以降 時計回り
                            isWarp = int.Parse(sharpCommand[1]);
                            break;
                    }
                }
                else if (fumenLine[^1] == ',') //コマンドでない行 かつ 行末に「,」がある場合
                {
                    measureBPMList.Add(currentBPM);
                    fumenMeasureStringList.Add(notesStringStock + fumenDataStrings[i]);
                    notesStringStock = "";
                    nodeDestinationList.Add(currentNode);
                    
                    currentNode = NextNode;
                }
                else //数字のみのとき、次の行に同じ小節の譜面が続く
                {
                    notesStringStock += fumenDataStrings[i];
                }
            }

            nodeDestinationList.Add(currentNode);

            var measureBPMs = measureBPMList.ToArray();
            var fumenMeasureStrings = fumenMeasureStringList.ToArray();
            var nodeDestinations = nodeDestinationList.ToArray();

            //HoldStartとHoldEndの間をHoldMidに変換する
            for (int i = 0; i < fumenMeasureStrings.Length; i++)
            {
                char[] fumenLine = fumenMeasureStrings[i].ToCharArray();
                fumenMeasureStrings[i] = HoldConvert(fumenLine);
            }

            return new FumenInfo(measureBPMs, fumenMeasureStrings, nodeDestinations);
        }
        
        // fumenMeasureStrings の HoldStartとHoldEndの間をHoldMidに置き換える
        private string HoldConvert(char[] line)
        {
            /* やること
            fumenMeasureStrings の要素に対して
            HoldStartからHoldEndの間を16分間隔でHoldMidを入れる処理を行う
            
            行をまたいだHoldについてはisHoldingを使って検知する
            */
            
            /* 実装
            _c 中にHoldが含まれる or _isHolding が true のとき、
            その小節の分割数 m = _c.Length -1
            (16 と m の最小公倍数 s )の要素数の配列を作成し、
            s/m おきに _cの譜面を入れていく。ただし、Hold中の部分には 0 を入れていく
            Hold開始 or _isHoldingの時ははじめから、 s/16 おきに Hold中のノーツ情報を入れる
            Hold終が現れたら、Hold終の前の(s/16 -1)個に 0 を入れ、_isHolding = false する。
            Hold中が小節の始めにあり、その小節が角で始まる場合は、HoldCurveを入れる
            完成した配列を文字列に直し、返す
            */

            if (isHolding || CheckContainHoldStart(line))
            {
                int m = line.Length - 1;
                int s = MathUtils.GetLCM(m, 16);
                int hold_i = 0; //Holdの開始位置(既にHold中なら0)
                char[] _nc = new char[s + 1];

                for (int i = 0; i < s + 1; i++)
                {
                    if (i % (s / m) == 0)
                    {
                        _nc[i] = line[i / (s / m)];
                    }
                    else
                    {
                        _nc[i] = (char)NoteType.EMPTY;
                    }
                }

                if (isHolding)
                {
                    hold_i = 0;
                }

                for (int i = 0; i < s; i++)
                {

                    if (_nc[i] == (char)NoteType.HOLD_END)
                    {
                        isHolding = false;

                        for (int j = 1; j < (s / 16); j++)
                        {
                            if (i - j >= 0)
                            {
                                //Debug.Log($"i = {i}, j = {j}, i - j = {(i - j)}");
                                _nc[i - j] = (char)NoteType.EMPTY;
                            }
                            else
                                break;
                        }

                    }

                    if (isHolding)
                    {
                        if (_nc[i] != (char)NoteType.EMPTY)
                        {
                            _nc[i] = (char)NoteType.EMPTY;
                        }

                        if ((i - hold_i) % (s / 16) == 0)
                        {
                            //角だったらカーブを入れる
                            _nc[i] = (char)NoteType.HOLD_MID;
                        }
                    }

                    if (_nc[i] == (char)NoteType.HOLD_START)
                    {
                        isHolding = true;
                        hold_i = i;
                    }

                }


                return new string(_nc);
            }

            return new string(line);
        }

        // HoldStartを含むかチェック
        private bool CheckContainHoldStart(char[] line)
        {
            for (int i = 0; i < line.Length - 1; i++)
            {
                if (line[i] == (char)NoteType.HOLD_START)
                    return true;
            }

            return false;
        }

        // 次のノード番号をワープやクロスなどを考慮して返す
        private int NextNode
        {
            get
            {
                // 優先順位
                // Warp > Cross > Clockwise

                // Warpしないとき _isWarp -1 , Warpするとき _isWarp 0～5
                if (isWarp >= 0)
                {
                    if (isWarp == currentNode)
                    {
                        Debug.Log("warning:ワープ先が現在地になっている");
                    }

                    var r = isWarp;
                    isWarp = -1;
                    return r;
                }

                if (isCross && (currentNode == 1 || currentNode == 4))
                {
                    isClockwise = isNextClockwise;
                    isCross = false;
                    return (currentNode + 3) % 6;
                }

                if (isClockwise)
                {
                    return (currentNode + 1) % 6;
                }
                else
                {
                    return (currentNode + 5) % 6;
                }
            }
        }
        
        #endregion

        #region NotesInfoの解析
        
        // notesの移動時間などの情報を fumenMeasureStrings から引っ張り出す
        private NotesInfo AnalyzeNotesInfo(FumenInfo fumenInfo)
        {
            //float _t0 = offset;
            var measureBeatTimeList = new List<float>();
            var noteTypeList = new List<NoteType>();
            var measureDurations = new float[fumenInfo.measureBPMs.Length];
            var measureStartTimes = new float[fumenInfo.measureBPMs.Length];
            var notesCountThisMeasure = new int[fumenInfo.fumenMeasureStrings.Length];
            var notesCountUpToMeasure = new int[fumenInfo.fumenMeasureStrings.Length + 1];
            int scoreNotesCount = 0;

            measureStartTimes[0] = (-240f / fumenInfo.measureBPMs[0] * 3) - offset;

            for (int i = 0; i < fumenInfo.fumenMeasureStrings.Length; i++)
            {
                char[] fumenLine = fumenInfo.fumenMeasureStrings[i].ToCharArray();
                measureDurations[i] = 240 / fumenInfo.measureBPMs[i];

                //0,のように最後に , がつくため Length-1
                for (int j = 0; j < fumenLine.Length - 1; j++)
                {
                    if (fumenLine[j] == (char)NoteType.EMPTY) continue;
                    measureBeatTimeList.Add(measureStartTimes[i] + measureDurations[i] * j / (fumenLine.Length - 1));
                    noteTypeList.Add((NoteType)fumenLine[j]);
                    notesCountThisMeasure[i]++;
                    
                    // HOLD_MID, EMPTY以外のノーツをカウント
                    if(fumenLine[j] != (char)NoteType.HOLD_MID)
                    {
                        scoreNotesCount++;
                    }
                }

                if (i < fumenInfo.fumenMeasureStrings.Length)
                    notesCountUpToMeasure[i + 1] = notesCountUpToMeasure[i] + notesCountThisMeasure[i];

                if (i < fumenInfo.fumenMeasureStrings.Length - 1)
                    measureStartTimes[i + 1] = measureStartTimes[i] + measureDurations[i];
            }

            var measureBeatTimes = measureBeatTimeList.ToArray();
            var noteTypes = noteTypeList.ToArray();

            return new NotesInfo(notesCountThisMeasure, notesCountUpToMeasure, measureBeatTimes.Length, scoreNotesCount, measureStartTimes, measureDurations, measureBeatTimes, noteTypes);
        }
        
        #endregion

        #region 解析結果をまとめるクラス

        /// <summary>
        /// ゲームで使用するmapDataの情報(BGM, Offset, Difficulty)をラップしたもの
        /// </summary>
        public class MapInfo
        {
            public AudioClip bgm;
            public float offset;
            public int mapDifficulty;
            public MapInfo(AudioClip _bgm, float _offset, int _mapDifficulty)
            {
                bgm = _bgm;
                offset = _offset;
                mapDifficulty = _mapDifficulty;
            }
        }
        
        /// <summary>
        /// 小節ごとのBPM、小節ごとの譜面配置、ノードの行き先配列をラップしたもの
        /// </summary>
        public class FumenInfo
        {
            public float[] measureBPMs;
            public string[] fumenMeasureStrings;
            public int[] nodeDestinations;

            public FumenInfo(
                float[] _measureBPMs,
                string[] _fumenMeasureStrings,
                int[] _nodeDestinations)
            {
                measureBPMs = _measureBPMs;
                fumenMeasureStrings = _fumenMeasureStrings;
                nodeDestinations = _nodeDestinations;
            }
        }
        
        /// <summary>
        /// ノーツ情報をラップしたもの
        /// </summary>
        public class NotesInfo
        {
            public int[] notesCountThisMeasure;
            public int[] notesCountUpToMeasure;
            public int allNotesCount;
            public int scoreNotesCount;
            public float[] measureStartTimes;
            public float[] measureDurations;
            public float[] notesBeatTimes;
            public NoteType[] noteTypes;
            
            public NotesInfo(
                int[] _notesCountThisMeasure,
                int[] _notesCountUpToMeasure,
                int _allNotesCount,
                int _scoreNotesCount,
                float[] _measureStartTimes,
                float[] _measureDurations,
                float[] _notesBeatTimes,
                NoteType[] _noteTypes)
            {
                notesCountThisMeasure = _notesCountThisMeasure;
                notesCountUpToMeasure = _notesCountUpToMeasure;
                allNotesCount = _allNotesCount;
                scoreNotesCount = _scoreNotesCount;
                measureStartTimes = _measureStartTimes;
                measureDurations = _measureDurations;
                notesBeatTimes = _notesBeatTimes;
                noteTypes = _noteTypes;
            }
        }

        #endregion
        

    }
}