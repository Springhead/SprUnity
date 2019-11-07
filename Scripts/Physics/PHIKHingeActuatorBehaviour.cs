using UnityEngine;
using System.Collections;
using SprCs;
using System;

[DefaultExecutionOrder(6)]
public class PHIKHingeActuatorBehaviour : PHIKActuatorBehaviour {
    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // メンバ変数

    public PHIKHingeActuatorDescStruct desc = null;

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // このBehaviourに対応するSpringheadオブジェクト

    public PHIKHingeActuatorIf phIKHingeActuator { get { return sprObject as PHIKHingeActuatorIf; } }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // SprBehaviourの派生クラスで実装するメソッド

    // -- DescStructオブジェクトを再構築する
    public override void ResetDescStruct() {
        desc = new PHIKHingeActuatorDescStruct();
    }

    // -- DescStructオブジェクトを取得する
    public override CsObject GetDescStruct() {
        return desc;
    }

    // -- DescオブジェクトをNewして返す
    public override CsObject CreateDesc() {
        return new PHIKHingeActuatorDesc();
    }

    // -- DescStructをDescに適用する
    public override void ApplyDesc(CsObject from, CsObject to) {
        (from as PHIKHingeActuatorDescStruct).ApplyTo(to as PHIKHingeActuatorDesc);
    }

    // -- Sprオブジェクトの構築を行う
    public override ObjectIf Build() {
        PHIKHingeActuatorIf phIKAct = phScene.CreateIKActuator(PHIKHingeActuatorIf.GetIfInfoStatic(), (PHIKHingeActuatorDesc)desc).Cast();
        phIKAct.SetName("ika:" + gameObject.name);
        phIKAct.Enable(true);

        PHHingeJointBehaviour bj = gameObject.GetComponent<PHHingeJointBehaviour>();
        if (bj != null && bj.sprObject != null) {
            phIKAct.AddChildObject(bj.sprObject);
        }

        return phIKAct;
    }

}
