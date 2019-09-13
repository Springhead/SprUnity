using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using XNode;
using XNodeEditor;
using SprUnity;
namespace VGent {
    [CustomNodeEditor(typeof(BlendPosRotScaleNode))]
    public class BlendPosRotScaleNodeEditor : ActionTargetNodeBaseEditor {
        public override void OnSceneGUI(Body body) {
            BlendPosRotScaleNode node = (BlendPosRotScaleNode)target;
            PosRotScale tempInput1 = node.GetInputValue<PosRotScale>("input1", node.input1);
            PosRotScale tempInput2 = node.GetInputValue<PosRotScale>("input2", node.input2);
            float tempBlend = node.GetInputValue<float>("blendRate", node.blendRate);
            Vector3 pos = (1 - tempBlend) * tempInput1.position + tempBlend * tempInput2.position;
            Quaternion rot = Quaternion.Lerp(tempInput1.rotation, tempInput2.rotation, tempBlend);
            Handles.PositionHandle(tempInput1.position, tempInput1.rotation);
            Handles.PositionHandle(tempInput2.position, tempInput2.rotation);
            Handles.PositionHandle(pos, rot);
        }
    }
}