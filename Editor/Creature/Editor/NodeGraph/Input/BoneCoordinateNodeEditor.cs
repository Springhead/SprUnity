using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using XNode;
using XNodeEditor;
using SprUnity;

namespace VGent{
    [CustomNodeEditor(typeof(BoneCoordinateNode))]
    public class BoneCoordinateNodeEditor : ActionTargetNodeBaseEditor {
        public override void OnSceneGUI(Body body = null) {
            BoneCoordinateNode node = (BoneCoordinateNode)target;
            PosRotScale tempPosRotScale = new PosRotScale();
            Body graphBody = (node.graph as ActionTargetGraph)?.body;
            Bone bone = graphBody == null ? body?[node.boneId] : graphBody[node.boneId];
            Vector3 pos;
            Quaternion rot;
            if(bone != null) {
                if (graphBody == null) {
                    // Graph has No active body
                    node.posRotScale = new PosRotScale(bone.transform.position, bone.transform.rotation, body.height * Vector3.one);
                }
                pos = bone.transform.position;
                rot = bone.transform.rotation;
                Handles.PositionHandle(pos, rot);
            } 
        }
    }
}