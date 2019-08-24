using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SprUnity {
    [CustomNodeEditor(typeof(QuaternionNode))]
    public class QuaternionNodeEditor : ActionTargetNodeBaseEditor { 
        public override void OnSceneGUI(Body body = null) {
            QuaternionNode node = (QuaternionNode)target;
            EditorGUI.BeginChangeCheck();
            Quaternion rot = Handles.RotationHandle(new Quaternion(node.x, node.y, node.z, node.w), Vector3.zero);
            Handles.PositionHandle(Vector3.zero, new Quaternion(node.x, node.y, node.z, node.w));
            if (EditorGUI.EndChangeCheck()) {
                node.x = rot.x;
                node.y = rot.y;
                node.z = rot.z;
                node.w = rot.w;
            }
        }
    }
}