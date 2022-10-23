using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SprCs;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(PHFixJointBehavior))]
[CanEditMultipleObjects]
public class PHFixJointBehaviorEditor : Editor {
}

#endif
[DefaultExecutionOrder(4)]
public class PHFixJointBehavior : PHJointBehaviour {
    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // メンバ変数

    public PHFixJointDescStruct desc = null;

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // このBehaviourに対応するSpringheadオブジェクト

    public PHFixJointIf phFixJoint { get { return sprObject as PHFixJointIf; } }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // SprBehaviourの派生クラスで実装するメソッド

    // -- DescStructオブジェクトを再構築する
    public override void ResetDescStruct() {
        desc = new PHFixJointDescStruct();
    }

    // -- DescStructオブジェクトを取得する
    public override CsObject GetDescStruct() {
        return desc;
    }

    // -- DescオブジェクトをNewして返す
    public override CsObject CreateDesc() {
        return new PHFixJointDesc();
    }

    // -- DescStructをDescに適用する
    public override void ApplyDesc(CsObject from, CsObject to) {
        (from as PHFixJointDescStruct).ApplyTo(to as PHFixJointDesc);
    }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // PHJointBehaviourの派生クラスで実装するメソッド

    // -- 関節を作成する
    public override PHJointIf CreateJoint(PHSolidIf soSock, PHSolidIf soPlug) {
        return phScene.CreateJoint(soSock, soPlug, PHFixJointIf.GetIfInfoStatic(), (PHFixJointDesc)desc);
    }
}
