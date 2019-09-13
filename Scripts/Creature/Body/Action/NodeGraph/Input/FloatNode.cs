using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace VGent{
    [CreateNodeMenu("Value/Float")]
    public class FloatNode : ActionTargetInputNodeBase {
        [Output] public float output;
        public float value;

        // Use this for initialization
        protected override void Init() {
            base.Init();

        }

        public override void SetInput<T>(T value) {
            if (value is float) {
                this.value = (float)(object)value;
            }
        }

        // Return the correct value of an output port when requested
        public override object GetValue(NodePort port) {
            if (port.fieldName == "output") {
                return value;
            } else { return 0f; }
        }
    }
}