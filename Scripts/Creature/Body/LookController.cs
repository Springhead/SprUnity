using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SprUnity;

#if UNITY_EDITOR
    using UnityEditor;

    [CustomEditor(typeof(LookController))]
    public class LookControllerEditor : Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            LookController action = (LookController)target;

            EditorGUILayout.PrefixLabel("速");
            action.speed = EditorGUILayout.Slider(action.speed, 0.0f, 1.0f);

            EditorGUILayout.PrefixLabel("直視");
            action.stare = EditorGUILayout.Slider(action.stare, 0.0f, 1.0f);
        }
    }
#endif

public class LookController : MonoBehaviour {
    [HideInInspector]
    public float speed = 0.5f;

    [HideInInspector]
    public float stare = 0.5f; // 目だけ動かす(0.0)か、頭も動かして見つめる(1.0)か

    // Target of Looking
    public GameObject target = null;

    // Straight mode : Look Forward no matter where target is. Use Mona-lisa Effect
    public bool straight = false;

    [HideInInspector]
    public Pose currentTargetPose = new Pose();

    // Target Position Output Object. Use this with VRMLookAtHead
    public GameObject eyeTargetOutput = null;

    public Body body = null;
    // public BlinkController blinkController = null;

    // 待機時間（サッケード中は次の運動を抑制する）
    private float waitTimer = 0.0f;
    public bool inAction { get { return waitTimer > 0; } }

    private Quaternion initialLocalRot = Quaternion.identity;
    private Pose initialHeadPose = new Pose();
    private Pose initialHipsPose = new Pose();

    private Quaternion originRotation = Quaternion.identity;
    private GameObject lastTarget = null;

    public Vector2 uvRatio = new Vector2(0.3f, 0.3f);

    private bool initialized = false;

    void Start() {
    }

