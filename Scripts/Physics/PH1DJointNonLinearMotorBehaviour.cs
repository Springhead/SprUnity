using UnityEngine;
using System.Collections;
using SprCs;
using System;

[DefaultExecutionOrder(5)]
public class PH1DJointNonLinearMotorBehaviour : SprSceneObjBehaviour {
    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // メンバ変数

    //descから設定するものはほぼないが一応
    public PH1DJointNonLinearMotorDescStruct desc = null;

    public int springMode;
    public double[] springParam = new double[2] { 0, 0 };
    public int damperMode;
    public double[] damperParam = new double[2] { 0, 0 };
    public double min = -Math.PI;
    public double max = Math.PI;
    public bool enableLimit;

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // このBehaviourに対応するSpringheadオブジェクト

    public PH1DJointNonLinearMotorIf phJointMotor { get { return sprObject as PH1DJointNonLinearMotorIf; } }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // 派生クラスで実装するメソッド

    // -- DescStructオブジェクトを再構築する
    public override void ResetDescStruct() {
        desc = new PH1DJointNonLinearMotorDescStruct();
    }

    // -- DescStructオブジェクトを取得する
    public override CsObject GetDescStruct() {
        return desc;
    }

    // -- DescオブジェクトをNewして返す
    public override CsObject CreateDesc() {
        return new PH1DJointNonLinearMotorDesc();
    }

    // -- DescStructをDescに適用する
    public override void ApplyDesc(CsObject from, CsObject to) {
        (from as PH1DJointNonLinearMotorDescStruct).ApplyTo(to as PH1DJointNonLinearMotorDesc);
    }

    // -- Sprオブジェクトの構築を行う
    public override ObjectIf Build() {
        PHHingeJointIf jo = gameObject.GetComponent<PHHingeJointBehaviour>().sprObject as PHHingeJointIf;
        if (jo == null) { Debug.Log("No Joint"); return null; }

        PH1DJointNonLinearMotorDesc d = (PH1DJointNonLinearMotorDesc)desc;
        PH1DJointNonLinearMotorIf m = jo.CreateMotor(PH1DJointNonLinearMotorIf.GetIfInfoStatic(), d) as PH1DJointNonLinearMotorIf;
        if (m == null) { Debug.Log("Motor Null"); return null; }

        arraywrapper_double s_double, d_double;
        switch (springMode) {
            case 0:
                s_double = new arraywrapper_double(2);
                Copy(springParam, s_double, 2);
                break;
            case 1:
                s_double = new arraywrapper_double(4);
                Copy(springParam, s_double, 4);
                break;
            default:
                springMode = 0;
                s_double = new arraywrapper_double(4);
                Copy(springParam, s_double, 4);
                break;
        }
        switch (damperMode) {
            case 0:
                d_double = new arraywrapper_double(2);
                Copy(damperParam, d_double, 2);
                break;
            case 1:
                d_double = new arraywrapper_double(4);
                Copy(damperParam, d_double, 4);
                break;
            default:
                damperMode = 0;
                d_double = new arraywrapper_double(4);
                Copy(damperParam, d_double, 4);
                break;
        }
        // m.SetFuncFromDatabase(springMode, damperMode, s_double, d_double);

        return m;
    }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // その他のメソッド

    void Copy(double[] p, arraywrapper_double param, int n) {
        int l = System.Math.Min(n, p.Length);
        for (int i = 0; i < l; i++) {
            param[i] = p[i];
        }
    }
}
