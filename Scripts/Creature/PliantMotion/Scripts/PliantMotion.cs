using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SprUnity;
using SprCs;
// PHSolidBehaviourの慣性テンソルを調整するLink(Startで呼ばれる)より後でStartを呼びたいのでPHSolidBehaviourよりもExecutionOrderを大きくする
[DefaultExecutionOrder(3)]
public class PliantMotion : MonoBehaviour {
    public PHSceneDescStruct desc = null;
    public bool enableDebugWindow = false;
    public float hightGainRatio = 5f;
    private PHSdkIf phSdk = null;
    private static FWApp fwApp = null;
    private List<Body> bodys;
    private PHSceneIf phScene; // HighGainシミュレーション
    private List<PliantMotionBone> pliantMotionBones;
    class PliantMotionBone {
        public bool isRootBone;
        public PHSolidIf solid; // HighGainのSolid
        public Bone bone; // LowGainのBone
        public PHJointIf phJointIf; // HighGainのJoint
        public PliantMotionBone(Bone bone, PHJointIf phJointIf, PHSolidIf solid = null, bool isRootBone = false) {
            this.isRootBone = isRootBone;
            this.solid = solid;
            this.bone = bone;
            this.phJointIf = phJointIf;
        }
    }
    // Use this for initialization
    void Start() {
        bodys = new List<Body>();
        var sceneBodys = FindObjectsOfType<Body>();
        if (enableDebugWindow) {
            fwApp = new FWApp();
            fwApp.InitInNewThread();

            // FWAppの初期化が終わるまで待つ
            while (fwApp.GetSdk() == null || fwApp.GetSdk().GetPHSdk() == null) { System.Threading.Thread.Sleep(10); }

            phSdk = fwApp.GetSdk().GetPHSdk();
            phScene = fwApp.GetSdk().GetScene(0).GetPHScene();
            phScene.Clear();
            // LowGain側のDescをコピーすべき
            phScene.SetDesc((PHSceneDesc)desc);

            FWSceneIf fwSceneIf = fwApp.GetSdk().GetScene(0);
            fwSceneIf.EnableRenderContact(true);
            fwSceneIf.EnableRenderForce(false, true);
            //fwSceneIf.SetForceScale(0.01f, 0.01f);

        } else {
            phSdk = PHSdkIf.CreateSdk();
            phScene = phSdk.CreateScene((PHSceneDesc)desc);
        }
        Validate();
        pliantMotionBones = new List<PliantMotionBone>();
        foreach (var sceneBody in sceneBodys) {
            AddBody(sceneBody);
        }
    }

