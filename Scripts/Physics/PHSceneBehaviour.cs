using UnityEngine;
using System.Collections.Generic;
using SprCs;
using System.Runtime.InteropServices;
using System;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(PHSceneBehaviour))]
public class PHSceneBehaviourEditor : Editor {

    public bool showCollision = false;

    public override void OnInspectorGUI() {
        PHSceneBehaviour phSceneBehaviour = (PHSceneBehaviour)target;

        DrawDefaultInspector();

        showCollision = EditorGUILayout.Foldout(showCollision, "Collision Setting");
        if (showCollision) {
            int i = 0;
            List<PHSceneBehaviour.CollisionSetting> removeList = new List<PHSceneBehaviour.CollisionSetting>();
            foreach (var collisionItem in phSceneBehaviour.collisionList) {
                i++;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Rule " + i + ":");
                if (GUILayout.Button(" - ")) {
                    removeList.Add(collisionItem);
                }
                if (GUILayout.Button(" ↑ ")) {
                    // -- TBD --
                }
                if (GUILayout.Button(" ↓ ")) {
                    // -- TBD -- 
                }
                EditorGUILayout.EndHorizontal();

                // -----

                EditorGUILayout.BeginHorizontal();
                collisionItem.targetSetMode1 = (PHSceneBehaviour.CollisionSetting.CollisionTargetSettingMode)(EditorGUILayout.EnumPopup("Target Solid 1", collisionItem.targetSetMode1));
                if (collisionItem.targetSetMode1 == PHSceneBehaviour.CollisionSetting.CollisionTargetSettingMode.One) {
                    collisionItem.solid1 = EditorGUILayout.ObjectField(collisionItem.solid1, typeof(PHSolidBehaviour), true) as PHSolidBehaviour;
                } else if (collisionItem.targetSetMode1 == PHSceneBehaviour.CollisionSetting.CollisionTargetSettingMode.NameMatching) {
                    collisionItem.solid1Pattern = EditorGUILayout.TextField(collisionItem.solid1Pattern);
                }
                EditorGUILayout.EndHorizontal();

                // --

                EditorGUILayout.BeginHorizontal();
                collisionItem.targetSetMode2 = (PHSceneBehaviour.CollisionSetting.CollisionTargetSettingMode)(EditorGUILayout.EnumPopup("Target Solid 2", collisionItem.targetSetMode2));
                if (collisionItem.targetSetMode2 == PHSceneBehaviour.CollisionSetting.CollisionTargetSettingMode.One) {
                    collisionItem.solid2 = EditorGUILayout.ObjectField(collisionItem.solid2, typeof(PHSolidBehaviour), true) as PHSolidBehaviour;
                } else if (collisionItem.targetSetMode2 == PHSceneBehaviour.CollisionSetting.CollisionTargetSettingMode.NameMatching) {
                    collisionItem.solid2Pattern = EditorGUILayout.TextField(collisionItem.solid2Pattern);
                }
                EditorGUILayout.EndHorizontal();

                // -----

                collisionItem.mode = (PHSceneDesc.ContactMode)(EditorGUILayout.EnumPopup("Contact Mode", collisionItem.mode));
            }

            foreach (var removeItem in removeList) {
                phSceneBehaviour.collisionList.Remove(removeItem);
            }

            if (GUILayout.Button("Add Collision Rule")) {
                phSceneBehaviour.collisionList.Add(new PHSceneBehaviour.CollisionSetting());
            }
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Late Awake/Start")) {
            SprBehaviour.ExecLateAwakeStart();
        }
    }
}
#endif

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
    public class CollisionSetting {
        public enum CollisionTargetSettingMode { All, One, NameMatching };

        public CollisionTargetSettingMode targetSetMode1 = CollisionTargetSettingMode.All;
        public PHSolidBehaviour solid1 = null;
        public string solid1Pattern = "";

        public CollisionTargetSettingMode targetSetMode2 = CollisionTargetSettingMode.All;
        public PHSolidBehaviour solid2 = null;
        public string solid2Pattern = "";

        public PHSceneDesc.ContactMode mode = PHSceneDesc.ContactMode.MODE_LCP;
    }
    [HideInInspector]
    public List<CollisionSetting> collisionList = new List<CollisionSetting>();

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

        ApplyCollisionList();
    }


    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // その他のメソッド

    public void ApplyCollisionList() {
        // <!!> PHJointBehaviourのdisableCollisionと衝突するので要検討
        if (sprObject != null) {
            for (int i = 0; i < collisionList.Count; i++) {
                CollisionSetting c = collisionList[i];

                // -----

                List<PHSolidBehaviour> solid1s = new List<PHSolidBehaviour>();
                List<PHSolidBehaviour> solid2s = new List<PHSolidBehaviour>();

                if (c.targetSetMode1 == CollisionSetting.CollisionTargetSettingMode.One) {
                    solid1s.Add(c.solid1);
                } else if (c.targetSetMode1 == CollisionSetting.CollisionTargetSettingMode.NameMatching) {
                    // -- TBD
                }

                if (c.targetSetMode2 == CollisionSetting.CollisionTargetSettingMode.One) {
                    solid2s.Add(c.solid2);
                } else if (c.targetSetMode2 == CollisionSetting.CollisionTargetSettingMode.NameMatching) {
                    // -- TBD
                }

                // -----

                if (c.targetSetMode1 == CollisionSetting.CollisionTargetSettingMode.All &&
                    c.targetSetMode2 == CollisionSetting.CollisionTargetSettingMode.All) {
                    phScene.SetContactMode(c.mode);

                } else if (c.targetSetMode1 == CollisionSetting.CollisionTargetSettingMode.All) {
                    foreach (var solid2 in solid2s) {
                        if (solid2.sprObject != null) {
                            phScene.SetContactMode(solid2.phSolid, c.mode);
                        }
                    }

                } else if (c.targetSetMode2 == CollisionSetting.CollisionTargetSettingMode.All) {
                    foreach (var solid1 in solid1s) {
                        if (solid1.sprObject != null) {
                            phScene.SetContactMode(solid1.phSolid, c.mode);
                        }
                    }

                } else {
                    foreach (var solid1 in solid1s) {
                        foreach (var solid2 in solid2s) {
                            if (solid1.sprObject != null && solid2.sprObject != null) {
                                phScene.SetContactMode(solid1.phSolid, solid2.phSolid, c.mode);
                            }
                        }
                    }

                }
            }
        }
    }

    public void RegisterPHSolidBehaviour(PHSolidBehaviour phSolid) {
        phSolidBehaviours.Add(phSolid);

        // スキンメッシュ描画時のカクつきを防ぐため、ツリー深さでソートしておく。
        phSolidBehaviours.Sort((a, b) => a.treeDepth.CompareTo(b.treeDepth));
    }

    public void RegisterPHIKEndEffectorBehaviour(PHIKEndEffectorBehaviour phIKEndEffector) {
        phIKEndEffectorBehaviours.Add(phIKEndEffector);
    }
}
