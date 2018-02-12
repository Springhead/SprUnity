using UnityEngine;
using System.Collections;
using SprCs;
using System;

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

}
