using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SprUnity;
using SprCs;
using System;
using UnityEditor.Experimental.UIElements;
using UnityEngine.Profiling;
using UnityEngine.Serialization;
using VGent;

// AnimatorがInput用とOutput用の二つあることが前提
[DefaultExecutionOrder(0)]
public abstract class TraceController : MonoBehaviour {
    public float upperLimitAngularVelocity = 1000f * Mathf.Deg2Rad;
    public float upperLimitSpringVelocity = 2f;
    protected List<TracePair> tracePairs;

    protected Body body;
    private List<TraceBallJointState> traceBallJointStates;
    private TraceSpringJointState traceSpringJointState;
    private TraceDynamicalOffSolidState traceDynamicalOffSolidState;
    // Use this for initialization
    protected void Start() {
        //Debug.Log(pair.srcAvatarBone.name +  
        //          "srcAvaLocRot " + srcAvaLocRot.ToQuaterniond() +
        //          "srcAvaLocRot 角度 " + Math.Sqrt(srcAvaLocRot.ToQuaterniond().Rotation().square()) + 
        //          "srcAvaLocRot 速度ベクトル " + srcAvaLocRot.ToQuaterniond().Rotation() + 
        //          "preSrcAvaLocRot " + preSrcAvaLocRot.ToQuaterniond() + 
        //          "preSrcAvaLocRot 角度 " + Math.Sqrt(preSrcAvaLocRot.ToQuaterniond().Rotation().square()) + 
        //          "preSrcAvaLocRot 速度ベクトル " + preSrcAvaLocRot.ToQuaterniond().Rotation() + 
        //          "srcAvaDiffRot " + srcAvaDiffRot.ToQuaterniond() +
        //          "srcAvaDiffRot 角度 " + Math.Sqrt(srcAvaDiffRot.ToQuaterniond().Rotation().square()) +
        //          "srcAvaDiffRot * preSrcAvaLocRot " + (srcAvaDiffRot * preSrcAvaLocRot).ToQuaterniond() +
        //          " srcAvaAngVel " + srcAvaAngVel);
        var srcAvaLocRot = new Quaterniond(0.604262, 0.788784, -0.0594855, 0.09565);
        var preSrcAvaLocRot = new Quaterniond(0.796369, -0.594963, -0.0331679, -0.103516);
        Quaternion srcAvaDiffRot = Quaternion.Slerp(Quaternion.identity, srcAvaLocRot.ToQuaternion() * Quaternion.Inverse(preSrcAvaLocRot.ToQuaternion()), 1.0f);
        Debug.Log(
                  "srcAvaLocRot " + srcAvaLocRot +
                  "srcAvaLocRot 角度 " + Math.Sqrt(srcAvaLocRot.Rotation().square()) +
                  "srcAvaLocRot 速度ベクトル " + srcAvaLocRot.Rotation() +
                  "preSrcAvaLocRot " + preSrcAvaLocRot +
                  "preSrcAvaLocRot 角度 " + Math.Sqrt(preSrcAvaLocRot.Rotation().square()) +
                  "preSrcAvaLocRot 速度ベクトル " + preSrcAvaLocRot.Rotation() +
                  "srcAvaDiffRot " + srcAvaDiffRot.ToQuaterniond() +
                  "srcAvaDiffRot 角度 " + Math.Sqrt(srcAvaDiffRot.ToQuaterniond().Rotation().square()) +
                  "srcAvaDiffRot * preSrcAvaLocRot " + (srcAvaDiffRot.ToQuaterniond() * preSrcAvaLocRot)
                  /*" srcAvaAngVel " + srcAvaAngVel*/);

        var q = new Quaterniond(0.181849, 0.961742, -0.0591918, 0.196256);
        Debug.Log("AngularVelocity " + Math.Sqrt(q.Rotation().square()));
        if (body == null) {
            body = GetComponent<Body>();
        }
        GetPairs();
        InitializeStringBonePairs();
        InitializeTraceBallJointState();
        // デバッグ表示用
        InitTarget();

        UpdateTraceJointStates();
    }

