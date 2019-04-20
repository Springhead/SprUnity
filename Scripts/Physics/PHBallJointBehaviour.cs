﻿using UnityEngine;
using System.Collections;
using SprUnity;
using SprCs;
using System;

#if UNITY_EDITOR
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(PHBallJointBehaviour))]
public class PHBallJointBehaviourEditor : PHJointBehaviourEditor {
    public void OnSceneGUI() {
        base.OnSceneGUI();
    }
}

#endif

[DefaultExecutionOrder(4)]
public class PHBallJointBehaviour : PHJointBehaviour {
    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // メンバ変数

    public PHBallJointDescStruct desc;

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // このBehaviourに対応するSpringheadオブジェクト

    public PHBallJointIf phBallJoint { get { return sprObject as PHBallJointIf; } }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // SprBehaviourの派生クラスで実装するメソッド

    // -- DescStructオブジェクトを再構築する
    public override void ResetDescStruct() {
        desc = new PHBallJointDescStruct();
    }

    // -- DescStructオブジェクトを取得する
    public override CsObject GetDescStruct() {
        return desc;
    }

    // -- DescオブジェクトをNewして返す
    public override CsObject CreateDesc() {
        return new PHBallJointDesc();
    }

    // -- DescStructをDescに適用する
    public override void ApplyDesc(CsObject from, CsObject to) {
        (from as PHBallJointDescStruct).ApplyTo(to as PHBallJointDesc);
    }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // PHJointBehaviourの派生クラスで実装するメソッド

    // -- 関節を作成する
    public override PHJointIf CreateJoint(PHSolidIf soSock, PHSolidIf soPlug) {
        return phScene.CreateJoint(soSock, soPlug, PHBallJointIf.GetIfInfoStatic(), (PHBallJointDesc)desc);
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
                PHSolidIf soPlug = phBallJoint.GetPlugSolid();
                Posed plugPose = new Posed();
                phBallJoint.GetPlugPose(plugPose);
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
                PHSolidIf soSocket = phBallJoint.GetSocketSolid();
                Posed socketPose = new Posed();
                phBallJoint.GetSocketPose(socketPose);
                return soSocket.GetPose() * socketPose;
            }
        }
    }
}
