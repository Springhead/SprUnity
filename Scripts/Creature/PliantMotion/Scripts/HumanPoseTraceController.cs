using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SprUnity;

// 一つのAnimatorがInputとOutputを兼ねる
[DefaultExecutionOrder(0)]
public class HumanPoseTraceController : TraceController {
    private Animator animator;
    public HumanPose humanPose;
    private HumanPoseHandler humanPoseHandler;
    new void Start() {
        base.Start();

        humanPoseHandler = new HumanPoseHandler(animator.avatar, animator.transform);
        humanPoseHandler.GetHumanPose(ref humanPose);
    }
    void FixedUpdate() {
        humanPoseHandler.SetHumanPose(ref humanPose);
        UpdateTraceJointStates();
        UpdateTargVelPos();
    }
    protected override void GetPairs() {
        body = GetComponent<Body>();
        if (body == null) {
            Debug.LogError("TraceController.csをアタッチするオブジェクトにBodyをアタッチしてください");
            return;
        }

        animator = body.animator;
        if (animator == null) {
            Debug.Log("BodyのAnimatorがnull");
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
