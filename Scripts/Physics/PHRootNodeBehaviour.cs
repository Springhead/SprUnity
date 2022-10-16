using System.Collections;
using System;
using System.Linq;

using UnityEngine;

using SprCs;
using SprUnity;

public class PHRootNodeBehaviour : SprSceneObjBehaviour {
    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // メンバ変数

    public PHRootNodeDescStruct desc = null;

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // このBehaviourに対応するSpringheadオブジェクト

    public PHRootNodeIf phRootNode { get { return sprObject as PHRootNodeIf; } }

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
        return new PHRootNodeDesc();
    }

    // -- DescStructをDescに適用する
    public override void ApplyDesc(CsObject from, CsObject to) {
        (from as PHRootNodeDescStruct).ApplyTo(to as PHRootNodeDesc);
    }

    // -- Sprオブジェクトの構築を行う
    public override ObjectIf Build(){
        return null;
    }

    // -- 全てのBuildが完了した後に行う処理を書く。オブジェクト同士をリンクするなど
    public override void Link() {
        if (sprObject == null) {
            sprObject = phScene.CreateRootNode(this.GetComponent<PHSolidBehaviour>().phSolid);
            //Debug.Log("CreateRootNode[" + this.name + "]");
            phRootNode.Enable();
        }
    }

}
