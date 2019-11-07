using UnityEngine;
using System.Collections.Generic;
using SprCs;
using System.Runtime.InteropServices;
using System;

[DefaultExecutionOrder(1)]
public class FWSdkBehaviour : SprBehaviour {
    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // メンバ変数

    public FWSdkDescStruct desc;

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // このBehaviourに対応するSpringheadオブジェクト

    public FWSdkIf fwSdk { get { return sprObject as FWSdkIf; } }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // 派生クラスで実装するメソッド

    // -- DescStructオブジェクトを再構築する
    public override void ResetDescStruct() {
        desc = new FWSdkDescStruct();
    }

    // -- DescStructオブジェクトを取得する
    public override CsObject GetDescStruct() {
        return desc;
    }

    // -- DescオブジェクトをNewして返す
    public override CsObject CreateDesc() {
        return new FWSdkDesc();
    }

    // -- DescStructをDescに適用する
    public override void ApplyDesc(CsObject from, CsObject to) {
        (from as FWSdkDescStruct).ApplyTo(to as FWSdkDesc);
    }

    // -- Sprオブジェクトの構築を行う
    public override ObjectIf Build() {
        FWSdkIf sdk = FWSdkIf.CreateSdk();
        return sdk;
    }

}