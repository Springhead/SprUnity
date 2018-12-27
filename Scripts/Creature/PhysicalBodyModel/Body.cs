using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

using SprCs;
using SprUnity;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace InteraWare {

#if UNITY_EDITOR
    [CustomEditor(typeof(Body))]
    public class BodyEditor : Editor {
        public override void OnInspectorGUI() {
            Body body = (Body)target;

            // ----- ----- ----- ----- -----

            DrawDefaultInspector();

            // ----- ----- ----- ----- -----

            if (GUILayout.Button("Setup From Animator")) {
                body.SetupFromAnimator();
            }
        }
    }
#endif

    public class Body : MonoBehaviour {

        public Animator animator = null;

        // ----- ----- ----- ----- -----

        [System.Serializable]
        public class StringBonePair {
            public string label;
            public Bone bone;
            public GameObject avatarBone;
            public StringBonePair(string l, Bone b) { label = l; bone = b; }

            [HideInInspector]
            public Quaternion solidAvatarRelRot = Quaternion.identity;
        }

        [SerializeField]
        public List<StringBonePair> bones = new List<StringBonePair>();

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        void Start() {
            // Record Relative Pose
            foreach (var pair in bones) {
                if (pair.avatarBone != null && pair.bone != null && pair.bone.solid != null) {
                    var so = pair.bone.solid.transform.rotation;
                    var av = pair.avatarBone.transform.rotation;
                    // pair.solidAvatarRelRot = av * Quaternion.Inverse(so);
                    pair.solidAvatarRelRot = Quaternion.Inverse(so) * av;
                }
            }
        }

        void FixedUpdate() {
            // Apply Body Pose to Avatar
            foreach (var pair in bones) {
                if (pair.avatarBone != null && pair.bone != null && pair.bone.solid != null) {
                    if (pair.label == "Hips" || pair.label.Contains("Leg") || pair.label.Contains("Foot")) {
                        pair.avatarBone.transform.position = pair.bone.solid.transform.position;
                    }
                    pair.avatarBone.transform.rotation = pair.bone.solid.transform.rotation * pair.solidAvatarRelRot;
                }
            }
        }

        // ----- ----- ----- ----- -----

        public Bone this[string key] {
            set {
                bool found = false;
                foreach (var item in bones) { if (item.label == key) { item.bone = value; found = true; } }
                if (!found) { bones.Add(new StringBonePair(key, value)); }
            }
            get {
                foreach (var item in bones) { if (item.label == key) { return item.bone; } }
                return null;
            }
        }

        public Bone this[HumanBodyBones key] {
            set { this[key.ToString()] = value; }
            get { return this[key.ToString()]; }
        }

        // ----- ----- ----- ----- -----

        public void SetupFromAnimator() {
            // ・標準的なボディの構成をプレハブ化しておく（アンカーの設定とかはどのプレハブを使うかで選べばよさそう）
            // ・ユーザはまずプレハブを実体化し、avatarに目的のAvatarをセット
            // ・Avatarの構造と合わない部分を自動調整（存在しない関節を削除して繋げたりとか）
            // ・Avatarに対して位置合わせ
            // ・Avatarとプレハブの姿勢差によってTransformCopyを設定韻具（PHSolidBehaviourに組み込んでも良いかも？）

            HumanBodyBones[] boneIds = {
                HumanBodyBones.Hips,
                HumanBodyBones.LeftUpperLeg,
                HumanBodyBones.RightUpperLeg,
                HumanBodyBones.LeftLowerLeg,
                HumanBodyBones.RightLowerLeg,
                HumanBodyBones.LeftFoot,
                HumanBodyBones.RightFoot,
                HumanBodyBones.Spine,
                HumanBodyBones.Chest,
                HumanBodyBones.UpperChest,
                HumanBodyBones.Neck,
                HumanBodyBones.Head,
                HumanBodyBones.LeftShoulder,
                HumanBodyBones.RightShoulder,
                HumanBodyBones.LeftUpperArm,
                HumanBodyBones.RightUpperArm,
                HumanBodyBones.LeftLowerArm,
                HumanBodyBones.RightLowerArm,
                HumanBodyBones.LeftHand,
                HumanBodyBones.RightHand,
                HumanBodyBones.LeftToes,
                HumanBodyBones.RightToes,
                HumanBodyBones.LeftEye,
                HumanBodyBones.RightEye,
            };

            // Find Avatar Bones
            foreach (var boneId in boneIds) {
                var trn = animator.GetBoneTransform(boneId);
                if (trn != null) {
                    var pair = bones.Find(p => p.label == boneId.ToString());
                    if (pair != null) {
                        pair.avatarBone = trn.gameObject;
                    } else {

                    }
                }
            }

            // Auto Set Position
            foreach (var boneId in boneIds) {
                var trn = animator.GetBoneTransform(boneId);
                if (trn != null) {
                    var bone = this[boneId.ToString()];
                    if (bone != null) {
                        bone.transform.position = trn.position;
                        bone.transform.rotation = Quaternion.identity;
                    }
                }
            }

            // Auto Adjust CoM
            foreach (var pair in bones) {
                var bone = pair.bone;
                if (bone != null && bone.children.Count > 0) {
                    Vector3 CoM = bone.transform.position; float cnt = 1.0f;
                    foreach (var child in bone.children) { CoM += child.transform.position; cnt += 1.0f; }
                    CoM /= cnt;

                    var CoMLocal = bone.transform.ToPosed().Inv() * CoM.ToVec3d();
                    bone.solid.desc.center = CoMLocal;
                }
            }
        }

        // Boneを付与する。新しいPrefabを作るときの補助用で、普通は使わない
        void SetupBones() {
            bones.Clear();
            SetupBoneRecursive(GameObject.Find("Hips")); // 本当はこのgameObjectの下にあるHipsを探すべき
        }
        void SetupBoneRecursive(GameObject obj) {
            if (obj.GetComponent<PHSolidBehaviour>() != null) {
                // Create Bone
                var bone = obj.GetComponent<Bone>();
                if (bone == null) {
                    bone = obj.AddComponent<Bone>();
                }
                bone.children.Clear();

                // Label
                bone.label = obj.name;

                // Settings
                if (obj.GetComponent<PHHingeJointBehaviour>() != null) {
                    bone.jointType = Bone.JointType.Hinge;
                } else if (obj.GetComponent<PHBallJointBehaviour>() != null) {
                    bone.jointType = Bone.JointType.Ball;
                }

                // Relationship
                bone.body = this;
                bone.parent = bone.transform.parent.GetComponent<Bone>();
                if (bone.parent != null) {
                    bone.parent.children.Add(bone);
                }

                // Springhead Objects
                bone.solid = obj.GetComponent<PHSolidBehaviour>();
                bone.shape = obj.GetComponent<CDShapeBehaviour>();

                bone.joint = null;
                if (bone.joint == null) { bone.joint = obj.GetComponent<PHBallJointBehaviour>(); }
                if (bone.joint == null) { bone.joint = obj.GetComponent<PHHingeJointBehaviour>(); }

                bone.ikEndEffector = obj.GetComponent<PHIKEndEffectorBehaviour>();
                bone.ikActuator = obj.GetComponent<PHIKActuatorBehaviour>();

                // Add To List
                this.bones.Add(new StringBonePair(bone.label, bone));
            }

            // Do Recursive
            for (int i = 0; i < obj.transform.childCount; i++) {
                SetupBoneRecursive(obj.transform.GetChild(i).gameObject);
            }
        }

    }

}