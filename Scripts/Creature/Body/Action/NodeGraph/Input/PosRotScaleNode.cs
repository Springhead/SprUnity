using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace SprUnity {
    [CreateNodeMenu("Value/PosRotScale")]
    public class PosRotScaleNode : Node {
        [Output] public PosRotScale output;
        [Input] public Vector3 pos = new Vector3();
        [Input] public Quaternion rot = Quaternion.identity;
        [Input] public Vector3 scale = new Vector3(1f, 1f, 1f);

        // Use this for initialization
        protected override void Init() {
            base.Init();

        }

        // Return the correct value of an output port when requested
        public override object GetValue(NodePort port) {
            if (port.fieldName == "output") {
                return new PosRotScale(GetInputValue<Vector3>("pos", pos), GetInputValue<Quaternion>("rot", rot), GetInputValue<Vector3>("scale", scale));
            }
            return new PosRotScale();
        }
    }
}