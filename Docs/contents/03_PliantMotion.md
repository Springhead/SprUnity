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

SprUnityとVGentCoreとUniVRM( https://github.com/vrm-c/UniVRM/releases )をAssets以下に追加。(今回はAssets/Libraries/)

![image-20191114010916233](C:\Users\sugi\AppData\Roaming\Typora\typora-user-images\image-20191114010916233.png)

##### PHSceneをシーンに追加



##### BodyのPrefabをシーンに追加

UnpackPrefab

BodyにAnimatorを追加してFitToAvatar

TraceControllerのAnimatorに追加

Play動く