# Game_NoAsset
 素材を抜いたゲームのリポジトリです


UniTask、QuickSave、FancyScrollView、DOTween
等のライブラリを導入し、

Assets/ABPackage/ABAssets/ImageAssets にあるimageid0に画像を設定し、
Assets/ABPackage/ABAssets/MusicAssets にあるmusicid0に曲を設定した後、

Editorの上部メニューから
Assets/Build ScriptableABでアセットバンドルをビルドし、
Assets/ABPackage/ABBuillds内のアセットバンドルをStreamingAssetsにコピーすることで遊ぶことができる仕組みになっています。

実際に動かすにはもう少し手順が必要かと思います
