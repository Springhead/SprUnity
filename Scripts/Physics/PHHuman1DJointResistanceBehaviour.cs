using UnityEngine;
using System.Collections;
using SprCs;
using System;

[DefaultExecutionOrder(5)]
public class PHHuman1DJointResistanceBehaviour : SprSceneObjBehaviour {
    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // メンバ変数

    public PHHuman1DJointResistanceDescStruct desc = null;

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // このBehaviourに対応するSpringheadオブジェクト

    public PHHuman1DJointResistanceIf phJointResistance { get { return sprObject as PHHuman1DJointResistanceIf; } }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // SprBehaviourの派生クラスで実装するメソッド

    // -- DescStructオブジェクトを再構築する
    public override void ResetDescStruct() {
        desc = new PHHuman1DJointResistanceDescStruct();
    }

    // -- DescStructオブジェクトを取得する
    public override CsObject GetDescStruct() {
        return desc;
    }

    // -- DescオブジェクトをNewして返す
    public override CsObject CreateDesc() {
        return new PHHuman1DJointResistanceDesc();
    }

    // -- DescStructをDescに適用する
    public override void ApplyDesc(CsObject from, CsObject to) {
        (from as PHHuman1DJointResistanceDescStruct).ApplyTo(to as PHHuman1DJointResistanceDesc);
    }

    // -- Sprオブジェクトの構築を行う
    public override ObjectIf Build() {
        PHHingeJointIf jo = gameObject.GetComponent<PHHingeJointBehaviour>().sprObject as PHHingeJointIf;
        if (jo == null) return null;

        PHHuman1DJointResistanceIf motor = jo.CreateMotor(PHHuman1DJointResistanceIf.GetIfInfoStatic(), (PHHuman1DJointResistanceDesc)desc) as PHHuman1DJointResistanceIf;

        return motor;
    }

}
