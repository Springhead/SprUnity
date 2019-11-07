using UnityEngine;
using System.Collections;
using SprCs;
using System;

[DefaultExecutionOrder(3)]
public class CDSphereBehaviour : CDShapeBehaviour {
    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // メンバ変数

    public CDSphereDescStruct desc = null;

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // このBehaviourに対応するSpringheadオブジェクト

    public CDSphereIf cdSphere { get { return sprObject as CDSphereIf; } }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // SprBehaviourの派生クラスで実装するメソッド

    // -- DescStructオブジェクトを再構築する
    public override void ResetDescStruct() {
        desc = new CDSphereDescStruct();
    }

    // -- DescStructオブジェクトを取得する
    public override CsObject GetDescStruct() {
        return desc;
    }

    // -- DescオブジェクトをNewして返す
    public override CsObject CreateDesc() {
        return new CDSphereDesc();
    }

    // -- DescStructをDescに適用する
    public override void ApplyDesc(CsObject from, CsObject to) {
        (from as CDSphereDescStruct).ApplyTo(to as CDSphereDesc);
    }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // CDShapeBehaviourの派生クラスで実装するメソッド

    // -- SpringheadのShapeオブジェクトを構築する
    public override CDShapeIf CreateShape(GameObject shapeObject) {
        SphereCollider sc = shapeObject.GetComponent<SphereCollider>();
        if (sc == null) { throw new ObjectNotFoundException("CDSphereBehaviour requires SphereCollider", shapeObject); }

        Vector3 scale = shapeObject.transform.lossyScale;
        desc.radius = sc.radius * (Mathf.Max(Mathf.Max(scale.x, scale.y), scale.z));
        
        return phSdk.CreateShape(CDSphereIf.GetIfInfoStatic(), (CDSphereDesc)desc);
    }

}
