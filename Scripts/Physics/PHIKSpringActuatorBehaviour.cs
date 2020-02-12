using System.Collections;
using System;
using SprCs;
using SprUnity;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(PHIKSpringActuatorBehaviour))]
[CanEditMultipleObjects]
public class PHIKSpringActuatorBehaviourEditor : Editor {
    PHIKSpringActuatorDesc desc = new PHIKSpringActuatorDesc();

    public void OnSceneGUI() {
        PHIKSpringActuatorBehaviour phIKActBehaviour = (PHIKSpringActuatorBehaviour)target;

        // ----- ----- ----- ----- -----
        // Pullback Target Handle
        // <!!>（本当は親剛体の姿勢を基準にしたほうがいいのでは？）
        if (phIKActBehaviour.showPullbackTargetHandle) {
            Tools.current = Tool.None;
/*
            if (phIKActBehaviour.phIKSpringActuator != null) {
                phIKActBehaviour.phIKSpringActuator.GetDesc(desc);
                Quaternion currPullbackTarget = desc.pullbackTarget.ToQuaternion();
                Quaternion handleRot = Handles.RotationHandle(currPullbackTarget, phIKActBehaviour.transform.position);
                desc.pullbackTarget = handleRot.ToQuaterniond();
                phIKActBehaviour.phIKSpringActuator.SetDesc(desc);
                phIKActBehaviour.desc.pullbackTarget = desc.pullbackTarget;

            } else if (phIKActBehaviour.desc != null) {
                Quaternion currPullbackTarget = ((Quaterniond)(phIKActBehaviour.desc.pullbackTarget)).ToQuaternion();
                Quaternion handleRot = Handles.RotationHandle(currPullbackTarget, phIKActBehaviour.transform.position);
                phIKActBehaviour.desc.pullbackTarget = handleRot.ToQuaterniond();
            }
*/
        }
    }
}
#endif

[DefaultExecutionOrder(6)]
public class PHIKSpringActuatorBehaviour : PHIKActuatorBehaviour {
    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // メンバ変数

    public PHIKSpringActuatorDescStruct desc = null;

    public bool showPullbackTargetHandle = false;

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // このBehaviourに対応するSpringheadオブジェクト

    public PHIKSpringActuatorIf phIKSpringActuator { get { return sprObject as PHIKSpringActuatorIf; } }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // SprBehaviourの派生クラスで実装するメソッド

    // -- DescStructオブジェクトを再構築する
    public override void ResetDescStruct() {
        desc = new PHIKSpringActuatorDescStruct();
    }

    // -- DescStructオブジェクトを取得する
    public override CsObject GetDescStruct() {
        return desc;
    }

    // -- DescオブジェクトをNewして返す
    public override CsObject CreateDesc() {
        return new PHIKSpringActuatorDesc();
    }

    // -- DescStructをDescに適用する
    public override void ApplyDesc(CsObject from, CsObject to) {
        (from as PHIKSpringActuatorDescStruct).ApplyTo(to as PHIKSpringActuatorDesc);
    }

    // -- Sprオブジェクトの構築を行う
    public override ObjectIf Build() {
        PHIKSpringActuatorIf phIKAct = phScene.CreateIKActuator(PHIKSpringActuatorIf.GetIfInfoStatic(), (PHIKSpringActuatorDesc)desc).Cast();
        phIKAct.SetName("ika:" + gameObject.name);
        phIKAct.Enable(true);
        
        PHSpringBehavior bj = gameObject.GetComponent<PHSpringBehavior>();
        if (bj != null && bj.sprObject != null) {
            phIKAct.AddChildObject(bj.sprObject);
        }

        return phIKAct;
    }

}
