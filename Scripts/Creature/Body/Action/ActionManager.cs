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
            for(int i = 0; i < manager.stateMachines.Count; i++) {
                if(GUILayout.Button(manager.stateMachines[i].name)) {
                    manager.Action(manager.stateMachines[i].name);
                }
            }
            if (GUILayout.Button("Stop")) {
                manager.QuitAction();
            }

            if (GUILayout.Button("SetASM")) {
                manager.GetActionStateMachineFromFolders();
            }
            if (GUILayout.Button("SetATG")) {
                manager.GetActionTargetGraphFromStateMachines();
            }
        }
    }
#endif

    public class FolderPathAttribute : PropertyAttribute {

    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(FolderPathAttribute))]
    public class FolderPathAttributeDrawer : PropertyDrawer {
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label) {
            if(property.propertyType != SerializedPropertyType.String) {
                return;
            }
            try {
                GUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                var pathText = EditorGUI.TextField(rect, property.stringValue);
                if (EditorGUI.EndChangeCheck()) {
                    property.stringValue = pathText;
                }
                if (GUILayout.Button("Folder")) {
                    var path = EditorUtility.SaveFolderPanel("ActionStateMachine Folder", "Assets", "");
                    if (path.Length != 0) {
                        property.stringValue = FileUtil.GetProjectRelativePath(path);
                    }
                }
                GUILayout.EndHorizontal();
            } catch {
                EditorGUI.PropertyField(rect, property, label);
            }
        }
    }
#endif

    public class ActionManager : MonoBehaviour {

        public Body body = null;
        public BlendShapeController blendController;

        [FolderPath] public string[] stateMachineFolders = new string[] { };
        public List<ActionStateMachine> stateMachines = new List<ActionStateMachine>();
        public bool autoSetGraphs = true;
        public List<ActionTargetGraph> targetGraphs = new List<ActionTargetGraph>();

        //[HideInInspector]
        public ActionStateMachine inAction = null;

        // ----- ----- ----- ----- -----

        private float time = 0.0f;

        // ----- ----- ----- ----- -----

        public ActionStateMachine this[string key] {
            get {
                foreach (var action in stateMachines) { if (action.name == key) return action; }//.GetInstance(this); }
                return null;
            }
        }

        void Start() {
            /*
            for (int i = 0; i < targetGraphs.Count; i++) {
                targetGraphs[i].GetInstance(this);
            }
            for (int i = 0; i < stateMachines.Count; i++) {
                stateMachines[i].instances.Add(this, stateMachines[i].Instantiate(this));
            }
            */
            if (autoSetGraphs) {
                GetActionTargetGraphFromStateMachines();
            }
            for (int i = 0; i < targetGraphs.Count; i++) {
                targetGraphs[i].manager = this;
            }
            for (int i = 0; i < stateMachines.Count; i++) {
                stateMachines[i].manager = this;
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
            foreach(var keyPoseGraph in targetGraphs) {
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

        public ActionStateMachine GetStateMachine(string name) {
            foreach (var action in stateMachines) { if (action.name == name) return action; }
            return null;
        }

        public ActionTargetGraph GetTargetGraph(string name) {
            foreach (var graph in targetGraphs) { if (graph.name == name) return graph; }
            return null;
        }

        // ----- ----- ----- ----- -----

        public void Action(string name) {
            if (inAction != null) {
                if (inAction.name == name) return;
                else inAction.End();
            }
            print("Action: " + name);
            foreach (var action in stateMachines) {
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
            stateMachines.Clear();
            // 
            var guids = AssetDatabase.FindAssets("t:ActionStateMachine", stateMachineFolders);

            foreach (var guid in guids) {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                var action = obj as ActionStateMachine;
                if (action != null && AssetDatabase.IsMainAsset(obj)) {
                    stateMachines.Add(action);
                }
            }
        }

        public void GetActionTargetGraphFromStateMachines() {
            foreach(var action in stateMachines) {
                var states = action?.states;
                foreach(var state in states) {
                    foreach(var node in state?.nodes) {
                        var graph = node?.graph as ActionTargetGraph;
                        Debug.Log(graph?.name);
                        if(graph != null) {
                            if (!targetGraphs.Contains(graph)) targetGraphs.Add(graph);
                        }
                    }
                }
            }
        }
    }

}