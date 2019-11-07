using UnityEngine;
using System.Collections;
using SprCs;
using SprUnity;
using System;

// CustomEditorは以下に定義
// SprUnity/Editor/Physics/PHBallJointLimitBehaviorEditor.cs

[DefaultExecutionOrder(5)]
public class PHBallJointLimitBehavior : SprSceneObjBehaviour {
    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // メンバ変数

    public PHBallJointConeLimitDescStruct desc = null;

    public GameObject jointObject = null;

    public Quaternion rot = new Quaternion(0, 0, 0, 1);

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // このBehaviourに対応するSpringheadオブジェクト

    public PHBallJointConeLimitIf phJointLimit { get { return sprObject as PHBallJointConeLimitIf; } }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // SprBehaviourの派生クラスで実装するメソッド

    // -- DescStructオブジェクトを再構築する
    public override void ResetDescStruct() {
        desc = new PHBallJointConeLimitDescStruct();
        desc.bEnabled = true; // <!!> なんでデフォルトでfalseなんだろう…？
    }

    // -- DescStructオブジェクトを取得する
    public override CsObject GetDescStruct() {
        return desc;
    }

    // -- DescオブジェクトをNewして返す
    public override CsObject CreateDesc() {
        return new PHBallJointConeLimitDesc();
    }

    // -- DescStructをDescに適用する
    public override void ApplyDesc(CsObject from, CsObject to) {
        (from as PHBallJointConeLimitDescStruct).ApplyTo(to as PHBallJointConeLimitDesc);
    }

    // -- Sprオブジェクトの構築を行う
    public override ObjectIf Build() {
        PHBallJointIf jo = null;

        var b = (jointObject ? jointObject : gameObject).GetComponent<PHBallJointBehaviour>();
        if (!b) { return null; }

        jo = b.sprObject as PHBallJointIf;
        if (jo == null) { return null; }

        PHBallJointLimitIf lim = jo.CreateLimit(PHBallJointConeLimitIf.GetIfInfoStatic(), (PHBallJointConeLimitDesc)desc);

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
            phJointLimit.SetSwingRange(desc.limitSwing);
            phJointLimit.SetSwingDirRange(desc.limitSwingDir);
            phJointLimit.SetTwistRange(desc.limitTwist);
        }
    }
}
