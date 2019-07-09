using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace SprUnity {
    public class BlendPosRotScaleNode : Node {
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
            return null; // Replace this
        }
    }
}