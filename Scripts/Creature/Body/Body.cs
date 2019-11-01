﻿using System.Collections;
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
        public bool showBoneList = false;
        public bool showFitting = true;

        public override void OnInspectorGUI() {
            Body body = (Body)target;

            // ----- ----- ----- ----- -----
            // Bone List
            showBoneList = EditorGUILayout.Foldout(showBoneList, "Bones");
            if (showBoneList) {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Root Bone");
                body.rootBone = EditorGUILayout.ObjectField(body.rootBone, typeof(Bone), true) as Bone;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();

                int removeNumber = -1;
                for (int i = 0; i < body.bones.Count; i++) {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel((body.bones[i] == null) ? "(null)" : body.bones[i].label);
                    body.bones[i] = EditorGUILayout.ObjectField(body.bones[i], typeof(Bone), true) as Bone;
                    if (body.bones[i] != null) {
                        body.bones[i].avatarBone = EditorGUILayout.ObjectField(body.bones[i].avatarBone, typeof(GameObject), true) as GameObject;
                    }
                    if (GUILayout.Button("x")) { removeNumber = i; }
                    EditorGUILayout.EndHorizontal();
                }

                if (removeNumber >= 0) {
                    if (EditorUtility.DisplayDialog("Remove Bone From Body", "Remove Bone From Body ? ", "Ok", "No")) {
                        body.bones.RemoveAt(removeNumber);
                    }
                }

                if (GUILayout.Button("Add Bone")) { body.bones.Add(new Bone()); }
            }

            EditorGUILayout.Space();

            // ----- ----- ----- ----- -----
            // Select Animator(with Avatar) and Fit to Avatar Button
            showFitting = EditorGUILayout.Foldout(showFitting, "Fitting");
            if (showFitting) {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Avatar Animator");
                body.animator = EditorGUILayout.ObjectField(body.animator, typeof(Animator), true) as Animator;
                EditorGUILayout.EndHorizontal();
                body.fitSpringDamper = EditorGUILayout.Toggle("Fit Spring Damper", body.fitSpringDamper);
                if (body.fitSpringDamper) {
                    body.momentToSpringCoeff = EditorGUILayout.FloatField("Moment to Spring", body.momentToSpringCoeff);
                    body.dampingRatio = EditorGUILayout.FloatField("Damping Ratio", body.dampingRatio);
                    body.minSpring = EditorGUILayout.FloatField("Min Spring Value", body.minSpring);

                    body.fitIKBiasOnFitSpring = EditorGUILayout.Toggle("Fit IK Bias", body.fitIKBiasOnFitSpring);
                    if (body.fitIKBiasOnFitSpring) {
                        body.momentToSqrtBiasCoeff = EditorGUILayout.FloatField("Moment to Sqrt(Bias)", body.momentToSqrtBiasCoeff);
                    }
                }
                if (GUILayout.Button("Fit To Avatar")) { body.FitToAvatar(); }
                if (GUILayout.Button("SetBones")) { body.SetBonesFromRootBone(); }
            }

            EditorGUILayout.Space();

            // ----- ----- ----- ----- -----

            bool initializeOnStart = EditorGUILayout.Toggle("Initialize On Start", body.initializeOnStart);
            if (body.initializeOnStart != initializeOnStart) {
                // LateAwakeStartフラグも同時に切り替える
                body.initializeOnStart = initializeOnStart;
                body.SetLateAwakeStart(!initializeOnStart);
            }
            body.changeRoundConeOnStart = EditorGUILayout.Toggle("Change RoundCone On Start", body.changeRoundConeOnStart);

            body.syncEnabled = EditorGUILayout.Toggle("Synchronize", body.syncEnabled);
            body.syncMode = (Body.SyncMode)EditorGUILayout.EnumPopup("Sync Mode", body.syncMode);

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

        public class BodyBones : IEnumerable<Bone> {
            public class BodyBonesEnumerator : IEnumerator<Bone> {

                private 

                public Bone Current => throw new NotImplementedException();

                object IEnumerator.Current => throw new NotImplementedException();

                public void Dispose() {
                    throw new NotImplementedException();
                }

                public override bool MoveNext() {

                }

                public void Reset() {
                    throw new NotImplementedException();
                }
            }

            public override IEnumerator<Bone> GetEnumerator() {

            }

            public override IEnumerator GetEnumerator() {

            }
        }

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
        // Public Members

        // List of Bones
        // 常にrootBoneが最初で親から子供の順になるように作成する
        public List<Bone> bones = new List<Bone>();

        // Root Bone of Physical Model
        public Bone rootBone = null;

        // Animator with humanoid avatar to be synchronized with this body
        public Animator animator = null;

        public bool syncEnabled = true;
        public enum SyncMode {
            Solid,
            IK,
        }
        public SyncMode syncMode = SyncMode.Solid;

        // Fit Target Flag
        public bool fitSpringDamper = true;
        public float momentToSpringCoeff = 500.0f;
        public float dampingRatio = 1.0f;
        public float minSpring = 100.0f;

        public bool fitIKBiasOnFitSpring = true;
        public float momentToSqrtBiasCoeff = 100.0f;

        public bool initializeOnStart = true;
        public bool changeRoundConeOnStart = true;

        // Initialized Flag
        public bool initialized = false;

        // Body Parameter for KeyPoses
        public float height = 1.6f;

        // Controllers
        public LookController lookController = null;
        public BodyBalancer bodyBalancer = null;

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
        // Private Members



        // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
        // MonoBehaviour Functions

        void Start() {
            if (initializeOnStart) {
                Initialize();
            }
        }

        void FixedUpdate() {
            if (syncEnabled) {
                // Synchronize Avatar Pose from PHSolid Poses
                if (syncMode == SyncMode.Solid) {
                    foreach (var bone in bones) {
                        bone.SyncAvatarBoneFromSolid();
                    }
                } else if (syncMode == SyncMode.IK) {
                    foreach (var bone in bones) {
                        bone.SyncAvatarBoneFromIK();
                    }
                }
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

        // Set LateAwakeStart Flag for Every Bone Springhead Object
        public void SetLateAwakeStart(bool lateAwakeStart) {
            foreach (var bone in bones) {
                foreach (var spr in bone.gameObject.GetComponents<SprBehaviour>()) {
                    spr.lateAwakeStart = lateAwakeStart;
                }
            }
        }

        // Fit each bone positions to given humanoid avatar
        public void FitToAvatar() {
            // Find Animator if it is not set
            if (animator == null) {
                animator = GameObject.FindObjectOfType<Animator>();
            }
            if (animator == null) {
                Debug.LogWarning("No Animator Component was found");
                // Initialize Body Parameter for KeyPoses
                height = this[HumanBodyBones.Head].transform.position.y - this[HumanBodyBones.LeftFoot].transform.position.y;
                return;
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
                    } else {
                        var go = FindChildObjectByNameRecursive(bone.label, animator.gameObject);
                        if (go != null) {
                            bone.avatarBone = go;
                        }
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
                if (bone.solid != null && bone.avatarBone != null) {
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
            if (changeRoundConeOnStart) {
                foreach (var bone in bones) {
                    if (bone.shape != null) {
                        var shapeObj = bone.shape.shapeObject;
                        if (shapeObj == null) { shapeObj = bone.shape.gameObject; }

                        var meshRoundCone = shapeObj.GetComponent<MeshRoundCone>();
                        if (meshRoundCone != null) {
                            meshRoundCone.pivot = MeshRoundCone.Pivot.R1;
                            meshRoundCone.positionR1 = bone.transform.position;

                            if (bone.children.Count > 0) {
                                Vector3 averagePos = new Vector3();
                                foreach (var child in bone.children) {
                                    averagePos += child.transform.position;
                                }
                                averagePos /= bone.children.Count;
                                meshRoundCone.positionR2 = averagePos;

                            } else {
                                meshRoundCone.positionR2 = bone.transform.position;
                            }

                            meshRoundCone.Reposition();
                            meshRoundCone.Reshape();
                        }
                    }
                }
            }

            // Auto Set Spring and Damper
            if (fitSpringDamper) {
                // Sum-up Inertia Moment for each Joint
                Dictionary<Bone, double> inertiaMomentSum = new Dictionary<Bone, double>();
                foreach (var bone in bones) {
                    inertiaMomentSum[bone] = CompInertiaMomentSumRecursive(bone);
                }

                // Set Spring and Damper
                foreach (var bone in bones) {
                    float spring = Mathf.Max(minSpring, (float)(inertiaMomentSum[bone]) * momentToSpringCoeff);
                    float damper = 2 * Mathf.Sqrt(spring * (float)(inertiaMomentSum[bone])) * dampingRatio;

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

            // Re-initialize Each Bone
            RecordRelativeRotSolidAvatar();

            // Initialize Body Parameter for KeyPoses
            height = this[HumanBodyBones.Head].transform.position.y - this[HumanBodyBones.LeftFoot].transform.position.y;
        }

        // Initialize Body : Must be called after fitting
        public void Initialize() {
            foreach (var bone in bones) {
                bone.SaveInitialSpringDamper();
                bone.InitializeController();
            }
            initialized = true;
        }

        // 

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

        // Find Child Object by Name
        private GameObject FindChildObjectByNameRecursive(string name, GameObject root) {
            if (root.name == name) { return root; }
            GameObject result = null;
            for (int i = 0; i < root.transform.childCount; i++) {
                result = FindChildObjectByNameRecursive(name, root.transform.GetChild(i).gameObject);
                if (result != null) { return result; }
            }
            return null;
        }

        private double CompInertiaMomentSumRecursive(Bone bone, Bone centerBone = null) {
            double inertiaMomentSum = 0;
            if (centerBone == null) { centerBone = bone; }
            if (bone.solid != null) {
                Vector3 solidCenter = (bone.transform.ToPosed() * bone.solid.desc.center).ToVector3();
                Vector3 jointCenter = centerBone.transform.position;
                double distance = (solidCenter - jointCenter).magnitude;
                inertiaMomentSum += (distance * distance * bone.solid.desc.mass);
            }
            foreach (var child in bone.children) {
                inertiaMomentSum += CompInertiaMomentSumRecursive(child, centerBone);
            }
            return inertiaMomentSum;
        }
        
        // bonesをセットする
        public void SetBonesFromRootBone() {
            bones.Clear();
            bones.Add(rootBone);
            SetBonesFromRootBoneRecursive(rootBone);
        }
        // 深さ順にbonesに追加
        private void SetBonesFromRootBoneRecursive(Bone parentBone) {
            foreach (var childBone in parentBone.children) {
                if (!bones.Contains(childBone)) {
                    bones.Add(childBone);
                    SetBonesFromRootBoneRecursive(childBone);
                }
            }
        }
    }

}