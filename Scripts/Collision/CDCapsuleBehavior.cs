using UnityEngine;
using System.Collections;
using SprCs;
using System;
using SprUnity;

[DefaultExecutionOrder(3)]
public class CDCapsuleBehavior : CDShapeBehaviour {
    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // メンバ変数

    public CDCapsuleDescStruct desc = null;

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // このBehaviourに対応するSpringheadオブジェクト

    public CDCapsuleIf cdCapsule { get { return sprObject as CDCapsuleIf; } }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // SprBehaviourの派生クラスで実装するメソッド

    // -- DescStructオブジェクトを再構築する
    public override void ResetDescStruct() {
        desc = new CDCapsuleDescStruct();
    }

    // -- DescStructオブジェクトを取得する
    public override CsObject GetDescStruct() {
        return desc;
    }

    // -- DescオブジェクトをNewして返す
    public override CsObject CreateDesc() {
        return new CDCapsuleDesc();
    }

    // -- DescStructをDescに適用する
    public override void ApplyDesc(CsObject from, CsObject to) {
        (from as CDCapsuleDescStruct).ApplyTo(to as CDCapsuleDesc);
    }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // CDShapeBehaviourの派生クラスで実装するメソッド

    // -- 形状固有のShapePoseの取得。剛体からの相対位置姿勢による分は除く
    public override Posed ShapePose(GameObject shapeObject) {
        CapsuleCollider cc = shapeObject.GetComponent<CapsuleCollider>();
        if (cc == null) { throw new ObjectNotFoundException("CDCapsuleBehaviour requires CapsuleCollider", shapeObject); }
        // SpringheadとUnityでカプセルの向きが違うことに対する補正
        Quaterniond diffRot = Quaternion.identity.ToQuaterniond();
        // X-Axis
        if (cc.direction == 0) {
            diffRot = Quaterniond.Rot(-90 * Mathf.Deg2Rad, new Vec3d(0, 1, 0));
        }
        // Y-Axis
        if (cc.direction == 1) {
            diffRot = Quaterniond.Rot(90 * Mathf.Deg2Rad, new Vec3d(1, 0, 0));
        }
        // Z-Axis
        if (cc.direction == 2) {
            diffRot = Quaternion.identity.ToQuaterniond();
        }
        return new Posed(cc.center.ToVec3d(), diffRot);
    }

    // -- SpringheadのShapeオブジェクトを構築する
    public override CDShapeIf CreateShape(GameObject shapeObject) {
        CapsuleCollider cc = shapeObject.GetComponent<CapsuleCollider>();
        if (cc == null) { throw new ObjectNotFoundException("CDCapsuleBehaviour requires CapsuleCollider", shapeObject); }

        Vector3 scale = shapeObject.transform.lossyScale;
        Vector3 position = shapeObject.GetComponent<Transform>().position;
        desc.radius = cc.radius * (Mathf.Max(scale.x, scale.z));
        // lengthが0だと落ちる
        desc.length = Mathf.Max(0.001f,cc.height * scale.y - desc.radius * 2);

        return phSdk.CreateShape(CDCapsuleIf.GetIfInfoStatic(), (CDCapsuleDesc)desc);
    }

}
