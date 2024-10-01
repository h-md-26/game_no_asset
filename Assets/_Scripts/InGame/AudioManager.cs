using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using DG.Tweening;
using StellaCircles.Utils;

public class AudioManager : Singleton<AudioManager>
{
    public AudioMixer audioMixer;

    // �Q�[�����ɗp���鉹�� MapAgent�o�R�ŖႤ
    public AudioClip BGM_game;

    // SE
    public AudioClip SE_tap;
    public AudioClip SE_tapmiss;
    public AudioClip SE_frick;
    public AudioClip SE_frickmiss;
    public AudioClip SE_holding;
    public AudioClip SE_holdfail;
    public AudioClip SE_holdend;


    // �Q�[�����ȊO�ŗp���鉹��
    public AudioClip BGM_selectAge;
    public AudioClip BGM_scroll;
    public AudioClip BGM_grade;

    public AudioClip SE_decide;
    public AudioClip SE_gameStart;

    // �eAudioSource
    [SerializeField] public AudioSource BGMso;
    [SerializeField] private AudioSource SEMAINso;
    [SerializeField] private AudioSource SEHOLDso;

    // �Ȃ̒�~���ɋȂ̒�~�ʒu����������
    float pouseTime;

    // AssetBundle�̃X�^�b�N�E���[�h�t���O
    public bool isMusicLoading = false; //���[�h�t���O
    public bool isMusicStacking = false; //���[�h�̃X�^�b�N�t���O
    public int musicStackId = -1; //�X�^�b�N���Ă���ID
    public int musicUsingId = -1; //���Ƀ��[�h����ID

    /// <summary>
    /// ���t�J�n���ɌĂ�
    /// </summary>
    public void AudioStart(AudioClip bgm)
    {
        BGM_game = bgm;
        BGMso.clip = BGM_game;
        BGMso.time = 0f;
        SEHOLDso.clip = SE_holding;
    }
    
    /// <summary>
    /// BGM�̍Đ����Ԃ��擾
    /// </summary>
    public float BGMtime
    {
        get { return BGMso.time; }
    }

    /// <summary>
    /// �Ȃ̍Đ����Ԃ��X�L�b�v���� �f�o�b�O�p
    /// </summary>
    public void PlusBGMtime(float plus)
    {
        BGMso.time += plus;
    }
    
    /// <summary>
    /// �Đ��E��~��Ԃ𔽓]����
    /// </summary>
    public void SwitchPouseUnpouseBGM()
    {
        if (BGMso.isPlaying)
        {
            pouseTime = BGMso.time;
            BGMso.Pause();
        }
        else
        {
            Debug.Log("�|�[�Y����");
            BGMso.time = pouseTime;
            BGMso.Play();
        }
    }
    
    /// <summary>
    /// BGM���Đ����Ȃ��~���ApouseTime����������
    /// </summary>
    public void TryPouseBGM()
    {
        if (BGMso.isPlaying)
        {
            pouseTime = BGMso.time;
            //BGMso.Stop();
            BGMso.Pause();
        }
    }

    /// <summary>
    /// BGM����~���Ȃ�ApouseTime����Đ�����
    /// </summary>
    public void TryUnpouseBGM()
    {
        if (!BGMso.isPlaying)
        {
            Debug.Log("�|�[�Y����");
            BGMso.time = pouseTime;
            BGMso.Play();
        }
    }

    
    /// <summary>
    /// �w��SE���Đ�
    /// </summary>
    public void ShotSE(AudioClip ac)
    {
        //SE���ςȃ^�C�~���O�łȂ��Ă���Ƃ���ON�ɂ���Əꏊ�����肵�₷��
        // Debug.Log($"SE{ac.name}");
        SEMAINso.PlayOneShot(ac);
    }

    /// <summary>
    /// HoldSE���Đ�
    /// </summary>
    public void PlayHold()
    {
        SEHOLDso.time = 0f;
        SEHOLDso.Play();
    }
    /// <summary>
    /// HoldSE���~
    /// </summary>
    public void StopHold()
    {
        SEHOLDso.Stop();
        SEHOLDso.time = 0f;
    }
    
    /// <summary>
    /// BGM���Đ�
    /// </summary>
    private void PlayBGM()
    {
        BGMso.Play();
    }
    
    /// <summary>
    /// ���݂�BGM���~
    /// </summary>
    public void StopBGM()
    {
        //Debug.Log("StopBGM!!");
        BGMso.Stop();
    }

    /// <summary>
    /// BGM��loop�Ȃ��ōĐ�
    /// �C���Q�[���̉��y�Ŏg�p
    /// </summary>
    public void PlayBGMnoloop()
    {
        BGMso.loop = false;
        PlayBGM();
    }

    /// <summary>
    /// �w��̋Ȃ��w�肵���ʒu����loop����ōĐ�
    /// Select�Ŏg�p
    /// </summary>
    public void PlayBGMOnSelect(AudioClip clip, float demoStart)
    {
        BGMso.loop = true;
        BGMso.clip = clip;
        BGMso.time = demoStart;
        BGMso.Play();
    }
    
    /// <summary>
    /// BGM���t�F�[�h�A�E�g���A���̌�ݒ��߂�
    /// </summary>
    public void FadeOutBGM(float duration = 0.5f)
    {
        BGMso.DOFade(0, duration).OnComplete( () =>
        {
            BGMso.Stop();
            BGMso.time = 0f;
            BGMso.volume = 1;
        });
    }

    /// <summary>
    /// �����_����SE��AudioClip��Ԃ�
    /// </summary>
    public AudioClip ShotRandomSE()
    {
        int i = Random.Range(0, 7);
        switch (i)
        {
            case (0):
                return SE_tap;
            case (1):
                return SE_tapmiss;
            case (2):
                return SE_frick;
            case (3):
                return SE_holdfail;
            case (4):
                return SE_holdend;
            case (5):
                return SE_decide;
            case (6):
                return SE_gameStart;
            default:
                return SE_tap;
        }
    }
}
