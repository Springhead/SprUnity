using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SprCs;
using SprUnity;

[DefaultExecutionOrder(10)]
public class HISpidarGBehaviour : SprBehaviour {
    static HISdkIf hiSdk = null;

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // メンバ変数

    public HISpidarGDescStruct desc;

    public GameObject pointer = null;
    public TextMesh lengthText = null;

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // このBehaviourに対応するSpringheadオブジェクト

    public HISpidarGIf hiSpidar { get { return sprObject as HISpidarGIf; } }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // SprBehaviourの派生クラスで実装するメソッド

    // -- DescStructオブジェクトを再構築する
    public override void ResetDescStruct() {
        // <!!> とりあえず作るけど実際には使わない。作らなくてもいいのなら作らないほうがいい
        desc = (HISpidarGDescStruct)(new HISpidarGDesc("SpidarG6X3R"));
    }

    // -- DescStructオブジェクトを取得する
    public override CsObject GetDescStruct() {
        return desc;
    }

    // -- DescオブジェクトをNewして返す
    public override CsObject CreateDesc() {
        return new HISpidarGDesc("SpidarG6X3R");
    }

    // -- DescStructをDescに適用する
    public override void ApplyDesc(CsObject from, CsObject to) {
        (from as HISpidarGDescStruct).ApplyTo(to as HISpidarGDesc);
    }

    // -- Sprオブジェクトの構築を行う
    public override ObjectIf Build() {
        if (hiSdk == null) { hiSdk = HISdkIf.CreateSdk(); }

        DRCyUsb20Sh4Desc cyDesc = new DRCyUsb20Sh4Desc();
        for (int i = 0; i < 10; ++i) {
            cyDesc.channel = i;
            hiSdk.AddRealDevice(DRCyUsb20Sh4If.GetIfInfoStatic(), cyDesc);
        }

        var spg = hiSdk.CreateHumanInterface(HISpidarGIf.GetIfInfoStatic());

        var d = new HISpidarGDesc("SpidarG6X3R"); // do not use "desc"
        spg.Init(d);
        spg.Calibration();

        return spg;
    }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // MonoBehaviourのメソッド

    void FixedUpdate() {
        if (sprObject != null) {
            hiSpidar.Update(Time.fixedDeltaTime);

            if (lengthText != null) {
                string text = "";
                for (int i = 0; i < (int)hiSpidar.NMotor(); i++) {
                    text += hiSpidar.GetMotor((uint)i).GetLength().ToString() + "\r\n";
                }
                lengthText.text = text;
            }

            if (pointer != null) {
                Posed pose = hiSpidar.GetPose();
                pointer.transform.position = pose.Pos().ToVector3();
                pointer.transform.rotation = pose.Ori().ToQuaternion();
            }
        }
    }

}