    void FixedUpdate() {
        if (!initialized) {
            if (body != null && body["Head"] != null && body["Head"].solid != null && body["Head"].solid.phSolid != null) {
                initialLocalRot = body["Head"].transform.localRotation;
                initialHeadPose = new Pose(body["Head"].transform.position, body["Head"].transform.rotation);
                initialHipsPose = new Pose(body["Hips"].transform.position, body["Hips"].transform.rotation);

                originRotation = initialHeadPose.rotation;

                initialized = true;
            }
        }

        if (!initialized) { return; }

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        if (eyeTargetOutput != null) {
            Vector3 eyeDir = (body["LeftEye"].transform.TransformDirection(Vector3.forward) + body["RightEye"].transform.TransformDirection(Vector3.forward)) * 0.5f;
            Vector3 eyePos = (body["LeftEye"].transform.position + body["RightEye"].transform.position) * 0.5f;
            eyeTargetOutput.transform.position = eyePos + eyeDir * 2.0f;

            // <!!>
            /*
            if (target != null) {
                eyeTargetOutput.transform.position = target.transform.position;
            }
            */
        }

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        if (waitTimer > 0) {
            waitTimer -= Time.fixedDeltaTime;
            return;
        }

        if (body == null || target == null) {
            return;
        }

        if (target == lastTarget) {
            // return;
        }
        lastTarget = target;

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
        // ＜目を動かす＞

        // 視線移動量を算出
        Vector3 targEyeDir = (target.transform.position - body["Head"].transform.position).normalized;
        if (targEyeDir.magnitude < 1e-5) { targEyeDir = Vector3.forward; }

        // ストレートモード：モナリザ効果を利用するため、ターゲットがどこにいようと正面を見る
        if (straight) { targEyeDir = Vector3.forward; }

        Vector3 currLEyeDir = body["LeftEye"].controller.rotTrajectory.Last().q1 * new Vector3(0, 0, 1);
        Vector3 currREyeDir = body["RightEye"].controller.rotTrajectory.Last().q1 * new Vector3(0, 0, 1);
        Vector3 currEyeDir = (currLEyeDir + currREyeDir) * 0.5f;
        float diffAngleEye = Vector3.Angle(targEyeDir, currEyeDir);

        // Smooth Persuitの最大追随速度は普通は30[deg/sec]らしいので、これを超えたらSaccade
        bool saccade = (diffAngleEye / Time.fixedDeltaTime > 30.0f);

        // 視線移動速度の決定
        float durationEye;
        if (saccade) {
            durationEye = diffAngleEye * (1 / 500.0f); //  1/500 [sec/deg]
        } else {
            durationEye = Time.fixedDeltaTime;
        }

        // 視線ベクトルをクォータニオンに変換
        Quaternion eyeTargetRotation = Quaternion.LookRotation(targEyeDir);

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
        // ＜頭を動かす＞

        // marginの量を設定
        // float margin = (saccade ? 60.0f : 70.0f) * (1 - stare); // Saccadeの時はmarginを小さくとる、stareパラメータで調節可能
        float margin = 0;

        // marginを考慮した上での頭の目標方向と移動量を計算
        GameObject targetForHead = target;
        Vector3 targHeadDir = (targetForHead.transform.position - body["Head"].transform.position).normalized;
        targHeadDir.y = Mathf.Clamp(targHeadDir.y, -0.1f, 0.05f); // 上過ぎや下過ぎを見ないようにクランプ
        Vector3 currHeadDir = body["Head"].controller.rotTrajectory.Last().q1 * new Vector3(0, 0, 1);
        float diffAngleHead = Vector3.Angle(currHeadDir, targHeadDir);
        bool headMove = false;
        if (margin >= 0 && (diffAngleHead > margin)) {
            Vector3 pullbackAxis = Vector3.Cross(targHeadDir, currHeadDir);
            Quaternion pullbackRot = Quaternion.AngleAxis(margin, pullbackAxis);
            targHeadDir = pullbackRot * targHeadDir;
            diffAngleHead -= margin;
            headMove = true;
        }

        // 頭の向きを算出
        if (targHeadDir.magnitude < 1e-5) { targHeadDir = Vector3.forward; Debug.Log("!!!"); }
        Pose headTargetPose = new Pose(initialHeadPose.position, Quaternion.LookRotation(targHeadDir, Vector3.up));

        // 頭部運動の速度を移動量とspeedの設定値に応じて決定
        float minDurationHead = 0.2f; //  0.5f + (1 - speed) * 0.5f;
        float durationHead = Mathf.Max((1 / (40.0f + 60.0f * speed)) * diffAngleHead, minDurationHead);

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
        // ＜エンドエフェクタへの動作指示＞

        if (saccade) {
            if (headMove) {
                // 目と頭が連動するのでdurationHeadに基づいてdurationEyeを決める　→　目の動きが遅すぎるのでやめましたよ！<!!>
                // durationEye = durationHead * 0.8f;

                // サッケード中＆終了後しばらくは、次の視線移動動作を抑制する
                waitTimer = durationHead * 1.0f;
                // Debug.Log("DH: " + durationHead);

                // 視線を大きく動かす前にはまばたきをする
                if (diffAngleHead > 20.0f) {
                    // <!!> blinkController.Action();
                }
            } else {
                // 頭を動かさない場合
                waitTimer = durationEye;
                // Debug.Log("DE: " + durationEye);
            }
        }
        // 動作指示
        body["LeftEye"].controller.AddSubMovement(new Pose(new Vector3(), eyeTargetRotation), new Vector2(1, 1), durationEye, durationEye);
        body["RightEye"].controller.AddSubMovement(new Pose(new Vector3(), eyeTargetRotation), new Vector2(1, 1), durationEye, durationEye);
        if (headMove) {
            // Debug.Log (headTargetPose.ToString() + durationHead);
            headTargetPose.rotation = Quaternion.Slerp(originRotation, headTargetPose.rotation, stare);
            body["Head"].controller.AddSubMovement(headTargetPose, new Vector2(1, 1), durationHead + 0.1f, durationHead, usePos: false);
            if (stare > 0.8f) {
                originRotation = headTargetPose.rotation;
            }

            Pose hipsTargetPose = new Pose();
            hipsTargetPose.position = body["Hips"].controller.rotTrajectory.Last().p1;
            hipsTargetPose.rotation = Quaternion.Slerp(body.transform.rotation, headTargetPose.rotation, 0.5f);
            // body["Hips"].controller.AddSubMovement(hipsTargetPose, new Vector2(1, 1), durationHead + 0.1f, durationHead, usePos: false);
            // <!!> ↑ このへんはBodyBalancerとかち合うのでそろそろ整理した方が
        }

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        // 他のクラスから利用するために現在の頭部目標姿勢を保存しておく
        currentTargetPose = headTargetPose;
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.green;

        if (body != null && body["LeftEye"] != null && body["RightEye"] != null && body["Head"] != null) {
            Vector3 currLEyeDir = body["LeftEye"].transform.rotation * new Vector3(0, 0, 1);
            Vector3 currREyeDir = body["RightEye"].transform.rotation * new Vector3(0, 0, 1);
            Vector3 currEyeDir = (currLEyeDir + currREyeDir) * 0.5f;
            Ray rayGaze = new Ray(body["Head"].transform.position, currEyeDir);
            Gizmos.DrawRay(rayGaze);

            Gizmos.color = Color.blue;
            Ray rayHead = new Ray(body["Head"].transform.position, currentTargetPose.rotation * new Vector3(0, 0, 1));
            Gizmos.DrawRay(rayHead);

            Gizmos.color = Color.cyan;
            Ray rayOrigin = new Ray(body["Head"].transform.position, originRotation * new Vector3(0, 0, 1));
            Gizmos.DrawRay(rayOrigin);

            if (target != null) {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(body["Head"].transform.position, target.transform.position);
            }
        }
    }

}

