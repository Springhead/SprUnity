using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using XNode;
using XNodeEditor;
using SprUnity;

namespace VGent{
    [CustomNodeEditor(typeof(RelativePosRotScaleNode))]
    public class RelativePosRotScaleNodeEditor : ActionTargetNodeBaseEditor {
        public override void OnSceneGUI(Body body = null) {
            RelativePosRotScaleNode node = (RelativePosRotScaleNode)target;
            PosRotScale tempOrigin = node.GetInputValue<PosRotScale>("origin");
            if (node.GetPort("origin").IsConnected) {
                SceneViewHandles.Axis(tempOrigin.position, tempOrigin.rotation);
            }
            PosRotScale tempRelative = node.GetInputValue<PosRotScale>("relative", node.relative);
            PosRotScale r = tempOrigin.TransformPosRotScale(tempRelative);
            if (Tools.pivotRotation == PivotRotation.Local) {
                EditorGUI.BeginChangeCheck();
                float handleSize= ActionTargetGraphEditorWindow.HandleSize;
                // Vector3 pos = SceneViewHandles.AxisMove(r.position, r.rotation, handleSize);
                Vector3 pos = Handles.PositionHandle(r.position, r.rotation);
                // Quaternion rot = SceneViewHandles.AxisRotate(r.rotation, r.position, handleSize);
                Quaternion rot = Handles.RotationHandle(r.rotation, r.position);

                /*
                string s = "";
                s += "r.position=" + r.position.ToString() + ", ";
                s += "r.rotation=" + r.rotation.ToString() + ", ";
                s += "pos=" + pos.ToString() + ", ";
                s += "rot=" + rot.ToString() + ", ";
                Debug.Log(s);
                */

                if (EditorGUI.EndChangeCheck() && !node.GetPort("relative").IsConnected) {
                    Undo.RecordObject(node, "Change RelativePosRotScaleNode");
                    node.relative = tempOrigin.InverseTransformPosRotScale(new PosRotScale(pos, rot, r.scale));
                }
                
            } else {
                // <!!>いずれ実装する
                EditorGUI.BeginChangeCheck();
                Vector3 pos = Handles.PositionHandle(r.position, r.rotation);
                Quaternion rot = Handles.RotationHandle(r.rotation, r.position);
                if (EditorGUI.EndChangeCheck() && !node.GetPort("relative").IsConnected)
                {
                    Undo.RecordObject(node, "Change RelativePosRotScaleNode");
                    node.relative = tempOrigin.InverseTransformPosRotScale(new PosRotScale(pos, rot, r.scale));
                }
                /*
                EditorGUI.BeginChangeCheck();
                Vector3 pos = Handles.PositionHandle(r.position, Quaternion.identity);
                if (EditorGUI.EndChangeCheck() && !node.GetPort("relative").IsConnected) {
                    Undo.RecordObject(node, "Change RelativePosRotScaleNode");
                    node.relative = tempOrigin.InverseTransformPosRotScale(new PosRotScale(pos, r.rotation, r.scale));
                }*/
            }
        }

    }
}