    [System.Serializable]
    public class TracePair {
        public Bone destBone;
        public GameObject srcAvatarBone;
        public TracePair(Bone bone) {
            destBone = bone;
        }
        [HideInInspector]
        public Quaternion firstDestBoneLRot = Quaternion.identity;
        [HideInInspector]
        public Vector3 firstLocalPosition = new Vector3();
        [HideInInspector]
        public Quaternion srcToDest = Quaternion.identity;
    }
    // TracePairの実行中に使用する変数
    public class TraceState {
        public TracePair stringBonePair;
        public TraceState parent; // Boneの情報から設定
        public Quaternion destBoneRotNoLimit = new Quaternion();
    }
    // BallJointのFeedForwardで必要な変数を一つのクラスで管理している
    public class TraceBallJointState : TraceState {
        public Quaternion preSrcAvaLocRot = new Quaternion(); // 前ステップのLocalRotation
        public Quaterniond targetRotation = new Quaterniond();
        // BallJointで使う
        public Vec3d targetVelocity = new Vec3d();
    }
    // SpringJointのFeedForwardで必要な変数を一つのクラスで管理している
    public class TraceSpringJointState : TraceState {
        public Quaternion preSrcAvaLocRot = new Quaternion(); // 前ステップのLocalRotation
        public Quaterniond targetRotation = new Quaterniond();
        // SpringJointで使う
        public Vec3d targetPosition = new Vec3d();
        public Vec6d targetVelocity = new Vec6d(); // 最初三つが位置,後の三つが角速度
        public Vec3d socketVelocity = new Vec3d();
        public Vec3d socketAngularVelocity = new Vec3d();
    }
    // BaseのDynamicalOffのSolidで必要な変数を一つのクラスで管理している
    public class TraceDynamicalOffSolidState : TraceState {
        public Quaterniond targetRotation = new Quaterniond();
        // SpringJointで使う
        public Vec3d targetPosition = new Vec3d();
        public Vec3d velocity = new Vec3d();
        public Vec3d angularVelocity = new Vec3d();
    }

    void InitializeStringBonePairs() {
        foreach (var pair in tracePairs) {
            // Baseに関してはやらない
            if (pair.destBone != null && pair.srcAvatarBone != null && pair.destBone.parent != null) {
                if (pair.destBone.solid != null) {
                    var so = pair.destBone.solid.transform.rotation;
                    var pso = pair.destBone.parent.solid.transform.rotation;
                    var av = pair.srcAvatarBone.transform.rotation;
                    var pav = pair.srcAvatarBone.transform.parent.rotation;
                    var lb = pair.destBone.solid.transform.localRotation;
                    var la = pair.srcAvatarBone.transform.localRotation;
                    pair.firstDestBoneLRot = lb;
                    pair.firstLocalPosition = pair.srcAvatarBone.transform.localPosition;
                    pair.srcToDest = Quaternion.Inverse(av) * so;
                } else {
                    var so = pair.destBone.transform.rotation;
                    var pso = pair.destBone.transform.parent.rotation;
                    var av = pair.srcAvatarBone.transform.rotation;
                    var pav = pair.srcAvatarBone.transform.parent.rotation;
                    var lb = pair.destBone.transform.localRotation;
                    var la = pair.srcAvatarBone.transform.localRotation;
                    pair.firstDestBoneLRot = lb;
                    pair.firstLocalPosition = pair.srcAvatarBone.transform.localPosition;
                    pair.srcToDest = Quaternion.Inverse(av) * so;
                }
            } else {
                pair.srcToDest = Quaternion.identity;
            }
        }
    }

