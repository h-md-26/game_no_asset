using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using DG.Tweening;
using StellaCircles.Utils;

public class AudioManager : Singleton<AudioManager>
{
    public AudioMixer audioMixer;

    // ゲーム中に用いる音源 MapAgent経由で貰う
    public AudioClip BGM_game;

    // SE
    public AudioClip SE_tap;
    public AudioClip SE_tapmiss;
    public AudioClip SE_frick;
    public AudioClip SE_frickmiss;
    public AudioClip SE_holding;
    public AudioClip SE_holdfail;
    public AudioClip SE_holdend;


    // ゲーム中以外で用いる音源
    public AudioClip BGM_selectAge;
    public AudioClip BGM_scroll;
    public AudioClip BGM_grade;

    public AudioClip SE_decide;
    public AudioClip SE_gameStart;

    // 各AudioSource
    [SerializeField] public AudioSource BGMso;
    [SerializeField] private AudioSource SEMAINso;
    [SerializeField] private AudioSource SEHOLDso;

    // 曲の停止時に曲の停止位置をメモする
    float pouseTime;

    // AssetBundleのスタック・ロードフラグ
    public bool isMusicLoading = false; //ロードフラグ
    public bool isMusicStacking = false; //ロードのスタックフラグ
    public int musicStackId = -1; //スタックしているID
    public int musicUsingId = -1; //既にロードしたID

    /// <summary>
    /// 演奏開始時に呼ぶ
    /// </summary>
    public void AudioStart(AudioClip bgm)
    {
        BGM_game = bgm;
        BGMso.clip = BGM_game;
        BGMso.time = 0f;
        SEHOLDso.clip = SE_holding;
    }
    
    /// <summary>
    /// BGMの再生時間を取得
    /// </summary>
    public float BGMtime
    {
        get { return BGMso.time; }
    }

    /// <summary>
    /// 曲の再生時間をスキップする デバッグ用
    /// </summary>
    public void PlusBGMtime(float plus)
    {
        BGMso.time += plus;
    }
    
    /// <summary>
    /// 再生・停止状態を反転する
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
            Debug.Log("ポーズ解除");
            BGMso.time = pouseTime;
            BGMso.Play();
        }
    }
    
    /// <summary>
    /// BGMが再生中なら停止し、pouseTimeをメモする
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
    /// BGMが停止中なら、pouseTimeから再生する
    /// </summary>
    public void TryUnpouseBGM()
    {
        if (!BGMso.isPlaying)
        {
            Debug.Log("ポーズ解除");
            BGMso.time = pouseTime;
            BGMso.Play();
        }
    }

    
    /// <summary>
    /// 指定SEを再生
    /// </summary>
    public void ShotSE(AudioClip ac)
    {
        //SEが変なタイミングでなっているときはONにすると場所が特定しやすい
        // Debug.Log($"SE{ac.name}");
        SEMAINso.PlayOneShot(ac);
    }

    /// <summary>
    /// HoldSEを再生
    /// </summary>
    public void PlayHold()
    {
        SEHOLDso.time = 0f;
        SEHOLDso.Play();
    }
    /// <summary>
    /// HoldSEを停止
    /// </summary>
    public void StopHold()
    {
        SEHOLDso.Stop();
        SEHOLDso.time = 0f;
    }
    
    /// <summary>
    /// BGMを再生
    /// </summary>
    private void PlayBGM()
    {
        BGMso.Play();
    }
    
    /// <summary>
    /// 現在のBGMを停止
    /// </summary>
    public void StopBGM()
    {
        //Debug.Log("StopBGM!!");
        BGMso.Stop();
    }

    /// <summary>
    /// BGMをloopなしで再生
    /// インゲームの音楽で使用
    /// </summary>
    public void PlayBGMnoloop()
    {
        BGMso.loop = false;
        PlayBGM();
    }

    /// <summary>
    /// 指定の曲を指定した位置からloopありで再生
    /// Selectで使用
    /// </summary>
    public void PlayBGMOnSelect(AudioClip clip, float demoStart)
    {
        BGMso.loop = true;
        BGMso.clip = clip;
        BGMso.time = demoStart;
        BGMso.Play();
    }
    
    /// <summary>
    /// BGMをフェードアウトし、その後設定を戻す
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
    /// ランダムなSEのAudioClipを返す
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
