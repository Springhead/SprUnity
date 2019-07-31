using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace SprUnity {
    [CreateNodeMenu("Converter/DecomposePosRotScale")]
    public class DecomposePosRotScaleNode : ActionTargetNodeBase {
        [Input] public PosRotScale posRotScale = new PosRotScale();
        [Output] public Vector3 pos;
        [Output] public Quaternion rot;
        [Output] public Vector3 scale;

        // Use this for initialization
        protected override void Init() {
            base.Init();

        }

        // Return the correct value of an output port when requested
        public override object GetValue(NodePort port) {
            PosRotScale tempPosRotScale = GetInputValue<PosRotScale>("posRotScale", this.posRotScale);
            if(port.fieldName == "pos") {
                return tempPosRotScale.position;
            }
            else if(port.fieldName == "rot") {
                return tempPosRotScale.rotation;
            }
            else if(port.fieldName == "scale") {
                return tempPosRotScale.scale;
            } else {
                return 0.0f;
            }
        }
    }
}