using UnityEngine;
using System.Collections;
using SprCs;
using System;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(PHSliderJointBehavior))]
public class PHSliderJointBehaviorEditor : PHJointBehaviourEditor {
    public void OnSceneGUI() {
        base.OnSceneGUI();
    }
}

#endif

[DefaultExecutionOrder(4)]
public class PHSliderJointBehavior : PHJointBehaviour {
    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // メンバ変数

    public PHSliderJointDescStruct desc = null;

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // このBehaviourに対応するSpringheadオブジェクト

    public PHSliderJointIf phSliderJoint { get { return sprObject as PHSliderJointIf; } }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // SprBehaviourの派生クラスで実装するメソッド

    // -- DescStructオブジェクトを再構築する
    public override void ResetDescStruct() {
        desc = new PHSliderJointDescStruct();
    }

    // -- DescStructオブジェクトを取得する
    public override CsObject GetDescStruct() {
        return desc;
    }

    // -- DescオブジェクトをNewして返す
    public override CsObject CreateDesc() {
        return new PHSliderJointDesc();
    }

    // -- DescStructをDescに適用する
    public override void ApplyDesc(CsObject from, CsObject to) {
        (from as PHSliderJointDescStruct).ApplyTo(to as PHSliderJointDesc);
    }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // PHJointBehaviourの派生クラスで実装するメソッド

    // -- 関節を作成する
    public override PHJointIf CreateJoint(PHSolidIf soSock, PHSolidIf soPlug) {
        return phScene.CreateJoint(soSock, soPlug, PHSliderJointIf.GetIfInfoStatic(), (PHSliderJointDesc)desc);
    }

}
