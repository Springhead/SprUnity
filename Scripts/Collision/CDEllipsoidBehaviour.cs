using UnityEngine;
using System.Collections;
using System.Linq;
using SprCs;
using SprUnity;
using System;

[DefaultExecutionOrder(3)]
public class CDEllipsoidBehaviour : CDShapeBehaviour {
    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // メンバ変数

    public CDEllipsoidDescStruct desc = null;

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // このBehaviourに対応するSpringheadオブジェクト

    public CDEllipsoidIf cdEllipsoid { get { return sprObject as CDEllipsoidIf; } }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // SprBehaviourの派生クラスで実装するメソッド

    // -- DescStructオブジェクトを再構築する
    public override void ResetDescStruct() {
        desc = new CDEllipsoidDescStruct();
    }

    // -- DescStructオブジェクトを取得する
    public override CsObject GetDescStruct() {
        return desc;
    }

    // -- DescオブジェクトをNewして返す
    public override CsObject CreateDesc() {
        return new CDEllipsoidDesc();
    }

    // -- DescStructをDescに適用する
    public override void ApplyDesc(CsObject from, CsObject to) {
        (from as CDEllipsoidDescStruct).ApplyTo(to as CDEllipsoidDesc);
    }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // CDShapeBehaviourの派生クラスで実装するメソッド

    // -- SpringheadのShapeオブジェクトを構築する
    public override CDShapeIf CreateShape(GameObject shapeObject) {
        SphereCollider sc = shapeObject.GetComponent<SphereCollider>();
        if (sc == null) { throw new ObjectNotFoundException("CDEllipsoidBehaviour requires SphereCollider", shapeObject); }

        Vector3 scale = shapeObject.transform.lossyScale;
        desc.radius = sc.radius * scale.ToVec3d();

        return phSdk.CreateShape(CDEllipsoidIf.GetIfInfoStatic(), (CDEllipsoidDesc)desc);
    }

}
