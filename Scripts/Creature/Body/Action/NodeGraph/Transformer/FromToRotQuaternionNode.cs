using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace SprUnity {
    [CreateNodeMenu("Transform/FromToRot")]
    public class FromToRotQuaternionNode : ActionTargetTransformNodeBase {
        [Output] public Quaternion output;
        [Input] public Vector3 from = Vector3.forward;
        [Input] public Vector3 to = Vector3.up;

        // Use this for initialization
        protected override void Init() {
            base.Init();

        }

        // Return the correct value of an output port when requested
        public override object GetValue(NodePort port) {
            Quaternion result = Quaternion.identity;
            Vector3 tempFrom = GetInputValue<Vector3>("from", this.from);
            Vector3 tempTo = GetInputValue<Vector3>("to", this.to);
            return Quaternion.FromToRotation(tempFrom, tempTo);
        }
    }
}