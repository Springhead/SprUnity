using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using SprUnity;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VGent{
    [CreateNodeMenu("Output/BoneKeypose")]
    public class ActionTargetOutputNode : KeyPoseNodeBase {
        [Output] public ActionTarget boneKeyPose;
        [Input] public HumanBodyBones boneId;
        [Input] public string boneLabel = "";
        [Input] public PosRotScale posRotScale;
        [Input] public bool usePosition;
        [Input] public bool useRotation;
        public bool enablePlay = true;

        // Use this for initialization
        protected override void Init() {
            base.Init();

        }

        // Return the correct value of an output port when requested
        public override object GetValue(NodePort port) {
            return GetBoneKeyPose(); // Replace this
        }

        public ActionTarget GetBoneKeyPose() {
            ActionTarget tempBoneKeyPose = new ActionTarget();
            PosRotScale tempPosRotScale = GetInputValue<PosRotScale>("posRotScale", this.posRotScale);
            tempBoneKeyPose.boneId = GetInputValue<HumanBodyBones>("boneID", this.boneId);
            tempBoneKeyPose.boneIdString = GetInputValue<string>("boneLabel", this.boneLabel);
            tempBoneKeyPose.localPosition = tempPosRotScale.position;
            tempBoneKeyPose.localRotation = tempPosRotScale.rotation;
            tempBoneKeyPose.usePosition = GetInputValue<bool>("usePosition", this.usePosition);
            tempBoneKeyPose.useRotation = GetInputValue<bool>("useRotation", this.useRotation);
            return tempBoneKeyPose;
        }

        public override KeyPose GetKeyPose() {
            KeyPose keyPose = new KeyPose();
            ActionTarget boneKeyPose = new ActionTarget();
            PosRotScale tempPosRotScale = GetInputValue<PosRotScale>("posRotScale", this.posRotScale);
            boneKeyPose.boneId = boneId;
            boneKeyPose.localPosition = tempPosRotScale.position;
            boneKeyPose.localRotation = tempPosRotScale.rotation;
            boneKeyPose.usePosition = GetInputValue<bool>("usePosition", this.usePosition);
            boneKeyPose.useRotation = GetInputValue<bool>("useRotation", this.useRotation);
            keyPose.boneKeyPoses.Add(boneKeyPose);
            return keyPose;
        }

        public override void OnSceneGUI(Body body = null) {
            ActionTarget temp = GetBoneKeyPose();
#if UNITY_EDITOR
            if (GetPort("posRotScale").IsConnected) {
                Handles.PositionHandle(temp.position, temp.rotation);
            } else {
                EditorGUI.BeginChangeCheck();
                Vector3 pos = Handles.PositionHandle(posRotScale.position, posRotScale.rotation);
                Quaternion rot = Handles.RotationHandle(posRotScale.rotation, posRotScale.position);
                if (EditorGUI.EndChangeCheck()) {
                    posRotScale.position = pos;
                    posRotScale.rotation = rot;
                }
            }
#endif
        }

    }

}