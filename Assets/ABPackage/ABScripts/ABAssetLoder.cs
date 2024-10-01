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
        // IReturnMonoAssetを実装した各アセットについてのLoadの実装と
        // 同じLoad処理が呼ばれたときの処理のスタックの実装

        // Musicを変更して再生する
        public static async UniTask ChangeMusicStack(this AudioSourceLoadManager aslm, int id, float startTime, CancellationToken ct)
        {
            // ロード処理中ならidをメモしてスタックフラグを立ててreturnする
            if (aslm.isMusicLoading)
            {
                aslm.musicStackId = id;
                aslm.isMusicStacking = true;
                return;
            }

            // ロード処理フラグを立てる
            aslm.isMusicLoading = true;
            // idのアセットをロードし、今まで使ってたアセットをリリースする
            var clip = await GetMusic(id, ct);
            if (aslm.musicUsingId >= 0)
            {
                await ReleaseMusic(aslm.musicUsingId, ct);
            }

            aslm.musicUsingId = id;

            // スタックフラグが立っているなら、メモしたidでアセットをロードし、さっきロードしたアセットをリリース
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

        // Musicを返す(スタック付き)
        public static async UniTask GetMusicStack(this AudioManager aM, int id, Action<AudioClip> action, CancellationToken ct)
        {
            Debug.Log(
                $"load {aM.isMusicLoading}, stack {aM.isMusicStacking}, usingId {aM.musicUsingId}, stackId {aM.musicStackId}, nowId {id}");


            // ロード処理中ならidをメモしてスタックフラグを立ててreturnする
            if (aM.isMusicLoading)
            {
                Debug.Log("曲がロード中なのでidだけメモしてリターン");
                aM.musicStackId = id;
                aM.isMusicStacking = true;
                return;
            }

            // ロード処理フラグを立てる
            aM.isMusicLoading = true;
            // idのアセットをロードし、今まで使ってたアセットをリリースする
            var clip = await GetMusic(id, ct);
            if (aM.musicUsingId >= 0 && id != aM.musicUsingId)
            {
                Debug.Log("今かかってた曲をリリースする");
                await ReleaseMusic(aM.musicUsingId, ct);
            }

            aM.musicUsingId = id;

            // スタックフラグが立っているなら、メモしたidでアセットをロードし、さっきロードしたアセットをリリース
            if (aM.isMusicStacking)
            {
                if (aM.musicStackId == id)
                {
                    Debug.Log("ID同じ");
                }

                clip = await GetMusic(aM.musicStackId, ct);
                aM.musicUsingId = aM.musicStackId;
                Debug.Log("リクエストが上書きされた曲をリリースする");
                await ReleaseMusic(id, ct);
                aM.isMusicStacking = false;
            }
            //aM.isMusicLoading = false;

            action.Invoke(clip);
        }

        // Imageを付け替える
        public static async UniTask ChangeImage(this ImageLoadManager ilm, int newId, CancellationToken ct)
        {
            ilm.image.sprite = await ABStockManager.LoadMonoABAndAsset<ImageAsset, Sprite>(newId, ct);
            await ReleaseImage(ilm.usingId, ct);
            ilm.usingId = newId;
        }

        //  SelectItemDataの拡張メソッドとして曲をload
        public static async UniTask<AudioClip> LoadItemMusic(this SelectItemData itemData, CancellationToken ct)
        {
            return await GetMusic(itemData.itemId, ct);
        }

        //  SelectItemDataの拡張メソッドとして曲をunload
        public static async UniTask ReleaseItemMusic(this SelectItemData itemData, CancellationToken ct)
        {
            await ReleaseMusic(itemData.itemId, ct);
        }

        //  SelectItemDataの拡張メソッドとして画像をload
        public static async UniTask<Sprite> LoadItemSprite(this SelectItemData itemData, CancellationToken ct)
        {
            return await GetImage(itemData.itemId, ct);
        }

        //  SelectItemDataの拡張メソッドとして画像をunload
        public static async UniTask ReleaseItemSprite(this SelectItemData itemData, CancellationToken ct)
        {
            await ReleaseImage(itemData.itemId, ct);
        }

        // AudioClipのGet, Release
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

        // SpriteのGet, Release
        public static async UniTask<Sprite> GetImage(int id, CancellationToken ct)
        {
            if (id < 0) throw new IndexOutOfRangeException();
            return await ABStockManager.LoadMonoABAndAsset<ImageAsset, Sprite>(id, ct);
        }

        public static async UniTask ReleaseImage(int id, CancellationToken ct)
        {
            if (id < 0) return;
            //Debug.Log("Imageリリース！！");
            await ABStockManager.ReleaseMonoAB<ImageAsset>(id, ct);
        }

        // MusicInfoのGet
        public static async UniTask<T2> GetMusicInfoAsset<T1, T2>(int id, CancellationToken ct)
            where T1 : UnityEngine.Object, IReturnMonoAsset<T2>
            where T2 : UnityEngine.Object
        {
            if (id < 0) throw new IndexOutOfRangeException();
            return await ABStockManager.LoadMonoABAndAsset<T1, T2>(id, ct);
        }

    }
}