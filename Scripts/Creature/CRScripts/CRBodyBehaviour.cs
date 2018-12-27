using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SprCs;
using SprUnity;


#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(CRBodyBehaviour))]
public class BodyEditor : Editor {
    public override void OnInspectorGUI() {
        CRBodyBehaviour crBodyBehaviour = (CRBodyBehaviour)target;

        // ----- ----- ----- ----- -----

        DrawDefaultInspector();

        // ----- ----- ----- ----- -----

        if (GUILayout.Button("Setup From Animator")) {
            crBodyBehaviour.SetupFromAnimator();
        }
    }
}
#endif


// CRBoneとGameObjectを結びつける構造体
[System.Serializable]
public class CRBoneAvatarBonePair {
    public CRBoneBehaviour crBoneBehaviour;
    public GameObject avatarBone;

    [HideInInspector]
    public Quaternion phSolidAvatarBoneRelativeRot = Quaternion.identity;
}

[DefaultExecutionOrder(20)]
public class CRBodyBehaviour : SprSceneObjBehaviour {
    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // メンバ変数

    public CRBodyDescStruct desc;

    // -- CRBodyの持つCRBoneと、Unity Avatarのボーンオブジェクトとの対応
    [SerializeField]
    public List<CRBoneAvatarBonePair> bonePairs = new List<CRBoneAvatarBonePair>();

    // Unity上で動かしたいキャラクタモデルのAnimator
    public Animator animator = null;

    // ----- ----- ----- ----- -----

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // このBehaviourに対応するSpringheadオブジェクト

    public CRBodyIf crBody { get { return sprObject as CRBodyIf; } }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // SprBehaviourの派生クラスで実装するメソッド

    // -- DescStructオブジェクトを再構築する
    public override void ResetDescStruct() {
        desc = new CRBodyDescStruct();
    }

    // -- DescStructオブジェクトを取得する
    public override CsObject GetDescStruct() {
        return desc;
    }

    // -- DescオブジェクトをNewして返す
    public override CsObject CreateDesc() {
        return new CRBodyDesc();
    }

    // -- DescStructをDescに適用する
    public override void ApplyDesc(CsObject from, CsObject to) {
        (from as CRBodyDescStruct).ApplyTo(to as CRBodyDesc);
    }

    // -- Sprオブジェクトの構築を行う
    public override ObjectIf Build() {
        var crSdk = CRSdkIf.GetSdk();
        var crCreature = crSdk.CreateCreature(CRCreatureIf.GetIfInfoStatic(), new CRCreatureDesc());
        var crBody = crCreature.CreateBody(CRBodyIf.GetIfInfoStatic(), desc);

        return crBody;
    }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // MonoBehaviourのメソッド

    // UnityのStart
    public override void Start() {
        base.Start();

        // <!!>
        // これはSetupFromAnimatorの中でやるべきかも
        // キャラクタ全体の配置（位置・回転）への対応だけやればいい

        // Record Initial Relative Pose between PHSolid and Avatar Bone
        foreach (var pair in bonePairs) {
            if (pair.avatarBone != null) {
                var so = pair.crBoneBehaviour.crBone.GetPHSolid().GetPose().Ori().ToQuaternion();
                var av = pair.avatarBone.transform.rotation;
                pair.phSolidAvatarBoneRelativeRot = Quaternion.Inverse(so) * av;
            }
        }
    }

    // UnityのFixedUpdate
    void FixedUpdate() {
        // Apply Body Pose to Avatar
        foreach (var pair in bonePairs) {
            if (pair.avatarBone != null && pair.crBoneBehaviour != null) {
                if (pair.crBoneBehaviour.crBone.GetLabel() == "Hips") {
                    pair.avatarBone.transform.position = pair.crBoneBehaviour.crBone.GetPHSolid().GetPose().Pos().ToVector3();
                }
                pair.avatarBone.transform.rotation = pair.crBoneBehaviour.crBone.GetPHSolid().GetPose().Ori().ToQuaternion() * pair.phSolidAvatarBoneRelativeRot;
            }
        }
    }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // その他のメソッド

