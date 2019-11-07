using UnityEditor;
using UnityEngine;
using System.Collections;

using SprCs;

namespace SprUnity {

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
                GameObject jointPositionObject = phBallJointBehaviour.jointObject ? phBallJointBehaviour.jointObject : phBallJointBehaviour.gameObject;
                Transform jointTransform = jointPositionObject.transform;
                Posed plugPose = phBallJointBehaviour.plugPose;
                Posed socketPose = phBallJointBehaviour.socketPose;

                Vector3 jointPosition = socketPose.Pos().ToVector3();
                Vector3 twistAxis = plugPose.Ori().ToQuaternion() * new Vector3(0, 0, 1);

                //Limit情報の取得
                //Vec2d swing = new Vec2d(); limit.phJointLimit.GetSwingRange(swing);
                Vec2d swing = limit.desc.limitSwing;
                //Vec2d twist = new Vec2d(); limit.phJointLimit.GetTwistRange(twist);
                Vec2d twist = limit.desc.limitTwist;
                Vec2i twistRap = new Vec2i((int)((twist[0] + Mathf.PI) / (2 * Mathf.PI)), (int)((twist[0] + Mathf.PI) / (2 * Mathf.PI)));
                Vec3d limitDir = limit.desc.limitDir;

                double currTwistAngle = (phBallJointBehaviour.sprObject != null) ? (phBallJointBehaviour.phBallJoint.GetAngle())[2] : 0;

                Color baseColor;

                if (limit.desc.bEnabled && limit.enabled && twist[0] < twist[1]) {
                    if (limit.phJointLimit == null) {
                        baseColor = Color.green;
                    } else {
                        if (limit.phJointLimit.IsOnLimit()) {
                            baseColor = Color.red;
                        } else {
                            baseColor = Color.green;
                        }
                    }
                    baseColor.a = 0.3f;
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
                    //Handles.DrawLine((socketPose * swingDirBase1).ToVector3(), (socketPose * swingDirBase2).ToVector3());
                    Vector3 arcNormal = Vector3.Cross(((socketPose * swingDirBase1).ToVector3() - jointPosition), ((socketPose * swingDirBase2).ToVector3() - jointPosition));
                    //Handles.DrawWireArc(jointPosition, arcNormal, (socketPose * swingDirBase1).ToVector3() - jointPosition, (float)(swing[1] - swing[0]) * Mathf.Rad2Deg, length);
                    swingDirBase1 = rot * swingDirBase1;
                    swingDirBase2 = rot * swingDirBase2;
                }
                Vec3d swingCircleCenter1 = socketPose * (length * Mathf.Cos((float)swing[0]) * limitDir);
                Vec3d swingCircleCenter2 = socketPose * (length * Mathf.Cos((float)swing[1]) * limitDir);
                Handles.DrawWireDisc(swingCircleCenter1.ToVector3(), swingBaseAxis.ToVector3() - jointPosition, length * Mathf.Abs(Mathf.Sin((float)swing[0])));
                Handles.DrawWireDisc(swingCircleCenter2.ToVector3(), swingBaseAxis.ToVector3() - jointPosition, length * Mathf.Abs(Mathf.Sin((float)swing[1])));
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
                Vector3 currentTwist = ((plugPose * new Vec3d((discRadius + 0.01f) * Mathf.Cos((float)currTwistAngle), (discRadius + 0.01f) * Mathf.Sin((float)currTwistAngle), lengthTwist)).ToVector3());
                Vector3 twistXHandlePos = (plugPose * (new Vec3d(0.03 * Mathf.Cos((float)twist[0]), 0.03 * Mathf.Sin((float)twist[0]), lengthTwist))).ToVector3();
                Vector3 twistXHandleDir = (plugPose * (new Vec3d(0.03 * -Mathf.Sin((float)twist[0]), 0.03 * Mathf.Cos((float)twist[0]), 0)) - plugPose.Pos()).ToVector3();
                Vector3 twistYHandlePos = (plugPose * (new Vec3d(0.03 * Mathf.Cos((float)twist[1]), 0.03 * Mathf.Sin((float)twist[1]), lengthTwist))).ToVector3();
                Vector3 twistYHandleDir = (plugPose * (new Vec3d(0.03 * -Mathf.Sin((float)twist[1]), 0.03 * Mathf.Cos((float)twist[1]), 0)) - plugPose.Pos()).ToVector3();
                Handles.DrawWireDisc(discCenter, discNormal, discRadius);
                Handles.DrawSolidArc(discCenter, discNormal, twistXHandlePos - discCenter, (float)(twist[1] - twist[0]) * Mathf.Rad2Deg, discRadius);
                Vector3 twistX = Handles.Slider2D(twistXHandlePos, twistAxis, twistXHandlePos - discCenter, twistXHandleDir, 0.01f, Handles.CubeHandleCap, 0f);
                Vector3 twistY = Handles.Slider2D(twistYHandlePos, twistAxis, twistYHandlePos - discCenter, twistYHandleDir, 0.01f, Handles.CubeHandleCap, 0f);
                Handles.color = Color.yellow;
                Handles.DrawLine(discCenter, currentTwist);
                if (twist[0] >= twist[1]) {
                    Handles.color = Color.white;
                } else {
                    Handles.color = baseColor;
                }

                if (EditorGUI.EndChangeCheck()) {
                    Undo.RecordObject(limit, "Undo Limit Chnage");
                    // Swing
                    limit.desc.limitSwing = new Vec2d(Mathf.Acos((Vector3.Dot((SwingX - socketPose.Pos().ToVector3()), (swingBaseAxis.ToVector3() - socketPose.Pos().ToVector3())) > 0 ? 1 : -1) * Mathf.Max(Mathf.Min((SwingX - socketPose.Pos().ToVector3()).magnitude / length, 1), -1)),
                        Mathf.Acos((Vector3.Dot((SwingY - socketPose.Pos().ToVector3()), (swingBaseAxis.ToVector3() - socketPose.Pos().ToVector3())) > 0 ? 1 : -1) * Mathf.Max(Mathf.Min((SwingY - socketPose.Pos().ToVector3()).magnitude / length, 1), -1)));
                    // Twist
                    float deltaTwistX = Vector3.SignedAngle(twistXHandlePos - discCenter, twistX - discCenter, discCenter - plugPose.Pos().ToVector3()) * Mathf.Deg2Rad;
                    float deltaTwistY = Vector3.SignedAngle(twistYHandlePos - discCenter, twistY - discCenter, discCenter - plugPose.Pos().ToVector3()) * Mathf.Deg2Rad;
                    limit.desc.limitTwist = new Vec2d(deltaTwistX + twist[0], deltaTwistY + twist[1]);
                    limit.OnValidate();
                }
            }
        }
    }

}