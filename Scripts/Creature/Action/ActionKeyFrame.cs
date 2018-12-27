using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[System.Serializable]
public class ActionKeyFrame{

    //public ReachController reachController;
    public HumanBodyBones bone;
    //public PHIKEndEffectorBehaviour ikEndEffector;
    //public Bone bone;

    //public GameObject poseObject;
    //public Transform pose;
    public KeyFramePose pose;
    public Vector3 position;
    public Vector3 rotation;
    //public GameObject origin;

    public float startDelay;
    public float duration;

    public Vector2 springDamper = new Vector2(1, 1);
    
    public void generateSubMovement(InteraWare.Body body) {
        // 目標位置の変換
        PosRot moveTo = new PosRot(pose.localPosition, Quaternion.Euler(pose.localEulerRotation));

        // 駆動対象のBoneを探す
        // 4階層も上からBodyを受け取るのはどうかと思う
        PHIKEndEffectorBehaviour ikEndEffector = body[bone].ikEndEffector;
        if (ikEndEffector == null) return;
        ReachController reachController = ikEndEffector.iktarget.GetComponent<ReachController>();
        if (reachController == null) return;

        reachController.AddSubMovement(
            moveTo,
            springDamper,
            startDelay + duration,
            duration
            );
    }
}

#endif