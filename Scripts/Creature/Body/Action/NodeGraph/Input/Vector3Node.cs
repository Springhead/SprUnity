using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace SprUnity {
    [CreateNodeMenu("Value/Vector3")]
    public class Vector3Node : ActionTargetInputNodeBase {
        [Output] public Vector3 output;
        [Input] public float x;
        [Input] public float y;
        [Input] public float z;

        // Use this for initialization
        protected override void Init() {
            base.Init();

        }

        // Return the correct value of an output port when requested
        public override object GetValue(NodePort port) {
            if (port.fieldName == "output") {
                return new Vector3(GetInputValue<float>("x", x), GetInputValue<float>("y", y), GetInputValue<float>("z", z));
            }
            return Vector3.zero;
        }
    }
}