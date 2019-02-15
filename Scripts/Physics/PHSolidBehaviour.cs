using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Reflection;
using SprCs;
using SprUnity;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(PHSolidBehaviour))]
[CanEditMultipleObjects]
public class PHSolidBehaviourEditor : Editor {
    public void OnSceneGUI() {
        PHSolidBehaviour pHSolidBehaviour = (PHSolidBehaviour)target;

        // ----- ----- ----- ----- -----
        // Fixed Solid Position Handle
        if (pHSolidBehaviour.fixedSolid) {
            Tools.current = Tool.None;
            pHSolidBehaviour.fixedSolidPosition = Handles.PositionHandle(pHSolidBehaviour.fixedSolidPosition, Quaternion.identity);
            pHSolidBehaviour.fixedSolidRotation = Handles.RotationHandle(pHSolidBehaviour.fixedSolidRotation, pHSolidBehaviour.fixedSolidPosition);
        }
    }
}

#endif

[DefaultExecutionOrder(2)]
public class PHSolidBehaviour : SprSceneObjBehaviour {
    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // メンバ変数

    public PHSolidDescStruct desc;
    public bool autoSetInertiaTensor = false;
    public GameObject centerOfMass = null;

    public bool fixedSolid = false;

    [HideInInspector]
    public Vector3 fixedSolidPosition = new Vector3();

    [HideInInspector]
    public Quaternion fixedSolidRotation = new Quaternion();

    // このGameObjectがScene Hierarchyでどれくらいの深さにあるか。浅いものから順にUpdatePoseするために使う
    [HideInInspector]
    public int treeDepth = 0;

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // このBehaviourに対応するSpringheadオブジェクト

    public PHSolidIf phSolid { get { return sprObject as PHSolidIf; } }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // SprBehaviourの派生クラスで実装するメソッド

    // -- DescStructオブジェクトを再構築する
    public override void ResetDescStruct() {
        desc = new PHSolidDescStruct();
    }

    // -- DescStructオブジェクトを取得する
    public override CsObject GetDescStruct() {
        return desc;
    }

    // -- DescオブジェクトをNewして返す
    public override CsObject CreateDesc() {
        return new PHSolidDesc();
    }

    // -- DescStructをDescに適用する
    public override void ApplyDesc(CsObject from, CsObject to) {
        (from as PHSolidDescStruct).ApplyTo(to as PHSolidDesc);
    }

    // -- Sprオブジェクトの構築を行う
    public override ObjectIf Build() {
		PHSolidIf so = phScene.CreateSolid (desc);
        so.SetName("so:" + gameObject.name);
		so.SetPose (gameObject.transform.ToPosed());

        fixedSolidPosition = gameObject.transform.position;
        fixedSolidRotation = gameObject.transform.rotation;

        // Scene Hierarchyでの深さを取得した上でPHSceneBehaviourに登録
        var t = transform;
        while (t.parent != null) { treeDepth++; t = t.parent; }
        phSceneBehaviour.RegisterPHSolidBehaviour(this);

        UpdateCenterOfMass();

        return so;
    }

    // -- 全てのBuildが完了した後に行う処理を書く。オブジェクト同士をリンクするなど
    public override void Link() {
        if (sprObject == null) { return; }

        // 慣性テンソルを自動決定する
        if (autoSetInertiaTensor && phSolid.NShape() > 0) {
            float totalVolume = 0;
            for (int i = 0; i < phSolid.NShape(); i++) {
                var shape = phSolid.GetShape(i);
                totalVolume += shape.CalcVolume();
            }
            for (int i = 0; i < phSolid.NShape(); i++) {
                var shape = phSolid.GetShape(i);
                shape.SetDensity((float)(phSolid.GetMass()) / totalVolume);
            }
            phSolid.CompInertia();

            // --

            // <!!> SpineとShoulderはCompInertiaすると落ちるのでデバッグ中
            if (name == "Spine" || name.Contains("Shoulder")) {
                var I = phSolid.GetInertia();
                string str = name + " : \r\n";
                for (int i = 0; i < 3; i++) {
                    for (int j = 0; j < 3; j++) {
                        str += I[i][j].ToString("F4") + ", ";
                    }
                    str += "\r\n";
                }

                CDRoundConeIf rc = phSolid.GetShape(0) as CDRoundConeIf;
                if (rc != null) {
                    float mass = (float)(phSolid.GetMass());
                    float radius = (rc.GetRadius().x + rc.GetRadius().y) * 0.5f;
                    float length = rc.GetLength() + rc.GetRadius().x + rc.GetRadius().y;
                    float Ix = 0.5f * mass * radius * radius;
                    float Iy = mass * ((radius * radius / 4.0f) + (length * length / 12.0f));
                    float Iz = Iy;
                    phSolid.SetInertia(new Matrix3d(Ix, 0, 0, 0, Iy, 0, 0, 0, Iz));
                }

                I = phSolid.GetInertia();
                str += "\r\n";
                for (int i = 0; i < 3; i++) {
                    for (int j = 0; j < 3; j++) {
                        str += I[i][j].ToString("F4") + ", ";
                    }
                    str += "\r\n";
                }
                Debug.Log(str);
            }

            // --

            PHSolidDesc desc_ = new PHSolidDesc();
            phSolid.GetDesc(desc_);
            desc = desc_;
        }

        //float I = (float)(phSolid.GetMass());
        //phSolid.SetInertia(new Matrix3d(I, 0, 0, 0, I, 0, 0, 0, I));
    }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // MonoBehaviourのメソッド

    // UnityのOnValidate
    public override void OnValidate() {
        if (desc.mass == 0) {
            desc.mass = 1e-5;
            Debug.LogWarning("PHSolidBehaviour(" + gameObject.name + ") has 0 mass.");
        }
        base.OnValidate();
        UpdateCenterOfMass();
    }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // その他のメソッド

    // PHSceneのStepが呼ばれる前に呼ばれる
    public void BeforeStep() {
        UpdateCenterOfMass();
    }

    // Springhead剛体とGameObjectの間での位置姿勢の同期：　更新順を制御するためPHSceneからまとめて呼び出す
    public void UpdatePose () {
        if (sprObject != null) {
            PHSolidIf so = sprObject as PHSolidIf;
            if (fixedSolid) {
                // Fixedな剛体はHandleの位置をSpringheadに反映
                so.SetPose(new Posed(fixedSolidPosition.ToVec3d(), fixedSolidRotation.ToQuaterniond()));

            } else {
                // Fixedでない剛体の場合

                if (!so.IsDynamical()) {
                    // Dynamicalでない剛体はUnityの位置をSpringheadに反映（操作可能）
                    so.SetPose(gameObject.transform.ToPosed());
                } else {
                    // Dynamicalな剛体はSpringheadのシミュレーション結果をUnityに反映
                    gameObject.transform.FromPosed(so.GetPose());
                }

                fixedSolidPosition = gameObject.transform.position;
                fixedSolidRotation = gameObject.transform.rotation;
            }
        }
	}

    public void UpdateCenterOfMass () {
        if (centerOfMass != null) {
            Vec3d centerOfMassLocalPos = gameObject.transform.ToPosed().Inv() * centerOfMass.transform.position.ToVec3d();
            desc.center = centerOfMassLocalPos;
            if (phSolid != null) {
                phSolid.SetCenterOfMass(centerOfMassLocalPos);
            }
        }
    }

}
