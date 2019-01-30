using UnityEditor;
using UnityEngine;
using System.Collections;
using SprUnity;
using SprCs;

[CustomEditor(typeof(PHBallJointLimitBehavior))]
public class PHBallJointLimitBehaviorEditor : Editor {
    // Swing軸長
    float length = 0.1f;
    // Twist軸長
    float lengthTwist = 0.12f;
    // Disc rudius
    float discRadius = 0.03f;
    // 
    int numDivision = 30;

    // JointのautoSetSockPlugPoseがonだという前提
    void OnSceneGUI() {
        PHBallJointLimitBehavior limit = (PHBallJointLimitBehavior)target;

        GameObject jointObject = limit.jointObject ? limit.jointObject : limit.gameObject;
        PHBallJointBehaviour phBallJointBehaviour = jointObject.GetComponent<PHBallJointBehaviour>();

        if (phBallJointBehaviour) {
            GameObject jointPositionObject = phBallJointBehaviour.jointPosition ? phBallJointBehaviour.jointPosition : phBallJointBehaviour.gameObject;
            Transform jointTransform = jointPositionObject.transform;
            Posed plugPose = phBallJointBehaviour.plugPose;
            Posed socketPose = phBallJointBehaviour.socketPose;

            /*
            Vector3 plugPosition;
            Vector3 socketPosition;
            Quaternion plugRot;
            Quaternion socketRot;
            // 
            if (limit.sprObject == null) {
                if (phBallJointBehaviour.autoSetSockPlugPose) {
                    plugPosition = socketPosition = jointPosition;
                    plugRot = socketRot = jointPositionObject.transform.rotation;
                    plugPose = new Posed(plugPosition.ToVec3d(), plugRot.ToQuaterniond());
                    socketPose = new Posed(socketPosition.ToVec3d(), socketRot.ToQuaterniond());
                } else {
                    if (!phBallJointBehaviour.socket) { return; }
                    if (!phBallJointBehaviour.plug) { return; }
                    plugPose = phBallJointBehaviour.plug.transform.ToPosed() * phBallJointBehaviour.desc.posePlug;
                    socketPose = phBallJointBehaviour.socket.transform.ToPosed() * phBallJointBehaviour.desc.poseSocket;
                    plugPosition = plugPose.Pos().ToVector3();
                    plugRot = plugPose.Ori().ToQuaternion();
                    socketPosition = socketPose.Pos().ToVector3();
                    socketRot = socketPose.Ori().ToQuaternion();
                }
            } else {
                plugRot = jointPositionObject.transform.rotation;
                phBallJointBehaviour.phBallJoint.GetSocketPose(socketPose);
                socketPose = phBallJointBehaviour.socket.GetComponent<PHSolidBehaviour>().phSolid.GetPose() * socketPose;
                plugPose = phBallJointBehaviour.plug.GetComponent<PHSolidBehaviour>().phSolid.GetPose() * plugPose;
                plugPosition = plugPose.Pos().ToVector3();
                plugRot = plugPose.Ori().ToQuaternion();
                socketPosition = socketPose.Pos().ToVector3();
                socketRot = socketPose.Ori().ToQuaternion();
            }
            */
            Vector3 jointPosition = socketPose.Pos().ToVector3();
            Vector3 twistAxis = plugPose.Ori().ToQuaternion() * new Vector3(0, 0, 1);

            //Limit情報の取得
            //Vec2d swing = new Vec2d(); limit.phJointLimit.GetSwingRange(swing);
            Vec2d swing = limit.desc.limitSwing;
            //Vec2d twist = new Vec2d(); limit.phJointLimit.GetTwistRange(twist);
            Vec2d twist = limit.desc.limitTwist;
            Vec3d limitDir = limit.desc.limitDir;

            Color baseColor;

            if(limit.desc.bEnabled && limit.enabled) {
                if(limit.phJointLimit == null) {
                    baseColor = Color.green;
                } else {
                    if (limit.phJointLimit.IsOnLimit()) {
                        baseColor = Color.red;
                    } else {
                        baseColor = Color.green;
                    }
                }
            } else {
                baseColor = Color.white;
            }

            // 編集開始
            EditorGUI.BeginChangeCheck();

            // Swing
            // 可視化
            Vec3d swingBaseAxis = socketPose * (length * limitDir);
            Quaterniond Jztol = Quaternion.FromToRotation(new Vector3(0, 0, 1), limitDir.ToVector3()).ToQuaterniond();
            Vec3d swingDirBase1 = (length * (limitDir * Mathf.Cos((float)swing[0]) + Jztol * new Vec3d(1, 0, 0) * Mathf.Sin((float)swing[0])));
            Vec3d swingDirBase2 = (length * (limitDir * Mathf.Cos((float)swing[1]) + Jztol * new Vec3d(1, 0, 0) * Mathf.Sin((float)swing[1])));
            if (swing[0] >= swing[1]) {
                Handles.color = Color.white;
            } else {
                Handles.color = baseColor;
            }
            Quaterniond rot = Quaterniond.Rot(2 * Mathf.PI / numDivision, limitDir);
            for (int i = 0; i < numDivision; i++) {
                Handles.DrawLine((socketPose * swingDirBase1).ToVector3(), jointPosition);
                Handles.DrawLine((socketPose * swingDirBase2).ToVector3(), jointPosition);
                Handles.DrawLine((socketPose * swingDirBase1).ToVector3(), (socketPose * swingDirBase2).ToVector3());
                swingDirBase1 = rot * swingDirBase1;
                swingDirBase2 = rot * swingDirBase2;
            }
            Handles.DrawLine(jointPosition, swingBaseAxis.ToVector3());
            // ハンドル
            Vector3 SwingXHandlePos = (socketPose * (length * Mathf.Cos((float)swing[0]) * limitDir)).ToVector3();
            Vector3 SwingX = Handles.Slider(SwingXHandlePos, (socketPose * limitDir).ToVector3() - SwingXHandlePos, 0.01f, Handles.ArrowHandleCap, 0f);
            Vector3 SwingYHandlePos = (socketPose * (length * Mathf.Cos((float)swing[1]) * limitDir)).ToVector3();
            Vector3 SwingY = Handles.Slider(SwingYHandlePos, (socketPose * limitDir).ToVector3() - SwingYHandlePos, 0.01f, Handles.ArrowHandleCap, 0f);

            // Twist
            // とりあえずplugのx軸を基準とする
            // 可視化
            Vec3d currentTwistAxis = plugPose * new Vec3d(0, 0, lengthTwist);
            Handles.DrawLine(plugPose.Pos().ToVector3(), currentTwistAxis.ToVector3());
            // ハンドル
            Vector3 discCenter = (plugPose * new Vec3d(0, 0, lengthTwist)).ToVector3();
            Vector3 discNormal = ((plugPose * new Vec3d(0, 0, lengthTwist)) - plugPose.Pos()).ToVector3();
            Vector3 currentTwist = ((plugPose * new Vec3d(discRadius + 0.01f, 0, lengthTwist)).ToVector3());
            Vector3 twistXHandlePos = (plugPose * (new Vec3d(0.03 * Mathf.Cos((float)twist[0]), 0.03 * Mathf.Sin((float)twist[0]), lengthTwist))).ToVector3();
            Vector3 twistXHandleDir = (plugPose * (new Vec3d(0.03 * -Mathf.Sin((float)twist[0]), 0.03 * Mathf.Cos((float)twist[0]), 0)) - plugPose.Pos()).ToVector3();
            Vector3 twistYHandlePos = (plugPose * (new Vec3d(0.03 * Mathf.Cos((float)twist[1]), 0.03 * Mathf.Sin((float)twist[1]), lengthTwist))).ToVector3();
            Vector3 twistYHandleDir = (plugPose * (new Vec3d(0.03 * -Mathf.Sin((float)twist[1]), 0.03 * Mathf.Cos((float)twist[1]), 0)) - plugPose.Pos()).ToVector3();
            Handles.DrawWireDisc(discCenter, discNormal, discRadius);
            Handles.DrawSolidArc(discCenter, discNormal, twistXHandlePos - discCenter, (float)(twist[1] - twist[0]) * Mathf.Rad2Deg, discRadius);
            Vector3 twistX = Handles.Slider2D(twistXHandlePos, twistXHandleDir, twistXHandlePos - discCenter, twistXHandleDir, 0.01f, Handles.ArrowHandleCap, 0f);
            Vector3 twistY = Handles.Slider2D(twistYHandlePos, twistYHandleDir, twistYHandlePos - discCenter, twistYHandleDir, 0.01f, Handles.ArrowHandleCap, 0f);
            Handles.color = Color.yellow;
            Handles.DrawLine(discCenter, currentTwist);
            if (twist[0] >= twist[1]) {
                Handles.color = Color.white;
            } else {
                Handles.color = baseColor;
            }

            if (EditorGUI.EndChangeCheck()) {
                // Swing
                limit.desc.limitSwing = new Vec2d(Mathf.Acos((Vector3.Dot((SwingX - socketPose.Pos().ToVector3()), (swingBaseAxis.ToVector3() - socketPose.Pos().ToVector3())) > 0 ? 1 : -1) * Mathf.Max(Mathf.Min((SwingX - socketPose.Pos().ToVector3()).magnitude / length, 1), -1)),
                    Mathf.Acos((Vector3.Dot((SwingY - socketPose.Pos().ToVector3()), (swingBaseAxis.ToVector3() - socketPose.Pos().ToVector3())) > 0 ? 1 : -1) * Mathf.Max(Mathf.Min((SwingY - socketPose.Pos().ToVector3()).magnitude / length, 1), -1)));
                // Twist
                float deltaTwistX = Quaternion.Angle(Quaternion.identity, Quaternion.FromToRotation(twistXHandlePos - discCenter, twistX - discCenter)) * Mathf.Deg2Rad;
                float deltaTwistY = Quaternion.Angle(Quaternion.identity, Quaternion.FromToRotation(twistYHandlePos - discCenter, twistY - discCenter)) * Mathf.Deg2Rad;
                Debug.Log("deltaTwistX:" + deltaTwistX + " deltaTwistY:" + deltaTwistY);
                float d;
                Vector3 a;
                Quaternion.FromToRotation(twistXHandlePos - discCenter, twistX - discCenter).ToAngleAxis(out d, out a);

                Debug.Log("angle:" + d + " axis:" + a + " axis?:" + (discCenter - plugPose.Pos().ToVector3()).normalized + "?:" + Vector3.Cross(twistXHandlePos - discCenter, twistX - discCenter).normalized);
                Handles.color = Color.cyan;
                Handles.DrawLine(discCenter, discCenter + (10 * a));
                limit.desc.limitTwist = new Vec2d(twist[0] + deltaTwistX, twist[1] + deltaTwistY);
                limit.OnValidate();
            }
        }
    }
}
