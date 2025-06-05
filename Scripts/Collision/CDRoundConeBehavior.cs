using UnityEngine;
using System.Collections;
using System.Linq;
using SprCs;
using System;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(CDRoundConeBehavior))]
[CanEditMultipleObjects]
public class CDRoundConeBehaviorEditor : Editor {
}

#endif
[DefaultExecutionOrder(3)]
public class CDRoundConeBehavior : CDShapeBehaviour {
    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // メンバ変数

    public CDRoundConeDescStruct desc = null;

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // このBehaviourに対応するSpringheadオブジェクト

    public CDRoundConeIf cdRoundCone { get { return sprObject as CDRoundConeIf; } }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // SprBehaviourの派生クラスで実装するメソッド

    // -- DescStructオブジェクトを再構築する
    public override void ResetDescStruct() {
        desc = new CDRoundConeDescStruct();
    }

    // -- DescStructオブジェクトを取得する
    public override CsObject GetDescStruct() {
        return desc;
    }

    // -- DescオブジェクトをNewして返す
    public override CsObject CreateDesc() {
        return new CDRoundConeDesc();
    }

    // -- DescStructをDescに適用する
    public override void ApplyDesc(CsObject from, CsObject to) {
        (from as CDRoundConeDescStruct).ApplyTo(to as CDRoundConeDesc);
    }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // CDShapeBehaviourの派生クラスで実装するメソッド

    // -- 形状固有のShapePoseの取得。剛体からの相対位置姿勢による分は除く
    public override Posed ShapePose(GameObject shapeObject) {
        MeshRoundCone mrc = shapeObject.GetComponent<MeshRoundCone>();
        if (mrc == null) { throw new ObjectNotFoundException("CDRoundConeBehaviour requires MeshRoundCone", shapeObject); }

        // SpringheadとUnityでカプセルの向きが違うことに対する補正
        Vec3f p = new Vec3f();
        if (mrc.pivot == MeshRoundCone.Pivot.R1) {
            p = new Vec3f(+0.5f * mrc.length, 0, 0);
        } else if (mrc.pivot == MeshRoundCone.Pivot.R2) {
            p = new Vec3f(-0.5f * mrc.length, 0, 0);
        }
        return new Posed(p, Quaterniond.Rot(90.0f * Mathf.Deg2Rad, new Vec3d(0, 1, 0)));
    }

    // -- SpringheadのShapeオブジェクトを構築する
    public override CDShapeIf CreateShape(GameObject shapeObject) {
        MeshRoundCone mrc = shapeObject.GetComponent<MeshRoundCone>();
        if (mrc == null) { throw new ObjectNotFoundException("CDRoundConeBehaviour requires MeshRoundCone", shapeObject); }

        Vector3 scale = shapeObject.transform.lossyScale;
        desc.radius = new Vec2f(mrc.r1, mrc.r2) * (Mathf.Max(scale.x, scale.z));
        desc.length = mrc.length * scale.y;

        return phSdk.CreateShape(CDRoundConeIf.GetIfInfoStatic(), (CDRoundConeDesc)desc);
    }

}
