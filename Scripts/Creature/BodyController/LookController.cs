using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InteraWare {

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
        public float stare = 0.0f; // 目だけ動かす(0.0)か、頭も動かして見つめる(1.0)か

        // Target of Looking
        public GameObject target = null;

		// Straight mode : Look Forward no matter where target is. Use Mona-lisa Effect
		public bool straight = false;

        [HideInInspector]
        public Pose currentTargetPose = new Pose();

        public Body body = null;
        public BlinkController blinkController = null;

        // 待機時間（サッケード中は次の運動を抑制する）
        private float waitTimer = 0.0f;
        public bool inAction { get { return waitTimer > 0; } }

        private Quaternion initialLocalRot = Quaternion.identity;
        private Pose initialHeadPose = new Pose();

        public Vector2 uvRatio = new Vector2(0.3f, 0.3f);

        void Start() {
            initialLocalRot = body["Head"].transform.localRotation;
            initialHeadPose = new Pose(body["Head"].transform.position, body["Head"].transform.rotation);
        }

        void FixedUpdate() {
            if (waitTimer > 0) {
                waitTimer -= Time.fixedDeltaTime;
                return;
            }

            if (body == null || target == null) {
                return;
            }

            // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
            // ＜目を動かす＞

            // 視線移動量を算出
            Vector3 targEyeDir = (target.transform.position - body["Head"].transform.position).normalized;
            if (targEyeDir.magnitude < 1e-5) { targEyeDir = Vector3.forward; }

			// ストレートモード：モナリザ効果を利用するため、ターゲットがどこにいようと正面を見る
			if (straight) { targEyeDir = Vector3.forward; }

            Vector3 currLEyeDir = body["LeftEye"].GetComponent<ReachController>().trajectory.Last().q1 * new Vector3(0, 0, 1);
            Vector3 currREyeDir = body["RightEye"].GetComponent<ReachController>().trajectory.Last().q1 * new Vector3(0, 0, 1);
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
            float margin = (saccade ? 60.0f : 70.0f) * (1 - stare); // Saccadeの時はmarginを小さくとる、stareパラメータで調節可能

            // marginを考慮した上での頭の目標方向と移動量を計算
            GameObject targetForHead = target;
            Vector3 targHeadDir = (targetForHead.transform.position - body["Head"].transform.position).normalized;
			targHeadDir.y = Mathf.Clamp (targHeadDir.y, -0.1f, 0.05f); // 上過ぎや下過ぎを見ないようにクランプ
            Vector3 currHeadDir = body["Head"].ikEndEffector.iktarget.GetComponent<ReachController>().trajectory.Last().q1 * new Vector3(0, 0, 1);
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
            float minDurationHead = 0.5f + (1 - speed) * 0.5f;
            float durationHead = Mathf.Max((1 / (40.0f + 60.0f * speed)) * diffAngleHead, minDurationHead);

            // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
            // ＜エンドエフェクタへの動作指示＞

            if (saccade) {
                if (headMove) {
                    // 目と頭が連動するのでdurationHeadに基づいてdurationEyeを決める
                    durationEye = durationHead * 0.8f;
                    // サッケード中＆終了後しばらくは、次の視線移動動作を抑制する
                    waitTimer = durationHead * 1.0f;

                    // 視線を大きく動かす前にはまばたきをする
                    if (diffAngleHead > 20.0f) {
                        blinkController.Action();
                    }
                } else {
                    // 頭を動かさない場合
                    waitTimer = durationEye;
                }
            }
            // 動作指示
            body["LeftEye"].GetComponent<ReachController>().AddSubMovement(new Pose(new Vector3(), eyeTargetRotation), new Vector2(1, 1), durationEye, durationEye);
            body["RightEye"].GetComponent<ReachController>().AddSubMovement(new Pose(new Vector3(), eyeTargetRotation), new Vector2(1, 1), durationEye, durationEye);
            if (headMove) {
				// Debug.Log (headTargetPose.ToString() + durationHead);
                body["Head"].ikEndEffector.iktarget.GetComponent<ReachController>().AddSubMovement(headTargetPose, new Vector2(0.8f, 0.5f), durationHead, durationHead);
            }

            // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

            // 他のクラスから利用するために現在の頭部目標姿勢を保存しておく
            currentTargetPose = headTargetPose;
        }

        void OnDrawGizmos() {
            Gizmos.color = Color.green;

            Vector3 currLEyeDir = body["LeftEye"].transform.rotation * new Vector3(0, 0, 1);
            Vector3 currREyeDir = body["RightEye"].transform.rotation * new Vector3(0, 0, 1);
            Vector3 currEyeDir = (currLEyeDir + currREyeDir) * 0.5f;
            Ray rayGaze = new Ray(body["Head"].transform.position, currEyeDir);
            Gizmos.DrawRay(rayGaze);

            Gizmos.color = Color.blue;
            Ray rayHead = new Ray(body["Head"].transform.position, currentTargetPose.rotation * new Vector3(0, 0, 1));
            Gizmos.DrawRay(rayHead);

            if (target != null) {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(body["Head"].transform.position, target.transform.position);
            }
        }

        // ----- ----- ----- ----- -----

        float GaussianRandom(float mu = 0.0f, float sigma = 0.5f) {
            float rand = 0.0f;
            while ((rand = Random.value) == 0.0f) ;
            float rand2 = Random.value;
            float normrand = Mathf.Sqrt(-2.0f * Mathf.Log(rand)) * Mathf.Cos(2.0f * Mathf.PI * rand2);
            normrand = normrand * sigma + mu;
            return normrand;
        }
    }

}