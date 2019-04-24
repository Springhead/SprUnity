using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SprUnity {

    public class BoneSubMovementPair {
        public Bone bone;
        public SubMovement subMovement;
        public BoneSubMovementPair(Bone bone, SubMovement sub) {
            this.bone = bone;
            this.subMovement = sub;
        }
    }

    // classname KeyFrame
    [Serializable]
    public class KeyPoseTimePair {
        public string label;
        public KeyPose keyPose;
        public enum StartCondition {
            AbsoluteTime,
            AbsoluteTimeFromPreviousKeyPoseStart,
            AbsoluteTimeFromPreviousKeyPoseEnd,
            RelativeTimeFromPreviousKeyPoseStart,
            OuterTrigger,
        }
        public StartCondition startCondition = StartCondition.AbsoluteTimeFromPreviousKeyPoseStart;
        public float startTimeCoeff = 1.0f;
        public float startTimeInterval = 1.0f;
        public float startTime;
        public enum DurationCondition {
            AbsoluteTimeLength,
            ProportionalToDistance,
        }
        public DurationCondition durationCondition = DurationCondition.AbsoluteTimeLength;
        public float durationCoeff = 1.0f;
        public float duration = 1.0f;
        public float endTime {
            get { return startTime + duration; }
        }
        public bool isUsed = false;

        public KeyPoseTimePair() { }
        public KeyPoseTimePair(KeyPoseTimePair k) {
            this.label = k.label;
            this.startTime = k.startTime;
            this.duration = k.duration;
        }
        
        public void UpdateTimeInfo(float lastStart, float lastEnd) {
            switch (startCondition) {
                case StartCondition.AbsoluteTime:
                    // 何もしない
                    break;
                case StartCondition.AbsoluteTimeFromPreviousKeyPoseStart:
                    // lastStartに固定のinterval加算
                    startTime = lastStart + startTimeInterval;
                    break;
                case StartCondition.AbsoluteTimeFromPreviousKeyPoseEnd:
                    // lastEndに固定のInterval加算
                    startTime = lastEnd + startTimeInterval;
                    break;
                case StartCondition.RelativeTimeFromPreviousKeyPoseStart:
                    // 
                    startTimeInterval = (lastEnd - lastStart) * startTimeCoeff;
                    startTime = lastStart + startTimeInterval;
                    break;
                case StartCondition.OuterTrigger:
                    break;
            }
            switch (durationCondition) {
                case DurationCondition.AbsoluteTimeLength:
                    break;
                case DurationCondition.ProportionalToDistance:
                    break;
            }
        }
    }
    /*
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(KeyPoseTimePair))]
    public class KeyPoseTimePairPropertyDrawer : PropertyDrawer {
        bool m_hideFlag = false;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            m_hideFlag = EditorGUI.Foldout(rect, m_hideFlag, label);

            if (!m_hideFlag) {
                return;
            }

            var backupIndent = EditorGUI.indentLevel;

            label = EditorGUI.BeginProperty(position, label, property);

            float y = position.y;
            {
                SerializedProperty keyPose = property.FindPropertyRelative("keyPose");

                y += EditorGUIUtility.singleLineHeight;
                backupIndent++;
                
                var KeyPoseRect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(KeyPoseRect, )
            }
            EditorGUI.EndProperty();

            EditorGUI.indentLevel = backupIndent;
        }
    }
#endif
*/
    public class SubMovementLog {
        private string source;
        public SubMovement subMovement; 
        public SubMovementLog() {
            this.source = "";
            this.subMovement = null;
        }
        public SubMovementLog(string s, SubMovement sub) {
            this.source = s;
            this.subMovement = sub;
        }
        public SubMovementLog Clone() {
            SubMovementLog clone = new SubMovementLog();
            clone.source = this.source;
            clone.subMovement = this.subMovement.Clone();
            return clone;
        }
    }

    [Serializable]
    public class BoneSubMovementStream {
        public Bone bone;
        public List<SubMovementLog> logSubMovements = new List<SubMovementLog>();
        public List<SubMovementLog> futureSubMovements = new List<SubMovementLog>();
        public List<float[]> logSubmovementSources = new List<float[]>();
        public List<float[]> futureSubMovementSources = new List<float[]>();
        public List<Vector3> calculatedTrajectory = new List<Vector3>();
        public List<Vector3> loggedTrajectory = new List<Vector3>();
        public BoneSubMovementStream(Bone bone) {
            this.bone = bone;
            logSubMovements = new List<SubMovementLog>();
            futureSubMovements = new List<SubMovementLog>();

        }
        public void AddLog(BoneSubMovementPair subMovement, string s) {
            if (subMovement.bone == this.bone) logSubMovements.Add(new SubMovementLog(s, subMovement.subMovement));
        }
        public void AddFuture(BoneSubMovementPair subMovement, string s) {
            if (subMovement.bone == this.bone) futureSubMovements.Add(new SubMovementLog(s, subMovement.subMovement));
        }
        public void ClearLog() {
            logSubMovements.Clear();
        }
        public void ClearFuture() {
            futureSubMovements.Clear();
        }
        public BoneSubMovementStream Clone() {
            BoneSubMovementStream clone = new BoneSubMovementStream(this.bone);
            foreach(var subMovement in this.logSubMovements) {
                clone.logSubMovements.Add(subMovement.Clone());
            }
            return clone;
        }
    }

    public class ActionLog {
        // 開始時のSceneLog
        public CharacterSceneLogger sceneLog;
        // 発行されたSubMovementとかその軌道とか
        public List<BoneSubMovementStream> subMovementLogs;
        public BoneSubMovementStream this[HumanBodyBones boneId] {
            get {
                foreach(var sub in subMovementLogs) {
                    if (sub.bone.label == boneId.ToString()) return sub;
                }
                return null;
            }
        }
        public ActionLog(Body body = null) {
            sceneLog = new CharacterSceneLogger(body);
            subMovementLogs = new List<BoneSubMovementStream>();
        }
        public void AddLog(BoneSubMovementPair boneSubMovement, string s) {
            foreach(var subMovementLog in subMovementLogs) {
                if(boneSubMovement.bone == subMovementLog.bone) {
                    subMovementLog.AddLog(boneSubMovement, s);
                    return;
                }
            }
            // なければ追加
            BoneSubMovementStream newLogs = new BoneSubMovementStream(boneSubMovement.bone);
            newLogs.AddLog(boneSubMovement, s);
            subMovementLogs.Add(newLogs);
        }
        public void AddFuture(BoneSubMovementPair boneSubMovement, string s) {
            foreach (var subMovementLog in subMovementLogs) {
                if (boneSubMovement.bone == subMovementLog.bone) {
                    subMovementLog.AddFuture(boneSubMovement, s);
                    return;
                }
            }
            // なければ追加
            BoneSubMovementStream newLogs = new BoneSubMovementStream(boneSubMovement.bone);
            newLogs.AddFuture(boneSubMovement, s);
            subMovementLogs.Add(newLogs);
        }
        public void AddFuture(BoneKeyPose boneKeyPose, string s, float startTime, float duration, float spring, float damper, Body body) {
            var bone = body[boneKeyPose.boneId];
            SubMovement boneSubMovement = new SubMovement();
            foreach (var subMovementLog in subMovementLogs) {
                if (subMovementLog.bone.label == boneKeyPose.boneId.ToString()) {
                    SubMovement last = subMovementLog.futureSubMovements.Count > 0 ? subMovementLog.futureSubMovements.Last().subMovement : (subMovementLog.logSubMovements.Count > 0 ? subMovementLog.logSubMovements.Last().subMovement : null);
                    if (last != null) {
                        boneSubMovement.p0 = last.p1;
                        boneSubMovement.q0 = last.q1;
                        boneSubMovement.s0 = last.s1;
                    } else {
                        boneSubMovement.p0 = bone.transform.position;
                        boneSubMovement.q0 = bone.transform.rotation;
                        boneSubMovement.s0 = new Vector2(bone.springRatio, bone.damperRatio);
                    }
                    boneSubMovement.t0 = startTime + boneKeyPose.boneKeyPoseTiming[0] * duration;

                    boneSubMovement.t1 = startTime + boneKeyPose.boneKeyPoseTiming[1] * duration;
                    boneSubMovement.p1 = boneKeyPose.position;
                    boneSubMovement.q1 = boneKeyPose.rotation;
                    boneSubMovement.s1 = new Vector2(spring, damper);

                    AddFuture(new BoneSubMovementPair(body[boneKeyPose.boneId], boneSubMovement), s);
                    return;
                }
            }
            boneSubMovement.t0 = startTime + boneKeyPose.boneKeyPoseTiming[0] * duration;
            boneSubMovement.p0 = bone.transform.position;
            boneSubMovement.q0 = bone.transform.rotation;
            boneSubMovement.s0 = new Vector2(bone.springRatio, bone.damperRatio);

            boneSubMovement.t1 = startTime + boneKeyPose.boneKeyPoseTiming[1] * duration;
            boneSubMovement.p1 = boneKeyPose.position;
            boneSubMovement.q1 = boneKeyPose.rotation;
            boneSubMovement.s1 = new Vector2(spring, damper);

            AddFuture(new BoneSubMovementPair(body[boneKeyPose.boneId], boneSubMovement), s);
            return;
        }
        public void ClearLog() {
            foreach(var subMovementLog in subMovementLogs) {
                subMovementLog.ClearLog();
            }
        }
        public void ClearFuture() {
            foreach (var subMovementLog in subMovementLogs) {
                subMovementLog.ClearFuture();
            }
        }
        public void ClearAll() {
            ClearLog();
            ClearFuture();
        }
        public ActionLog Clone() {
            ActionLog clone = new ActionLog();
            clone.sceneLog = this.sceneLog.Clone();
            clone.subMovementLogs = new List<BoneSubMovementStream>();
            foreach(var subMovementLog in this.subMovementLogs) {
                clone.subMovementLogs.Add(subMovementLog.Clone());
            }
            return clone;
        }
    }

#if UNITY_EDITOR
    public class AutoInstantiateAttribute : Attribute { }
    public class AutoCreateInstanceAttribute : Attribute {  // 名前与えてあったら追加、なかったら作る
        public string name;
        public AutoCreateInstanceAttribute(string n) {
            this.name = n;
        }
    }
#endif    

    public class KeyPoseHandler : Dictionary<string, Pose>{
        // 変換途中の座標とか入れる
        void DrawHandlers() {
            foreach(var p in this) {

            }
        }
    }



    public abstract class ScriptableAction : MonoBehaviour {

        public bool isEditing;
        public bool actionEnabled;

        // 移行完了に伴い消します
        protected CancellationTokenSource tokenSource;
        protected CancellationToken cancelToken;

        protected Body body;

        public float timer;
        [HideInInspector]
        public List<KeyPoseTimePair> generatedKeyPoses;
        [HideInInspector]
        public List<KeyPoseTimePair> generatedKeyPosesHistory;
        [HideInInspector]
        public List<BoneSubMovementStream> boneSubMevements;
        [HideInInspector]
        public List<BoneSubMovementStream> boneSubMovementsHistory;

        protected CharacterSceneLogger sceneLog;

        // Use this for initialization
        public void Start() {
            timer = 0.0f;
            generatedKeyPoses = new List<KeyPoseTimePair>();
            generatedKeyPosesHistory = new List<KeyPoseTimePair>();
            boneSubMovementsHistory = new List<BoneSubMovementStream>();
            Type t = this.GetType();
            FieldInfo[] fields = t.GetFields();
            for(int i = 0; i < fields.Length; i++) {
                var field = fields[i];
                Debug.Log(field.Name + " " + field.FieldType);
                if (field.FieldType == typeof(KeyPoseData)) {
                    field.SetValue(this, Instantiate<KeyPoseData>((KeyPoseData)field.GetValue(this)));
                }
            }
            sceneLog = new CharacterSceneLogger();
        }

        // Update is called once per frame
        public void FixedUpdate() {
            generatedKeyPoses.Clear();
            GenerateMovement();
            if (actionEnabled) {
                ExecuteMovement();
                timer += Time.fixedDeltaTime;
            }
        }

        // Automatically create KeyPose
        void Reset() {
            Type t = this.GetType();
            string actionName = t.ToString();
            FieldInfo[] fields = t.GetFields();
#if UNITY_EDITOR
            foreach(var field in fields) {
                var createorAttribute = field.GetCustomAttributes(typeof(AutoCreateInstanceAttribute), false);
                if (createorAttribute.Length == 1) {
                    if (field.FieldType == typeof(KeyPoseData)) {

                    }
                }
            }
#endif
        }

        // ----- ----- ----- ----- ----- -----
        // 

        public void Begin(Body body = null) {
            if(body == null) { body = GameObject.FindObjectOfType<Body>(); }
            if (body == null) return;
            this.body = body;
            actionEnabled = true;
            timer = 0.0f;
            InitializeHistory();
            sceneLog.Save();
            // Turn Perception Generators on 
        }

        public void End() {
            actionEnabled = false;
            ResetAction();
            // Turn Perception Generators off
        }

        // ----- ----- ----- ----- ----- -----
        // Should be overrided in inherited Action Class

        // Generate KeyPoses considering action target
        virtual public void GenerateMovement() {
        }

        // Execute KeyPose when start time comes
        // 実行
        virtual public void ExecuteMovement() {
            // History全検索してLastStartとLastEndを取得
            float lastStart = 0.0f;
            float lastEnd = 0.0f;
            foreach(var history in generatedKeyPosesHistory) {
                if (lastStart < history.startTime) lastStart = history.startTime;
                if (lastEnd < history.endTime) lastEnd = history.endTime;
            }
            foreach (var k in generatedKeyPoses) {
                k.UpdateTimeInfo(lastStart, lastEnd);
                lastStart = k.startTime;
                lastEnd = k.endTime;
            }
            foreach(var keyPose in generatedKeyPoses) {
                if(keyPose.startTime <= timer) {
                    var logs = keyPose.keyPose.Action(body, duration:keyPose.duration, startTime:0.0f);
                    keyPose.isUsed = true;
                    generatedKeyPosesHistory.Add(new KeyPoseTimePair(keyPose));
                    AddSubMovementHistory(logs, keyPose.label);
                }
            }
        }

        virtual public void ResetAction() { }

        protected void ClearHistory() {
            boneSubMovementsHistory.Clear();
            generatedKeyPosesHistory.Clear();
        }

        private void InitializeHistory() {
            ClearHistory();
            if (this.body == null) return;
            foreach(var bone in body.bones) {
                if(bone.controller != null) {
                    boneSubMovementsHistory.Add(new BoneSubMovementStream(bone));
                }
            }
        }

        private void AddSubMovementHistory(List<BoneSubMovementPair> logs, string s) {
            foreach (var log in logs) {
                float duration = log.subMovement.t1 - log.subMovement.t0;
                log.subMovement.t0 += timer;
                log.subMovement.t1 += timer;
            }
            for (int i = 0; i < boneSubMovementsHistory.Count; i++) {
                foreach (var log in logs) {
                    if (log.bone == boneSubMovementsHistory[i].bone) boneSubMovementsHistory[i].AddLog(log, s);
                }
            }
        }

        void OnDrawGizmos() {
            foreach(var boneHistory in boneSubMovementsHistory) {
                if (boneHistory.bone.controller.controlPosition) {

                }
            }
        }
    }

}