    // ボーンを取得するインデクサ

    // -- 文字列で検索
    public CRBoneBehaviour this[string key] {
        get {

            // <!!>
            // CRBodyIfの機能を使う方法にかえるべき

            foreach (var pair in bonePairs) { if (pair.crBoneBehaviour.crBone.GetLabel() == key) { return pair.crBoneBehaviour; } }
            return null;
        }
    }

    // -- UnityのAvatarに使われるEnumで検索
    public CRBoneBehaviour this[HumanBodyBones key] {
        get { return this[key.ToString()]; }
    }

    // ----- ----- ----- ----- -----

    // 与えられたUnity Avatarにあわせて、CRBodyに含まれるCRBoneの位置を自動調整する
    public void SetupFromAnimator() {
        // ・標準的なボディの構成をプレハブ化しておく（アンカーの設定とかはどのプレハブを使うかで選べばよさそう）
        // ・ユーザはまずプレハブを実体化し、avatarに目的のAvatarをセット
        // ・Avatarの構造と合わない部分を自動調整（存在しない関節を削除して繋げたりとか）
        // ・Avatarに対して位置合わせ
        // ・Avatarとプレハブの姿勢差を記録

        HumanBodyBones[] boneIds = {
                HumanBodyBones.Hips,
                HumanBodyBones.LeftUpperLeg,
                HumanBodyBones.RightUpperLeg,
                HumanBodyBones.LeftLowerLeg,
                HumanBodyBones.RightLowerLeg,
                HumanBodyBones.LeftFoot,
                HumanBodyBones.RightFoot,
                HumanBodyBones.Spine,
                HumanBodyBones.Chest,
                HumanBodyBones.UpperChest,
                HumanBodyBones.Neck,
                HumanBodyBones.Head,
                HumanBodyBones.LeftShoulder,
                HumanBodyBones.RightShoulder,
                HumanBodyBones.LeftUpperArm,
                HumanBodyBones.RightUpperArm,
                HumanBodyBones.LeftLowerArm,
                HumanBodyBones.RightLowerArm,
                HumanBodyBones.LeftHand,
                HumanBodyBones.RightHand,
                HumanBodyBones.LeftToes,
                HumanBodyBones.RightToes,
                HumanBodyBones.LeftEye,
                HumanBodyBones.RightEye,
            };

        // Find Avatar Bones
        foreach (var boneId in boneIds) {
            var trn = animator.GetBoneTransform(boneId);
            if (trn != null) {
                var pair = bonePairs.Find(p => p.crBoneBehaviour.crBone.GetLabel() == boneId.ToString());
                if (pair != null) {
                    pair.avatarBone = trn.gameObject;
                }
            }
        }

        // Auto Set Position
        foreach (var boneId in boneIds) {
            var trn = animator.GetBoneTransform(boneId);
            if (trn != null) {
                var bone = this[boneId.ToString()];
                if (bone != null) {
                    bone.transform.position = trn.position;
                    bone.transform.rotation = Quaternion.identity;
                }
            }
        }

        // Auto Adjust CoM
        foreach (var pair in bonePairs) {
            var bone = pair.crBoneBehaviour;
            if (bone != null && bone.crBone.NChildBones() > 0) {
                Vector3 CoM = bone.transform.position; float cnt = 1.0f;
                for (int i = 0; i < bone.crBone.NChildBones(); i++) {
                    var child = bone.crBone.GetChildBone(i);
                    CoM += child.GetBehaviour<CRBoneBehaviour>().transform.position;
                    cnt += 1.0f;
                }
                CoM /= cnt;

                var CoMLocal = bone.transform.ToPosed().Inv() * CoM.ToVec3d();
                bone.crBone.GetPHSolid().GetBehaviour<PHSolidBehaviour>().desc.center = CoMLocal;
            }
        }
    }

}