    void InitializeTraceBallJointState() {
        traceBallJointStates = new List<TraceBallJointState>();
        foreach (var pair in tracePairs) {
            if (pair.destBone != null && pair.srcAvatarBone != null) {
                if (pair.destBone.joint != null) {
                    var bj = pair.destBone.joint.phJoint as PHBallJointIf;
                    if (bj != null) {
                        var newState = new TraceBallJointState();
                        newState.stringBonePair = pair;
                        traceBallJointStates.Add(newState);
                        newState.preSrcAvaLocRot = pair.srcAvatarBone.transform.localRotation;
                    } else {
                        var sj = pair.destBone.joint.phJoint as PHSpringIf;
                        if (sj != null) {
                            var newState = new TraceSpringJointState();
                            newState.stringBonePair = pair;
                            traceSpringJointState = newState;
                            traceSpringJointState.preSrcAvaLocRot = pair.srcAvatarBone.transform.localRotation;
                        }
                    }
                } else {
                    // BaseのBone
                    if (pair.destBone.parent == null) {
                        var newState = new TraceDynamicalOffSolidState();
                        newState.stringBonePair = pair;
                        traceDynamicalOffSolidState = newState;
                        traceDynamicalOffSolidState.targetRotation = pair.srcAvatarBone.transform.rotation.ToQuaterniond();
                        traceDynamicalOffSolidState.targetPosition = pair.srcAvatarBone.transform.position.ToVec3d();
                    }
                }
            }
        }
        // traceBallJointStateのparentをBoneの情報から設定
        foreach (var traceBallJointState in traceBallJointStates) {
            foreach (var pTraceBallJointState in traceBallJointStates) {
                if (traceBallJointState.stringBonePair.destBone.parent ==
                    pTraceBallJointState.stringBonePair.destBone) {
                    traceBallJointState.parent = pTraceBallJointState;
                    break;
                }
            }
        }
        // traceBallJointStateの親がtraceSpringJointStateのparentを設定
        foreach (var traceBallJointState in traceBallJointStates) {
            if (traceBallJointState.stringBonePair.destBone.parent == traceSpringJointState.stringBonePair.destBone) {
                traceBallJointState.parent = traceSpringJointState;
            }
        }
        // traceSpringJointStateのparetを設定
        if (traceSpringJointState.stringBonePair.destBone.parent ==
            traceDynamicalOffSolidState.stringBonePair.destBone) {
            traceSpringJointState.parent = traceDynamicalOffSolidState;
        }
    }

    protected void UpdateTargVelPos() {
        foreach (var state in traceBallJointStates) {
            PHBallJointIf bj = state.stringBonePair.destBone.joint.phJoint as PHBallJointIf;
            if (bj != null) {
                bj.SetTargetVelocity(state.targetVelocity);
                bj.SetTargetPosition(state.targetRotation);
            } else {
            }
        }
        // HipsのSpring
        PHSpringIf sj = traceSpringJointState.stringBonePair.destBone.joint.phJoint as PHSpringIf;
        if (sj != null) {
            sj.SetTargetVelocity(traceSpringJointState.targetVelocity);
            sj.SetTargetOrientation(traceSpringJointState.targetRotation);
            sj.SetTargetPosition(traceSpringJointState.targetPosition);
        }
        // BaseのDynamicalオフの剛体の位置同期
        var baseBoneSolid = traceDynamicalOffSolidState.stringBonePair.destBone.solid.phSolid;
        var baseSrcAvatarBone = traceDynamicalOffSolidState.stringBonePair.srcAvatarBone;
        //baseBoneSolid.SetPose(baseSrcAvatarBone.transform.ToPosed());
        baseBoneSolid.SetPose(new Posed(traceDynamicalOffSolidState.targetPosition,
            traceDynamicalOffSolidState.targetRotation));
        baseBoneSolid.SetVelocity(traceDynamicalOffSolidState.velocity);
        baseBoneSolid.SetAngularVelocity(traceDynamicalOffSolidState.angularVelocity);
    }

