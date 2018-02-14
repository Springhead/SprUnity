using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Reflection;
using SprCs;
using SprUnity;

[DefaultExecutionOrder(2)]
public class PHSolidBehaviour : SprSceneObjBehaviour {
    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // メンバ変数

    public PHSolidDescStruct desc;

    // このGameObjectがScene Hierarchyでどれくらいの深さにあるか。浅いものから順にUpdatePoseするために使う
    [HideInInspector]
    public int treeDepth = 0;

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // このBehaviourに対応するSpringheadオブジェクト

    public PHSolidIf phSolid { get { return sprObject as PHSolidIf; } }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // SprBehaviourの派生クラスで実装するメソッド

    // -- DescStructオブジェクトを再構築する
    public override void ResetDescStruct() {
        desc = new PHSolidDescStruct();
    }

    // -- DescStructオブジェクトを取得する
    public override CsObject GetDescStruct() {
        return desc;
    }

    // -- DescオブジェクトをNewして返す
    public override CsObject CreateDesc() {
        return new PHSolidDesc();
    }

    // -- DescStructをDescに適用する
    public override void ApplyDesc(CsObject from, CsObject to) {
        (from as PHSolidDescStruct).ApplyTo(to as PHSolidDesc);
    }

    // -- Sprオブジェクトの構築を行う
    public override ObjectIf Build() {
		PHSolidIf so = phScene.CreateSolid (desc);
        so.SetName("so:" + gameObject.name);
		so.SetPose (gameObject.transform.ToPosed());

        // Scene Hierarchyでの深さを取得した上でPHSceneBehaviourに登録
        var t = transform;
        while (t.parent != null) { treeDepth++; t = t.parent; }
        phSceneBehaviour.RegisterPHSolidBehaviour(this);

        return so;
    }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // その他のメソッド

    // Springhead剛体とGameObjectの間での位置姿勢の同期：　更新順を制御するためPHSceneからまとめて呼び出す
    public void UpdatePose () {
        if (sprObject != null) {
            PHSolidIf so = sprObject as PHSolidIf;
            if (so.IsDynamical()) {
                // Dynamicalな剛体はSpringheadのシミュレーション結果をUnityに反映
                gameObject.transform.FromPosed(so.GetPose());
            } else {
                // Dynamicalでない剛体はUnityの位置をSpringheadに反映（操作可能）
                so.SetPose(gameObject.transform.ToPosed());
            }
        }
	}

}
