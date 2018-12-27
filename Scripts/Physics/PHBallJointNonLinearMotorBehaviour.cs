using UnityEngine;
using System.Collections;
using SprCs;
using System;
using SprUnity;

[DefaultExecutionOrder(5)]
public class PHBallJointNonLinearMotorBehaviour : SprSceneObjBehaviour {
    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // メンバ変数

    //descから設定するものはほぼないが一応
    public PHBallJointNonLinearMotorDescStruct desc = null;

    [System.Serializable]
    public struct Springdamper {
        public int springMode;
        public double[] springParam;
        public int damperMode;
        public double[] damperParam;
        public double min;
        public double max;
        public Springdamper(int m) {
            springMode = m;
            springParam = new double[4];
            damperMode = m;
            damperParam = new double[4];
            min = -Math.PI;
            max = Math.PI;
        }
    }
    public Springdamper[] values = new Springdamper[3] { new Springdamper(0), new Springdamper(0), new Springdamper(0) };
    public bool enableLimit = false;
    public double limitSpring = 10000;
    public double limitDamper = 1000;

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // このBehaviourに対応するSpringheadオブジェクト

    public PHBallJointNonLinearMotorIf phJointMotor { get { return sprObject as PHBallJointNonLinearMotorIf; } }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // 派生クラスで実装するメソッド

    // -- DescStructオブジェクトを再構築する
    public override void ResetDescStruct() {
        desc = new PHBallJointNonLinearMotorDescStruct();
    }

    // -- DescStructオブジェクトを取得する
    public override CsObject GetDescStruct() {
        return desc;
    }

    // -- DescオブジェクトをNewして返す
    public override CsObject CreateDesc() {
        return new PHBallJointNonLinearMotorDesc();
    }

    // -- DescStructをDescに適用する
    public override void ApplyDesc(CsObject from, CsObject to) {
        (from as PHBallJointNonLinearMotorDescStruct).ApplyTo(to as PHBallJointNonLinearMotorDesc);
    }

    // -- Sprオブジェクトの構築を行う
    public override ObjectIf Build() {
        PHBallJointIf jo = gameObject.GetComponent<PHBallJointBehaviour>().sprObject as PHBallJointIf;
        if (jo == null) return null;
        PHBallJointNonLinearMotorDesc d = new PHBallJointNonLinearMotorDesc();
        PHBallJointNonLinearMotorIf motor = jo.CreateMotor(PHBallJointNonLinearMotorIf.GetIfInfoStatic(), d) as PHBallJointNonLinearMotorIf;
        if (motor == null) return null;
        arraywrapper_double s_double, d_double;
        for (int i = 0; i < 3; i++) {
            switch (values[i].springMode) {
                case 0:
                    s_double = new arraywrapper_double(2);
                    Copy(values[i].springParam, s_double, 2);
                    break;
                case 1:
                    s_double = new arraywrapper_double(4);
                    Copy(values[i].springParam, s_double, 4);
                    break;
                default:
                    values[i].springMode = 0;
                    s_double = new arraywrapper_double(4);
                    Copy(values[i].springParam, s_double, 4);
                    break;
            }
            switch (values[i].damperMode) {
                case 0:
                    d_double = new arraywrapper_double(2);
                    Copy(values[i].damperParam, d_double, 2);
                    break;
                case 1:
                    d_double = new arraywrapper_double(4);
                    Copy(values[i].damperParam, d_double, 4);
                    break;
                default:
                    values[i].damperMode = 0;
                    d_double = new arraywrapper_double(4);
                    Copy(values[i].damperParam, d_double, 4);
                    break;
            }
            motor.SetFuncFromDatabaseN(i, values[i].springMode, values[i].damperMode, s_double, d_double);
            print(i + " " + values[i].springMode + " " + values[i].damperMode + " " + s_double + " " + d_double);
        }
        return motor;
    }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // MonoBehaviourのメソッド

    // Update is called once per frame
    public void FixedUpdate() {
        PHBallJointIf ball = gameObject.GetComponent<PHBallJointBehaviour>().sprObject as PHBallJointIf;
        Vec3d delta = ToEuler(ball.GetPosition());
        Vec3d torque = ball.GetMotorForceN(1);
        for (int i = 0; i < 3; i++) {
            if (values[i].springMode == 1) {
                double torqueFromCalc = Math.Exp(values[i].springParam[0] * (delta[i] - values[i].springParam[1])) - Math.Exp(values[i].springParam[2] * (values[i].springParam[3] - delta[i]));
                print(gameObject.name + "ResistTorque[" + i + "] (fromMotor):" + torque[i] + " (fromCalc):" + torqueFromCalc);
            }
        }
    }

    // 可視化
    public void OnDrawGizmos() {
        if (sprObject != null) {
            float length = 0.01f;
            Vector3 top;
            Posed socketPose = new Posed();
            Posed plugPose = new Posed();

            //各種Ifの取得
            PHBallJointIf ball = gameObject.GetComponent<PHBallJointBehaviour>().sprObject as PHBallJointIf;

            Transform jointtrans = gameObject.transform;
            ball.GetSocketPose(socketPose);
            socketPose = ball.GetSocketSolid().GetPose() * socketPose;
            top = socketPose.Pos().ToVector3();
            Vec3d torque = ball.GetMotorForceN(1);
            if (torque.norm() > 100) {
                Quaternion q = ball.GetPosition().ToQuaternion();
                print(gameObject.name + "torque(1):" + torque + " pos:" + q.eulerAngles);
                Gizmos.color = Color.red;
                Gizmos.DrawLine(top, length * (socketPose * new Vec3d(torque.x, 0, 0)).ToVector3());
                Gizmos.color = Color.green;
                Gizmos.DrawLine(top, length * (socketPose * new Vec3d(0, torque.y, 0)).ToVector3());
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(top, length * (socketPose * new Vec3d(0, 0, torque.z)).ToVector3());
            }
        }
    }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // その他のメソッド

    void Copy(double[] p, arraywrapper_double param, int n) {
        int l = System.Math.Min(n, p.Length);
        for (int i = 0; i < l; i++) {
            param[i] = p[i];
        }
    }

    // <!!> Unity本体のToEulerやSpringheadのToEulerとはどう違うのか…？
    Vec3d ToEuler(Quaterniond q) {
        double poleCheck = q.X() * q.Y() + q.Z() * q.W();
        double heading;
        double attitude;
        double bank;
        if (poleCheck > 0.499) {               //	north pole
            heading = 2 * Math.Atan2(q.X(), q.W());
            attitude = 0;
            bank = 0;
        } else if (poleCheck < -0.499) {       //	south pole
            heading = -2 * Math.Atan2(q.X(), q.W());
            attitude = 0;
            bank = 0;
        } else {
            heading = Math.Atan2(2 * q.Y() * q.W() - 2 * q.X() * q.Z(), 1 - 2 * q.Y() * q.Y() - 2 * q.Z() * q.Z());
            attitude = Math.Asin(2 * q.X() * q.Y() + 2 * q.Z() * q.W());
            bank = Math.Atan2(2 * q.X() * q.W() - 2 * q.Y() * q.Z(), 1 - 2 * q.X() * q.X() - 2 * q.Z() * q.Z());
        }
        //返す値がy, z, xの順
        //return new Vec3d(heading, attitude, bank);
        //返す値がx, y, zの順
        return new Vec3d(bank, heading, attitude);
    }
}
