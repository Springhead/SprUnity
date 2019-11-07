using UnityEngine;
using System.Collections;
using SprCs;
using System;

[DefaultExecutionOrder(5)]
public class PHHumanBallJointResistanceBehaviour : SprSceneObjBehaviour {
    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // メンバ変数

    // <!!> これは何のために必要なもの？
    public Vec4d xcoeff;

    public PHHumanBallJointResistanceDescStruct desc = null;

    // このBehaviourに対応するSpringheadオブジェクト

    public PHHumanBallJointResistanceIf phJointResistance { get { return sprObject as PHHumanBallJointResistanceIf; } }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // SprBehaviourの派生クラスで実装するメソッド

    // -- DescStructオブジェクトを再構築する
    public override void ResetDescStruct() {
        desc = new PHHumanBallJointResistanceDescStruct();
    }

    // -- DescStructオブジェクトを取得する
    public override CsObject GetDescStruct() {
        return desc;
    }

    // -- DescオブジェクトをNewして返す
    public override CsObject CreateDesc() {
        return new PHHumanBallJointResistanceDesc();
    }

    // -- DescStructをDescに適用する
    public override void ApplyDesc(CsObject from, CsObject to) {
        (from as PHHumanBallJointResistanceDescStruct).ApplyTo(to as PHHumanBallJointResistanceDesc);
    }

    // -- Sprオブジェクトの構築を行う
    public override ObjectIf Build() {
        PHBallJointIf jo = gameObject.GetComponent<PHBallJointBehaviour>().sprObject as PHBallJointIf;
        if (jo == null) return null;

        PHHumanBallJointResistanceIf motor = jo.CreateMotor(PHHumanBallJointResistanceIf.GetIfInfoStatic(), (PHHumanBallJointResistanceDesc)desc) as PHHumanBallJointResistanceIf;

        return motor;
    }

}

