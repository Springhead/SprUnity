using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace SprUnity {
    [CreateNodeMenu("Converter/DecomposePose")]
    public class DecomposePoseNode : VGentNodeBase {
        [Input] public Pose pose = new Pose();
        [Output] public Vector3 pos = Vector3.zero;
        [Output] public Quaternion rot = Quaternion.identity;

        // Use this for initialization
        protected override void Init() {
            base.Init();

        }

        // Return the correct value of an output port when requested
        public override object GetValue(NodePort port) {
            var tempPose = GetInputValue<Pose>("pose", this.pose);
            if(port.fieldName == "pos") {
                return tempPose.position;
            } else if(port.fieldName == "rot") {
                return tempPose.rotation;
            } else {
                return null;
            }
        }
    }
}