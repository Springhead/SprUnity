using UnityEngine;
using System.Collections.Generic;
using System.IO;
using SprCs;

public class SprBehaviourBase : MonoBehaviour {
    // -- DLLパスをセットするWindows API
    [System.Runtime.InteropServices.DllImport("kernel32", SetLastError = true)]
    protected static extern bool SetDllDirectory(string lpPathName);

    // -- 一度だけDLLパスをセットする（一度呼べば十分なので）
    public static bool dllPathAlreadySet = false;
    protected void SetDLLPath() {
        if (!dllPathAlreadySet) {
            // 非実行中にはApplication.dataPathは使えないので
            string currDir = Directory.GetCurrentDirectory();

            List<string> dirCands = new List<string>();
            dirCands.Add(currDir);

            // SprUnity/Pluginsフォルダの場所を探す
            for (int i = 0; i < 10; i++) { // フォルダ階層を10階層までは辿る
                List<string> newDirCands = new List<string>();
                foreach (var dir in dirCands) {
                    if (Directory.Exists(dir + "/SprUnity/Plugins")) {
                        SetDllDirectory(dir + "/SprUnity/Plugins");
                        Debug.Log("SprUnity Plugins Found at : " + dir + "/SprUnity/Plugins");
                        dllPathAlreadySet = true;
                        newDirCands.Clear();
                        break;

                    } else if (File.Exists(dir + "/Plugins/SprExport.dll")) {
                        SetDllDirectory(dir + "/Plugins");
                        dllPathAlreadySet = true;
                        newDirCands.Clear();
                        break;

                    } else {
                        foreach (var subDir in Directory.GetDirectories(dir)) {
                            newDirCands.Add(subDir);
                        }
                    }
                }
                dirCands = newDirCands;
            }
        }
    }

    // -- Spr関連オブジェクトがコンストラクトされる時に自動で呼ぶ
    public SprBehaviourBase() {
        SetDLLPath();
    }
}

public abstract class SprBehaviour : SprBehaviourBase {
    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // SpringheadオブジェクトからSprBehaviourを逆引きするマップ
    public static Dictionary<ObjectIf, SprBehaviour> sprBehaviourMap = new Dictionary<ObjectIf, SprBehaviour>();

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // AwakeとStartをすぐに実行せずに外部から指示があるまで待つ機能

    // AwakeとStartの実行待ち行列
    public static Queue<SprBehaviour> lateAwakeQueue = new Queue<SprBehaviour>();
    public static Queue<SprBehaviour> lateStartQueue = new Queue<SprBehaviour>();

    // AwakeとStartの実行待ちを有効化するフラグ
    public bool lateAwakeStart = false;

    // 実行待ち中のAwakeとStartを実行する
    public static void ExecLateAwakeStart() {
        while (lateAwakeQueue.Count > 0) {
            var sprBehaviour = lateAwakeQueue.Dequeue();
            if (sprBehaviour != null) {
                // print("Awake : " + sprBehaviour.name);
                sprBehaviour.AwakeImpl(fromLateExecQueue: true);
            }
        }
        while (lateStartQueue.Count > 0) {
            var sprBehaviour = lateStartQueue.Dequeue();
            if (sprBehaviour != null) {
                // print("Start : " + sprBehaviour.name);
                sprBehaviour.StartImpl(fromLateExecQueue: true);
            }
        }
    }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // 対応するSpringheadオブジェクト

    private ObjectIf sprObject_ = null;
    public ObjectIf sprObject { get { return sprObject_; }  protected set { sprObject_ = value; } }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // 派生クラスで実装するメソッド

    // -- DescStructオブジェクトを再構築する
    public abstract void ResetDescStruct();
    // -- DescStructオブジェクトを取得する
    public abstract CsObject GetDescStruct();
    // -- DescオブジェクトをNewして返す
    public abstract CsObject CreateDesc();
    // -- DescStructをDescに適用する
    public abstract void ApplyDesc(CsObject from, CsObject to);
    // -- Sprオブジェクトの構築を行う
    public abstract ObjectIf Build();
    // -- 全てのBuildが完了した後に行う処理を書く。オブジェクト同士をリンクするなど。無くても良い
    public virtual void Link() { }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // MonoBehaviourのメソッド

    // --
    public void Reset() {
        ResetDescStruct();
    }

    // --
    private bool awakeCalled = false;
    public virtual void Awake() { AwakeImpl(); }
    public void AwakeImpl(bool fromLateExecQueue = false) {
        if (lateAwakeStart && !(fromLateExecQueue)) {
            // 今すぐ実行せず、待ち行列に入れる
            lateAwakeQueue.Enqueue(this);

        } else {
            if (!awakeCalled && GetDescStruct() != null) {
                if (!enabled) { return; }
                sprObject = Build();
                // print(name + " : sprObject = " + sprObject.ToString());
                sprBehaviourMap[sprObject] = this; // 逆引き辞書に登録
                awakeCalled = true;
            }
        }
    }

    // --
    private bool startCalled = false;
    public virtual void Start() { StartImpl(); }
    public virtual void StartImpl(bool fromLateExecQueue = false) {
        if (lateAwakeStart && !(fromLateExecQueue)) {
            // 今すぐ実行せず、待ち行列に入れる
            lateStartQueue.Enqueue(this);

        } else {
            if (!startCalled && GetDescStruct() != null) {
                Link();
                // オブジェクトの作成が一通り完了したら一度OnValidateを読んで設定を確実に反映しておく
                OnValidate();
                startCalled = true;
            }
        }
    }

    // --
    public virtual void OnValidate() {
        if (GetDescStruct() == null) {
            ResetDescStruct();
        }

        if (sprObject != null) {
            CsObject d = CreateDesc();
            if (d != null) {
                sprObject.GetDesc(d);
                ApplyDesc(GetDescStruct(), d);
                sprObject.SetDesc(d);
            }
        }
    }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // その他のメソッド

    // Springheadオブジェクトに対応するGameObjectを返す
    public static Type GetBehaviour<Type>(ObjectIf springheadObject) where Type : SprBehaviour {
        if (sprBehaviourMap.ContainsKey(springheadObject)) {
            return sprBehaviourMap[springheadObject] as Type;
        }
        return null;
    }

}

public abstract class SprSceneObjBehaviour : SprBehaviour {
    //
    public PHSceneBehaviour phSceneBehaviour {
        get {
            PHSceneBehaviour pb = gameObject.GetComponentInParent<PHSceneBehaviour>();
            if (pb == null) {
                pb = FindObjectOfType<PHSceneBehaviour>();
                if (pb == null) {
                    throw new ObjectNotFoundException("PHSceneBehaviour was not found", gameObject);
                }
            }
            return pb;
        }
    }

    public PHSceneIf phScene {
        get {
            return phSceneBehaviour.sprObject as PHSceneIf;
        }
    }

    //
    public PHSdkIf phSdk {
        get {
            return phScene.GetSdk();
        }
    }
}

public class ObjectNotFoundException : System.Exception {
    public GameObject gameObject { get; private set; }

    public ObjectNotFoundException(string message, GameObject obj) : base(message) {
        gameObject = obj;
    }

    public override string ToString() {
        return gameObject.ToString() + " : " + Message;
    }
}