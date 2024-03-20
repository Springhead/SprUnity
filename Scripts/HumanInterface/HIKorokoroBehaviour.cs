using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SprCs;
using SprUnity;
using UnityEditor;

[DefaultExecutionOrder(10)]
public class HIKorokoroBehaviour : SprBehaviour
{
    static HISdkIf hiSdk = null;

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // メンバ変数

    public HIKorokoroDescStruct desc;

    public GameObject pointer = null;
    public TextMesh lengthText = null;

    private Transform pointerTransform;
    private Transform controllerTransform;

    public int nMotors = 3;

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // このBehaviourに対応するSpringheadオブジェクト

    public HIKorokoroIf hiKorokoro { get { return sprObject as HIKorokoroIf; } }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // SprBehaviourの派生クラスで実装するメソッド

    // -- DescStructオブジェクトを再構築する
    public override void ResetDescStruct()
    {
        // <!!> とりあえず作るけど実際には使わない。作らなくてもいいのなら作らないほうがいい
        desc = (HIKorokoroDescStruct)(new HIKorokoroDesc());
    }

    // -- DescStructオブジェクトを取得する
    public override CsObject GetDescStruct()
    {
        return desc;
    }

    // -- DescオブジェクトをNewして返す
    public override CsObject CreateDesc()
    {
        return new HIKorokoroDesc();
    }

    // -- DescStructをDescに適用する
    public override void ApplyDesc(CsObject from, CsObject to)
    {
        (from as HIKorokoroDescStruct).ApplyTo(to as HIKorokoroDesc);
    }

    // -- Sprオブジェクトの構築を行う
    public override ObjectIf Build()
    {
        if (hiSdk == null) { hiSdk = HISdkIf.CreateSdk(); }

        DRCyUsb20Sh4Desc cyDesc = new DRCyUsb20Sh4Desc();
        for (int i = 0; i < 10; ++i)
        {
            cyDesc.channel = i;
            hiSdk.AddRealDevice(DRCyUsb20Sh4If.GetIfInfoStatic(), cyDesc);
        }

        //

        DRUARTMotorDriverDesc umDesc = new DRUARTMotorDriverDesc();
       DRUARTMotorDriverIf uartMotorDriver = (DRUARTMotorDriverIf)hiSdk.AddRealDevice(DRUARTMotorDriverIf.GetIfInfoStatic(), umDesc);
        hiSdk.AddRealDevice(DRKeyMouseWin32If.GetIfInfoStatic());

        //

        var korokoro = hiSdk.CreateHumanInterface(HIKorokoroIf.GetIfInfoStatic());


        var d = new HIKorokoroDesc(); // do not use "desc"

        for (int i = 0; i < nMotors; i++)
        {
            var dM = new HIKorokoroMotorDesc();
            d.motors.push_back(dM);
        }
        
            korokoro.Init(d); 

        return korokoro;
    }

    


    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // MonoBehaviourのメソッド

    void FixedUpdate()
    {
       
        if (sprObject != null)
        {
            //pointerの位置をSetPose
            pointerTransform = GameObject.Find("Pointer").transform;
            Posed pointerPose = new Posed(pointerTransform.position.ToVec3d(), pointerTransform.rotation.ToQuaterniond());
  
            //if (hiKorokoro.IsGood())
            {
                hiKorokoro.SetPose(pointerPose);
            }


            if (lengthText != null)
            {
                string text = "";
                for (int i = 0; i < (int)hiKorokoro.NMotor(); i++)
                {
                    text += hiKorokoro.GetMotor((uint)i).ToString() + "\r\n";
                    
                }
                lengthText.text = text;
            }


            if (pointer != null)
            {
                Posed pose = hiKorokoro.GetPose();
                pointer.transform.position = pose.Pos().ToVector3();
                pointer.transform.rotation = pose.Ori().ToQuaternion();
            }
        }
    }

    
}
