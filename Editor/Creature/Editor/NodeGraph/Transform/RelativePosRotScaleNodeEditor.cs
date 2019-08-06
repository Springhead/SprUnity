using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using XNode;
using XNodeEditor;

namespace SprUnity {
    [CustomNodeEditor(typeof(RelativePosRotScaleNode))]
    public class RelativePosRotScaleNodeEditor : ActionTargetNodeBaseEditor {
        public override void OnSceneGUI(Body body = null) {
            RelativePosRotScaleNode node = (RelativePosRotScaleNode)target;
            PosRotScale tempOrigin = node.GetInputValue<PosRotScale>("origin");
            if (node.GetPort("origin").IsConnected) {
                Handles.PositionHandle(tempOrigin.position, tempOrigin.rotation);
            }
            PosRotScale tempRelative = node.GetInputValue<PosRotScale>("relative", node.relative);
            PosRotScale r = tempOrigin.TransformPosRotScale(tempRelative);
            if (Tools.pivotRotation == PivotRotation.Local) {
                EditorGUI.BeginChangeCheck();
                Vector3 pos = Handles.PositionHandle(r.position, r.rotation);
                Quaternion rot = Handles.RotationHandle(r.rotation, r.position);
                if (EditorGUI.EndChangeCheck() && !node.GetPort("relative").IsConnected) {
                    Undo.RecordObject(node, "Change RelativePosRotScaleNode");
                    node.relative = tempOrigin.InverseTransformPosRotScale(new PosRotScale(pos, rot, r.scale));
                }
            } else {
                EditorGUI.BeginChangeCheck();
                Vector3 pos = Handles.PositionHandle(r.position, Quaternion.identity);
                if (EditorGUI.EndChangeCheck() && !node.GetPort("relative").IsConnected) {
                    Undo.RecordObject(node, "Change RelativePosRotScaleNode");
                    node.relative = tempOrigin.InverseTransformPosRotScale(new PosRotScale(pos, r.rotation, r.scale));
                }
            }
        }

    }
}