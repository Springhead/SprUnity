using UnityEngine;
using System.Collections;
using SprUnity;
using SprCs;
using System;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(PHHingeJointBehaviour))]
[CanEditMultipleObjects]
public class PHHingeJointBehaviourEditor : PHJointBehaviourEditor {
    PHHingeJointDesc desc = new PHHingeJointDesc();

    public void OnSceneGUI() {
        base.OnSceneGUI();

        PHHingeJointBehaviour phHingeJointBehaviour = target as PHHingeJointBehaviour;

        if (phHingeJointBehaviour.showJointTargetPositionHandle) {
            Tools.current = Tool.None;
            /*
            if (phHingeJointBehaviour.phHingeJoint != null) {
                phHingeJointBehaviour.phHingeJoint.GetDesc(desc);
                double currTargetPosition = desc.targetPosition;
                Quaternion handleRot = Handles.Di(currTargetPosition, phBallJointBehaviour.transform.position);
                desc.targetPosition = handleRot.ToQuaterniond();
                phBallJointBehaviour.phBallJoint.SetDesc(desc);
                phBallJointBehaviour.desc.targetPosition = desc.targetPosition;

            } else if (phHingeJointBehaviour.desc != null) {
                Quaternion currPullbackTarget = ((Quaterniond)(phBallJointBehaviour.desc.targetPosition)).ToQuaternion();
                Quaternion handleRot = Handles.RotationHandle(currPullbackTarget, phBallJointBehaviour.transform.position);
                phBallJointBehaviour.desc.targetPosition = handleRot.ToQuaterniond();
            }
            */
        }
    }
}

#endif

[DefaultExecutionOrder(4)]
public class PHHingeJointBehaviour : PHJointBehaviour {
    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // メンバ変数

    public PHHingeJointDescStruct desc;

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // このBehaviourに対応するSpringheadオブジェクト

    public PHHingeJointIf phHingeJoint { get { return sprObject as PHHingeJointIf; } }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // SprBehaviourの派生クラスで実装するメソッド

    // -- DescStructオブジェクトを再構築する
    public override void ResetDescStruct() {
        desc = new PHHingeJointDescStruct();
    }

    // -- DescStructオブジェクトを取得する
    public override CsObject GetDescStruct() {
        return desc;
    }

    // -- DescオブジェクトをNewして返す
    public override CsObject CreateDesc() {
        return new PHHingeJointDesc();
    }

    // -- DescStructをDescに適用する
    public override void ApplyDesc(CsObject from, CsObject to) {
        (from as PHHingeJointDescStruct).ApplyTo(to as PHHingeJointDesc);
        (from as PH1DJointDescStruct).ApplyTo(to as PH1DJointDesc);
    }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // PHJointBehaviourの派生クラスで実装するメソッド

    // -- 関節を作成する
    public override PHJointIf CreateJoint(PHSolidIf soSock, PHSolidIf soPlug) {
        return phScene.CreateJoint(soSock, soPlug, PHHingeJointIf.GetIfInfoStatic(), (PHHingeJointDesc)desc);
    }

    // -- プラグ姿勢を取得する
    public Posed plugPose {
        get {
            if (sprObject == null) {
                if (autoSetSockPlugPose) {
                    return (jointObject ? jointObject.transform.ToPosed() : gameObject.transform.ToPosed() * new Posed(jointPosition.ToVec3d(), jointOrientation.ToQuaterniond()));
                } else {
                   return plug.transform.ToPosed() * desc.posePlug;
                }
            } else {
                PHSolidIf soPlug = phHingeJoint.GetPlugSolid();
                Posed plugPose = new Posed();
                phHingeJoint.GetPlugPose(plugPose);
                return soPlug.GetPose() * plugPose;
            }
        }
    }

    // -- ソケット姿勢を取得する
    public Posed socketPose {
        get {
            if (sprObject == null) {
                if (autoSetSockPlugPose) {
                    return (jointObject ? jointObject.transform.ToPosed() : gameObject.transform.ToPosed() * new Posed(jointPosition.ToVec3d(), jointOrientation.ToQuaterniond()));
                } else {
                    return socket.transform.ToPosed() * desc.poseSocket;
                }
            } else {
                PHSolidIf soSocket = phHingeJoint.GetSocketSolid();
                Posed socketPose = new Posed();
                phHingeJoint.GetSocketPose(socketPose);
                return soSocket.GetPose() * socketPose;
            }
        }
    }
}
