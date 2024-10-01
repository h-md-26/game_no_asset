using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using System.Text.RegularExpressions;
using StellaCircles.Utils;

namespace StellaCircles.AssetBundleManagement.Web
{
    public static class DownloadAsset
    {
        #region WebRequest

        /*
         * アプリ起動時にサーバーから
         *   ・AssetBundleの名前
         * 　・AssetBundleの最新バージョンリスト
         * 　・AssetBundleの共有URLリスト
         * 　の入ったリストのJSONファイルを受け取る
         *
         * SaveしていたAssetBundleの現在のバージョンリストをロードする
         *
         * 2つのバージョンリストを比較して、
         * 　・新しいバージョンのあるAssetBundle
         * 　・新たに追加されたAssetBundle　　　　をURLリストを使ってダウンロードし、
         * 　・最新リストで消えたAssetBundle　　　を削除する
         *
         * ダウンロードしたAssetBundleを(bundleDirPath)に配置し、古いバージョンのAssetBundleを削除する
         * 　名前はどうなるか確認する
         *
         */

        public static async UniTask UpdateAB()
        {
            (Dictionary<string, ABInfo> rdict, Dictionary<string, ABInfo> ldict) = await GetBothABDict();
            await CompareABDict(rdict, ldict);
        }

        private static async UniTask CompareABDict(Dictionary<string, ABInfo> rdict, Dictionary<string, ABInfo> ldict)
        {
            /*
            rdictの要素remoteに対して ldictから Key(fileName) が一致するものを探す
            一致するものがあった場合、verを比較し、
            verが同じ場合、何もしない
            verが異なる場合、remote の url から最新のAssetBundleを取得する
            一致するものがなかった場合、remote の url から最新のAssetBundleを取得する

            ldictにあってrdictにないものの処理は今は考えないことにする

             */
            List<(string name, int ver)> dlFileList = new();
            List<UniTask> dlTaskList = new();

            foreach (var r in rdict)
            {
                // rdictに含まれるKeyを持っていない or そのバージョンが異なる
                if (!ldict.ContainsKey(r.Key) || ldict[r.Key].ver != r.Value.ver)
                {
                    // r url から最新バージョンのAssetBundle をダウンロードし(ここではメモをし、後でまとめて並列処理する)、ldictも更新する
                    Debug.Log($"localのabdictに {r.Key}がない or Verが古い");
                    dlFileList.Add((r.Key, r.Value.ver));
                }
            }

            // dlFileListをKeyとして rdictの要素の中の url から最新バージョンのAssetBundle をダウンロード(並列処理)
            foreach (var k in dlFileList)
            {
                var dlTask = WriteFileFromWebRequest(k.name, k.ver, GamePath.abApiUrl + k.name, GamePath.bundleDirPath);
                Debug.Log($"{GamePath.abApiUrl + k.name}からリクエスト");
                dlTaskList.Add(dlTask);
            }

            Debug.Log("ABダウンロード/書き込み開始");
            // 取得したDLタスクをまとめて実行し、全て終わるまで待つ
            await UniTask.WhenAll(dlTaskList);
            Debug.Log("ABダウンロード/書き込み完了");

            /*
             * ダウンロードが成功したか確認し、成功したものをlocalabdictに書き込む
             * 失敗したものがあったとき、失敗したことを知らせて終了する
             *
             * 成功したかどうかの確認
             * 　idea1：保存するファイル名にバージョンをつけておき、（$"{name}_{ver}"）
             * 　       rdictとファイル名＋バージョンが一致した場合成功とし、
             * 　       古いバージョン(ldictから取得)のファイルは削除する
             *
             * 成功したものをldictに書き込む、失敗したものは次回のrdictとの比較でもう一度ダウンロードされる
             *
             * ldictをjsonで保存する
             */
            Dictionary<string, ABInfo> dldict = GetDictsFromFilesContainName_Ver(GamePath.bundleDirPath);
            if (dldict != null)
            {
                foreach (var r in rdict)
                {
                    //ダウンロード成功(rdictとdldictでname,verが一致)したなら
                    if (dldict.ContainsKey(r.Key) && dldict[r.Key].ver == r.Value.ver)
                    {
                        //Debug.Log($"{GamePath.bundleDirPath}/{r.Key}_{r.Value.ver} のダウンロードに成功");
                        if (ldict.ContainsKey(r.Key))
                        {
                            //古いバージョン(ldictのverから取得)を削除
                            //Debug.Log($"{GamePath.bundleDirPath}/{r.Key}_{ldict[r.Key].ver} を削除");
                        }
                    }
                }
            }

            Debug.Log("abdict上書き開始");
            // ldictをdldictで上書き保存する
            await ABDictJson.WriteABDictJson(dldict);
            Debug.Log("abdict上書き完了");

            //ABStockmanagerにあげる
            ABStockManager.abDict = dldict;
        }

