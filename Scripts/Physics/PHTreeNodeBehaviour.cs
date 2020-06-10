using System.Collections;
using System;
using System.Linq;

using UnityEngine;

using SprCs;
using SprUnity;

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
    public override void Link() {
        // まず、このTreeNodeに付随する関節を探す。同じGameObjectに付いているはず
        PHJointBehaviour jo = gameObject.GetComponent<PHJointBehaviour>();

        if (jo != null && jo.sprObject != null) {
            // 次に、関節の親関節を探す。関節のソケット剛体を探し、それを基準に探す
            // （親関節　＝　この関節のソケット剛体をプラグ剛体として持つ関節）
            PHJointBehaviour joParent = null;
            var jos = jo.socket.GetComponentsInChildren<PHJointBehaviour>();
            foreach (var j in jos) {
                if (j.plug == jo.socket) { joParent = j; break; }
            }

            PHTreeNodeBehaviour parentTreeNode = null;
            PHRootNodeBehaviour parentRootNode = null;

            if (joParent != null && jo != joParent) {
                // -- 親関節があった場合
                parentTreeNode = joParent.GetComponent<PHTreeNodeBehaviour>();
                parentRootNode = joParent.GetComponent<PHRootNodeBehaviour>();

                if (parentTreeNode != null && parentTreeNode.isActiveAndEnabled) {
                    // 親ノードを先に構築する
                    if (parentTreeNode.sprObject == null) { parentTreeNode.Link(); }

                    // その後、親TreeNodeに接続する形で自分のTreeNodeを作成する
                    if (sprObject == null) {
                        sprObject = phScene.CreateTreeNode(parentTreeNode.phTreeNode,
                            jo.plug.GetComponent<PHSolidBehaviour>().phSolid);
                        //Debug.Log("CreateTreeNode[" + this.name + "] = Tree(" + parentTreeNode.name + ") <= Solid(" + jo.plug.name + ")");

                        phTreeNode.Enable();
                    }

                } else if (parentRootNode != null && parentRootNode.isActiveAndEnabled) {
                    // 親ノードを先に構築する
                    if (parentRootNode.sprObject == null) { parentRootNode.Link(); }

                    // その後、親RootNodeに接続する形で自分のTreeNodeを作成する
                    if (sprObject == null) {
                        sprObject = phScene.CreateTreeNode(parentRootNode.phRootNode,
                            jo.plug.GetComponent<PHSolidBehaviour>().phSolid);
                        //Debug.Log("CreateTreeNode[" + this.name + "] = Root(" + parentRootNode.name + ") <= Solid(" + jo.plug.name + ")");

                        phTreeNode.Enable();
                    }

                } else {
                    Debug.LogError("No Parent Node for " + this.name);
                }

            } else {
                // -- 親関節がない場合：この関節のsocketにRootNodeがくっついているはず
                parentRootNode = jo.socket.GetComponent<PHRootNodeBehaviour>();

                // 親ノードを先に構築する
                if (parentRootNode.sprObject == null) { parentRootNode.Link(); }

                // その後、親RootNodeに接続する形で自分のTreeNodeを作成する
                if (sprObject == null) {
                    sprObject = phScene.CreateTreeNode(parentRootNode.phRootNode,
                        jo.plug.GetComponent<PHSolidBehaviour>().phSolid);
                    Debug.Log("CreateTreeNode[" + this.name + "] = Root(" + parentRootNode.name + ") <= Solid(" + jo.plug.name + ")");

                    phTreeNode.Enable();
                }

            }
        }
    }

}
