using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SprUnity {
    [CreateNodeMenu("Transform/Blend")]
    public class BlendPosRotScaleNode : VGentNodeBase {
        [Output] public PosRotScale output;
        [Input] public PosRotScale input1;
        [Input] public PosRotScale input2;
        [Input] [Range(0, 1.0f)] public float blendRate;

        // Use this for initialization
        protected override void Init() {
            base.Init();

        }

        // Return the correct value of an output port when requested
        public override object GetValue(NodePort port) {
            PosRotScale tempInput1 = GetInputValue<PosRotScale>("input1", this.input1);
            PosRotScale tempInput2 = GetInputValue<PosRotScale>("input2", this.input2);
            float tempBlend = GetInputValue<float>("blendRate", this.blendRate);
            Vector3 pos = (1 - tempBlend) * tempInput1.position + tempBlend * tempInput2.position;
            Quaternion rot = Quaternion.Lerp(tempInput1.rotation, tempInput2.rotation, tempBlend);
            Vector3 scale = (1 - tempBlend) * tempInput1.scale + tempBlend * tempInput2.scale;
            return new PosRotScale(pos, rot, scale);
        }

        public override void OnSceneGUI(Body body = null) {
#if UNITY_EDITOR
            PosRotScale tempInput1 = GetInputValue<PosRotScale>("input1", this.input1);
            PosRotScale tempInput2 = GetInputValue<PosRotScale>("input2", this.input2);
            float tempBlend = GetInputValue<float>("blendRate", this.blendRate);
            Vector3 pos = (1 - tempBlend) * tempInput1.position + tempBlend * tempInput2.position;
            Quaternion rot = Quaternion.Lerp(tempInput1.rotation, tempInput2.rotation, tempBlend);
            Handles.PositionHandle(tempInput1.position, tempInput1.rotation);
            Handles.PositionHandle(tempInput2.position, tempInput2.rotation);
            Handles.PositionHandle(pos, rot);
#endif
        }
    }
}