        //両側からabdictを集める
        private static async UniTask<(Dictionary<string, ABInfo>, Dictionary<string, ABInfo>)> GetBothABDict()
        {
            var rtask = GetRemoteABDict();
            var ltask = GetLocalABDict();
            return await UniTask.WhenAll(rtask, ltask);
        }

        //サーバーからabdictを取得する
        private static async UniTask<Dictionary<string, ABInfo>> GetRemoteABDict()
        {
            var r = await ABDictJson.LoadRemoteABDictJson();
            Debug.Log("サーバーからabdictをロード");
            return r;
        }

        //ローカルからabdictを取得する
        private static async UniTask<Dictionary<string, ABInfo>> GetLocalABDict()
        {
            var r = await ABDictJson.LoadLocalABDictJson();
            Debug.Log("ローカルからabdictをロード");
            return r;
        }

        //バージョンの古いファイルの最新バージョンをURLから取得し、上書きする
        //通信に失敗したら上書きしない
        private static async UniTask WriteFileFromWebRequest(string name, int ver, string url, string savePath)
        {
            // url から byte[] にし、 savePath に name として上書き
            byte[] data = await WebUtilities.ByteFromWebRequest(url);
            if (data == default)
            {
                Debug.Log("データを取得できませんでした");
                //データが取得できなかったとき、上の処理でダウンロード済リストに入れないため、問題なし
            }
            else
            {
                await ABFileUtilities.WriteByteData(name, ver, data, savePath);
            }
        }

        //pathにあるファイルの内、"{name}_{ver}"の形のものをnameをKey, ABInfo.verをValueにして返す
        private static Dictionary<string, ABInfo> GetDictsFromFilesContainName_Ver(string path)
        {
            return Name_VerStrings2Dicts(GetFilesName_Ver(path));
        }

        //$"{name}_{ver}"の形にしたファイル名を分解する
        //事前にSelectFilesContainTailAnderScoreAndNumber(string[] files)で絞ったものを渡す
        private static Dictionary<string, ABInfo> Name_VerStrings2Dicts(string[] strings)
        {
            Dictionary<string, ABInfo> _dict = new();
            for (int i = 0; i < strings.Length; i++)
            {
                string[] array = strings[i].Split('_');
                string[] newArray = array.Take(array.Length - 1).ToArray();

                string _name = string.Join('_', newArray);
                ABInfo _abinfo = new(int.Parse(array[^1]));

                //Debug.Log($"{strings[i]} -> {_name} {array[^1]}");
                _dict.Add(_name, _abinfo);
            }

            return _dict;
        }

        //末尾が *****_1 のようになったファイルのみを返す
        private static string[] SelectFilesName_Ver(string[] files)
        {
            List<string> resultlist = new();
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                if (System.Text.RegularExpressions.Regex.IsMatch(fileName, @"_\d+$"))
                {
                    resultlist.Add(fileName);
                }
            }

            var resultarray = resultlist.ToArray();

            return resultarray;
        }

        // pathから読みだしたファイルの内、末尾が *****_1 のようになったファイルのみを返す
        private static string[] GetFilesName_Ver(string path)
        {
            return SelectFilesName_Ver(FileUtilities.GetFiles(path));
        }

        #endregion
    }

}