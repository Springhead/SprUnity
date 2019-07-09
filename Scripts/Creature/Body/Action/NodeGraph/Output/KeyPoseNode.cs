﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace SprUnity {
    [CreateNodeMenu("Output/KeyPose")]
    public class KeyPoseNode : KeyPoseNodeBase {
        [Output] public KeyPose key;
        [Input(dynamicPortList = true)] public BoneKeyPose[] boneKeyPoses;

        // Use this for initialization
        protected override void Init() {
            base.Init();

        }

        // Return the correct value of an output port when requested
        public override object GetValue(NodePort port) {
            return null; // Replace this
        }

        public override KeyPose GetKeyPose() {
            KeyPose keyPose = new KeyPose();
            BoneKeyPose[] tempBoneKeyPoses = GetInputValues<BoneKeyPose>("boneKeyPoses", this.boneKeyPoses);
            foreach(var boneKeyPose in tempBoneKeyPoses) {
                if (boneKeyPose.Enabled()) {
                    keyPose.boneKeyPoses.Add(boneKeyPose);
                }
            }
            return keyPose;
        }
    }

}