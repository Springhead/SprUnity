using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace SprUnity {
    [CreateNodeMenu("Value/Int")]
    public class IntNode : ActionTargetNodeBase {
        [Output] public int output;
        public int value;

        // Use this for initialization
        protected override void Init() {
            base.Init();

        }

        // Return the correct value of an output port when requested
        public override object GetValue(NodePort port) {
            if (port.fieldName == "output") {
                return value;
            } else { return 0; }
        }
    }
}