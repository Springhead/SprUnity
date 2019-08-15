using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace SprUnity {
    [CreateNodeMenu("Converter/DecomposeVector3")]
    public class DecomposeVector3Node : ActionTargetTransformNodeBase {
        [Input] public Vector3 vec = Vector3.zero;
        [Output] public float x;
        [Output] public float y;
        [Output] public float z;

        // Use this for initialization
        protected override void Init() {
            base.Init();

        }

        // Return the correct value of an output port when requested
        public override object GetValue(NodePort port) {
            Vector3 tempVec = GetInputValue<Vector3>("vec", this.vec);
            if(port.fieldName == "x") {
                return tempVec.x;
            }
            else if(port.fieldName == "y") {
                return tempVec.y;
            }
            else if(port.fieldName == "z") {
                return tempVec.z;
            } else {
                return null;
            }
        }
    }
}