using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace VGent{

    public abstract class KeyPoseNodeBase : ActionTargetNodeBase {

        // Use this for initialization
        protected override void Init() {
            base.Init();

        }

        // Return the correct value of an output port when requested
        public override object GetValue(NodePort port) {
            return null; // Replace this
        }

        public abstract KeyPose GetKeyPose();
    }

}