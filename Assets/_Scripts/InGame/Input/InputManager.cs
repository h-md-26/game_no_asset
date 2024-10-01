using System.Collections;
using System.Collections.Generic;
using StellaCircles.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StellaCircles.InGame
{
    public class InputManager
    {
        readonly Judgementer judgementer;

        public InputManager(Judgementer judgementer)
        {
            this.judgementer = judgementer;
            
            //tap_count = 0;
            isfrick = new bool[20];
            isSETimingOperation = GameSetting.gameSettingValue.isSETimingOperation;
            flickSensitivity = GameSetting.gameSettingValue.frickSensitivity;
        }

        int fing_count;

        //int tap_count;
        int frick_count;
        bool[] isfrick;
        bool isSETimingOperation;
        
        // 入力を受け付けるかどうか
        public bool IsAcceptInput;

        int flickSensitivity = 500;

        public void GetInput()
        {
            // エディタ、実機で処理を分ける
            if (Application.isEditor)
            {
                // エディタで実行中
                if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.J))
                {
                    InputTap(1);
                }

                if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.K))
                {
                    InputFlick(1);
                }

                if (Input.GetKey(KeyCode.F) || Input.GetKey(KeyCode.J))
                {
                    InputHolding(1);
                }

                if (Input.GetKeyUp(KeyCode.F) || Input.GetKeyUp(KeyCode.J))
                {
                    InputRelease(1);
                }


                if (Input.GetKeyUp(KeyCode.E))
                {
                    GameSetting.gameSettingValue.isEffect ^= true;
                    Debug.Log("isEffect" + GameSetting.gameSettingValue.isEffect);
                }
            }
            else
            {
                #region PC用

                if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.J))
                {
                    InputTap(1);
                }

                if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.K))
                {
                    InputFlick(1);
                }

                if (Input.GetKey(KeyCode.F) || Input.GetKey(KeyCode.J))
                {
                    // hold中
                    InputHolding(1);
                }

                if (Input.GetKeyUp(KeyCode.F) || Input.GetKeyUp(KeyCode.J))
                {
                    // hold終
                    InputRelease(1);
                }

                #endregion

                #region スマホ用

                // 実機で実行中
                // タッチされているかチェック
                fing_count = Input.touchCount;
                if (fing_count > 0)
                {
                    for (int i = 0; i < fing_count; i++)
                    {
                        // タッチ情報の取得
                        Touch touch = Input.GetTouch(i);

                        // まだフリックしていない指が素早く動いたら
                        if (!isfrick[touch.fingerId] &&
                            (touch.deltaPosition / touch.deltaTime).magnitude > flickSensitivity)
                        {
                            InputFlick(touch.fingerId);
                        }

                        //押しはじめ：Tap判定
                        if (touch.phase == TouchPhase.Began)
                        {
                            InputTap(touch.fingerId);
                        }
                        //指離し：Hold終わり、中断
                        else if (touch.phase == TouchPhase.Ended)
                        {
                            ////ホールド終わり・ホールド中断を判定すべきタイミングはここ！
                            InputRelease(touch.fingerId);
                        }
                        //Hold中
                        else if (touch.phase == TouchPhase.Moved)
                        {
                            InputHolding(touch.fingerId);
                        }
                        //Hold中
                        else if (touch.phase == TouchPhase.Stationary)
                        {
                            InputHolding(touch.fingerId);
                        }
                        //キャンセル：指離しとして扱う
                        else if (touch.phase == TouchPhase.Canceled)
                        {
                            InputRelease(touch.fingerId);
                        }
                    }
                }

                #endregion

            }
        }

        public void InputFlick(int fingerId)
        {
            if (!IsAcceptInput) return;
            isfrick[fingerId] = true;
            judgementer.JudgeNotes(InputType.Flick, fingerId);
            if (isSETimingOperation)
            {
                AudioManager.Instance.ShotSE(AudioManager.Instance.SE_frick);
            }
        }

        public void InputTap(int fingerId)
        {
            // Debug.Log("Tap!");
            
            if (!IsAcceptInput) return;
            isfrick[fingerId] = false;
            ////タップを判定すべきタイミングはここ！
            judgementer.JudgeNotes(InputType.Tap, fingerId);
            if (isSETimingOperation)
            {
                AudioManager.Instance.ShotSE(AudioManager.Instance.SE_tap);
            }
        }

        public void InputHolding(int fingerId)
        {
            if (!IsAcceptInput) return;
            judgementer.JudgeNotes(InputType.Holding, fingerId);
        }

        public void InputRelease(int fingerId)
        {
            if (!IsAcceptInput) return;
            judgementer.JudgeNotes(InputType.Release, fingerId);
        }
    }

    public enum InputType
    {
        Tap,
        Flick,
        Holding,
        Release
    }
}