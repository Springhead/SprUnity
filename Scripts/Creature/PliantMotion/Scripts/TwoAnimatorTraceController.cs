using System.Collections.Generic;
using UnityEngine;
using SprUnity;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
[CustomEditor(typeof(TwoAnimatorTraceController))]
public class TwoAnimatorTraceControllerEditor : Editor {
    private bool skinOn = false;
    public override void OnInspectorGUI() {
        TwoAnimatorTraceController trace = (TwoAnimatorTraceController)target;
        base.OnInspectorGUI();
        if (GUILayout.Button("Target Mesh OnOFF")) {
            if (trace.animator != null) {
                var skinnedMeshs = trace.animator.GetComponentsInChildren<SkinnedMeshRenderer>();
                foreach (var skinnedMesh in skinnedMeshs) {
                    skinnedMesh.enabled = !skinOn;
                    skinOn = !skinOn;
                }
            }
        }
    }
}
#endif

// AnimatorがInput用とOutput用の二つあることが前提
[DefaultExecutionOrder(0)]
public class TwoAnimatorTraceController : TraceController {
    public Animator animator;
    protected new void Start() {
        base.Start();
        if (animator.updateMode == AnimatorUpdateMode.AnimatePhysics) {
            phSceneBehaviour.AddFixedUpadateCallback(UpdateTraceJointStates,
                PHSceneBehaviour.CallbackPriority.BeforeStep, 0);
        }
        phSceneBehaviour.AddFixedUpadateCallback(UpdateTargVelPos,
            PHSceneBehaviour.CallbackPriority.BeforeStep, 1);
    }
    protected override void GetPairs() {
        if (animator == null) {
            Debug.LogError("animatorがnull");
            return;
        }

        body = GetComponent<Body>();
        if (body == null) {
            Debug.LogError("TraceController.csをアタッチするオブジェクトにBodyをアタッチしてください");
            return;
        }

        Dictionary<string, HumanBodyBones> labelToBoneId = new Dictionary<string, HumanBodyBones>();
        for (int i = 0; i < (int)HumanBodyBones.LastBone; i++) {
            labelToBoneId[((HumanBodyBones)i).ToString()] = (HumanBodyBones)i;
        }
        tracePairs = new List<TracePair>();
        foreach (var bone in body.bones) {
            TracePair pair = new TracePair(bone);
            if (!labelToBoneId.ContainsKey(bone.label)) {
                //Debug.Log(pair.label + "がTrace用アバターにない");
                // BaseのBone
                if (bone.parent == null) {
                    pair.srcAvatarBone = animator.gameObject;
                    tracePairs.Add(pair);
                }
                continue;
            }
            var avatarBone = animator.GetBoneTransform(labelToBoneId[bone.label]);
            pair.srcAvatarBone = avatarBone.gameObject;
            tracePairs.Add(pair);
        }
    }
}
