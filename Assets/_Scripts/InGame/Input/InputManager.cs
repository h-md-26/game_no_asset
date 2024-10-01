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
        
        // ���͂��󂯕t���邩�ǂ���
        public bool IsAcceptInput;

        int flickSensitivity = 500;

        public void GetInput()
        {
            // �G�f�B�^�A���@�ŏ����𕪂���
            if (Application.isEditor)
            {
                // �G�f�B�^�Ŏ��s��
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
                #region PC�p

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
                    // hold��
                    InputHolding(1);
                }

                if (Input.GetKeyUp(KeyCode.F) || Input.GetKeyUp(KeyCode.J))
                {
                    // hold�I
                    InputRelease(1);
                }

                #endregion

                #region �X�}�z�p

                // ���@�Ŏ��s��
                // �^�b�`����Ă��邩�`�F�b�N
                fing_count = Input.touchCount;
                if (fing_count > 0)
                {
                    for (int i = 0; i < fing_count; i++)
                    {
                        // �^�b�`���̎擾
                        Touch touch = Input.GetTouch(i);

                        // �܂��t���b�N���Ă��Ȃ��w���f������������
                        if (!isfrick[touch.fingerId] &&
                            (touch.deltaPosition / touch.deltaTime).magnitude > flickSensitivity)
                        {
                            InputFlick(touch.fingerId);
                        }

                        //�����͂��߁FTap����
                        if (touch.phase == TouchPhase.Began)
                        {
                            InputTap(touch.fingerId);
                        }
                        //�w�����FHold�I���A���f
                        else if (touch.phase == TouchPhase.Ended)
                        {
                            ////�z�[���h�I���E�z�[���h���f�𔻒肷�ׂ��^�C�~���O�͂����I
                            InputRelease(touch.fingerId);
                        }
                        //Hold��
                        else if (touch.phase == TouchPhase.Moved)
                        {
                            InputHolding(touch.fingerId);
                        }
                        //Hold��
                        else if (touch.phase == TouchPhase.Stationary)
                        {
                            InputHolding(touch.fingerId);
                        }
                        //�L�����Z���F�w�����Ƃ��Ĉ���
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
            ////�^�b�v�𔻒肷�ׂ��^�C�~���O�͂����I
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