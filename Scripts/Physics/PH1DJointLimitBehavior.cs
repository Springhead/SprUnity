using UnityEngine;
using System.Collections;
using SprCs;
using System;

// CustomEditorは以下に定義
// SprUnity/Editor/Physics/PH1DJointLimitBehaviorEditor.cs

[DefaultExecutionOrder(5)]
public class PH1DJointLimitBehavior : SprSceneObjBehaviour {
    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // メンバ変数

    public PH1DJointLimitDescStruct desc = null;

    public GameObject jointObject = null;

    public Quaternion rot = new Quaternion(0, 0, 0, 1);

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // このBehaviourに対応するSpringheadオブジェクト

    public PH1DJointLimitIf phJointLimit { get { return sprObject as PH1DJointLimitIf; } }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // SprBehaviourの派生クラスで実装するメソッド

    // -- DescStructオブジェクトを再構築する
    public override void ResetDescStruct() {
        desc = new PH1DJointLimitDescStruct();
        desc.bEnabled = true; // <!!> なんでデフォルトでfalseなんだろう…？
    }

    // -- DescStructオブジェクトを取得する
    public override CsObject GetDescStruct() {
        return desc;
    }

    // -- DescオブジェクトをNewして返す
    public override CsObject CreateDesc() {
        return new PH1DJointLimitDesc();
    }

    // -- DescStructをDescに適用する
    public override void ApplyDesc(CsObject from, CsObject to) {
        (from as PH1DJointLimitDescStruct).ApplyTo(to as PH1DJointLimitDesc);
    }

    // -- Sprオブジェクトの構築を行う
    public override ObjectIf Build() {
        PH1DJointIf jo = null;

        PHJointBehaviour b = (jointObject ? jointObject : gameObject).GetComponent<PHHingeJointBehaviour>();
        if (!b) {
            b = (jointObject ? jointObject : gameObject).GetComponent<PHSliderJointBehavior>();
            if (!b) {
                return null;
            }
        }

        jo = b.sprObject as PH1DJointIf;
        if (jo == null) { return null; }

        PH1DJointLimitIf lim = jo.CreateLimit((PH1DJointLimitDesc)desc);

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
            phJointLimit.SetSpring(desc.spring);
            phJointLimit.SetRange(desc.range);
        }
    }
}