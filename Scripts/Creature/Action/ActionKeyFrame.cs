using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[System.Serializable]
public class ActionKeyFrame : ScriptableObject {
    // ScriptableObjectにするべきか
    // 利点：多分Inspectorに単独表示できる
    // 欠点：管理が面倒
    // 　　　もしかしたらStateからKeyframe情報が見れたほうがいいか

    //public ReachController reachController;
    // 動かすBone
    public HumanBodyBones bone;

    // 実行時の座標変換用
    // 親
    public enum CoordinateType {
        World,
        Bone
    }
    public CoordinateType coordinate = CoordinateType.Bone;
    public HumanBodyBones coordinateBaseBone;
    // ターゲット
    public Vector3 centerForTranslate;
    public Vector3 baseTargetPosition;
    public float effectRate;

    public Vector3 currentTargetPosition;

    public KeyFramePose pose;

    public float startDelay;
    public float duration = 0.5f;

    public Vector2 springDamper = new Vector2(1, 1);
    
    public void GenerateSubMovement(InteraWare.Body body) {
        // 目標位置の変換
        Vector3 position = body[coordinateBaseBone].transform.TransformPoint(pose.localPosition);
        Quaternion rotation = Quaternion.Euler(pose.localEulerRotation) * body[coordinateBaseBone].transform.rotation;
        //PosRot moveTo = new PosRot(pose.worldPosition, Quaternion.Euler(pose.worldEulerRotation));
        Pose moveTo = new Pose(position, rotation);

        // 駆動対象のBoneを探す
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