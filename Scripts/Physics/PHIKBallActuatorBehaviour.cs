using System.Collections;
using System;
using SprCs;
using SprUnity;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(PHIKBallActuatorBehaviour))]
public class PHIKBallActuatorBehaviourEditor : Editor {
    PHIKBallActuatorDesc desc = new PHIKBallActuatorDesc();

    public void OnSceneGUI() {
        PHIKBallActuatorBehaviour phIKActBehaviour = (PHIKBallActuatorBehaviour)target;

        // ----- ----- ----- ----- -----
        // Pullback Target Handle
        // <!!>（本当は親剛体の姿勢を基準にしたほうがいいのでは？）
        if (phIKActBehaviour.showPullbackTargetHandle) {
            Tools.current = Tool.None;

            if (phIKActBehaviour.phIKBallActuator != null) {
                phIKActBehaviour.phIKBallActuator.GetDesc(desc);
                Quaternion currPullbackTarget = desc.pullbackTarget.ToQuaternion();
                Quaternion handleRot = Handles.RotationHandle(currPullbackTarget, phIKActBehaviour.transform.position);
                desc.pullbackTarget = handleRot.ToQuaterniond();
                phIKActBehaviour.phIKBallActuator.SetDesc(desc);
                phIKActBehaviour.desc.pullbackTarget = desc.pullbackTarget;

            } else if (phIKActBehaviour.desc != null) {
                Quaternion currPullbackTarget = ((Quaterniond)(phIKActBehaviour.desc.pullbackTarget)).ToQuaternion();
                Quaternion handleRot = Handles.RotationHandle(currPullbackTarget, phIKActBehaviour.transform.position);
                phIKActBehaviour.desc.pullbackTarget = handleRot.ToQuaterniond();
            }
        }
    }
}
#endif

[DefaultExecutionOrder(6)]
public class PHIKBallActuatorBehaviour : PHIKActuatorBehaviour {
    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // メンバ変数

    public PHIKBallActuatorDescStruct desc = null;

    public bool showPullbackTargetHandle = false;

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // このBehaviourに対応するSpringheadオブジェクト

    public PHIKBallActuatorIf phIKBallActuator { get { return sprObject as PHIKBallActuatorIf; } }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // SprBehaviourの派生クラスで実装するメソッド

    // -- DescStructオブジェクトを再構築する
    public override void ResetDescStruct() {
        desc = new PHIKBallActuatorDescStruct();
    }

    // -- DescStructオブジェクトを取得する
    public override CsObject GetDescStruct() {
        return desc;
    }

    // -- DescオブジェクトをNewして返す
    public override CsObject CreateDesc() {
        return new PHIKBallActuatorDesc();
    }

    // -- DescStructをDescに適用する
    public override void ApplyDesc(CsObject from, CsObject to) {
        (from as PHIKBallActuatorDescStruct).ApplyTo(to as PHIKBallActuatorDesc);
    }

    // -- Sprオブジェクトの構築を行う
    public override ObjectIf Build() {
        PHIKBallActuatorIf phIKAct = phScene.CreateIKActuator(PHIKBallActuatorIf.GetIfInfoStatic(), (PHIKBallActuatorDesc)desc).Cast();
        phIKAct.SetName("ika:" + gameObject.name);
        phIKAct.Enable(true);

        PHBallJointBehaviour bj = gameObject.GetComponent<PHBallJointBehaviour>();
        if (bj != null && bj.sprObject != null) {
            phIKAct.AddChildObject(bj.sprObject);
        }

        return phIKAct;
    }

}
