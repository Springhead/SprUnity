using UnityEngine;
using System.Collections.Generic;
using SprCs;
using System.Runtime.InteropServices;
using System;
using System.Linq;

[DefaultExecutionOrder(1)]
public class PHSceneBehaviour : SprBehaviour {
    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // メンバ変数

    protected List<PHSolidBehaviour> phSolidBehaviours = new List<PHSolidBehaviour>();
    protected List<PHIKEndEffectorBehaviour> phIKEndEffectorBehaviours = new List<PHIKEndEffectorBehaviour>();

    private static PHSdkIf phSdk = null;

    public PHSceneDescStruct desc = null;
    public PHIKEngineDescStruct descIK = null;

    public bool enableIK = true;
    public bool enableStep = true;
    public bool enableUpdate = true;

    [Serializable]
    public struct CollisionSetting {
        public PHSolidBehaviour solid1;
        public PHSolidBehaviour solid2;
        public PHSceneDesc.ContactMode mode;
    }
    public List<CollisionSetting> collision = new List<CollisionSetting>();
    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // このBehaviourに対応するSpringheadオブジェクト

    public PHSceneIf phScene { get { return sprObject as PHSceneIf; } }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // SprBehaviourの派生クラスで実装するメソッド

    // -- DescStructオブジェクトを再構築する
    public override void ResetDescStruct() {
        desc = new PHSceneDescStruct();
        desc.timeStep = Time.fixedDeltaTime; // 初期値ではUnityに合わせておく

        descIK = new PHIKEngineDescStruct();
        descIK.regularizeParam = 0.1f;
    }

    // -- DescStructオブジェクトを取得する
    public override CsObject GetDescStruct() {
        return desc;
    }

    // -- DescオブジェクトをNewして返す
    public override CsObject CreateDesc() {
        return new PHSceneDesc();
    }

    // -- DescStructをDescに適用する
    public override void ApplyDesc(CsObject from, CsObject to) {
        (from as PHSceneDescStruct).ApplyTo(to as PHSceneDesc);
    }

    // -- Sprオブジェクトの構築を行う
    public override ObjectIf Build() {
        SEH_Exception.init();

        FWAppBehaviour appB = GetComponent<FWAppBehaviour>();

        PHSceneIf phScene;
        if (appB != null) {
            FWApp app = FWAppBehaviour.app;
            phSdk   = app.GetSdk().GetPHSdk();
            phScene = app.GetSdk().GetScene(0).GetPHScene();
            phScene.Clear();
            phScene.SetDesc((PHSceneDesc)desc);
        } else {
            phSdk = PHSdkIf.CreateSdk();
            phScene = phSdk.CreateScene((PHSceneDesc)desc);
        }

        return phScene;
    }

    // -- 全てのBuildが完了した後に行う処理を書く。オブジェクト同士をリンクするなど
    public override void Link() {
        OnValidate();
    }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // MonoBehaviourのメソッド

    void FixedUpdate () {
        if (sprObject != null && enableStep) {
            foreach(var phSolidBehaviour in phSolidBehaviours){
                phSolidBehaviour.BeforeStep();
            }
            foreach(var phIKEndEffectorBehaviour in phIKEndEffectorBehaviours){
                phIKEndEffectorBehaviour.BeforeStep();
            }
            lock (sprObject) {
                (sprObject as PHSceneIf).Step();
            }
        }
    }

    void Update() {
        if (enableUpdate) {
            lock (sprObject) {
                foreach (var phSolidBehaviour in phSolidBehaviours) {
                    if (phSolidBehaviour != null) {
                        phSolidBehaviour.UpdatePose();
                    }
                }
            }
            if (FWAppBehaviour.app != null) {
                FWAppBehaviour.app.PostRedisplay();
            }
        }
    }

    // UnityのOnValidate : SprBehaviourのものをオーバーライド
    public override void OnValidate() {
        if (GetDescStruct() == null) {
            ResetDescStruct();
        }

        if (sprObject != null) {
            // PHSceneの設定
            {
                PHSceneDesc d = new PHSceneDesc();
                phScene.GetDesc(d);
                desc.ApplyTo(d);
                phScene.SetDesc(d);
            }

            // PHIKEngineの設定
            {
                PHIKEngineDesc d = new PHIKEngineDesc();
                phScene.GetIKEngine().GetDesc(d);
                descIK.ApplyTo(d);
                phScene.GetIKEngine().SetDesc(d);
            }

            // DescではなくStateに含まれる変数。ApplyToで自動同期されないので手動で設定
            phScene.SetTimeStep(desc.timeStep);
            phScene.SetHapticTimeStep(desc.haptictimeStep);

            // IKの有効・無効の切り替え
            phScene.GetIKEngine().Enable(enableIK);
        }

        // <!!> PHJointBehaviourのdisableCollisionと衝突するので要検討
        if (sprObject != null) {
            for (int i = 0; i < collision.Count; i++) {
                CollisionSetting c = collision[i];
                if (c.solid1 == null && c.solid2 == null) {
                    phScene.SetContactMode(c.mode);
                } else if (c.solid1 == null) {
                    if (c.solid2.sprObject != null) phScene.SetContactMode(c.solid2.phSolid, c.mode);
                } else if (c.solid2 == null) {
                    if(c.solid1.sprObject != null) phScene.SetContactMode(c.solid1.phSolid, c.mode);
                } else {
                    if (c.solid1.sprObject != null && c.solid2.sprObject != null) phScene.SetContactMode(c.solid1.phSolid, c.solid2.phSolid, c.mode);
                }
            }
        }
    }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // その他のメソッド

    public void RegisterPHSolidBehaviour(PHSolidBehaviour phSolid) {
        phSolidBehaviours.Add(phSolid);

        // スキンメッシュ描画時のカクつきを防ぐため、ツリー深さでソートしておく。
        phSolidBehaviours.Sort((a, b) => a.treeDepth.CompareTo(b.treeDepth));
    }

    public void RegisterPHIKEndEffectorBehaviour(PHIKEndEffectorBehaviour phIKEndEffector) {
        phIKEndEffectorBehaviours.Add(phIKEndEffector);
    }
}
