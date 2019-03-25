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

    [Serializable]
    public class KeyPoseTimePair {
        public string label;
        public KeyPoseData keyPose;
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

    public class SubMovementLog {
        private string source;
        public SubMovement subMovement; 
        public SubMovementLog(string s, SubMovement sub) {
            this.source = s;
            this.subMovement = sub;
        }
    }

    [Serializable]
    public class BoneSubMovementLogs {
        public Bone bone;
        public List<SubMovementLog> subMovements;
        public BoneSubMovementLogs(Bone bone) {
            this.bone = bone;
            subMovements = new List<SubMovementLog>();
        }
        public void Add(BoneSubMovementPair subMovement, string s) {
            if (subMovement.bone == this.bone) subMovements.Add(new SubMovementLog(s, subMovement.subMovement));
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
    public class ActionTimeScheduler {

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
        public List<BoneSubMovementLogs> boneSubMevements;
        [HideInInspector]
        public List<BoneSubMovementLogs> boneSubMovementsHistory;

        // Use this for initialization
        public void Start() {
            timer = 0.0f;
            generatedKeyPoses = new List<KeyPoseTimePair>();
            generatedKeyPosesHistory = new List<KeyPoseTimePair>();
            boneSubMovementsHistory = new List<BoneSubMovementLogs>();
            Type t = this.GetType();
            FieldInfo[] fields = t.GetFields();
            for(int i = 0; i < fields.Length; i++) {
                var field = fields[i];
                Debug.Log(field.Name + " " + field.FieldType);
                if (field.FieldType == typeof(KeyPoseData)) {
                    field.SetValue(this, Instantiate<KeyPoseData>((KeyPoseData)field.GetValue(this)));
                }
            }
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
                    boneSubMovementsHistory.Add(new BoneSubMovementLogs(bone));
                }
            }
        }

        private void AddSubMovementHistory(List<BoneSubMovementPair> logs, string s) {
            foreach (var log in logs) {
                float duration = log.subMovement.t1 - log.subMovement.t0;
                log.subMovement.t0 = timer;
                log.subMovement.t1 = timer + duration;
            }
            for (int i = 0; i < boneSubMovementsHistory.Count; i++) {
                foreach (var log in logs) {
                    if (log.bone == boneSubMovementsHistory[i].bone) boneSubMovementsHistory[i].Add(log, s);
                }
            }
        }
    }

}