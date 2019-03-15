using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SprUnity {

    public class BoneSubMevementPair {
        public HumanBodyBones boneId;
        public List<SubMovement> subMovements;
    }

    public class KeyPoseTimePair {
        public KeyPose keyPose;
        public enum StartCondition {
            AbsoluteTime,
            AbsoluteTimeFromPreviousKeyPoseStart,
            AbsoluteTimeFromPreviousKeyPoseEnd,
            RelativeTimeFromPreviousKeyPoseStart,
            RelativeTimeFromPreviousKeyPoseEnd,
        }
        public float startTime;
        public enum DurationCondition {
            AbsoluteTimeLength,
            ProportionalToDistance,
        }
        public float duration;
        public float endTime {
            get { return startTime + duration; }
        }
    }

#if UNITY_EDITOR
    public class AutoInstantiateAttribute : Attribute { }
    public class AutoCreateInstanceAttribute : Attribute {  // 名前与えてあったら追加、なかったら作る
        public string name;
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
        public List<KeyPoseTimePair> generatedKeyPoses;
        public List<KeyPoseTimePair> generatedKeyPosesHistory;
        public List<BoneSubMevementPair> boneSubMevements;
        public List<BoneSubMevementPair> boneSubMovementsHistory;

        // Use this for initialization
        public void Start() {
            timer = 0.0f;
            Type t = this.GetType();
            FieldInfo[] fields = t.GetFields();
            for(int i = 0; i < fields.Length; i++) {
                var field = fields[i];
                Debug.Log(field.Name + " " + field.FieldType);
                if (field.FieldType == typeof(KeyPose)) {
                    field.SetValue(this, Instantiate<KeyPose>((KeyPose)field.GetValue(this)));
                    Debug.Log(field.Name);
                }
            }
        }

        // Update is called once per frame
        public void FixedUpdate() {
            GenerateMovement();
            ExecuteMovement();
        }

        void Reset() {

        }

        virtual public KeyPose[] GenerateMovement() { return null; }
        virtual public void ExecuteMovement() { }

        protected void GenerateSubMovement() {

        }

        protected void RefleshSubMovementsHistory() {
            boneSubMovementsHistory.Clear();
        }
    }

}