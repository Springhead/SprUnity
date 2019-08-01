using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using XNode;
using XNodeEditor;

namespace SprUnity {
    [CustomNodeEditor(typeof(BodyCoordinateNode))]
    public class BodyCoordinateNodeEditor : ActionTargetNodeBaseEditor {
        public override void OnSceneGUI(Body body) {
            BodyCoordinateNode node = (BodyCoordinateNode)target;
                Handles.PositionHandle(Vector3.zero, Quaternion.identity);
        }
    }
}