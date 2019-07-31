using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using SprCs;

namespace SprUnity {

#if UNITY_EDITOR
    [CustomEditor(typeof(ActionManager))]
    public class ActionManagerEditor : Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            ActionManager manager = (ActionManager)target;
            for(int i = 0; i < manager.actions.Count; i++) {
                if(GUILayout.Button(manager.actions[i].name)) {
                    manager.Action(manager.actions[i].name);
                }
            }
            if (GUILayout.Button("Stop")) {
                manager.QuitAction();
            }
        }
    }
#endif

    public class ActionManager : MonoBehaviour {

        public Body body = null;
        public BlendShapeController blendController;

        public List<ActionTargetGraph> keyPoseGraphs = new List<ActionTargetGraph>();
        public string[] stateMachineFolders = new string[] { };
        public List<ActionStateMachine> actions = new List<ActionStateMachine>();

        //[HideInInspector]
        public ActionStateMachine inAction = null;

        // ----- ----- ----- ----- -----

        private float time = 0.0f;

        // ----- ----- ----- ----- -----

        public ActionStateMachine this[string key] {
            get {
                foreach (var action in actions) { if (action.name == key) return action; }//.GetInstance(this); }
                return null;
            }
        }

        void Start() {
            /*
            for (int i = 0; i < keyPoseGraphs.Count; i++) {
                keyPoseGraphs[i].GetInstance(this);
            }
            for (int i = 0; i < actions.Count; i++) {
                actions[i].instances.Add(this, actions[i].Instantiate(this));
            }
            */
            for (int i = 0; i < keyPoseGraphs.Count; i++) {
                keyPoseGraphs[i].manager = this;
            }
            for (int i = 0; i < actions.Count; i++) {
                actions[i].manager = this;
            }
            inAction = null;
        }

        private void FixedUpdate() {
            if (body != null && inAction != null) {
                inAction.UpdateStateMachine();
            }
        }

        void OnDisable() {
            QuitAction();
        }

        // ----- ----- ----- ----- -----

        public void SetInput<T>(string graphName, string nodeName, object value) {
            foreach(var keyPoseGraph in keyPoseGraphs) {
                if(keyPoseGraph.name == graphName) {
                    var graph = keyPoseGraph;//.GetInstance(this);
                    foreach(var inputNode in graph.inputNodes) {
                        if (inputNode.name.Contains(nodeName)) {
                            (inputNode as ActionTargetNodeBase).SetInput<T>((T)value);
                        }
                    }
                }
            }
        }


        // ----- ----- ----- ----- -----

        public void Action(string name) {
            if (inAction != null) {
                if (inAction.name == name) return;
                else inAction.End();
            }
            print("Action: " + name);
            foreach (var action in actions) {
                if (action.name == name) {
                    inAction = action;//.instances[this];
                    inAction.Begin();
                }
            }
        }

        public void QuitAction() {
            if (inAction != null) {
                inAction.End();
                inAction = null;
            }
        }


        // ----- ----- ----- ----- -----

        public void GetActionStateMachineFromFolders() {
            actions.Clear();
            // 特定フォルダ
            var guids = AssetDatabase.FindAssets("t:ActionStateMachine", stateMachineFolders);

            foreach (var guid in guids) {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                var action = obj as ActionStateMachine;
                if (action != null && AssetDatabase.IsMainAsset(obj)) {
                    actions.Add(action);
                }
            }
        }

        public void GetActionTargetGraphFromStateMachines() {
            foreach(var action in actions) {
                var states = action.states;
                foreach(var state in states) {
                    foreach(var node in state.nodes) {
                        var graph = node.graph as ActionTargetGraph;
                        if(graph != null) {
                            if (!keyPoseGraphs.Contains(graph)) keyPoseGraphs.Add(graph);
                        }
                    }
                }
            }
        }
    }

}