    private Quaternion preTrueRot;
    void getAngVelAndTargRot(TracePair pair, TraceState traceState, ref Quaternion preSrcAvaLocRot, out Vec3d angularVelocity, ref Quaterniond targetRotation) {
        Profiler.BeginSample("getAngVelAndTragRot");
        Quaternion srcAvaLocRot = pair.srcAvatarBone.transform.localRotation;
        if (srcAvaLocRot.w < 0) {
            srcAvaLocRot.w *= -1;
            srcAvaLocRot.x *= -1;
            srcAvaLocRot.y *= -1;
            srcAvaLocRot.z *= -1;
        }
        Quaternion srcAvaDiffRot = Quaternion.Slerp(Quaternion.identity, srcAvaLocRot * Quaternion.Inverse(preSrcAvaLocRot), 1.0f);
        //Quaternion srcAvaDiffRot = srcAvaLocRot * Quaternion.Inverse(preSrcAvaLocRot); //これだと落ちる
        Vec3d srcAvaAngVel = srcAvaDiffRot.ToQuaterniond().RotationHalf() / Time.deltaTime;

        //Vec3d srcAvaAngVel;
        //float angle;
        //Vector3 axis;
        //srcAvaDiffRot.ToAngleAxis(out angle, out axis);
        //srcAvaAngVel = (angle * axis * Mathf.Deg2Rad / Time.deltaTime).ToVec3d();

        if (srcAvaAngVel.square() > upperLimitAngularVelocity * upperLimitAngularVelocity) {
            //Debug.Log(pair.srcAvatarBone.name +
            //          " srcAvaLocRot " + srcAvaLocRot.ToQuaterniond() +
            //          " srcAvaLocRot 角度 " + Math.Sqrt(srcAvaLocRot.ToQuaterniond().Rotation().square()) +
            //          " srcAvaLocRot 速度ベクトル " + srcAvaLocRot.ToQuaterniond().Rotation() +
            //          " preSrcAvaLocRot " + preSrcAvaLocRot.ToQuaterniond() +
            //          " preSrcAvaLocRot 角度 " + Math.Sqrt(preSrcAvaLocRot.ToQuaterniond().Rotation().square()) +
            //          " preSrcAvaLocRot 速度ベクトル " + preSrcAvaLocRot.ToQuaterniond().Rotation() +
            //          " srcAvaDiffRot " + srcAvaDiffRot.ToQuaterniond() +
            //          " srcAvaDiffRot 角度 " + Math.Sqrt(srcAvaDiffRot.ToQuaterniond().Rotation().square()) +
            //          " srcAvaDiffRot * preSrcAvaLocRot " + (srcAvaDiffRot * preSrcAvaLocRot).ToQuaterniond() +
            //          " srcAvaAngVel " + srcAvaAngVel +
            //          " srcTrueAvaLocRot " + pair.srcAvatarBone.transform.localRotation.ToQuaterniond() +
            //          " presrcTrueAvaLocRot " + preTrueRot.ToQuaterniond());
            srcAvaAngVel = srcAvaAngVel * Math.Sqrt(upperLimitAngularVelocity * upperLimitAngularVelocity / srcAvaAngVel.square());
            srcAvaDiffRot = Quaterniond.Rot(srcAvaAngVel * Time.deltaTime).ToQuaternion();
            srcAvaLocRot = srcAvaDiffRot * preSrcAvaLocRot; // これが外に出てるとだめだ
        }

        if (pair.destBone.name == "RightLowerArm") {
            preTrueRot = pair.srcAvatarBone.transform.localRotation;
        }

        preSrcAvaLocRot = srcAvaLocRot;

        var srcAvaGloRot = pair.srcAvatarBone.transform.parent.rotation * srcAvaLocRot;
        var destBoneRot = srcAvaGloRot * pair.srcToDest;
        //var destBoneParentRot = pair.parent.srcAvatarBone.transform.rotation * pair.parent.srcToDest;
        var preTargetRotation = targetRotation;
        // parentはAngularVelocityの制限付きではなく制限なしのQuaternionを使用.制限付きのQuaternionだとparentが曲がらなかった分曲がろうとしてしまう.
        // parentが更新されていることが前提,traceStateが親から子の順にgetAngVelAndTargetRotが呼ばれなければならない
        targetRotation = (Quaternion.Inverse(pair.firstDestBoneLRot) *
                          Quaternion.Inverse(traceState.parent.destBoneRotNoLimit) * destBoneRot).ToQuaterniond();
        var destBoneDiffRot = targetRotation * Quaternion.Inverse(preTargetRotation.ToQuaternion()).ToQuaterniond();
        Profiler.BeginSample("shita");
        angularVelocity = destBoneDiffRot.RotationHalf() / Time.deltaTime;
        //angularVelocity = new Vec3d(0,0,0);
        //angularVelocity = (Quaternion.Inverse(pair.firstDestBoneLRot) * 
        //                   srcAvaDiffRot * pair.srcToDest).ToQuaterniond().RotationHalf()/ Time.deltaTime;
        // AngularVelocityの制限なし
        traceState.destBoneRotNoLimit = pair.srcAvatarBone.transform.rotation * pair.srcToDest;
        Profiler.EndSample();
        Profiler.EndSample();
    }

