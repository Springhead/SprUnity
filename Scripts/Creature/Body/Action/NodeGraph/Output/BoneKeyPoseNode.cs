using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SprUnity {
    [CreateNodeMenu("Output/BoneKeypose")]
    public class BoneKeyPoseNode : KeyPoseNodeBase {
        [Output] public BoneKeyPose boneKeyPose;
        [Input] public HumanBodyBones boneId;
        [Input] public PosRotScale posRotScale;
        [Input] public bool usePosition;
        [Input] public bool useRotation;

        private Material editableMat, visibleMat;
        private Mesh leftHand;
        private Mesh rightHand;
        private Mesh head;
        private Mesh leftFoot;
        private Mesh rightFoot;

        private float handleSize = 0.05f;
        private float selectedHandleSize = 0.15f;
        private StaticBoneKeyPose selectedboneKeyPose; // マウスが上にあるKeyPoseだけハンドルを大きくする

        // Use this for initialization
        protected override void Init() {
            base.Init();

        }

        // Return the correct value of an output port when requested
        public override object GetValue(NodePort port) {
            return GetBoneKeyPose(); // Replace this
        }

        public BoneKeyPose GetBoneKeyPose() {
            BoneKeyPose tempBoneKeyPose = new BoneKeyPose();
            PosRotScale tempPosRotScale = GetInputValue<PosRotScale>("posRotScale", this.posRotScale);
            tempBoneKeyPose.boneId = GetInputValue<HumanBodyBones>("boneID", this.boneId);
            tempBoneKeyPose.localPosition = tempPosRotScale.position;
            tempBoneKeyPose.localRotation = tempPosRotScale.rotation;
            tempBoneKeyPose.usePosition = GetInputValue<bool>("usePosition", this.usePosition);
            tempBoneKeyPose.useRotation = GetInputValue<bool>("useRotation", this.useRotation);
            return tempBoneKeyPose;
        }

        public override KeyPose GetKeyPose() {
            KeyPose keyPose = new KeyPose();
            BoneKeyPose boneKeyPose = new BoneKeyPose();
            PosRotScale tempPosRotScale = GetInputValue<PosRotScale>("posRotScale", this.posRotScale);
            boneKeyPose.boneId = boneId;
            boneKeyPose.localPosition = tempPosRotScale.position;
            boneKeyPose.localRotation = tempPosRotScale.rotation;
            boneKeyPose.usePosition = GetInputValue<bool>("usePosition", this.usePosition);
            boneKeyPose.useRotation = GetInputValue<bool>("useRotation", this.useRotation);
            keyPose.boneKeyPoses.Add(boneKeyPose);
            return keyPose;
        }

        protected new void OnEnable() {
            base.OnEnable();

            var modelpath = "Assets/Libraries/SprUnity/Editor/Creature/Models/";

            editableMat = AssetDatabase.LoadAssetAtPath(modelpath + "editable.mat", typeof(Material)) as Material;
            visibleMat = AssetDatabase.LoadAssetAtPath(modelpath + "visible.mat", typeof(Material)) as Material;
            if (editableMat == null) {
                Debug.Log("mat null");
            }
            if (visibleMat == null) {
                Debug.Log("mat null");
            }

            leftHand = AssetDatabase.LoadAssetAtPath(
                modelpath + "LeftHand.fbx", typeof(Mesh)) as Mesh;
            if (leftHand == null) {
                Debug.Log("fbx null");
            }

            rightHand = AssetDatabase.LoadAssetAtPath(
                modelpath + "RightHand.fbx", typeof(Mesh)) as Mesh;
            if (rightHand == null) {
                Debug.Log("fbx null");
            }

            head = AssetDatabase.LoadAssetAtPath(
                modelpath + "Head.fbx", typeof(Mesh)) as Mesh;
            if (head == null) {
                Debug.Log("fbx null");
            }

            leftFoot = AssetDatabase.LoadAssetAtPath(
                modelpath + "LeftFoot.fbx", typeof(Mesh)) as Mesh;
            if (leftFoot == null) {
                Debug.Log("fbx null");
            }

            rightFoot = AssetDatabase.LoadAssetAtPath(
                modelpath + "RightFoot.fbx", typeof(Mesh)) as Mesh;
            if (rightFoot == null) {
                Debug.Log("fbx null");
            }
        }
        public override void OnSceneGUI(Body body = null) {
            BoneKeyPose temp = GetBoneKeyPose();
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

        void DrawHumanBone(BoneKeyPose boneKeyPose) {
            if (boneKeyPose.usePosition || boneKeyPose.useRotation) {
                // 調整用の手などを表示
                editableMat.SetPass(0); // 1だと影しか見えない？ 
                if (boneKeyPose.boneId == HumanBodyBones.LeftHand) {
                    Graphics.DrawMeshNow(leftHand, boneKeyPose.position, boneKeyPose.rotation.normalized, 0);
                } else if (boneKeyPose.boneId == HumanBodyBones.RightHand) {
                    Graphics.DrawMeshNow(rightHand, boneKeyPose.position, boneKeyPose.rotation.normalized, 0);
                } else if (boneKeyPose.boneId == HumanBodyBones.Head) {
                    Graphics.DrawMeshNow(head, boneKeyPose.position, boneKeyPose.rotation.normalized, 0);
                } else if (boneKeyPose.boneId == HumanBodyBones.LeftFoot) {
                    Graphics.DrawMeshNow(leftFoot, boneKeyPose.position, boneKeyPose.rotation.normalized, 0);
                } else if (boneKeyPose.boneId == HumanBodyBones.RightFoot) {
                    Graphics.DrawMeshNow(rightFoot, boneKeyPose.position, boneKeyPose.rotation.normalized, 0);
                }
            }
            if (boneKeyPose.usePosition || boneKeyPose.useRotation) {
                // 調整用の手などを表示
                visibleMat.SetPass(0); // 1だと影しか見えない？ 
                if (boneKeyPose.boneId == HumanBodyBones.LeftHand) {
                    Graphics.DrawMeshNow(leftHand, boneKeyPose.position, boneKeyPose.rotation.normalized, 0);
                } else if (boneKeyPose.boneId == HumanBodyBones.RightHand) {
                    Graphics.DrawMeshNow(rightHand, boneKeyPose.position, boneKeyPose.rotation.normalized, 0);
                } else if (boneKeyPose.boneId == HumanBodyBones.Head) {
                    Graphics.DrawMeshNow(head, boneKeyPose.position, boneKeyPose.rotation.normalized, 0);
                } else if (boneKeyPose.boneId == HumanBodyBones.LeftFoot) {
                    Graphics.DrawMeshNow(leftFoot, boneKeyPose.position, boneKeyPose.rotation.normalized, 0);
                } else if (boneKeyPose.boneId == HumanBodyBones.RightFoot) {
                    Graphics.DrawMeshNow(rightFoot, boneKeyPose.position, boneKeyPose.rotation.normalized, 0);
                }
            }
        }
    }

}