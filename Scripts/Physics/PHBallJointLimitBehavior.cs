using UnityEngine;
using System.Collections;
using SprCs;
using SprUnity;
using System;

// CustomEditorは以下に定義
// SprUnity/Editor/Physics/PHBallJointLimitBehaviorEditor.cs

[DefaultExecutionOrder(5)]
public class PHBallJointLimitBehavior : SprSceneObjBehaviour {
    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // メンバ変数

    public PHBallJointConeLimitDescStruct desc = null;

    public GameObject jointObject = null;

    public Quaternion rot = new Quaternion(0, 0, 0, 1);

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // このBehaviourに対応するSpringheadオブジェクト

    public PHBallJointConeLimitIf phJointLimit { get { return sprObject as PHBallJointConeLimitIf; } }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // SprBehaviourの派生クラスで実装するメソッド

    // -- DescStructオブジェクトを再構築する
    public override void ResetDescStruct() {
        desc = new PHBallJointConeLimitDescStruct();
        desc.bEnabled = true; // <!!> なんでデフォルトでfalseなんだろう…？
    }

    // -- DescStructオブジェクトを取得する
    public override CsObject GetDescStruct() {
        return desc;
    }

    // -- DescオブジェクトをNewして返す
    public override CsObject CreateDesc() {
        return new PHBallJointConeLimitDesc();
    }

    // -- DescStructをDescに適用する
    public override void ApplyDesc(CsObject from, CsObject to) {
        (from as PHBallJointConeLimitDescStruct).ApplyTo(to as PHBallJointConeLimitDesc);
    }

    // -- Sprオブジェクトの構築を行う
    public override ObjectIf Build() {
        PHBallJointIf jo = null;

        var b = (jointObject ? jointObject : gameObject).GetComponent<PHBallJointBehaviour>();
        if (!b) { return null; }

        jo = b.sprObject as PHBallJointIf;
        if (jo == null) { return null; }

        PHBallJointLimitIf lim = jo.CreateLimit(PHBallJointConeLimitIf.GetIfInfoStatic(), (PHBallJointConeLimitDesc)desc);

        return lim;
    }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // MonoBehaviourのメソッド

    // UnityのOnValidate : SprBehaviourのものをオーバーライド
    public override void OnValidate() {
        if (GetDescStruct() == null) {
            ResetDescStruct();
        }

        // LimitにSetDescしても効果がなかったので直接セット
        // <!!> SetDescが使えるようにすべき
        if (sprObject != null) {
            phJointLimit.Enable(desc.bEnabled);
            phJointLimit.SetDamper(desc.damper);
            phJointLimit.SetLimitDir(desc.limitDir);
            phJointLimit.SetSpring(desc.spring);
            phJointLimit.SetSwingRange(desc.limitSwing);
            phJointLimit.SetSwingDirRange(desc.limitSwingDir);
            phJointLimit.SetTwistRange(desc.limitTwist);
        }
    }

    // 可視化
    public void OnDrawGizmos() {/*
        //可動範囲(SwingDirとTwistは無視)の表示
        if (sprObject != null) {
            float length = 0.1f;
            int n = 30;
            Vector3 top;
            Posed socketPose = new Posed();
            Posed plugPose = new Posed();

            //各種Ifの取得
            PHBallJointIf ball = (jointObject ? jointObject : gameObject).GetComponent<PHBallJointBehaviour>().sprObject as PHBallJointIf;
            PHBallJointConeLimitIf limit = sprObject as PHBallJointConeLimitIf;

            //Limit情報の取得
            Vec2d swing = new Vec2d(); limit.GetSwingRange(swing);
            Vec3d limitDir = limit.GetLimitDir();

            //可動範囲円の計算表示
            Transform jointtrans = (jointObject ? jointObject : gameObject).transform;
            ball.GetSocketPose(socketPose);
            socketPose = ball.GetSocketSolid().GetPose() * socketPose;
            top = socketPose.Pos().ToVector3();
            Vec3d bottom = socketPose * (length * limit.GetLimitDir());
            Quaterniond Jztol = Quaternion.FromToRotation(new Vector3(0, 0, 1), limit.GetLimitDir().ToVector3()).ToQuaterniond();
            Vec3d swingDirBase1 = (length * (limitDir * Mathf.Cos((float)swing[1]) + Jztol * new Vec3d(1, 0, 0) * Mathf.Sin((float)swing[1])));
            Vec3d swingDirBase2 = (length * (limitDir * Mathf.Cos((float)swing[0]) + Jztol * new Vec3d(1, 0, 0) * Mathf.Sin((float)swing[0])));
            if (limit.IsOnLimit()) {
                Gizmos.color = Color.red;
            } else {
                Gizmos.color = Color.white;
            }
            Quaterniond rot = Quaterniond.Rot(2 * Mathf.PI / n, limitDir);
            for (int i = 0; i < n; i++) {
                Gizmos.DrawLine((socketPose * swingDirBase1).ToVector3(), (socketPose * (rot * swingDirBase1)).ToVector3());
                Gizmos.DrawLine((socketPose * swingDirBase2).ToVector3(), (socketPose * (rot * swingDirBase2)).ToVector3());
                Gizmos.DrawLine((socketPose * swingDirBase1).ToVector3(), (socketPose * swingDirBase2).ToVector3());
                swingDirBase1 = rot * swingDirBase1;
                swingDirBase2 = rot * swingDirBase2;
            }

            //Swing軸の表示
            ball.GetPlugPose(plugPose);
            plugPose = ball.GetPlugSolid().GetPose() * plugPose;
            Vec3d axis = plugPose * (length * new Vec3d(0, 0, 1));
            Gizmos.DrawLine(top, axis.ToVector3());
            //Gizmos.DrawLine(top, bottom);
        }*/
    }
}
