using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using StellaCircles.Data;
using UnityEngine;
using UnityEngine.UI;

namespace StellaCircles.AssetBundleManagement
{

    public static class ABAssetLoder
    {
        // IReturnMonoAsset�����������e�A�Z�b�g�ɂ��Ă�Load�̎�����
        // ����Load�������Ă΂ꂽ�Ƃ��̏����̃X�^�b�N�̎���

        // Music��ύX���čĐ�����
        public static async UniTask ChangeMusicStack(this AudioSourceLoadManager aslm, int id, float startTime, CancellationToken ct)
        {
            // ���[�h�������Ȃ�id���������ăX�^�b�N�t���O�𗧂Ă�return����
            if (aslm.isMusicLoading)
            {
                aslm.musicStackId = id;
                aslm.isMusicStacking = true;
                return;
            }

            // ���[�h�����t���O�𗧂Ă�
            aslm.isMusicLoading = true;
            // id�̃A�Z�b�g�����[�h���A���܂Ŏg���Ă��A�Z�b�g�������[�X����
            var clip = await GetMusic(id, ct);
            if (aslm.musicUsingId >= 0)
            {
                await ReleaseMusic(aslm.musicUsingId, ct);
            }

            aslm.musicUsingId = id;

            // �X�^�b�N�t���O�������Ă���Ȃ�A��������id�ŃA�Z�b�g�����[�h���A���������[�h�����A�Z�b�g�������[�X
            if (aslm.isMusicStacking)
            {
                clip = await GetMusic(aslm.musicStackId, ct);
                aslm.musicUsingId = aslm.musicStackId;
                await ReleaseMusic(id, ct);
                aslm.isMusicStacking = false;
            }

            aslm.audioSource.clip = clip;
            aslm.audioSource.time = startTime;
            aslm.audioSource.Play();

            aslm.isMusicLoading = false;
        }

        // Music��Ԃ�(�X�^�b�N�t��)
        public static async UniTask GetMusicStack(this AudioManager aM, int id, Action<AudioClip> action, CancellationToken ct)
        {
            Debug.Log(
                $"load {aM.isMusicLoading}, stack {aM.isMusicStacking}, usingId {aM.musicUsingId}, stackId {aM.musicStackId}, nowId {id}");


            // ���[�h�������Ȃ�id���������ăX�^�b�N�t���O�𗧂Ă�return����
            if (aM.isMusicLoading)
            {
                Debug.Log("�Ȃ����[�h���Ȃ̂�id�����������ă��^�[��");
                aM.musicStackId = id;
                aM.isMusicStacking = true;
                return;
            }

            // ���[�h�����t���O�𗧂Ă�
            aM.isMusicLoading = true;
            // id�̃A�Z�b�g�����[�h���A���܂Ŏg���Ă��A�Z�b�g�������[�X����
            var clip = await GetMusic(id, ct);
            if (aM.musicUsingId >= 0 && id != aM.musicUsingId)
            {
                Debug.Log("���������Ă��Ȃ������[�X����");
                await ReleaseMusic(aM.musicUsingId, ct);
            }

            aM.musicUsingId = id;

            // �X�^�b�N�t���O�������Ă���Ȃ�A��������id�ŃA�Z�b�g�����[�h���A���������[�h�����A�Z�b�g�������[�X
            if (aM.isMusicStacking)
            {
                if (aM.musicStackId == id)
                {
                    Debug.Log("ID����");
                }

                clip = await GetMusic(aM.musicStackId, ct);
                aM.musicUsingId = aM.musicStackId;
                Debug.Log("���N�G�X�g���㏑�����ꂽ�Ȃ������[�X����");
                await ReleaseMusic(id, ct);
                aM.isMusicStacking = false;
            }
            //aM.isMusicLoading = false;

            action.Invoke(clip);
        }

        // Image��t���ւ���
        public static async UniTask ChangeImage(this ImageLoadManager ilm, int newId, CancellationToken ct)
        {
            ilm.image.sprite = await ABStockManager.LoadMonoABAndAsset<ImageAsset, Sprite>(newId, ct);
            await ReleaseImage(ilm.usingId, ct);
            ilm.usingId = newId;
        }

        //  SelectItemData�̊g�����\�b�h�Ƃ��ċȂ�load
        public static async UniTask<AudioClip> LoadItemMusic(this SelectItemData itemData, CancellationToken ct)
        {
            return await GetMusic(itemData.itemId, ct);
        }

        //  SelectItemData�̊g�����\�b�h�Ƃ��ċȂ�unload
        public static async UniTask ReleaseItemMusic(this SelectItemData itemData, CancellationToken ct)
        {
            await ReleaseMusic(itemData.itemId, ct);
        }

        //  SelectItemData�̊g�����\�b�h�Ƃ��ĉ摜��load
        public static async UniTask<Sprite> LoadItemSprite(this SelectItemData itemData, CancellationToken ct)
        {
            return await GetImage(itemData.itemId, ct);
        }

        //  SelectItemData�̊g�����\�b�h�Ƃ��ĉ摜��unload
        public static async UniTask ReleaseItemSprite(this SelectItemData itemData, CancellationToken ct)
        {
            await ReleaseImage(itemData.itemId, ct);
        }

        // AudioClip��Get, Release
        public static async UniTask<AudioClip> GetMusic(int id, CancellationToken ct)
        {
            if (id < 0) throw new IndexOutOfRangeException();
            return await ABStockManager.LoadMonoABAndAsset<MusicAsset, AudioClip>(id, ct);
        }

        public static async UniTask ReleaseMusic(int id, CancellationToken ct)
        {
            if (id < 0) return;
            await ABStockManager.ReleaseMonoAB<MusicAsset>(id, ct);
        }

        // Sprite��Get, Release
        public static async UniTask<Sprite> GetImage(int id, CancellationToken ct)
        {
            if (id < 0) throw new IndexOutOfRangeException();
            return await ABStockManager.LoadMonoABAndAsset<ImageAsset, Sprite>(id, ct);
        }

        public static async UniTask ReleaseImage(int id, CancellationToken ct)
        {
            if (id < 0) return;
            //Debug.Log("Image�����[�X�I�I");
            await ABStockManager.ReleaseMonoAB<ImageAsset>(id, ct);
        }

        // MusicInfo��Get
        public static async UniTask<T2> GetMusicInfoAsset<T1, T2>(int id, CancellationToken ct)
            where T1 : UnityEngine.Object, IReturnMonoAsset<T2>
            where T2 : UnityEngine.Object
        {
            if (id < 0) throw new IndexOutOfRangeException();
            return await ABStockManager.LoadMonoABAndAsset<T1, T2>(id, ct);
        }

    }
}