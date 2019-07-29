using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace SprUnity {
    [CreateNodeMenu("Transform/Mirror")]
    public class Mirror : VGentNodeBase {

        [Output] public PosRotScale output;
        [Input] public PosRotScale input;
        [Input] public Vector3 mirrorPos;
        [Input] public Vector3 mirrorNormal = new Vector3(1, 0, 0);
        public bool mirrorPosition;
        public bool mirrorRotation;

        // Use this for initialization
        protected override void Init() {
            base.Init();
        }

        // Return the correct value of an output port when requested
        public override object GetValue(NodePort port) {
            if(port.fieldName == "output") {
                PosRotScale temp = GetInputValue<PosRotScale>("input", this.input);
                Vector3 mirror = GetInputValue<Vector3>("mirrorNormal", this.mirrorNormal);

            }
            return null;
        }
    }
}