    void FixedUpdate() {
        foreach (var pbone in pliantMotionBones) {
            if (pbone.isRootBone) {
                pbone.solid.SetPose(pbone.bone.solid.phSolid.GetPose());
                pbone.solid.SetVelocity(pbone.bone.solid.phSolid.GetVelocity());
                pbone.solid.SetAngularVelocity(pbone.bone.solid.phSolid.GetAngularVelocity());
                pbone.solid.SetCenterOfMass(pbone.bone.solid.phSolid.GetCenterOfMass());
            } else {
                PHBallJointIf targetBJ = pbone.bone.joint.phJoint as PHBallJointIf;
                pbone.phJointIf.GetPlugSolid().SetCenterOfMass(pbone.bone.joint.phJoint.GetPlugSolid().GetCenterOfMass());
                if (targetBJ != null) {
                    PHBallJointIf receiveBJ = pbone.phJointIf as PHBallJointIf;
                    //Debug.Log(pbone.bone.name + targetBJ.GetTargetPosition() + " spring" + receiveBJ.GetSpring());
                    receiveBJ.SetTargetVelocity(targetBJ.GetTargetVelocity());
                    receiveBJ.SetTargetPosition(targetBJ.GetTargetPosition());
                    // JointのGainを変更
                    receiveBJ.SetSpring(targetBJ.GetSpring() * hightGainRatio);
                    receiveBJ.SetDamper(targetBJ.GetDamper() * hightGainRatio);
                } else {
                    PHSpringIf targetSJ = pbone.bone.joint.phJoint as PHSpringIf;
                    if (targetSJ != null) {
                        PHSpringIf receiveSJ = pbone.phJointIf as PHSpringIf;
                        //Debug.Log(pbone.bone.name + targetSJ.GetTargetPosition() + targetSJ.GetTargetOrientation()+targetSJ.GetTargetVelocity());
                        receiveSJ.SetTargetVelocity(targetSJ.GetTargetVelocity());
                        receiveSJ.SetTargetOrientation(targetSJ.GetTargetOrientation());
                        receiveSJ.SetTargetPosition(targetSJ.GetTargetPosition());
                        // JointのGainを変更
                        receiveSJ.SetSpring(targetSJ.GetSpring() * hightGainRatio);
                        receiveSJ.SetDamper(targetSJ.GetDamper() * hightGainRatio);
                        receiveSJ.SetSpringOri(targetSJ.GetSpringOri() * hightGainRatio);
                        receiveSJ.SetDamperOri(targetSJ.GetDamperOri() * hightGainRatio);
                    }
                }
            }
        }
        phScene.Step();
        foreach (var pbone in pliantMotionBones) {
            if (pbone.isRootBone) {
            } else {
                PHBallJointIf targetBJ = pbone.bone.joint.phJoint as PHBallJointIf;
                if (targetBJ != null) {
                    PHBallJointIf receiveBJ = pbone.phJointIf as PHBallJointIf;
                    targetBJ.SetOffsetForce(receiveBJ.GetMotorForce());
                } else {
                    PHSpringIf targetSJ = pbone.bone.joint.phJoint as PHSpringIf;
                    if (targetSJ != null) {
                        PHSpringIf receiveSJ = pbone.phJointIf as PHSpringIf;
                        targetSJ.SetOffsetForce(receiveSJ.GetMotorForce());
                    }
                }
            }
        }
    }

