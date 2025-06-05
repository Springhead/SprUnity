using System.Collections;
using System;
using System.Linq;

using UnityEngine;

using SprCs;
using SprUnity;

// 対応するPHJointBehaviourの派生クラスがアタッチされているGameObjectにアタッチ
public class PHTreeNodeBehaviour : SprSceneObjBehaviour {
    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // メンバ変数

    public PHTreeNodeDescStruct desc = null;

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // このBehaviourに対応するSpringheadオブジェクト

    public PHTreeNodeIf phTreeNode { get { return sprObject as PHTreeNodeIf; } }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // SprBehaviourの派生クラスで実装するメソッド

    // -- DescStructオブジェクトを再構築する
    public override void ResetDescStruct() {
    }

    // -- DescStructオブジェクトを取得する
    public override CsObject GetDescStruct() {
        return desc;
    }

    // -- DescオブジェクトをNewして返す
    public override CsObject CreateDesc() {
        return new PHTreeNodeDesc();
    }

    // -- DescStructをDescに適用する
    public override void ApplyDesc(CsObject from, CsObject to) {
        (from as PHTreeNodeDescStruct).ApplyTo(to as PHTreeNodeDesc);
    }

    // -- Sprオブジェクトの構築を行う
    public override ObjectIf Build(){
        return null;
    }

    // -- 全てのBuildが完了した後に行う処理を書く。オブジェクト同士をリンクするなど
    // PHRootNodeBehaviourで作成される
    public override void Link() {
    }

    // sprObjectのSetter、PHTreeNodeIfはPHRootNodeBehaviourで作成される
    public void SetTreeNode(PHTreeNodeIf treeNode) {
        sprObject = treeNode;
    }
}
