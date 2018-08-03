using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SprCs;
using SprUnity;

[DefaultExecutionOrder(21)]
public class CRBoneBehaviour : SprSceneObjBehaviour {
    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // メンバ変数

    public CRBoneDescStruct desc;

    // 所属先のCRBody
    public CRBodyBehaviour crBodyBehaviour = null;

    // CRBone同士の親子関係（インスペクタ等で明示的に設定する必要あり）
    public CRBoneBehaviour parent = null;
    public List<CRBoneBehaviour> children = new List<CRBoneBehaviour>();

    // CRBoneに所属する剛体・関節等
    public PHSolidBehaviour solid = null;
    // public CDShapeBehaviour shape = null; // <!!> 要るの？
    public PHJointBehaviour joint = null;
    public PHIKEndEffectorBehaviour ikEndEffector = null;
    public PHIKActuatorBehaviour ikActuator = null;

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // このBehaviourに対応するSpringheadオブジェクト

    public CRBoneIf crBone { get { return sprObject as CRBoneIf; } }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // SprBehaviourの派生クラスで実装するメソッド

    // -- DescStructオブジェクトを再構築する
    public override void ResetDescStruct() {
        desc = new CRBoneDescStruct();
    }

    // -- DescStructオブジェクトを取得する
    public override CsObject GetDescStruct() {
        return desc;
    }

    // -- DescオブジェクトをNewして返す
    public override CsObject CreateDesc() {
        return new CRBoneDesc();
    }

    // -- DescStructをDescに適用する
    public override void ApplyDesc(CsObject from, CsObject to) {
        (from as CRBoneDescStruct).ApplyTo(to as CRBoneDesc);
    }

    // -- Sprオブジェクトの構築を行う
    public override ObjectIf Build() {
        var crBone = crBodyBehaviour.crBody.CreateObject(CRBoneIf.GetIfInfoStatic(), desc);
        return crBone;
    }

    // -- 全てのBuildが完了した後に行う処理を書く。オブジェクト同士をリンクするなど。
    public override void Link() {
        // 構成要素のセット
        if (solid != null) { crBone.SetPHSolid(solid.phSolid); }
        if (joint != null) { crBone.SetPHJoint(joint.phJoint); }
        if (ikEndEffector != null) { crBone.SetIKEndEffector(ikEndEffector.phIKEndEffector); }
        if (ikActuator != null) { crBone.SetIKActuator(ikActuator.phIKActuator); }

        // 親子関係のセット
        crBone.SetParentBone(parent.crBone);
        foreach (var child in children) {
            crBone.AddChildBone(child.crBone);
        }
    }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // MonoBehaviourのメソッド

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // その他のメソッド

}
