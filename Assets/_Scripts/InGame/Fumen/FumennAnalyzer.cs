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
    /// ���ʂ���͂���FumenData�Ƃ��ēn��
    /// </summary>
    public class FumenAnalyzer
    {
        private SelectItemData mapData; // mapData ���߂�BPM�ɃA�N�Z�X���邽�߂Ɏg�p
        private string[] fumenDataStrings; //�R�}���h����s�ʕ��ʃf�[�^
        private float offset; // �Ȃƕ��ʂ̊J�n�ʒu�̃I�t�Z�b�g
        
        /// <summary>
        /// mapData�Ɠ�Փx��񂩂�FumenData���쐬���ĕԂ�
        /// </summary>
        public async UniTask<FumenData> AnalyzeMapData(SelectItemData mapData, int mapDifficulty, CancellationToken ct)
        {
            this.mapData = mapData;
            offset = mapData.itemOffset;
            // mapData��id���特�y�����[�h����
            var bgm = await mapData.LoadItemMusic(ct);
            
            fumenDataStrings = GetFumenDataStrings(this.mapData, mapDifficulty);
            
            // ��͂̂���Ȃ�����������
            var mapInfo = new MapInfo(bgm, offset, mapDifficulty);
            
            // ��͂������ʏ���������
            var fumenInfo = AnalyzeFumenInfo();
            
            // ��͂����m�[�c����������
            var notesInfo = AnalyzeNotesInfo(fumenInfo);
            
            return new FumenData(mapInfo, fumenInfo, notesInfo);
        }
        
        
        // ���ʏ����s���Ƃɕ��������z���Ԃ�
        private string[] GetFumenDataStrings(SelectItemData mapData, int mapDifficulty)
        {
            return mapData.itemMaps[mapDifficulty].text.Split('\r', '\n');
        }

        #region FumenInfo�̉��
        
        private bool isHolding = false; // Hold�����ǂ���
        int currentNode = 0; // ���݋���m�[�h�̔ԍ� 0?5�܂�
        bool isClockwise = true; // ���݂̂܂��������v��肩�ǂ���
        bool isNextClockwise = true; // CROSS������̂܂��������v��肩�ǂ���
        bool isCross = false; // ���� 1, 4 �m�[�h�ɗ������ACROSS���邩�ǂ���
        int isWarp = -1; // ���[�v��̃m�[�h ���[�v���Ȃ��Ȃ畉�̐�������
        
        // ���ʏ��� #�R�}���h �� Hold ��ǂݎ���Đ�������
        private FumenInfo AnalyzeFumenInfo()
        {
            /*
            ��邱��
                FumenInfo : measureBPMs, fumenMeasureStrings, nodeDestinations
                �� fumenDataStrings�����͂���
            
                measureBPMs ��
                    #BPM ��ڈ�ɐ�������
                
                fumenMeasureStrings ��
                    #�R�}���h���Ȃ��s������
                    HoldStart��HoldEnd�̊Ԃ�HoldMid�Ŗ��߂�
                    
                nodeDestinations ��
                    #WARP , #CROSS_CR , #CROSS_CCR ��ڈ�ɐ�������
            
            */

            float currentBPM = mapData.itemBPM; //���߂��Ƃ�BPM������
            string notesStringStock = ""; // �s���܂������ʏ��̏ꍇ�ɖڂ̍s�̕������ۑ����Ă������߂̕ϐ�
            isHolding = false; // Hold�����ǂ��������Ă���
            currentNode = 0; // ���݋���m�[�h�̔ԍ� 0?5�܂�
            isClockwise = true; // ���݂̂܂��������v��肩�ǂ���
            isNextClockwise = true; // CROSS������̂܂��������v��肩�ǂ���
            isCross = false; // ���� 1, 4 �m�[�h�ɗ������ACROSS���邩�ǂ���
            isWarp = -1; // ���[�v��̃m�[�h ���[�v���Ȃ��Ȃ畉�̐�������

            var measureBPMList = new List<float>();
            var fumenMeasureStringList = new List<string>();
            var nodeDestinationList = new List<int>();

            // �n�܂�̓����� �m�[�h3 �� 4 �� 5 �� 0 �ŌŒ�
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
                if (fumenLine[0] == '#') //#�R�}���h�̏���
                {
                    string[] sharpCommand = fumenDataStrings[i].Split(' ');

                    switch (sharpCommand[0])
                    {
                        case "#BPM":
                            // BPM�� sharpCommand[1] �ɕύX����
                            currentBPM = float.Parse(sharpCommand[1]);
                            break;
                        case "#DELAY":
                            // sharpCommand[1] ���������ĂP���߈ړ�����
                            measureBPMList.Add(240 / float.Parse(sharpCommand[1]));
                            fumenMeasureStringList.Add("0");
                            nodeDestinationList.Add(currentNode);
                            currentNode = NextNode;
                            break;
                        case "#CROSS_CW":
                            //1��4,4��1 �Ɠn���āA���v���
                            //cross�t���O�����āA1 or 4 �ɗ������� nowNode + 3 �ɓn��A�ȍ~ ���v���
                            isCross = true;
                            isNextClockwise = true;
                            break;
                        case "#CROSS_CCW":
                            //1��4,4��1 �Ɠn���āA�����v���
                            //cross�t���O�����āA1 or 4 �ɗ������� nowNode + 3 �ɓn��A�ȍ~ �����v���
                            isCross = true;
                            isNextClockwise = false;
                            break;
                        case "#WARP":
                            // sharpCommand[1] �ɓn��A�ȍ~ ���v���
                            isWarp = int.Parse(sharpCommand[1]);
                            break;
                    }
                }
                else if (fumenLine[^1] == ',') //�R�}���h�łȂ��s ���� �s���Ɂu,�v������ꍇ
                {
                    measureBPMList.Add(currentBPM);
                    fumenMeasureStringList.Add(notesStringStock + fumenDataStrings[i]);
                    notesStringStock = "";
                    nodeDestinationList.Add(currentNode);
                    
                    currentNode = NextNode;
                }
                else //�����݂̂̂Ƃ��A���̍s�ɓ������߂̕��ʂ�����
                {
                    notesStringStock += fumenDataStrings[i];
                }
            }

            nodeDestinationList.Add(currentNode);

            var measureBPMs = measureBPMList.ToArray();
            var fumenMeasureStrings = fumenMeasureStringList.ToArray();
            var nodeDestinations = nodeDestinationList.ToArray();

            //HoldStart��HoldEnd�̊Ԃ�HoldMid�ɕϊ�����
            for (int i = 0; i < fumenMeasureStrings.Length; i++)
            {
                char[] fumenLine = fumenMeasureStrings[i].ToCharArray();
                fumenMeasureStrings[i] = HoldConvert(fumenLine);
            }

            return new FumenInfo(measureBPMs, fumenMeasureStrings, nodeDestinations);
        }
        
        // fumenMeasureStrings �� HoldStart��HoldEnd�̊Ԃ�HoldMid�ɒu��������
        private string HoldConvert(char[] line)
        {
            /* ��邱��
            fumenMeasureStrings �̗v�f�ɑ΂���
            HoldStart����HoldEnd�̊Ԃ�16���Ԋu��HoldMid�����鏈�����s��
            
            �s���܂�����Hold�ɂ��Ă�isHolding���g���Č��m����
            */
            
            /* ����
            _c ����Hold���܂܂�� or _isHolding �� true �̂Ƃ��A
            ���̏��߂̕����� m = _c.Length -1
            (16 �� m �̍ŏ����{�� s )�̗v�f���̔z����쐬���A
            s/m ������ _c�̕��ʂ����Ă����B�������AHold���̕����ɂ� 0 �����Ă���
            Hold�J�n or _isHolding�̎��͂͂��߂���A s/16 ������ Hold���̃m�[�c��������
            Hold�I�����ꂽ��AHold�I�̑O��(s/16 -1)�� 0 �����A_isHolding = false ����B
            Hold�������߂̎n�߂ɂ���A���̏��߂��p�Ŏn�܂�ꍇ�́AHoldCurve������
            ���������z��𕶎���ɒ����A�Ԃ�
            */

            if (isHolding || CheckContainHoldStart(line))
            {
                int m = line.Length - 1;
                int s = MathUtils.GetLCM(m, 16);
                int hold_i = 0; //Hold�̊J�n�ʒu(����Hold���Ȃ�0)
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
                            //�p��������J�[�u������
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

        // HoldStart���܂ނ��`�F�b�N
        private bool CheckContainHoldStart(char[] line)
        {
            for (int i = 0; i < line.Length - 1; i++)
            {
                if (line[i] == (char)NoteType.HOLD_START)
                    return true;
            }

            return false;
        }

        // ���̃m�[�h�ԍ������[�v��N���X�Ȃǂ��l�����ĕԂ�
        private int NextNode
        {
            get
            {
                // �D�揇��
                // Warp > Cross > Clockwise

                // Warp���Ȃ��Ƃ� _isWarp -1 , Warp����Ƃ� _isWarp 0�`5
                if (isWarp >= 0)
                {
                    if (isWarp == currentNode)
                    {
                        Debug.Log("warning:���[�v�悪���ݒn�ɂȂ��Ă���");
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

        #region NotesInfo�̉��
        
        // notes�̈ړ����ԂȂǂ̏��� fumenMeasureStrings �����������o��
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

                //0,�̂悤�ɍŌ�� , �������� Length-1
                for (int j = 0; j < fumenLine.Length - 1; j++)
                {
                    if (fumenLine[j] == (char)NoteType.EMPTY) continue;
                    measureBeatTimeList.Add(measureStartTimes[i] + measureDurations[i] * j / (fumenLine.Length - 1));
                    noteTypeList.Add((NoteType)fumenLine[j]);
                    notesCountThisMeasure[i]++;
                    
                    // HOLD_MID, EMPTY�ȊO�̃m�[�c���J�E���g
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

        #region ��͌��ʂ��܂Ƃ߂�N���X

        /// <summary>
        /// �Q�[���Ŏg�p����mapData�̏��(BGM, Offset, Difficulty)�����b�v��������
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
        /// ���߂��Ƃ�BPM�A���߂��Ƃ̕��ʔz�u�A�m�[�h�̍s����z������b�v��������
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
        /// �m�[�c�������b�v��������
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