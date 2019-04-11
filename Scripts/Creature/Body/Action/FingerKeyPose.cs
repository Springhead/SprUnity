using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(FingerKeyPose))]
public class KeyPoseEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        FingerKeyPose keyPose = (FingerKeyPose)target;

        if (GUILayout.Button("Get GameObjects")) {
            keyPose.GetGameObjects();
        }

        if (GUILayout.Button("Use Current Pose")) {
            keyPose.InitializeByCurrentPose();
        }

        if (GUILayout.Button("Test Pose")) {
            keyPose.TakePose();
        }
    }
}
#endif

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Action/Create Finger KeyPose")]
#endif
public class FingerKeyPose : ScriptableObject {
    public bool mirror = false;
    public string namePrefix = "";
    public string[] fingerNames = { "Thumb", "Index", "Middle", "Ring", "Pinky" };
    public Quaternion[] rotations = new Quaternion[15];

    // ----- ----- ----- ----- -----

    private bool haveGameObjects = false;
    private GameObject[] objects = new GameObject[15];

    // ----- ----- ----- ----- -----

    public void InitializeByCurrentPose() {
        for (int n = 0; n < 5; n++) {
            var fingerName = fingerNames[n];
            for (int i = 0; i < 3; i++) {
                var obj = GameObject.Find(namePrefix + fingerName + (i + 1));
                rotations[n * 3 + i] = obj.transform.localRotation;
            }
        }
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
#endif
    }

    public void GetGameObjects() {
        for (int n = 0; n < 5; n++) {
            var fingerName = fingerNames[n];
            for (int i = 0; i < 3; i++) {
                objects[n * 3 + i] = GameObject.Find(namePrefix + fingerName + (i + 1));
            }
        }
    }

    public void TakePose(FingerKeyPose basePose = null, float ratio = 1.0f) {
        if (!haveGameObjects) { GetGameObjects(); }

        for (int i = 0; i < rotations.Length; i++) {
            var rotation = rotations[i];
            if (basePose != null) {
                rotation = Quaternion.Slerp(basePose.rotations[i], rotation, ratio);
            }
            objects[i].transform.localRotation = rotation;
        }
    }
}