    protected void UpdateTraceJointStates() {
        Quaternion preDestRot = Quaternion.identity; // getAngVelAndTargRotで使用,親のグローバルの目標角度
        // ストップした後にプレイを押すとTime.deltaTimeが0になり0割になる
        if (Time.deltaTime == 0) {
            return;
        }
        // 親から更新していく
        {
            // Baseの角速度,速度を更新
            var pair = traceDynamicalOffSolidState.stringBonePair;
            if (pair.srcAvatarBone == null) {
                return;
            }

            // PositionはDynamicalOffなので更新不要
            var preTargetPosition = traceDynamicalOffSolidState.targetPosition;
            traceDynamicalOffSolidState.targetPosition = pair.srcAvatarBone.transform.position.ToVec3d();
            traceDynamicalOffSolidState.velocity = (pair.srcAvatarBone.transform.position.ToVec3d() - preTargetPosition) / Time.deltaTime;
            if (traceDynamicalOffSolidState.velocity.square() > upperLimitSpringVelocity * upperLimitSpringVelocity) {
                //Debug.Log("DynamicalOff velocity Limit ");
                traceDynamicalOffSolidState.velocity = traceDynamicalOffSolidState.velocity * Math.Sqrt(upperLimitSpringVelocity * upperLimitSpringVelocity / traceDynamicalOffSolidState.velocity.square());
                traceDynamicalOffSolidState.targetPosition = traceDynamicalOffSolidState.velocity * Time.deltaTime + preTargetPosition;
            }
            traceDynamicalOffSolidState.angularVelocity = Quaternion.Slerp(Quaternion.identity,
                pair.srcAvatarBone.transform.rotation * Quaternion.Inverse(traceDynamicalOffSolidState.targetRotation.ToQuaternion()),
                1.0f).ToQuaterniond().Rotation() / Time.deltaTime;
            traceDynamicalOffSolidState.targetRotation = pair.srcAvatarBone.transform.rotation.ToQuaterniond();
            traceDynamicalOffSolidState.destBoneRotNoLimit = traceDynamicalOffSolidState.targetRotation.ToQuaternion();
        }
        {
            // BaseとHipsをつなぐSpringJoint(PHSpringIf)の目標角度,目標位置,目標角速度,目標速度を更新
            var pair = traceSpringJointState.stringBonePair;
            if (pair.srcAvatarBone == null) {
                return;
            }
            Vec3d angularVelocity;
            getAngVelAndTargRot(pair, traceSpringJointState, ref traceSpringJointState.preSrcAvaLocRot, out angularVelocity, ref traceSpringJointState.targetRotation);

            // targetPositionは初期位置からの相対位置
            var preTargetPosition = traceSpringJointState.targetPosition;
            traceSpringJointState.targetPosition = (pair.srcAvatarBone.transform.localPosition - pair.firstLocalPosition).ToVec3d();
            // velocityはすべてHipの上の座標系から見たGlobalVelocity,localAngularVelocityらしい
            var velocity = (traceSpringJointState.targetPosition - preTargetPosition) / Time.deltaTime;
            if (velocity.square() > upperLimitSpringVelocity * upperLimitSpringVelocity) {
                //Debug.Log("velocity Limit ");
                velocity = velocity * Math.Sqrt(upperLimitSpringVelocity * upperLimitSpringVelocity / velocity.square());
                traceSpringJointState.targetPosition = velocity * Time.deltaTime + preTargetPosition;
            }
            traceSpringJointState.targetVelocity = new Vec6d(velocity.x, velocity.y, velocity.z, angularVelocity.x, angularVelocity.y, angularVelocity.z);
        }
        // BallJointの目標角度,目標角速度を更新
        foreach (var traceBallJointState in traceBallJointStates) {
            var pair = traceBallJointState.stringBonePair;
            //オイラー角の差分は角速度ベクトルじゃない
            // 長さが角速度の大きさ,方向が軸左手系
            if (pair.srcAvatarBone == null) {
                continue;
            }
            Vec3d angularVelocity;
            getAngVelAndTargRot(pair, traceBallJointState, ref traceBallJointState.preSrcAvaLocRot, out angularVelocity, ref traceBallJointState.targetRotation);
            // <!!> これってFixedUpdateが早いと0になるのでは？
            traceBallJointState.targetVelocity = angularVelocity;
        }

        // デバッグ表示
        foreach (var traceBallJointState in traceBallJointStates) {
            var destBone = traceBallJointState.stringBonePair.destBone;
            boneTransformDic[destBone].localPosRot.rotation =
                boneTransformDic[destBone].firstLocalPosRot.rotation*traceBallJointState.targetRotation.ToQuaternion();
        }
    }

