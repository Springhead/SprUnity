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

            if (GUILayout.Button("Fit To Avatar")) {
                body.FitToAvatar();
            }
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

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
        // MonoBehaviour Functions

        void Start() {
            // Record Relative Pose between PHSolid and Avatar
            foreach (var bone in bones) {
                bone.RecordRelativeRotSolidAvatar();
            }
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

            // Fit Bones to Avatar
            foreach (var bone in bones) {
                if (bone.avatarBone != null) {
                    // -- Fit Bone Position
                    bone.transform.position = bone.avatarBone.transform.position;
                    bone.transform.rotation = Quaternion.identity;

                    // -- Fit Center of Mass Position
                    if (bone.children.Count > 0) {
                        Vector3 CoM = bone.transform.position; float cnt = 1.0f;
                        foreach (var child in bone.children) { CoM += child.transform.position; cnt += 1.0f; }
                        CoM /= cnt;

                        var CoMLocal = bone.transform.ToPosed().Inv() * CoM.ToVec3d();
                        bone.solid.desc.center = CoMLocal;
                        bone.solid.OnValidate();
                    }

                    // -- Fit Collision Shape Length

                    // <TBD>
                }
            }
        }

        // ----- ----- ----- ----- -----
        // Private Functions

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
