﻿using System.Collections;
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
            Body graphBody = (node.graph as ActionTargetGraph)?.body;
            Vector3 pos;
            Quaternion rot;
            if(graphBody == null) {
                // Graph has No active body
                node.posRotScale = new PosRotScale(body.transform.position, body.transform.rotation, body.height * Vector3.one);
                pos = node.posRotScale.position;
                rot = node.posRotScale.rotation;
            } else {
                pos = graphBody.transform.position;
                rot = graphBody.transform.rotation;
            }
            Handles.PositionHandle(pos, rot);
        }
    }
}