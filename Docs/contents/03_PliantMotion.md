# PliantMotionの使い方

## 入手

長谷川研究室のGitレポジトリ（注：アカウント作成が必要）から入手してください。

**※PliantMotionは研究中のソフトウェアです。PliantMotionのソースを外部に公開しないでください。**



PliantMotionの利用には2種類のライブラリが必要です。

 

- SprUnity

  - http://git.haselab.net/haselab/SprUnity.git
  - 物理エンジンSpringheadをUnity上で利用するためのライブラリです。

- VGent Core

  - http://git.haselab.net/haselab/VGentCore.git 

  - 人の物理モデル用のスクリプトが入っているレポジトリです。動作にはSprUnityが必要です。



​    

#### サンプルプロジェクト

- PliantMotionSamples
  - http://git.haselab.net/Ken/PliantMotionSamples.git
  - PliantMotion用のサンプル
  - ライブラリはgit submoduleの形で含まれているので、cloneした後に**submodule update** を行ってください。



## チュートリアル

**チュートリアルの進め方**

- ゼロから始める：　「[Unityの新規プロジェクトを作る](#Unityの新規プロジェクトを作る)」 から
- サンプルを元に試す
  - 新しい動作を作りたい：　「[Bodyを動かしてみる](#Bodyを動かしてみる)」から
  - キャラクタを変えたい：　「[キャラクタを追加する](#キャラクタを追加する)」から
##### Unityをインストール
今回使用しているUnityのバージョンは「2018.4.5f1」です。

##### Unityの新規プロジェクトを作る

`SprUnity`と`VGentCore`と`PliantMition`とUniVRM( https://github.com/vrm-c/UniVRM/releases )をAssets以下に追加。(今回はAssets/Libraries/)。開発する場合は`SprUnity`と`VGentCore`と`PliantMition`をGitのSubmoduleという機能を使ってください。

![image-20191127220813975](C:\Users\sugi\AppData\Roaming\Typora\typora-user-images\image-20191127220813975.png)

##### PHSceneをシーンに追加

新しいGameObjectを作り、PHSceneBehaviourをアタッチします

![image-20191127221535131](C:\Users\sugi\AppData\Roaming\Typora\typora-user-images\image-20191127221535131.png)



##### BodyのPrefabをシーンに追加

人の物理モデルの PliantBody.prefab(Assets/Libraries/PliantMotion/Prefabs/) をSceneに追加します

![image-20191127225231911](C:\Users\sugi\AppData\Roaming\Typora\typora-user-images\image-20191127225231911.png)

PliantBody.prefabをUnpack Prefabします

![Unpack](C:\Users\sugi\Pictures\研究\Unpack.png)

キャラクタをhttps://hub.vroid.com/characters/675572020956181239/models/6535695942068248968 からダウンロードしてインポート,キャラクタのPrefabをシーンに追加

![image-20191127234745530](C:\Users\sugi\AppData\Roaming\Typora\typora-user-images\image-20191127234745530.png)

PliantBodyのBodyスクリプトのAnimatorにキャラクタ(Darkness_Shibu)をセットしてFitToAvatarボタンを押す(PliantBodyの関節位置などをキャラクタに適用)

![image-20191128000029159](C:\Users\sugi\AppData\Roaming\Typora\typora-user-images\image-20191128000029159.png)

HierarchyWindowでDarkness_Shibuを選択してCtrl-Dで複製する

![image-20191128000930010](C:\Users\sugi\AppData\Roaming\Typora\typora-user-images\image-20191128000930010.png)

分かりやすさのために複製したDarkness_Shibu(1)をDarkness_Shibu_Inputに名前変更

![image-20191128001111666](C:\Users\sugi\AppData\Roaming\Typora\typora-user-images\image-20191128001111666.png)

Animation Trace ControllerのAnimatorをDarkness_Shibu_Inputをセットする

![image-20191128002102579](C:\Users\sugi\AppData\Roaming\Typora\typora-user-images\image-20191128002102579.png)

実行してみる

![image-20191128002157469](C:\Users\sugi\AppData\Roaming\Typora\typora-user-images\image-20191128002157469.png)

何が起こっているかの解説

PliantBody:各関節をバネで引っ張っているキャラクタの物理モデル

Darkness_Shibu: PliantBodyの結果を

Darkness_Shibu_Input: PliantBodyのバネの目標



Darkness_Shibuの腕が下がっているのは、腕自体の重みに耐えきれずにバネが下がってしまっているため



この状態で躍らせてみます

Darkness_Shibu_InputのAnimatorのControllerにC86unitychan_001_SIM01_Finalをセット

![image-20191128014745471](C:\Users\sugi\AppData\Roaming\Typora\typora-user-images\image-20191128014745471.png)



Animationを再生してみる。バネが弱く、うまく追従してくれません。ConsoleWindowにエラーが出ていますが無視して大丈夫です。

![AnimationLowGain](C:\Users\sugi\Google ドライブ\hase\Movie\AnimationLowGain.gif)

Darkness_Shibu_InputのSkinnedMeshRendererをOFFにする



Play動く