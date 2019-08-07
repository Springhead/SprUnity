using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using XNode;
using XNodeEditor;

namespace SprUnity {
    [CustomNodeEditor(typeof(MirrorNode))]
    public class MirrorNodeEditor : ActionTargetNodeBaseEditor {
        static Color mirrorColor = new Color(0.5f, 0.5f, 0.5f, 0.1f);
        public override void OnSceneGUI(Body body = null) {
            MirrorNode node = (MirrorNode)target;
            Vector3 mirrorPos = node.mirrorPos;
            Vector3 mirrorNormal = node.mirrorNormal;
            Quaternion mirrorRot = Quaternion.FromToRotation(Vector3.right, mirrorNormal);
            Vector3 vert1 = mirrorPos + mirrorRot * new Vector3(0, -1, -2);
            Vector3 vert2 = mirrorPos + mirrorRot * new Vector3(0, 1, -2);
            Vector3 vert3 = mirrorPos + mirrorRot * new Vector3(0, 1, 2);
            Vector3 vert4 = mirrorPos + mirrorRot * new Vector3(0, -1, 2);
            Handles.DrawSolidRectangleWithOutline(new Vector3[] { vert1, vert2, vert3, vert4 }, mirrorColor, Color.black);
            EditorGUI.BeginChangeCheck();
            if (!node.GetPort("mirrorPos").IsConnected) {
                mirrorPos = Handles.PositionHandle(mirrorPos, mirrorRot);
            }
            if (!node.GetPort("mirrorNormal").IsConnected) {
                mirrorRot = Handles.RotationHandle(mirrorRot, mirrorPos);
            }
            if (EditorGUI.EndChangeCheck()) {
                node.mirrorPos = mirrorPos;
                node.mirrorNormal = mirrorRot * Vector3.right;
            }
        }
    }
}