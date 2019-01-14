using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

using SprCs;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SprUnity {

#if UNITY_EDITOR
    [CustomEditor(typeof(Body))]
    public class BodyEditor : Editor {
        public bool showBoneList = true;

        public override void OnInspectorGUI() {
            Body body = (Body)target;

            // ----- ----- ----- ----- -----
            // Bone List
            showBoneList = EditorGUILayout.Foldout(showBoneList, "Bones");
            if (showBoneList) {
                foreach (var bone in body.bones) {
                    EditorGUILayout.LabelField(bone.label);
                    EditorGUILayout.ObjectField(bone, typeof(Bone), true);
                    bone.avatarBone = EditorGUILayout.ObjectField(bone.avatarBone, typeof(GameObject), true) as GameObject;
                }
            }

            EditorGUILayout.Space();

            // ----- ----- ----- ----- -----
            // Select Animator(with Avatar) and Fit to Avatar Button
            body.animator = EditorGUILayout.ObjectField(body.animator, typeof(Animator), true) as Animator;
            body.fitSpringDamper = EditorGUILayout.Toggle("Fit Spring Damper", body.fitSpringDamper);
            if (body.fitSpringDamper) {
                body.momentToSpringCoeff = EditorGUILayout.FloatField("Moment to Spring", body.momentToSpringCoeff);
                body.springToDamperCoeff = EditorGUILayout.FloatField("Spring to Damper", body.springToDamperCoeff);
                body.minSpring = EditorGUILayout.FloatField("Min Spring Value", body.minSpring);

                body.fitIKBiasOnFitSpring = EditorGUILayout.Toggle("Fit IK Bias", body.fitIKBiasOnFitSpring);
                if (body.fitIKBiasOnFitSpring) {
                    body.momentToSqrtBiasCoeff = EditorGUILayout.FloatField("Moment to Sqrt(Bias)", body.momentToSqrtBiasCoeff);
                }
            }

            if (GUILayout.Button("Fit To Avatar")) {
                body.FitToAvatar();
            }

            /*
            // For BodyGenerator
            if (GUILayout.Button("Print Bone Positions")) {
                string str = "";
                foreach (var bone in body.bones) {
                    str += "body[\"" + bone.label + "\"].transform.position = new Vector3(";
                    str += bone.transform.position.x + "f, ";
                    str += bone.transform.position.y + "f, ";
                    str += bone.transform.position.z + "f);";
                    str += "\r\n";
                }
                Debug.Log(str);
            }
            */
        }
    }
#endif

    public class Body : MonoBehaviour {

        // List of Bones
        public List<Bone> bones = new List<Bone>();

        // Root Bone
        public Bone rootBone = null;

        // Animator with humanoid avatar to be synchronized with this body
        public Animator animator = null;

        // Fit Target Flag
        public bool fitSpringDamper = false;
        public float momentToSpringCoeff = 500.0f;
        public float springToDamperCoeff = 0.1f;
        public float minSpring = 100.0f;

        public bool fitIKBiasOnFitSpring = false;
        public float momentToSqrtBiasCoeff = 100.0f;

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
        // MonoBehaviour Functions

        void Start() {
            // Record Relative Pose between PHSolid and Avatar
            RecordRelativeRotSolidAvatar();
        }

        void FixedUpdate() {
            // Synchronize Avatar Pose from PHSolid Poses
            foreach (var bone in bones) {
                bone.SyncAvatarBoneFromSolid();
            }
        }

        // ----- ----- ----- ----- -----
        // Public Functions

        public Bone this[string key] {
            get {
                foreach (var bone in bones) { if (bone.label == key) { return bone; } }
                return null;
            }
        }

        public Bone this[HumanBodyBones key] {
            get { return this[key.ToString()]; }
        }

        // Fit each bone positions to given humanoid avatar
        public void FitToAvatar() {
            // Find Animator if it is not set
            if (animator == null) {
                animator = GameObject.FindObjectOfType<Animator>();
            }

            // Make Table to convert Label String To HumanBodyBones
            Dictionary<string, HumanBodyBones> labelToBoneId = new Dictionary<string, HumanBodyBones>();
            for (int i = 0; i < (int)HumanBodyBones.LastBone; i++) {
                labelToBoneId[((HumanBodyBones)i).ToString()] = (HumanBodyBones)i;
            }

            // Find Corresponding Avatar Bone
            foreach (var bone in bones) {
                if (labelToBoneId.ContainsKey(bone.label)) {
                    var trn = animator.GetBoneTransform(labelToBoneId[bone.label]);
                    if (trn != null) {
                        bone.avatarBone = trn.gameObject;
                    }
                }
            }

            // Remove Missing Bone and Reconnect Bones
            RemoveMissingBoneRecursive(rootBone);

            // Fit Bone Position to Avatar
            foreach (var bone in bones) {
                if (bone.avatarBone != null) {
                    bone.transform.position = bone.avatarBone.transform.position;
                    bone.transform.rotation = Quaternion.identity;
                    if (bone.label == "LeftLowerArm") {
                        bone.transform.rotation = Quaternion.Euler(-90, 0, 0);
                    }
                    if (bone.label == "RightLowerArm") {
                        bone.transform.rotation = Quaternion.Euler(+90, 0, 0);
                    }
                    if (bone.label == "LeftLowerLeg") {
                        bone.transform.rotation = Quaternion.Euler(0, 90, 0);
                    }
                    if (bone.label == "RightLowerLeg") {
                        bone.transform.rotation = Quaternion.Euler(0, 90, 0);
                    }

                } else {
                    if (bone.label == "UnifiedUpperLeg") {
                        // Use Position of LowerLeg (not UpperLeg)
                        bone.transform.position = (this["LeftLowerLeg"].avatarBone.transform.position + this["RightLowerLeg"].avatarBone.transform.position) * 0.5f;
                        bone.transform.rotation = Quaternion.Euler(0, 90, 0);
                    }
                    if (bone.label == "UnifiedLowerLeg") {
                        // Use Position of Foot (not LowerLeg)
                        bone.transform.position = (this["LeftFoot"].avatarBone.transform.position + this["RightFoot"].avatarBone.transform.position) * 0.5f;
                        bone.transform.rotation = Quaternion.identity;
                    }
                    if (bone.label == "UnifiedFoot") {
                        bone.transform.position = (this["LeftFoot"].avatarBone.transform.position + this["RightFoot"].avatarBone.transform.position) * 0.5f;
                        bone.transform.rotation = Quaternion.identity;
                    }
                }
            }

            // Fit Center of Mass Position
            foreach (var bone in bones) {
                if (bone.avatarBone != null) {
                    Vector3 CoM = bone.transform.position;
                    if (bone.children.Count > 0) {
                        // Have Child
                        float cnt = 1.0f;
                        foreach (var child in bone.children) { CoM += child.transform.position; cnt += 1.0f; }
                        CoM /= cnt;

                    } else {
                        // No Child (=End Bone (Head, Hand, Foot))
                        if (bone.label == "Head") {
                            // Guess from Eye Position
                            var trnLEye = animator.GetBoneTransform(HumanBodyBones.LeftEye);
                            var trnREye = animator.GetBoneTransform(HumanBodyBones.RightEye);
                            if (trnLEye != null && trnREye != null) {
                                Vector3 eyeCenter = (trnLEye.position + trnREye.position) * 0.5f;
                                CoM = new Vector3(bone.transform.position.x, eyeCenter.y, bone.transform.position.z);
                            }
                        }

                        if (bone.label.Contains("Hand")) {
                            // Guess from LowerArm Length and Direction
                            Vector3 wristPos = bone.transform.position;
                            Vector3 elbowPos = bone.parent.transform.position;
                            CoM = elbowPos + (wristPos - elbowPos) * (1.0f + (1.0f / 4.0f));
                        }
                    }

                    // Set CoM to Solid
                    var CoMLocal = bone.transform.ToPosed().Inv() * CoM.ToVec3d();
                    bone.solid.desc.center = CoMLocal;
                    bone.solid.OnValidate();
                }
            }

            // Fit IK Target Position
            foreach (var bone in bones) {
                var phIKEEBehaviour = bone.GetComponent<PHIKEndEffectorBehaviour>();
                if (phIKEEBehaviour != null) {
                    phIKEEBehaviour.desc.targetLocalPosition = bone.solid.desc.center;
                    phIKEEBehaviour.desc.targetPosition = bone.transform.ToPosed() * bone.solid.desc.center;
                }
            }

            // -- Fit Collision Shape Length
            // <TBD>

            // Auto Set Spring and Damper
            if (fitSpringDamper) {
                // Initialize Moment Sum Table
                Dictionary<Bone, double> inertiaMomentSum = new Dictionary<Bone, double>();
                foreach (var bone in bones) {
                    inertiaMomentSum[bone] = 0.0f;
                }

                // Sum-up Inertia Moment for each Joint
                foreach (var bone in bones) {
                    Vector3 solidCenter = (bone.transform.ToPosed() * bone.solid.desc.center).ToVector3();
                    var b = bone;
                    while (b != null) {
                        Vector3 jointCenter = b.transform.position;
                        double distance = (solidCenter - jointCenter).magnitude;
                        inertiaMomentSum[b] += (distance * b.solid.desc.mass);
                        b = b.parent;
                    }
                }

                // Set Spring and Damper
                foreach (var bone in bones) {
                    float spring = Mathf.Max(minSpring, (float)(inertiaMomentSum[bone]) * momentToSpringCoeff);
                    float damper = spring * springToDamperCoeff;

                    PHHingeJointBehaviour hj = bone.joint as PHHingeJointBehaviour;
                    if (hj != null) {
                        hj.desc.spring = spring;
                        hj.desc.damper = damper;
                    }

                    PHBallJointBehaviour bj = bone.joint as PHBallJointBehaviour;
                    if (bj != null) {
                        bj.desc.spring = spring;
                        bj.desc.damper = damper;
                    }

                    // Also Fit IK Bias
                    if (fitIKBiasOnFitSpring) {
                        float sqrtBias = (float)(inertiaMomentSum[bone]) * momentToSqrtBiasCoeff;
                        float ikBias = 1.0f + Mathf.Pow(sqrtBias, 2);

                        // Special Rule
                        if (bone.label.Contains("Shoulder")) {
                            ikBias = 5000.0f;
                        } else if (bone.label.Contains("Spine")) {
                            ikBias = 5000.0f;
                        } else if (bone.label.Contains("Chest")) {
                            ikBias = 1000.0f;
                        } else if (bone.label.Contains("UpperArm")) {
                            ikBias = 10.0f;
                        } else if (bone.label.Contains("LowerArm")) {
                            ikBias = 1.0f;
                        } else if (bone.label.Contains("Hand")) {
                            ikBias = 1.0f;
                        } else {
                            ikBias = 10000.0f;
                        }

                        PHIKHingeActuatorBehaviour hik = bone.ikActuator as PHIKHingeActuatorBehaviour;
                        if (hik != null) {
                            hik.desc.bias = ikBias;
                        }

                        PHIKBallActuatorBehaviour bik = bone.ikActuator as PHIKBallActuatorBehaviour;
                        if (bik != null) {
                            bik.desc.bias = ikBias;
                        }
                    }
                }
            }

            // Re-initialize Relative Rotation Info
            RecordRelativeRotSolidAvatar();
        }

        // ----- ----- ----- ----- -----
        // Private Functions

        // Record Relative Pose between PHSolid and Avatar
        private void RecordRelativeRotSolidAvatar() {
            foreach (var bone in bones) {
                bone.RecordRelativeRotSolidAvatar();
            }
        }

        // Remove Missing Bone and Reconnect Bones
        private void RemoveMissingBoneRecursive(Bone bone) {
            List<Bone> childBones = new List<Bone>();
            foreach (var child in bone.children) {
                childBones.Add(child);
            }

            bool destroy = false;
            if (bone.parent != null && bone.removeIfNotInAvatar && bone.avatarBone == null) {
                foreach (var child in childBones) {
                    // Pass Child Bone to the Parent
                    bone.parent.children.Add(child);
                    child.parent = bone.parent;

                    // Reconnect GameObject Tree
                    child.transform.parent = bone.parent.transform;

                    // Reconnect Joint
                    if (child.joint != null) {
                        child.joint.socket = bone.parent.solid.gameObject;
                    }
                }

                // Conbine Mass
                bone.parent.solid.desc.mass += bone.solid.desc.mass;

                // Remove from body
                bones.Remove(bone);

                // Set Destroy Flag
                destroy = true;
            }

            // Do Recursively
            foreach (var child in childBones) {
                RemoveMissingBoneRecursive(child);
            }

            // Destroy
            if (destroy) {
                Destroy(bone.gameObject);
            }
        }

    }

}
