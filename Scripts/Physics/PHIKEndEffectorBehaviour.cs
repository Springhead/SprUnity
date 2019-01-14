using UnityEngine;
using System.Collections;
using SprCs;
using SprUnity;
using System;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(PHIKEndEffectorBehaviour))]
public class PHIKEndEffectorBehaviourEditor : Editor {
    public void OnSceneGUI() {
        PHIKEndEffectorBehaviour phIKEEBehaviour = (PHIKEndEffectorBehaviour)target;

        // ----- ----- ----- ----- -----
        // Target Position Handle
        if (phIKEEBehaviour.iktarget == null) {
            if (phIKEEBehaviour.phIKEndEffector != null) {
                Vector3 currTargetPos = phIKEEBehaviour.phIKEndEffector.GetTargetPosition().ToVector3();
                Vector3 handlePos = Handles.PositionHandle(currTargetPos, Quaternion.identity);
                phIKEEBehaviour.desc.targetPosition = handlePos.ToVec3d();
                phIKEEBehaviour.phIKEndEffector.SetTargetPosition(handlePos.ToVec3d());

            } else if (phIKEEBehaviour.desc != null) {
                Vector3 currTargetPos = ((Vec3d)(phIKEEBehaviour.desc.targetPosition)).ToVector3();
                Vector3 handlePos = Handles.PositionHandle(currTargetPos, Quaternion.identity);
                phIKEEBehaviour.desc.targetPosition = handlePos.ToVec3d();
            }
        }
    }
}

#endif

[DefaultExecutionOrder(7)]
public class PHIKEndEffectorBehaviour : SprSceneObjBehaviour {
    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // メンバ変数

    public PHIKEndEffectorDescStruct desc = null;
    public GameObject iktarget = null;
    public GameObject ikLocalTarget = null;

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // このBehaviourに対応するSpringheadオブジェクト

    public PHIKEndEffectorIf phIKEndEffector { get { return sprObject as PHIKEndEffectorIf; } }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // SprBehaviourの派生クラスで実装するメソッド

    // -- DescStructオブジェクトを再構築する
    public override void ResetDescStruct() {
        desc = new PHIKEndEffectorDescStruct();
    }

    // -- DescStructオブジェクトを取得する
    public override CsObject GetDescStruct() {
        return desc;
    }

    // -- DescオブジェクトをNewして返す
    public override CsObject CreateDesc() {
        return new PHIKEndEffectorDesc();
    }

    // -- DescStructをDescに適用する
    public override void ApplyDesc(CsObject from, CsObject to) {
        (from as PHIKEndEffectorDescStruct).ApplyTo(to as PHIKEndEffectorDesc);
    }

    // -- Sprオブジェクトの構築を行う
    public override ObjectIf Build() {
        PHIKEndEffectorIf phIKee = phScene.CreateIKEndEffector((PHIKEndEffectorDesc)desc);
        phIKee.SetName("ike:" + gameObject.name);

        PHSolidBehaviour solidBehaviour = gameObject.GetComponent<PHSolidBehaviour>();
        if (solidBehaviour != null && solidBehaviour.sprObject != null) {
            phIKee.AddChildObject(solidBehaviour.sprObject);
        }
        phSceneBehaviour.RegisterPHIKEndEffectorBehaviour(this);

        return phIKee;
    }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // SprBehaviourの派生クラスで実装するメソッド

    // -- 全てのBuildが完了した後に行う処理を書く。オブジェクト同士をリンクするなど
    public override void Link() {
        PHIKBallActuatorBehaviour ba = gameObject.GetComponentInChildren<PHIKBallActuatorBehaviour>();
        if (ba != null && ba.sprObject != null && sprObject != null) {
            ba.sprObject.AddChildObject(sprObject);
        }
        PHIKHingeActuatorBehaviour ha = gameObject.GetComponentInChildren<PHIKHingeActuatorBehaviour>();
        if (ha != null && ha.sprObject != null && sprObject != null) {
            ha.sprObject.AddChildObject(sprObject);
        }
        UpdateIKTargetPosition();
    }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // MonoBehaviourのメソッド


    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // その他のメソッド
    
    // PHSceneのStepが呼ばれる前に呼ばれる
    public void BeforeStep() {
        UpdateIKTargetPosition();
    }

    public void UpdateIKTargetPosition() {
        if (iktarget != null) {
            if (sprObject != null) {
                PHIKEndEffectorIf phIKee = sprObject as PHIKEndEffectorIf;
                if (phIKee.GetOriCtlMode() == PHIKEndEffectorDesc.OriCtlMode.MODE_LOOKAT) {
                    phIKee.SetTargetLookat(iktarget.transform.position.ToVec3d());
                } else {
                    phIKee.SetTargetPosition(iktarget.transform.position.ToVec3d());

                    // 現在の姿勢からの回転角がより少ない方のQuaternionに変換
                    Quaterniond qT = iktarget.transform.rotation.ToQuaterniond();
                    Quaterniond qDiff = qT * phIKee.GetSolid().GetPose().Ori().Inv();
                    qDiff = Quaterniond.Rot(qDiff.RotationHalf());
                    qT = qDiff * phIKee.GetSolid().GetPose().Ori();
                    phIKee.SetTargetOrientation(qT);
                }
            }
        }

        if (ikLocalTarget != null) {
            if (sprObject != null) {
                Vec3d targetLocalPos = gameObject.transform.ToPosed().Inv() * ikLocalTarget.transform.position.ToVec3d();
                phIKEndEffector.SetTargetLocalPosition(targetLocalPos);
            }
        }
    }

}
