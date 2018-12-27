using UnityEngine;
using System.Collections;
using SprCs;
using System;

[DefaultExecutionOrder(6)]
public class PHIKBallActuatorBehaviour : PHIKActuatorBehaviour {
    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // メンバ変数

    public PHIKBallActuatorDescStruct desc = null;

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // このBehaviourに対応するSpringheadオブジェクト

    public PHIKBallActuatorIf phIKBallActuator { get { return sprObject as PHIKBallActuatorIf; } }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // SprBehaviourの派生クラスで実装するメソッド

    // -- DescStructオブジェクトを再構築する
    public override void ResetDescStruct() {
        desc = new PHIKBallActuatorDescStruct();
    }

    // -- DescStructオブジェクトを取得する
    public override CsObject GetDescStruct() {
        return desc;
    }

    // -- DescオブジェクトをNewして返す
    public override CsObject CreateDesc() {
        return new PHIKBallActuatorDesc();
    }

    // -- DescStructをDescに適用する
    public override void ApplyDesc(CsObject from, CsObject to) {
        (from as PHIKBallActuatorDescStruct).ApplyTo(to as PHIKBallActuatorDesc);
    }

    // -- Sprオブジェクトの構築を行う
    public override ObjectIf Build() {
        PHIKBallActuatorIf phIKAct = phScene.CreateIKActuator(PHIKBallActuatorIf.GetIfInfoStatic(), (PHIKBallActuatorDesc)desc).Cast();
        phIKAct.SetName("ika:" + gameObject.name);
        phIKAct.Enable(true);

        PHBallJointBehaviour bj = gameObject.GetComponent<PHBallJointBehaviour>();
        if (bj != null && bj.sprObject != null) {
            phIKAct.AddChildObject(bj.sprObject);
        }

        return phIKAct;
    }

}
