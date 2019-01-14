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

        // Generate Target Flags
        public bool generateFingers = false;
        public bool generateUnifiedLeg = false;
        public bool generateShape = false;

        // Body Mass (not including UnifiedLeg)
        public float bodyMass = 40.0f;

        // ----- ----- ----- ----- -----

        private Body body;

        // ----- ----- ----- ----- -----

        public GameObject Generate() {
            // Body
            var obj = new GameObject("Body");
            obj.transform.parent = gameObject.transform;
            body = obj.AddComponent<Body>();

            // Body Parts Mass Ratio Table
            // -- from https://unit.aist.go.jp/hiri/dhrg/ja/dhdb/properties/segment/k-07.html
            // -- (Group 16)
            float massPercentHead = 9.4f; // -> Head:4 Neck:1
            float massPercentTorso = 46.0f; // -> Hips:1 Spine:2 Chest:2 UpperChest:0.7 LeftShoulder:0.15 RightShoulder:0.15
            float massPercentUpperArm = 2.3f;
            float massPercentLowerArm = 1.4f;
            float massPercentHand = 0.9f;
            float massPercentUpperLeg = 11.2f;
            float massPercentLowerLeg = 4.7f;
            float massPercentFoot = 1.8f; // -> Foot:2 Toes:1

            // Bones
            Bone hips = null;

            if (generateUnifiedLeg) {
                Bone unifiedFoot = GenerateBone(null, "UnifiedFoot", dynamical: false, massPercent: massPercentFoot);
                Bone unifiedLowerLeg = GenerateBone(unifiedFoot, "UnifiedLowerLeg", massPercent: massPercentLowerLeg);
                Bone unifiedUpperLeg = GenerateBone(unifiedLowerLeg, "UnifiedUpperLeg",hinge: true, massPercent: massPercentUpperLeg);
                hips = GenerateBone(unifiedUpperLeg, "Hips", eePos: true, massPercent: massPercentTorso * (1.0f / 6.0f));
                body.rootBone = unifiedFoot;
            } else {
                hips = GenerateBone(null, "Hips", dynamical: false, massPercent: massPercentTorso * (1.0f / 6.0f));
                body.rootBone = hips;
            }

            Bone spine = GenerateBone(hips, "Spine", removeIfNotInAvatar: true, massPercent: massPercentTorso * (2.0f / 6.0f));
            Bone chest = GenerateBone(spine, "Chest", removeIfNotInAvatar: true, massPercent: massPercentTorso * (2.0f / 6.0f));
            Bone upperChest = GenerateBone(chest, "UpperChest", removeIfNotInAvatar: true, massPercent: massPercentTorso * (0.7f / 6.0f));
            Bone neck = GenerateBone(upperChest, "Neck", removeIfNotInAvatar: true, massPercent: massPercentHead * (1.0f / 5.0f));
            Bone head = GenerateBone(neck, "Head", eeOri: true, massPercent: massPercentHead * (4.0f / 5.0f));

            Bone leftShoulder = GenerateBone(upperChest, "LeftShoulder", removeIfNotInAvatar: true, massPercent: massPercentTorso * (0.15f / 6.0f));
            Bone leftUpperArm = GenerateBone(leftShoulder, "LeftUpperArm", massPercent: massPercentUpperArm);
            Bone leftLowerArm = GenerateBone(leftUpperArm, "LeftLowerArm", hinge: true, massPercent: massPercentLowerArm);

            Bone rightShoulder = GenerateBone(upperChest, "RightShoulder", removeIfNotInAvatar: true, massPercent: massPercentTorso * (0.15f / 6.0f));
            Bone rightUpperArm = GenerateBone(rightShoulder, "RightUpperArm", massPercent: massPercentUpperArm);
            Bone rightLowerArm = GenerateBone(rightUpperArm, "RightLowerArm", hinge: true, massPercent: massPercentLowerArm);

            Bone leftUpperLeg = GenerateBone(hips, "LeftUpperLeg", massPercent: massPercentUpperLeg);
            Bone leftLowerLeg = GenerateBone(leftUpperLeg, "LeftLowerLeg", hinge:true, massPercent: massPercentLowerLeg);
            Bone leftFoot = GenerateBone(leftLowerLeg, "LeftFoot", eePos: true, massPercent: massPercentFoot * (2.0f / 3.0f));
            Bone leftToes = GenerateBone(leftFoot, "LeftToes", removeIfNotInAvatar: true, massPercent: massPercentFoot * (1.0f / 3.0f));

            Bone rightUpperLeg = GenerateBone(hips, "RightUpperLeg", massPercent: massPercentUpperLeg);
            Bone rightLowerLeg = GenerateBone(rightUpperLeg, "RightLowerLeg", hinge:true, massPercent: massPercentLowerLeg);
            Bone rightFoot = GenerateBone(rightLowerLeg, "RightFoot", eePos: true, massPercent: massPercentFoot * (2.0f / 3.0f));
            Bone rightToes = GenerateBone(rightFoot, "RightToes", removeIfNotInAvatar: true, massPercent: massPercentFoot * (1.0f / 3.0f));

            if (generateFingers) {
                // <TBD>
            } else {
                Bone leftHand = GenerateBone(leftLowerArm, "LeftHand", eePos: true, massPercent: massPercentHand);
                Bone rightHand = GenerateBone(rightLowerArm, "RightHand", eePos: true, massPercent: massPercentHand);
            }

            // ----- ----- ----- ----- -----

            body.rootBone.gameObject.transform.parent = obj.transform;

            hips.syncPosition = true;

            // ----- ----- ----- ----- -----
            // IK Pullback Target

            /*
            ((PHIKHingeActuatorBehaviour)(body["UnifiedUpperLeg"].ikActuator)).desc.pullbackTarget = Mathf.Deg2Rad * -60.0f;
            ((PHIKHingeActuatorBehaviour)(body["LeftLowerLeg"].ikActuator)).desc.pullbackTarget = Mathf.Deg2Rad * 60.0f;
            ((PHIKHingeActuatorBehaviour)(body["RightLowerLeg"].ikActuator)).desc.pullbackTarget = Mathf.Deg2Rad * 60.0f;

            ((PHIKHingeActuatorBehaviour)(body["LeftLowerArm"].ikActuator)).desc.pullbackTarget = Mathf.Deg2Rad * 100.0f;
            ((PHIKHingeActuatorBehaviour)(body["RightLowerArm"].ikActuator)).desc.pullbackTarget = Mathf.Deg2Rad * 100.0f;
            */

            ((PHIKBallActuatorBehaviour)(body["UnifiedUpperLeg"].ikActuator)).desc.pullbackTarget = Quaterniond.Rot(new Vec3d(0, 0, Mathf.Deg2Rad * -60.0f));
            ((PHIKBallActuatorBehaviour)(body["LeftLowerLeg"].ikActuator)).desc.pullbackTarget = Quaterniond.Rot(new Vec3d(0, 0, Mathf.Deg2Rad * 60.0f));
            ((PHIKBallActuatorBehaviour)(body["RightLowerLeg"].ikActuator)).desc.pullbackTarget = Quaterniond.Rot(new Vec3d(0, 0, Mathf.Deg2Rad * 60.0f));

            ((PHIKBallActuatorBehaviour)(body["LeftLowerArm"].ikActuator)).desc.pullbackTarget = Quaterniond.Rot(new Vec3d(0, 0, Mathf.Deg2Rad * 100.0f)); 
            ((PHIKBallActuatorBehaviour)(body["RightLowerArm"].ikActuator)).desc.pullbackTarget = Quaterniond.Rot(new Vec3d(0, 0, Mathf.Deg2Rad * 100.0f));

            ((PHIKBallActuatorBehaviour)(body["LeftUpperArm"].ikActuator)).desc.pullbackTarget = Quaterniond.Rot(new Vec3d(Mathf.Deg2Rad * -70.0f, 0, 0));
            ((PHIKBallActuatorBehaviour)(body["RightUpperArm"].ikActuator)).desc.pullbackTarget = Quaterniond.Rot(new Vec3d(Mathf.Deg2Rad * -70.0f, 0, 0));

            // ----- ----- ----- ----- -----
            // IK Pullback Rate

            ((PHIKBallActuatorBehaviour)(body["LeftShoulder"].ikActuator)).desc.pullbackRate = 1.0f;
            ((PHIKBallActuatorBehaviour)(body["RightShoulder"].ikActuator)).desc.pullbackRate = 1.0f;

            ((PHIKBallActuatorBehaviour)(body["LeftUpperArm"].ikActuator)).desc.pullbackRate = 0.1f;
            ((PHIKBallActuatorBehaviour)(body["RightUpperArm"].ikActuator)).desc.pullbackRate = 0.1f;

            /*
            ((PHIKHingeActuatorBehaviour)(body["LeftLowerArm"].ikActuator)).desc.pullbackRate = 0.1f;
            ((PHIKHingeActuatorBehaviour)(body["RightLowerArm"].ikActuator)).desc.pullbackRate = 0.1f;
            */
            ((PHIKBallActuatorBehaviour)(body["LeftLowerArm"].ikActuator)).desc.pullbackRate = 0.1f;
            ((PHIKBallActuatorBehaviour)(body["RightLowerArm"].ikActuator)).desc.pullbackRate = 0.1f;

            // ----- ----- ----- ----- -----
            // Sample Bone Position & Rotation

            // -- Rot (need to be set before position)
            body["LeftLowerArm"].transform.rotation = Quaternion.Euler(-90, 0, 0);
            body["RightLowerArm"].transform.rotation = Quaternion.Euler(+90, 0, 0);
            body["LeftLowerLeg"].transform.rotation = Quaternion.Euler(0, 90, 0);
            body["RightLowerLeg"].transform.rotation = Quaternion.Euler(0, 90, 0);
            body["UnifiedUpperLeg"].transform.rotation = Quaternion.Euler(0, 90, 0);

            // -- Pos
            body["UnifiedFoot"].transform.position = new Vector3(0.0002727155f, 0.1237467f, -0.02814926f);
            body["UnifiedLowerLeg"].transform.position = new Vector3(0.0002727155f, 0.1237467f, -0.02814926f);
            body["UnifiedUpperLeg"].transform.position = new Vector3(0.0002731606f, 0.456106f, -0.004115552f);
            body["Hips"].transform.position = new Vector3(0.0002732715f, 0.7874568f, 0.007158294f);
            body["Spine"].transform.position = new Vector3(0.0002732649f, 0.832569f, 0.0155344f);
            body["Chest"].transform.position = new Vector3(0.00027325f, 0.9368055f, 0.026839f);
            body["UpperChest"].transform.position = new Vector3(0.0002732668f, 1.042611f, 0.01520747f);
            body["Neck"].transform.position = new Vector3(0.0002732789f, 1.140327f, -0.01326437f);
            body["Head"].transform.position = new Vector3(0.0001326706f, 1.20189f, -0.005329609f);
            body["LeftShoulder"].transform.position = new Vector3(-0.01881186f, 1.117009f, -0.008189231f);
            body["LeftUpperArm"].transform.position = new Vector3(-0.09238342f, 1.104332f, -0.003616303f);
            body["LeftLowerArm"].transform.position = new Vector3(-0.2784835f, 1.095667f, -0.002054989f);
            body["RightShoulder"].transform.position = new Vector3(0.01935838f, 1.117012f, -0.008189186f);
            body["RightUpperArm"].transform.position = new Vector3(0.0929298f, 1.104335f, -0.003616288f);
            body["RightLowerArm"].transform.position = new Vector3(0.279035f, 1.095684f, -0.002195418f);
            body["LeftUpperLeg"].transform.position = new Vector3(-0.0654783f, 0.7539356f, 0.001144975f);
            body["LeftLowerLeg"].transform.position = new Vector3(-0.04832686f, 0.4561059f, -0.004115582f);
            body["LeftFoot"].transform.position = new Vector3(-0.03882539f, 0.1237467f, -0.02814922f);
            body["LeftToes"].transform.position = new Vector3(-0.03941011f, 0.07023409f, 0.0614768f);
            body["RightUpperLeg"].transform.position = new Vector3(0.06602485f, 0.7539355f, 0.001144975f);
            body["RightLowerLeg"].transform.position = new Vector3(0.04887318f, 0.456106f, -0.004115522f);
            body["RightFoot"].transform.position = new Vector3(0.03937082f, 0.1237467f, -0.02814934f);
            body["RightToes"].transform.position = new Vector3(0.03995499f, 0.07023397f, 0.06147671f);
            body["LeftHand"].transform.position = new Vector3(-0.4576918f, 1.095276f, 0.01273805f);
            body["RightHand"].transform.position = new Vector3(0.4582374f, 1.095318f, 0.01264149f);

            // ----- ----- ----- ----- -----

            return hips.gameObject;
        }

        private Bone GenerateBone(Bone parent, string label, bool hinge=false, bool eePos=false, bool eeOri=false, bool dynamical = true, bool removeIfNotInAvatar = false, float massPercent = 10.0f) {
            hinge = false; // <!!>

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
            bone.removeIfNotInAvatar = removeIfNotInAvatar;

            // Solid
            bone.solid = obj.AddComponent<PHSolidBehaviour>();
            bone.solid.lateAwakeStart = true;
            if (!dynamical) {
                bone.solid.desc.dynamical = false;
            }
            double mass = bodyMass * massPercent * 0.01f;
            bone.solid.desc.mass = mass;
            bone.solid.desc.inertia = new Matrix3d(
                mass, 0, 0,
                0, mass, 0,
                0, 0, mass
            );

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
                    var ikAct = obj.AddComponent<PHIKHingeActuatorBehaviour>();
                    ikAct.desc.pullbackRate = 1.0f;
                    bone.ikActuator = ikAct;
                } else {
                    var ikAct = obj.AddComponent<PHIKBallActuatorBehaviour>();
                    ikAct.desc.pullbackRate = 1.0f;
                    bone.ikActuator = ikAct;
                }
                bone.ikActuator.lateAwakeStart = true;

                // IK Endeffector
                if (eePos || eeOri) {
                    bone.ikEndEffector = obj.AddComponent<PHIKEndEffectorBehaviour>();
                    bone.ikEndEffector.lateAwakeStart = true;
                    bone.ikEndEffector.desc.bEnabled = (eePos || eeOri);
                    bone.ikEndEffector.desc.bPosition = eePos;
                    bone.ikEndEffector.desc.bOrientation = eeOri;
                }
            }

            body.bones.Add(bone);

            return bone;
        }

    }

}