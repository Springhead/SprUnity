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

    public bool enableRootNode = false;
        
    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // このBehaviourに対応するSpringheadオブジェクト

    public PHTreeNodeIf phTreeNode { get { return sprObject as PHTreeNodeIf; } }

    public PHRootNodeIf phRootNode = null;

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
            // 親関節がないかどんどん上に行って探す
            while (true) {
                PHJointBehaviour joParent = null;
                var jos = jo.socket.GetComponentsInChildren<PHJointBehaviour>(); //InChildrenで探す必要はない？
                foreach (var j in jos) {
                    if (j.plug == jo.socket) { joParent = j; break; }
                }

                if (joParent != null && jo != joParent) {
                    // 親関節のノードが存在していれば、そちらを先に構築する
                    PHTreeNodeBehaviour parentNode = joParent.GetComponent<PHTreeNodeBehaviour>();
                    if (parentNode != null && parentNode.isActiveAndEnabled) {
                        if (parentNode.sprObject == null) { parentNode.Link(); }
                    }

                    // その後、親TreeNodeに接続する形で自分のTreeNodeを作成する
                    if (sprObject == null) {
                        sprObject = phScene.CreateTreeNode(parentNode.phTreeNode, jo.plug.GetComponent<PHSolidBehaviour>().phSolid);
                        Debug.Log("CreateTreeNode[" + this.name + "] = Tree(" + parentNode.name + ") <= Solid(" + jo.plug.name + ")");
                    }

                    break;

                } else {
                    // 親関節がない場合はまずRootNodeを作る
                    if (phRootNode == null) {
                        phRootNode = phScene.CreateRootNode(jo.socket.GetComponent<PHSolidBehaviour>().phSolid);
                        Debug.Log("CreateRootNode[" + this.name + "] = " + jo.socket.name);
                    }

                    // その後、RootNodeに接続する形で自分のTreeNodeを作成する
                    if (sprObject == null) {
                        sprObject = phScene.CreateTreeNode(phRootNode, jo.plug.GetComponent<PHSolidBehaviour>().phSolid);
                        Debug.Log("CreateTreeNode[" + this.name + "] = Root(" + this.name + ") <= Solid(" + jo.plug.name + ")");
                    }

                    break;
                }
            }
        }
    }

    public override void OnValidate() {
        if (phRootNode != null) {
            phRootNode.Enable(enableRootNode);
            Debug.Log("RootNode " + (enableRootNode ? "enabled" : "disabled") + ".");
        }
    }

    /*
    bool initialized = false, first = true;
    void FixedUpdate() {
        if (initialized && first) {
            if (phRootNode != null) {
                phRootNode.Enable(false);
                // Debug.Log("RootNode enabled.");
            }
            first = false;
        }
    }
    */

}
