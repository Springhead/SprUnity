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
    public override ObjectIf Build() {
        return null;
    }

    // -- 全てのBuildが完了した後に行う処理を書く。オブジェクト同士をリンクするなど
    public override void Link() {
        if (sprObject == null) {
            var solid = this.GetComponent<PHSolidBehaviour>().phSolid;
            sprObject = phScene.CreateRootNode(solid);
            //Debug.Log("CreateRootNode[" + this.name + "]");
            CreateTreeNodesRecurs(phRootNode, solid);
            phRootNode.Enable();
        }
    }

    // RootNodeからJointのPlugを辿り、全てのTreeNodeを再帰的に作成
    private void CreateTreeNodesRecurs(PHTreeNodeIf node, PHSolidIf solid) {
        var allTreeNodes = Resources.FindObjectsOfTypeAll<PHTreeNodeBehaviour>().Where(n => n.gameObject.activeInHierarchy);
        for (int i = 0; i < phScene.NJoints(); i++) {
            var joint = phScene.GetJoint(i);
            // PHJointBehaviourがアタッチされたGameObjectにPHTreeNodeBehaviourがアタッチされているかを確認
            var treeNodeBehaviours = allTreeNodes.Where(n => n.gameObject.activeInHierarchy && n.GetComponent<PHJointBehaviour>()?.phJoint == joint);
            if (treeNodeBehaviours.Count() == 0) continue;
            else if (treeNodeBehaviours.Count() > 1) {
                Debug.LogError("一つのJointに対して複数のPHTreeNodeがアタッチされています");
            }
            var treeNodeBehaviour = treeNodeBehaviours.First();
            var socket = joint.GetSocketSolid();
            var plug = joint.GetPlugSolid();
            if (socket == solid && plug.GetTreeNode() == null) {
                PHTreeNodeIf childNode = phScene.CreateTreeNode(node, plug);
                if (childNode != null) { // JointがTreeNodeをサポートしていない場合がある
                    //Debug.Log("CreateTreeNode[" + this.name + "] = Tree(" + node.GetName() + ") <= Solid(" + plug.GetName() + ")");
                    treeNodeBehaviour.SetTreeNode(childNode);
                    CreateTreeNodesRecurs(childNode, plug);
                }
            }
        }
    }
}
