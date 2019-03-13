using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SprUnity {

    public class ScriptableAction : MonoBehaviour {

        public bool isEditing;
        public bool actionEnabled;

        protected CancellationTokenSource tokenSource;
        protected CancellationToken cancelToken;

        protected Body body;

        // 編集に関してはLogをとってそれを編集することで
        // submovement(class?)の一覧
        // waitTime(class?)の一覧

        // Use this for initialization
        void Start() {

        }

        // Update is called once per frame
        void FixedUpdate() {

        }

        void OnDisable() {
            EndAction();
        }

        public virtual void BeginAction(Body body) {

        }
        public void UpdateAction() {

        }
        public void EndAction() {

        }

        // Actionの目的
        public virtual float Objective() {
            return 0;
        }

        public void Action() {
            // 
        }
    }

}