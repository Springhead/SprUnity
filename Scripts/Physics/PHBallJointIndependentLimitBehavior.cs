using UnityEngine;
using System.Collections;
using SprCs;
using System;

[DefaultExecutionOrder(5)]
public class PHBallJointIndependentLimitBehavior : SprSceneObjBehaviour {
    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // メンバ変数

    public PHBallJointIndependentLimitDescStruct desc = null;

    public GameObject jointObject = null;

    public Quaternion rot = new Quaternion(0, 0, 0, 1);

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // このBehaviourに対応するSpringheadオブジェクト

    public PHBallJointIndependentLimitIf phJointLimit { get { return sprObject as PHBallJointIndependentLimitIf; } }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // SprBehaviourの派生クラスで実装するメソッド

    // -- DescStructオブジェクトを再構築する
    public override void ResetDescStruct() {
        desc = new PHBallJointIndependentLimitDescStruct();
        desc.bEnabled = true; // <!!> なんでデフォルトでfalseなんだろう…？
    }

    // -- DescStructオブジェクトを取得する
    public override CsObject GetDescStruct() {
        return desc;
    }

    // -- DescオブジェクトをNewして返す
    public override CsObject CreateDesc() {
        return new PHBallJointIndependentLimitDesc();
    }

    // -- DescStructをDescに適用する
    public override void ApplyDesc(CsObject from, CsObject to) {
        (from as PHBallJointIndependentLimitDescStruct).ApplyTo(to as PHBallJointIndependentLimitDesc);
    }

    // -- Sprオブジェクトの構築を行う
    public override ObjectIf Build() {
        PHBallJointIf jo = null;

        var b = (jointObject ? jointObject : gameObject).GetComponent<PHBallJointBehaviour>();
        if (!b) { return null; }

        jo = b.sprObject as PHBallJointIf;
        if (jo == null) { return null; }

        PHBallJointIndependentLimitDesc d = desc;
        PHBallJointLimitIf lim = jo.CreateLimit(PHBallJointIndependentLimitIf.GetIfInfoStatic(), d);

        return lim;
    }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // MonoBehaviourのメソッド

    // UnityのOnValidate : SprBehaviourのものをオーバーライド
    public override void OnValidate() {
        if (GetDescStruct() == null) {
            ResetDescStruct();
        }

        // LimitにSetDescしても効果がなかったので直接セット
        // <!!> SetDescが使えるようにすべき
        if (sprObject != null) {
            phJointLimit.Enable(desc.bEnabled);
            phJointLimit.SetDamper(desc.damper);
            phJointLimit.SetLimitDir(desc.limitDir);
            phJointLimit.SetSpring(desc.spring);
		    phJointLimit.SetLimitRangeN(0, desc.limitX);
		    phJointLimit.SetLimitRangeN(1, desc.limitY);
            phJointLimit.SetLimitRangeN(2, desc.limitZ);
        }
    }

}
