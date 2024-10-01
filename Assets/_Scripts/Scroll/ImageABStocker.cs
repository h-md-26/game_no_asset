using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using StellaCircles.Data;
using StellaCircles.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace StellaCircles.AssetBundleManagement
{
    public class ImageABStocker : Singleton<ImageABStocker>
    {
        [SerializeField] Sprite loadingSprite;

        Dictionary<int, Sprite> IdImageDict = new();

        public Sprite GetJacketImage(int id)
        {
            // ���O�ɓǂݍ��񂾃W���P�b�g�摜������n��

            if (IdImageDict.ContainsKey(id))
            {
                //Debug.Log($"{id}�������ɂ���̂ŖႤ");
                return IdImageDict[id];
            }
            else
            {
                //Debug.Log($"{id}�������ɂȂ��̂Ń��[�h����");
                return loadingSprite;
            }
        }

        public async UniTask GetJacketImagesOfAge(int ageId, CancellationToken ct)
        {
            //Debug.Log("�摜�ǂݍ���");

            if (0 > ageId || ageId >= MapDataBase.Instance.mapDBA.mapIdPerAgeIDs.GetLength(0))
            {
                Debug.Log($"ageId���͈͊O{ageId}");
                return;
            }

            // �O�Ɏg���Ă��������������ăA�Z�b�g��Unload����
            await ReleaseDictAssets(ct);

            for (int i = 0; i < MapDataBase.Instance.mapDBA.mapIdPerAgeIDs.GetLength(1); i++)
            {
                int mapId = MapDataBase.Instance.mapDBA.mapIdPerAgeIDs[ageId, i];

                if (mapId < 0)
                {
                    break;
                }

                if (!IdImageDict.ContainsKey(mapId))
                {
                    var sp = await ABAssetLoder.GetImage(mapId, ct);

                    IdImageDict.Add(mapId, sp);
                }
            }
        }

        async UniTask ReleaseDictAssets(CancellationToken ct)
        {
            List<UniTask> uniTasks = new();
            foreach (var item in IdImageDict)
            {
                uniTasks.Add(ABAssetLoder.ReleaseImage(item.Key, ct));
            }

            await UniTask.WhenAll(uniTasks);
            IdImageDict.Clear();

            if (ct.IsCancellationRequested)
            {
                throw new System.OperationCanceledException();
            }
            await Resources.UnloadUnusedAssets();
        }
    }
}