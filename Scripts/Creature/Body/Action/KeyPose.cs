using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using SprCs;
using SprUnity;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(KeyPose))]
public class KeyPoseEditor : Editor {

    void OnEnable() {
        SceneView.onSceneGUIDelegate += OnSceneGUI;
    }

    void OnDisable() {
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
    }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        KeyPose keyPose = (KeyPose)target;

        if (GUILayout.Button("Test")) {
            keyPose.Action();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Use Current Pose")) {
            keyPose.InitializeByCurrentPose();
        }
    }

    public void OnSceneGUI(SceneView sceneView) {
        KeyPose keyPose = (KeyPose)target;

        foreach (var boneKeyPose in keyPose.boneKeyPoses) {
            if (boneKeyPose.usePosition) {
                EditorGUI.BeginChangeCheck();
                Vector3 position = Handles.PositionHandle(boneKeyPose.position, Quaternion.identity);
                if (EditorGUI.EndChangeCheck()) {
                    Undo.RecordObject(target, "Change KeyPose Target Position");
                    boneKeyPose.position = position;
                    EditorUtility.SetDirty(target);
                }
            }

            if (boneKeyPose.useRotation) {
                EditorGUI.BeginChangeCheck();
                Quaternion rotation = Handles.RotationHandle(boneKeyPose.rotation, boneKeyPose.position);
                if (EditorGUI.EndChangeCheck()) {
                    Undo.RecordObject(target, "Change KeyPose Target Rotation");
                    boneKeyPose.rotation = rotation;
                    EditorUtility.SetDirty(target);
                }
            }
        }
    }
}
#endif

[Serializable]
public class BoneKeyPose {
    public HumanBodyBones boneId = HumanBodyBones.Hips;
    public string boneIdString = "";
    public Vector3 position = new Vector3();
    public Quaternion rotation = new Quaternion();
    public bool usePosition = true;
    public bool useRotation = true;
    public float lookAtRatio = 0;
}

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Action/Create KeyPose")]
#endif
public class KeyPose : ScriptableObject {
    public List<BoneKeyPose> boneKeyPoses = new List<BoneKeyPose>();

    public float testDuration = 1.0f;
    public float testSpring = 1.0f;
    public float testDamper = 1.0f;

    public void InitializeByCurrentPose(Body body = null) {
        if (body == null) { body = GameObject.FindObjectOfType<Body>(); }
        if (body != null) {
            boneKeyPoses.Clear();
            foreach (var bone in body.bones) {
                if (bone.ikEndEffector != null && bone.controller != null && bone.controller.enabled) {
                    BoneKeyPose boneKeyPose = new BoneKeyPose();
                    boneKeyPose.position = bone.transform.position;
                    boneKeyPose.rotation = bone.transform.rotation;

                    for (int i = 0; i < (int)HumanBodyBones.LastBone; i++) {
                        if (((HumanBodyBones)i).ToString() == bone.label) {
                            boneKeyPose.boneId = (HumanBodyBones)i;
                        }
                    }

                    if (bone.ikEndEffector.phIKEndEffector != null) {

                        if (bone.ikEndEffector.phIKEndEffector.IsPositionControlEnabled()) {
                            boneKeyPose.position = bone.ikEndEffector.phIKEndEffector.GetTargetPosition().ToVector3();
                            boneKeyPose.usePosition = true;
                        } else {
                            boneKeyPose.usePosition = false;
                        }

                        if (bone.ikEndEffector.phIKEndEffector.IsOrientationControlEnabled()) {
                            boneKeyPose.rotation = bone.ikEndEffector.phIKEndEffector.GetTargetOrientation().ToQuaternion();
                            boneKeyPose.useRotation = true;
                        } else {
                            boneKeyPose.useRotation = false;
                        }

                    } else {

                        if (bone.ikEndEffector.desc.bPosition) {
                            boneKeyPose.position = ((Vec3d)(bone.ikEndEffector.desc.targetPosition)).ToVector3();
                            boneKeyPose.usePosition = true;
                        } else {
                            boneKeyPose.usePosition = false;
                        }

                        if (bone.ikEndEffector.desc.bOrientation) {
                            boneKeyPose.rotation = ((Quaterniond)(bone.ikEndEffector.desc.targetOrientation)).ToQuaternion();
                            boneKeyPose.useRotation = true;
                        } else {
                            boneKeyPose.useRotation = false;
                        }

                    }

                    boneKeyPoses.Add(boneKeyPose);
                }
            }

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
#endif
        }
    }

    public void Action(Body body = null, float duration = -1, float startTime = -1, float spring = -1, float damper = -1, Quaternion? rotate = null) {
        if (!rotate.HasValue) { rotate = Quaternion.identity; }
        
        if (duration < 0) { duration = testDuration; }
        if (startTime < 0) { startTime = 0; }
        if (spring < 0) { spring = testSpring; }
        if (damper < 0) { damper = testDamper; }

        if (body == null) { body = GameObject.FindObjectOfType<Body>(); }
        if (body != null) {
            foreach (var boneKeyPose in boneKeyPoses) {
                if (boneKeyPose.usePosition || boneKeyPose.useRotation) {
                    Bone bone = (boneKeyPose.boneIdString != "") ? body[boneKeyPose.boneIdString] : body[boneKeyPose.boneId];
                    Quaternion ratioRotate = Quaternion.Slerp(Quaternion.identity, (Quaternion)rotate, boneKeyPose.lookAtRatio);
                    var pose = new Pose(ratioRotate * boneKeyPose.position, ratioRotate * boneKeyPose.rotation);
                    var springDamper = new Vector2(spring, damper);
                    bone.controller.AddSubMovement(pose, springDamper, startTime + duration, duration, usePos: boneKeyPose.usePosition, useRot: boneKeyPose.useRotation);
                }
            }
        }
    }
}
