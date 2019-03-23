using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using SprCs;

namespace SprUnity {

    [CustomEditor(typeof(PH1DJointLimitBehavior))]
    public class PH1DJointLimitBehaviorEditor : Editor {
        // DiscRadius
        float discRadius = 0.03f;
        // 可視化する軸(zは0)
        Vector3 handDir = new Vector3(1, 0, 0);
        void OnSceneGUI() {
            PH1DJointLimitBehavior limit = (PH1DJointLimitBehavior)target;
            Vec2d range = limit.desc.range;

            GameObject jointObject = limit.jointObject ? limit.jointObject : limit.gameObject;
            // Hinge
            PHHingeJointBehaviour phHingeJointBehaviour = jointObject.GetComponent<PHHingeJointBehaviour>();

            if (phHingeJointBehaviour) {
                GameObject jointPositionObject = phHingeJointBehaviour.jointObject ? phHingeJointBehaviour.jointObject : phHingeJointBehaviour.gameObject;
                Transform jointTransform = jointPositionObject.transform;
                Posed plugPose = phHingeJointBehaviour.plugPose;
                Posed socketPose = phHingeJointBehaviour.socketPose;

                Vector3 plugPosition = plugPose.Pos().ToVector3();
                Vector3 socketPosition = socketPose.Pos().ToVector3();
                Quaternion plugRot = plugPose.Ori().ToQuaternion();
                Quaternion socketRot = socketPose.Ori().ToQuaternion();

                Color baseColor;
                if (limit.desc.bEnabled && limit.enabled && limit.desc.range.x < limit.desc.range.y) {
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

                EditorGUI.BeginChangeCheck();

                Vector3 jointPosition = socketPosition;
                Vector3 jointAxis = socketRot * new Vector3(0, 0, 1);

                Handles.color = baseColor;
                Vector3 rangeXHandlePos = jointPosition + Quaternion.AngleAxis((float)(range[0] * Mathf.Rad2Deg), socketRot * new Vector3(0, 0, 1)) * (socketRot * (discRadius * handDir));
                Vector3 rangeXHandleDir = Quaternion.AngleAxis((float)(range[0] * Mathf.Rad2Deg) + 90, socketRot * new Vector3(0, 0, 1)) * (socketRot * (discRadius * handDir));
                Vector3 rangeYHandlePos = jointPosition + Quaternion.AngleAxis((float)(range[1] * Mathf.Rad2Deg), socketRot * new Vector3(0, 0, 1)) * (socketRot * (discRadius * handDir));
                Vector3 rangeYHandleDir = Quaternion.AngleAxis((float)(range[1] * Mathf.Rad2Deg) + 90, socketRot * new Vector3(0, 0, 1)) * (socketRot * (discRadius * handDir));

                Handles.DrawWireDisc(jointPosition, socketRot * new Vector3(0, 0, 1), discRadius);
                Handles.DrawSolidArc(jointPosition, jointAxis, rangeXHandlePos - jointPosition, (float)(range[1] - range[0]) * Mathf.Rad2Deg, discRadius);

                Vector3 rangeX = Handles.Slider2D(rangeXHandlePos, jointAxis, rangeXHandlePos - jointPosition, rangeXHandleDir, 0.01f, Handles.CubeHandleCap, 0f);
                Vector3 rangeY = Handles.Slider2D(rangeYHandlePos, jointAxis, rangeYHandlePos - jointPosition, rangeYHandleDir, 0.01f, Handles.CubeHandleCap, 0f);

                Handles.color = Color.yellow;
                Vector3 currentHand = plugPosition + (plugRot * ((discRadius + 0.01f) * handDir));
                Handles.DrawLine(plugPosition, currentHand);

                if (EditorGUI.EndChangeCheck()) {
                    Undo.RecordObject(limit, "Undo Limit Chnage");
                    float deltaRangeX = Vector3.SignedAngle(rangeXHandlePos - jointPosition, rangeX - jointPosition, jointAxis) * Mathf.Deg2Rad;
                    float deltaRangeY = Vector3.SignedAngle(rangeYHandlePos - jointPosition, rangeY - jointPosition, jointAxis) * Mathf.Deg2Rad;
                    limit.desc.range = new Vec2d(deltaRangeX + range[0], deltaRangeY + range[1]);
                    limit.OnValidate();
                }
            }
            // Slider
        }
    }

}