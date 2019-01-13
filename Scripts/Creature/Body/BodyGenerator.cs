using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SprCs;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SprUnity {

    // Bodyの骨組みを生成するクラス
    // -- 基本的にはBodyはprefab化されたものを使うが、そのprefab自体を作成する時に用いる補助クラス

#if UNITY_EDITOR
    [CustomEditor(typeof(BodyGenerator))]
    public class BodyGeneratorEditor : Editor {

        public override void OnInspectorGUI() {
            BodyGenerator bodyGen = (BodyGenerator)target;

            DrawDefaultInspector();

            if (GUILayout.Button("Generate")) {
                bodyGen.Generate();
            }
        }
    }
#endif

    public class BodyGenerator : MonoBehaviour {

        public bool generateFingers = false;
        public bool generateUnifiedLeg = false;
        public bool generateShape = false;

        // ----- ----- ----- ----- -----

        private Body body;

        // ----- ----- ----- ----- -----

        public GameObject Generate() {
            // Body
            var obj = new GameObject("Body");
            obj.transform.parent = gameObject.transform;
            body = obj.AddComponent<Body>();

            // Bones
            Bone root = null;
            Bone hips = null;

            if (generateUnifiedLeg) {
                Bone unifiedFoot = GenerateBone(null, "UnifiedFoot", dynamical: false);
                Bone unifiedLowerLeg = GenerateBone(unifiedFoot, "UnifiedLowerLeg");
                Bone unifiedUpperLeg = GenerateBone(unifiedLowerLeg, "UnifiedUpperLeg");
                hips = GenerateBone(unifiedUpperLeg, "Hips");
                root = unifiedFoot;
            } else {
                hips = GenerateBone(null, "Hips", dynamical: false);
                root = hips;
            }

            Bone spine = GenerateBone(hips, "Spine");
            Bone chest = GenerateBone(spine, "Chest");
            Bone upperChest = GenerateBone(chest, "UpperChest");
            Bone neck = GenerateBone(upperChest, "Neck");
            Bone head = GenerateBone(neck, "Head");

            Bone leftShoulder = GenerateBone(upperChest, "LeftShoulder");
            Bone leftUpperArm = GenerateBone(leftShoulder, "LeftUpperArm");
            Bone leftLowerArm = GenerateBone(leftUpperArm, "LeftLowerArm", hinge: true);
            Bone leftHand = GenerateBone(leftLowerArm, "LeftHand", eePos: true);

            Bone rightShoulder = GenerateBone(upperChest, "RightShoulder");
            Bone rightUpperArm = GenerateBone(rightShoulder, "RightUpperArm");
            Bone rightLowerArm = GenerateBone(rightUpperArm, "RightLowerArm", hinge: true);
            Bone rightHand = GenerateBone(rightLowerArm, "RightHand", eePos: true);

            Bone leftUpperLeg = GenerateBone(hips, "LeftUpperLeg");
            Bone leftLowerLeg = GenerateBone(leftUpperLeg, "LeftLowerLeg", hinge:true);
            Bone leftFoot = GenerateBone(leftLowerLeg, "LeftFoot");
            Bone leftToe = GenerateBone(leftFoot, "LeftToe");

            Bone rightUpperLeg = GenerateBone(hips, "RightUpperLeg");
            Bone rightLowerLeg = GenerateBone(rightUpperLeg, "RightLowerLeg", hinge:true);
            Bone rightFoot = GenerateBone(rightLowerLeg, "RightFoot");
            Bone rightToe = GenerateBone(rightFoot, "RightToe");

            if (generateFingers) {
                // <TBD>
            }

            // ----- ----- ----- ----- -----

            root.gameObject.transform.parent = obj.transform;

            hips.syncPosition = true;

            return hips.gameObject;
        }

        private Bone GenerateBone(Bone parent, string label, bool hinge=false, bool eePos=false, bool eeOri=false, bool dynamical = true) {
            // GameObject
            var obj = new GameObject(label);
            if (parent != null) {
                obj.transform.parent = parent.transform;
            }

            // Bone
            var bone = obj.AddComponent<Bone>();
            bone.label = label;
            bone.body = body;
            bone.parent = parent;
            if (bone.parent != null) {
                bone.parent.children.Add(bone);
            }

            // Solid
            bone.solid = obj.AddComponent<PHSolidBehaviour>();
            bone.solid.lateAwakeStart = true;
            if (!dynamical) {
                bone.solid.desc.dynamical = false;
            }

            // Shape
            if (generateShape) {
                bone.shape = obj.AddComponent<CDRoundConeBehavior>();
                bone.shape.lateAwakeStart = true;
            }

            if (parent != null) {
                // Joint
                if (hinge) {
                    bone.joint = obj.AddComponent<PHHingeJointBehaviour>();
                    ((PHHingeJointBehaviour)(bone.joint)).desc.spring = 200.0f;
                    ((PHHingeJointBehaviour)(bone.joint)).desc.damper = 20.0f;
                } else {
                    bone.joint = obj.AddComponent<PHBallJointBehaviour>();
                    ((PHBallJointBehaviour)(bone.joint)).desc.spring = 200.0f;
                    ((PHBallJointBehaviour)(bone.joint)).desc.damper = 20.0f;
                }
                bone.joint.lateAwakeStart = true;
                bone.joint.socket = parent.solid.gameObject;
                bone.joint.plug = bone.solid.gameObject;

                // IK Actuator
                if (hinge) {
                    bone.ikActuator = obj.AddComponent<PHIKHingeActuatorBehaviour>();
                } else {
                    bone.ikActuator = obj.AddComponent<PHIKBallActuatorBehaviour>();
                }
                bone.ikActuator.lateAwakeStart = true;

                // IK Endeffector
                bone.ikEndEffector = obj.AddComponent<PHIKEndEffectorBehaviour>();
                bone.ikEndEffector.lateAwakeStart = true;
                bone.ikEndEffector.desc.bEnabled = (eePos || eeOri);
                bone.ikEndEffector.desc.bPosition = eePos;
                bone.ikEndEffector.desc.bOrientation = eeOri;
            }

            body.bones.Add(bone);

            return bone;
        }

    }

}