using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace SprUnity {
    [CreateNodeMenu("Coordinate/Bone")]
    public class BoneCoordinateNode : VGentNodeBase {
        [Output] public PosRotScale posRotScale = new PosRotScale();
        public HumanBodyBones boneId;

        // Use this for initialization
        protected override void Init() {
            base.Init();
        }

        // Return the correct value of an output port when requested
        public override object GetValue(NodePort port) {
            if(port.fieldName == "posRotScale") {
                PosRotScale tempPosRotScale = new PosRotScale();
                Body body = (graph as KeyPoseNodeGraph)?.body;
                Bone bone = body?[boneId];
                if (body != null && bone != null) {
                    tempPosRotScale.position = bone.transform.position;
                    tempPosRotScale.rotation = bone.transform.rotation;
                    tempPosRotScale.scale = body.height * Vector3.one;
                }
                return tempPosRotScale;
            } else {
                return null;
            }
        }
    }
}