    class PosRotParent {
        public PosRot localPosRot = new PosRot();
        public PosRot firstLocalPosRot = new PosRot();
        public PosRotParent parent;
        public PosRotParent(Transform transform, PosRotParent parent) {
            localPosRot.position = transform.localPosition;
            localPosRot.rotation = transform.localRotation;
            firstLocalPosRot.position = transform.localPosition;
            firstLocalPosRot.rotation = transform.localRotation;
            this.parent = parent;
        }

        public Vector3 Position {
            get {
                if (parent != null) {
                    return parent.Position + parent.Rotation * localPosRot.position;
                }

                return localPosRot.position;
            }
        }

        public Quaternion Rotation {
            get {
                if (parent != null) {
                    return parent.Rotation * localPosRot.rotation;
                }
                return localPosRot.rotation;
            }
        }
    }
    private Dictionary<Bone, PosRotParent> boneTransformDic;
    public void InitTarget() {
        boneTransformDic = new Dictionary<Bone, PosRotParent>();
        var rootPosRotParent = new PosRotParent(body.rootBone.solid.transform, null);
        boneTransformDic.Add(body.rootBone, rootPosRotParent);
        InitRecursive(body.rootBone, rootPosRotParent);
    }

    void InitRecursive(Bone bone, PosRotParent parent) {
        foreach (var boneChild in bone.children) {
            if (boneChild.solid != null) {
                var newPosRotParent = new PosRotParent(boneChild.solid.transform, parent);
                boneTransformDic.Add(boneChild, newPosRotParent);
                InitRecursive(boneChild, newPosRotParent);
            }
        }
    }
    public void ShowTarget() {
        if (Application.isPlaying && isActiveAndEnabled) {
            Gizmos.color = Color.black;
            foreach (var posrotParent in boneTransformDic.Values) {
                Gizmos.DrawWireSphere(posrotParent.Position, 0.01f);
            }
        }
    }

    private void OnDrawGizmos() {
        ShowTarget();
    }

    protected abstract void GetPairs();
}
