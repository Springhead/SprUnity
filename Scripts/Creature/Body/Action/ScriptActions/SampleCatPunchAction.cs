using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using SprUnity;

#if UNITY_EDITOR
[CustomEditor(typeof(SampleCatPunchAction))]
public class SampleCatPunchActionEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        SampleCatPunchAction action = (SampleCatPunchAction)target;
        if (GUILayout.Button("Start")) {
            action.Begin();
        }
        if (GUILayout.Button("Go")) {
            action.ResetAction();
        }
        if (GUILayout.Button("End")) {
            action.End();
        }
    }
}
#endif

public class SampleCatPunchAction : ScriptableAction {

    public KeyPoseData catPunch;
    public KeyPoseData catPunchBack;
    public KeyPoseTimePair go, punch, back;

    public GameObject target;

    public bool isRight;
    private HumanBodyBones useHand, unuseHand;

	// Use this for initialization
	void Start () {
        go.keyPose = ScriptableObject.Instantiate<KeyPoseData>(catPunch);
        punch.keyPose = ScriptableObject.Instantiate<KeyPoseData>(catPunch);
        back.keyPose = ScriptableObject.Instantiate<KeyPoseData>(catPunch);
        for (int i = 0; i < go.keyPose.boneKeyPoses.Count; i++) {
            go.keyPose.boneKeyPoses[i].usePosition = false;
            go.keyPose.boneKeyPoses[i].useRotation = false;
        }
        for (int i = 0; i < go.keyPose.boneKeyPoses.Count; i++) {
            punch.keyPose.boneKeyPoses[i].usePosition = false;
            punch.keyPose.boneKeyPoses[i].useRotation = false;
        }
        for (int i = 0; i < back.keyPose.boneKeyPoses.Count; i++) {
            back.keyPose.boneKeyPoses[i].usePosition = false;
            back.keyPose.boneKeyPoses[i].useRotation = false;
        }
        base.Start();
    }

    public override void GenerateMovement() {
        if (!go.isUsed) {
            if (isRight) {
                useHand = HumanBodyBones.RightHand;
                unuseHand = HumanBodyBones.LeftHand;
            } else {
                useHand = HumanBodyBones.LeftHand;
                unuseHand = HumanBodyBones.RightHand;
            }
            go.keyPose[useHand].position = target.transform.position + new Vector3(0, 0.2f, -0.05f);
            go.keyPose[useHand].rotation = catPunch[useHand].rotation;
            go.startTime = timer;
            go.keyPose[useHand].Enable(true);
            go.keyPose[unuseHand].Enable(false);
            generatedKeyPoses.Add(go);
        }
        if (!punch.isUsed) {
            punch.keyPose[useHand].position = target.transform.position + new Vector3(0, -0.1f, 0.05f);
            punch.keyPose[useHand].rotation = Quaternion.AngleAxis(30f, Vector3.right) * catPunch[useHand].rotation;
            punch.keyPose[useHand].Enable(true);
            punch.keyPose[unuseHand].Enable(false);
            generatedKeyPoses.Add(punch);
        }
        if (!back.isUsed) {
            back.keyPose[useHand].position = catPunchBack[useHand].position;
            back.keyPose[useHand].rotation = catPunchBack[useHand].rotation;
            back.keyPose[useHand].Enable(true);
            back.keyPose[unuseHand].Enable(false);
            generatedKeyPoses.Add(back);
        }
    }

    public override void ResetAction() {
        go.isUsed = false;
        punch.isUsed = false;
        back.isUsed = false;
    }
}
