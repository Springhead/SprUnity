using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using XNode;
using XNodeEditor;
using SprUnity;

namespace VGent{
    [CustomNodeEditor(typeof(PosRotScaleNode))]
    public class PosRotScaleNodeEditor : ActionTargetNodeBaseEditor {
        public override void OnSceneGUI(Body body = null) {
            PosRotScaleNode node = (PosRotScaleNode)target;
            Vector3 tempPos = node.GetInputValue<Vector3>("pos", node.pos);
            Quaternion tempRot = node.GetInputValue<Quaternion>("rot", node.rot);
            if (!node.GetPort("pos").IsConnected) {
                EditorGUI.BeginChangeCheck();
                Vector3 pos = Handles.PositionHandle(tempPos, tempRot);
                if (EditorGUI.EndChangeCheck()) {
                    node.pos = pos;
                }
            }
            if (!node.GetPort("rot").IsConnected) {
                EditorGUI.BeginChangeCheck();
                Quaternion rot = Handles.RotationHandle(tempRot, tempPos);
                if (EditorGUI.EndChangeCheck()) {
                    node.rot = rot;
                }
            }
        }

    }
}