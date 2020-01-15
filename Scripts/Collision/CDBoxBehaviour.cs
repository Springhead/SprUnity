using UnityEngine;
using System.Collections;
using SprCs;
using System;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(CDBoxBehaviour))]
[CanEditMultipleObjects]
public class CDBoxBehaviourEditor : Editor {
}
#endif

[DefaultExecutionOrder(3)]
public class CDBoxBehaviour : CDShapeBehaviour {
    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // メンバ変数

    public CDBoxDescStruct desc = null;

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // このBehaviourに対応するSpringheadオブジェクト

    public CDBoxIf cdBox { get { return sprObject as CDBoxIf; } }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // SprBehaviourの派生クラスで実装するメソッド

    // -- DescStructオブジェクトを再構築する
    public override void ResetDescStruct() {
        desc = new CDBoxDescStruct();
    }

    // -- DescStructオブジェクトを取得する
    public override CsObject GetDescStruct() {
        return desc;
    }

    // -- DescオブジェクトをNewして返す
    public override CsObject CreateDesc() {
        return new CDBoxDesc();
    }

    // -- DescStructをDescに適用する
    public override void ApplyDesc(CsObject from, CsObject to) {
        (from as CDBoxDescStruct).ApplyTo(to as CDBoxDesc);
    }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // CDShapeBehaviourの派生クラスで実装するメソッド

    // -- 形状固有のShapePoseの取得。剛体からの相対位置姿勢による分は除く
    public override Posed ShapePose(GameObject shapeObject) {
        BoxCollider bc = shapeObject.GetComponent<BoxCollider>();
        Posed pose = new Posed();
        pose.px = bc.center.x;
        pose.py = bc.center.y;
        pose.pz = bc.center.z;
        return pose;
    }

    // -- SpringheadのShapeオブジェクトを構築する
    public override CDShapeIf CreateShape(GameObject shapeObject) {
        BoxCollider bc = shapeObject.GetComponent<BoxCollider>();
        if (bc == null) { throw new ObjectNotFoundException("CDBoxBehaviour requires BoxCollider", shapeObject); }

        Vector3 size = bc.size;
        Vector3 scale = shapeObject.transform.lossyScale;
        desc.boxsize = new Vec3f((float)(size.x * scale.x), (float)(size.y * scale.y), (float)(size.z * scale.z));

        return phSdk.CreateShape(CDBoxIf.GetIfInfoStatic(), (CDBoxDesc)desc);
    }
}
