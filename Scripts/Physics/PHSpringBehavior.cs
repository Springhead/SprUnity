using UnityEngine;
using System.Collections;
using SprCs;

#if UNITY_EDITOR
    using UnityEditor;

    [CustomEditor(typeof(PHSpringBehavior))]
    [CanEditMultipleObjects]
    public class PHSpringBehaviorEditor : Editor {
    }

#endif
[DefaultExecutionOrder(4)]
public class PHSpringBehavior : PHJointBehaviour {
    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // メンバ変数

    public PHSpringDescStruct desc = null;

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // このBehaviourに対応するSpringheadオブジェクト

    public PHSpringIf phSpring { get { return sprObject as PHSpringIf; } }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // SprBehaviourの派生クラスで実装するメソッド

    // -- DescStructオブジェクトを再構築する
    public override void ResetDescStruct() {
        desc = new PHSpringDescStruct();

        // Springheadだと初期値がゼロなので、多少はバネとして機能する値にしておく
        desc.spring = new Vec3d(10.0, 10.0, 10.0);
        desc.damper = new Vec3d(1.0, 1.0, 1.0);
        desc.springOri = 10.0;
        desc.damperOri = 1.0;
    }

    // -- DescStructオブジェクトを取得する
    public override CsObject GetDescStruct() {
        return desc;
    }

    // -- DescオブジェクトをNewして返す
    public override CsObject CreateDesc() {
        return new PHSpringDesc();
    }

    // -- DescStructをDescに適用する
    public override void ApplyDesc(CsObject from, CsObject to) {
        (from as PHSpringDescStruct).ApplyTo(to as PHSpringDesc);
    }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // PHJointBehaviourの派生クラスで実装するメソッド

    // -- 関節を作成する
    public override PHJointIf CreateJoint(PHSolidIf soSock, PHSolidIf soPlug) {
        return phScene.CreateJoint(soSock, soPlug, PHSpringIf.GetIfInfoStatic(), (PHSpringDesc)desc);
    }
}
