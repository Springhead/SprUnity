using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Reflection;
using UnityEngine;
using SprUnity;

#if UNITY_EDITOR
using UnityEditor;
#endif
using SprCs;

namespace VGent {

#if UNITY_EDITOR
    [CustomEditor(typeof(ActionManager))]
    public class ActionManagerEditor : Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            ActionManager manager = (ActionManager)target;
            for(int i = 0; i < manager.stateMachines.Count; i++) {
                if (manager.stateMachines[i] != null) {
                    if (GUILayout.Button(manager.stateMachines[i].name)) {
                        manager.Action(manager.stateMachines[i].name);
                    }
                }
            }
            if (GUILayout.Button("Stop")) {
                manager.QuitAction();
            }

            if (GUILayout.Button("Update List")) {
                manager.UpdateList();
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

        public static void UpdateFolderList() {
        }

        public static List<string> StateMachineFolders() {
            List<string> allStateMachineFolders = new List<string>();
            foreach (var actionManager in FindObjectsOfType<ActionManager>()) {
                foreach (var stateMachineFolder in actionManager.stateMachineFolders) {
                    if (!allStateMachineFolders.Contains(stateMachineFolder)) {
                        string folder = stateMachineFolder;
                        if (folder.Last() != '/') { folder = folder + "/"; }
                        allStateMachineFolders.Add(folder);
                    }
                }
            }
            return allStateMachineFolders;
        }

        public static List<string> TargetGraphFolders() {
            List<string> allTargetGraphFolders = new List<string>();
            foreach (var actionManager in FindObjectsOfType<ActionManager>()) {
                foreach (var targetGraphFolder in actionManager.targetGraphFolders) {
                    if (!allTargetGraphFolders.Contains(targetGraphFolder)) {
                        string folder = targetGraphFolder;
                        if (folder.Last() != '/') { folder = folder + "/"; }
                        allTargetGraphFolders.Add(folder);
                    }
                }
            }
            return allTargetGraphFolders;
        }

        public static ActionStateMachine FindStateMachine(string name) {
            foreach (var actionManager in FindObjectsOfType<ActionManager>()) {
                actionManager.UpdateList();
                foreach (var stateMachine in actionManager.stateMachines) {
                    if (stateMachine.name == name) { return stateMachine; }
                }
            }
            return null;
        }

        public static ActionTargetGraph FindTargetGraph(string name) {
            foreach (var actionManager in FindObjectsOfType<ActionManager>()) {
                actionManager.UpdateList();
                foreach (var targetGraph in actionManager.targetGraphs) {
                    if (targetGraph.name == name) { return targetGraph; }
                }
            }
            return null;
        }

        // ----- ----- ----- ----- -----

        public Body body = null;
        public BlendShapeController blendController;

        [FolderPath] public string[] stateMachineFolders = new string[] { };
        [FolderPath] public string[] targetGraphFolders = new string[] { };
        public List<ActionStateMachine> stateMachines = new List<ActionStateMachine>();
        public bool autoSetGraphs = true;
        public List<ActionTargetGraph> targetGraphs = new List<ActionTargetGraph>();

        //[HideInInspector]
        public ActionStateMachine inAction = null;

        // ----- ----- ----- ----- -----

        private float time = 0.0f;

        // ----- ----- ----- ----- -----

        [Obsolete("Please use GetStateMachine(name) instead")]
        public ActionStateMachine this[string key] {
            get {
                foreach (var action in stateMachines) { if (action.name == key) return action; }
                return null;
            }
        }

        void Start() {
            if (autoSetGraphs) {
                UpdateList();
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
                var stop = inAction.UpdateStateMachine();
                if (stop) {
                    QuitAction();
                }
            }
        }

        void OnDisable() {
            QuitAction();
        }

        // ----- ----- ----- ----- -----

        [Obsolete("Please use GetTargetGraph(graphName).SetInput<Type>(nodeName, value) instead")]
        public void SetInput<T>(string graphName, string nodeName, object value) {
            foreach(var keyPoseGraph in targetGraphs) {
                if(keyPoseGraph.name == graphName) {
                    var graph = keyPoseGraph;
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
                else QuitAction();
            }
            print("Action: " + name);
            foreach (var action in stateMachines) {
                if (action.name == name) {
                    inAction = action;
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

        public void UpdateList() {
#if UNITY_EDITOR
            // Action State Machine
            {
                stateMachines.Clear();
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

            // Action Target Graph
            {
                targetGraphs.Clear();
                var guids = AssetDatabase.FindAssets("t:ActionTargetGraph", targetGraphFolders);
                foreach (var guid in guids) {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                    var action = obj as ActionTargetGraph;
                    if (action != null && AssetDatabase.IsMainAsset(obj)) {
                        targetGraphs.Add(action);
                    }
                }
            }
#endif
        }

    }

}