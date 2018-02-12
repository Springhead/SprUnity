using UnityEngine;
using System.Collections;
using SprCs;
using System;

[DefaultExecutionOrder(3)]
public class CDConvexMeshBehaviour : CDShapeBehaviour {
    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // メンバ変数

    public CDConvexMeshDescStruct desc = null;

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // このBehaviourに対応するSpringheadオブジェクト

    public CDConvexMeshIf cdConvexMesh { get { return sprObject as CDConvexMeshIf; } }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // SprBehaviourの派生クラスで実装するメソッド

    // -- DescStructオブジェクトを再構築する
    public override void ResetDescStruct() {
        desc = new CDConvexMeshDescStruct();
    }

    // -- DescStructオブジェクトを取得する
    public override CsObject GetDescStruct() {
        return desc;
    }

    // -- DescオブジェクトをNewして返す
    public override CsObject CreateDesc() {
        return new CDConvexMeshDesc();
    }

    // -- DescStructをDescに適用する
    public override void ApplyDesc(CsObject from, CsObject to) {
        // Verticesには代入できないしすべきでないので、Materialだけを適用する
        (from as CDConvexMeshDescStruct).material.ApplyTo((to as CDConvexMeshDesc).material);
    }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // CDShapeBehaviourの派生クラスで実装するメソッド

    // -- SpringheadのShapeオブジェクトを構築する
    public override CDShapeIf CreateShape(GameObject shapeObject) {
		Mesh mesh = shapeObject.GetComponent<MeshFilter>().mesh;

        // Verticesを除いて、Materialだけを持ってくる
        CDConvexMeshDesc d = new CDConvexMeshDesc();
        ApplyDesc(desc, d);

        // Initialize CDConvexMeshDesc by Unity Mesh
        for (int vi = 0; vi < mesh.vertices.Length; vi++) {
            Vector3 vU = mesh.vertices[vi];
            Vec3f v = new Vec3f();
            v.x = vU.x;
            v.y = vU.y;
            v.z = vU.z;
            d.vertices.push_back(v);
        }

        return phSdk.CreateShape(CDConvexMeshIf.GetIfInfoStatic(), d);
	}
}