    // boneにはBodyのrootBone,socketにはrootBoneのPHSolidIfを渡す
    // boneのjointのplugがchildのsocketになっている前提
    void CreateSolidRecursive(Bone bone, PHSolidIf socketSolid) {
        if (bone.joint != null) {
            PHBallJointIf bj = bone.joint.phJoint as PHBallJointIf;
            PHSolidDesc plugDesc = new PHSolidDesc();
            PHSolidIf plugSolid = null;
            if (bj != null) {
                bj.GetPlugSolid().GetDesc(plugDesc);
                plugSolid = phScene.CreateSolid(plugDesc);
                PHBallJointDesc ballJointDesc = new PHBallJointDesc();
                bone.joint.phJoint.GetDesc(ballJointDesc);
                //Debug.Log(bone.name + plugSolid.GetInertia());
                var newJoint = phScene.CreateJoint(socketSolid, plugSolid, PHBallJointIf.GetIfInfoStatic(), ballJointDesc) as PHBallJointIf;
                newJoint.SetSpring(newJoint.GetSpring() * hightGainRatio);
                newJoint.SetDamper(newJoint.GetDamper() * hightGainRatio);
                var newBone = new PliantMotionBone(bone, newJoint,plugSolid);
                pliantMotionBones.Add(newBone);
            } else {
                PHSpringIf sj = bone.joint.phJoint as PHSpringIf;
                if (sj != null) {
                    sj.GetPlugSolid().GetDesc(plugDesc);
                    plugSolid = phScene.CreateSolid(plugDesc);
                    PHSpringDesc springDesc = new PHSpringDesc();
                    bone.joint.phJoint.GetDesc(springDesc);
                    var newJoint = phScene.CreateJoint(socketSolid, plugSolid, PHSpringIf.GetIfInfoStatic(), springDesc) as PHSpringIf;
                    newJoint.SetSpring(newJoint.GetSpring() * hightGainRatio);
                    newJoint.SetDamper(newJoint.GetDamper() * hightGainRatio);
                    newJoint.SetSpringOri(newJoint.GetSpringOri() * hightGainRatio);
                    newJoint.SetDamperOri(newJoint.GetDamperOri() * hightGainRatio);
                    var newBone = new PliantMotionBone(bone, newJoint,plugSolid);
                    pliantMotionBones.Add(newBone);
                }
            }
            foreach (var childBone in bone.children) {
                CreateSolidRecursive(childBone, plugSolid);
            }
        } else {
            // rootBoneにはジョイントがないので別処理
            foreach (var childBone in bone.children) {
                CreateSolidRecursive(childBone, socketSolid);
            }
        }
        return;
    }
    void CreateTreeNodeRecursive(PHTreeNodeIf lowGainTreeNodeIf,PHTreeNodeIf highGainTreeNodeIf) {
        for (int i = 0; i < lowGainTreeNodeIf.NChildren(); i++) {
            var childNode = lowGainTreeNodeIf.GetChildNode(i);
            foreach (var pliantMotionBone in pliantMotionBones) {
                if (pliantMotionBone.bone.solid.phSolid == childNode.GetSolid()) {
                    var newTreeNode = phScene.CreateTreeNode(highGainTreeNodeIf, pliantMotionBone.solid);
                    newTreeNode.Enable();
                    CreateTreeNodeRecursive(lowGainTreeNodeIf,newTreeNode);
                }
            }
        }

    }
    // BodyをHighGainシミュレーションに追加
    void AddBody(Body body) {
        PHSolidIf socketSolid = phScene.CreateSolid(body.rootBone.solid.desc);
        // RootBoneを追加
        var newBone = new PliantMotionBone(body.rootBone, null, socketSolid, true);
        pliantMotionBones.Add(newBone);
        // RootBone以外を追加
        CreateSolidRecursive(body.rootBone, socketSolid);
        // ABAのTreeがあればHighGain側も作る
        for (int i = 0; i < phScene.NRootNodes(); i++) {
            var rootNode = phScene.GetRootNode(i);
            foreach (var pliantMotionBone in pliantMotionBones) {
                if (pliantMotionBone.bone.solid.phSolid == rootNode.GetSolid()) {
                    var newRootNode = phScene.CreateRootNode(pliantMotionBone.solid);
                    newRootNode.Enable();
                    CreateTreeNodeRecursive(rootNode,newRootNode);
                    break;
                }
            }
        }
        bodys.Add(body);
    }
    void OnDrawGizmos() {
        if (Application.isPlaying && isActiveAndEnabled) {
            Gizmos.color = Color.yellow;
            Posed socketPosed;
            Posed plugPosed;
            foreach (var pbone in pliantMotionBones) {
                if (pbone.isRootBone) {
                    Gizmos.DrawWireSphere(pbone.solid.GetPose().Pos().ToVector3(), 0.05f);
                } else {
                    var ball = pbone.phJointIf as PHBallJointIf;

                    //Debug.Log(pbone.bone.name+" " +pbone.phJointIf.GetPlugSolid().GetPose().Pos() + " "+  +" " + pbone.bone.solid.phSolid.GetPose().Pos());
                    socketPosed = pbone.phJointIf.GetSocketSolid().GetPose();
                    plugPosed = pbone.phJointIf.GetPlugSolid().GetPose();
                    Gizmos.DrawLine(socketPosed.Pos().ToVector3(), plugPosed.Pos().ToVector3());
                    Gizmos.DrawWireSphere(plugPosed.Pos().ToVector3(), 0.01f);
                }
            }
        }
    }
    // UnityのOnValidate : SprBehaviourのものをオーバーライド
    public void Validate() {
        if (desc == null) {
            ResetDescStruct();
        }

        // PHSceneの設定
        {
            PHSceneDesc d = new PHSceneDesc();
            phScene.GetDesc(d);
            desc.ApplyTo(d);
            phScene.SetDesc(d);
        }

        // DescではなくStateに含まれる変数。ApplyToで自動同期されないので手動で設定
        phScene.SetTimeStep(desc.timeStep);
        phScene.SetHapticTimeStep(desc.haptictimeStep);
    }

    // -- DescStructオブジェクトを再構築する
    public void ResetDescStruct() {
        desc = new PHSceneDescStruct();
        desc.timeStep = Time.fixedDeltaTime; // 初期値ではUnityに合わせておく
    }
    private void OnDestroy() {
        if (fwApp != null) {
            fwApp.EndThread();
            fwApp = null;
            if (phSdk != null) {
                phSdk.Clear();
                phSdk = null;
            }
        }
    }
}
