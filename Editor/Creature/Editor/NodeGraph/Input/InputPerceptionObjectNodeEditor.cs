using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using XNode;
using XNodeEditor;
using SprUnity;

namespace VGent{
    [CustomNodeEditor(typeof(InputPerceptionObjectNode))]
    public class InputPerceptionObjectNodeEditor : ActionTargetNodeBaseEditor {
        public override void OnSceneGUI(Body body = null) {
            InputPerceptionObjectNode node = (InputPerceptionObjectNode)target;
            string label = node.name;
            if (node.perceptionObj == null) {
                EditorGUI.BeginChangeCheck();
                Vector3 pos = Handles.PositionHandle(node.posRotScale.position, node.posRotScale.rotation);
                Quaternion rot = Handles.RotationHandle(node.posRotScale.rotation, node.posRotScale.position);
                Handles.Label(pos, label + " dummy");
                if (EditorGUI.EndChangeCheck()) {
                    node.posRotScale.position = pos;
                    node.posRotScale.rotation = rot;
                }
            } else {
                var posRotScale = new PosRotScale(node.perceptionObj.transform);
                SceneViewHandles.Axis(posRotScale.position, posRotScale.rotation);
                Handles.Label(posRotScale.position, label);
            }
        }

    }
}