using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEditor;

using SprUnity;

namespace SprUnity {

    [Serializable]
    public class ActionStateMachineStatus {
        public ActionStateMachine action;
        public bool isSelected = false;
        public List<ActionTransition> templeteTransition = new List<ActionTransition>();
    }

    public class ActionSelectWindow : EditorWindow, IHasCustomMenu {

        // インスタンス
        static ActionSelectWindow window;

        private GUISkin myskin;
        private string skinpath = "GUISkins/SprGUISkin.guiskin";

        // GUI
        private Vector2 scrollPos;

        [MenuItem("Window/Action Select Window")]
        static void Open() {
            window = GetWindow<ActionSelectWindow>();
            ActionEditorWindowManager.instance.actionSelectWindow = window;
            ReloadActionList();
        }

        public void AddItemsToMenu(GenericMenu menu) {
            menu.AddItem(new GUIContent("Reload"), false, () => {
                Open();
            });
        }

        public void OnEnable() {
            // <!!> これ、ここか？
            for (int i = 0; i < ActionEditorWindowManager.instance.actions.Count; i++) {
                var action = ActionEditorWindowManager.instance.actions[i];
                action.isSelected = SessionState.GetBool(action.action.name, false);
                Debug.Log(action.action.name + " " + action.isSelected + " " + SessionState.GetBool(action.action.name, false));
            }
            if (myskin == null) {
                var mono = MonoScript.FromScriptableObject(this);
                var scriptpath = AssetDatabase.GetAssetPath(mono);
                scriptpath = scriptpath.Replace("KeyPoseWindow.cs", "");
                myskin = AssetDatabase.LoadAssetAtPath<GUISkin>(scriptpath + skinpath);
            }
        }

        public void OnDisable() {
            foreach (var action in ActionEditorWindowManager.instance.actions) {
                SessionState.SetBool(action.action.name, action.isSelected);
            }
            window = null;
            ActionEditorWindowManager.instance.actionSelectWindow = null;
        }

        public void OnGUI() {
            if (window == null) Open();

            if (myskin != null) {
                GUI.skin = myskin;
            } else {
                Debug.Log("GUISkin is null");
            }

            bool textChangeComp = false;
            scrollPos = GUILayout.BeginScrollView(scrollPos);
            GUILayout.Label("Actions");
            if (window == null) GUILayout.Label("window null");
            if (ActionEditorWindowManager.instance.actionSelectWindow == null) GUILayout.Label("Manager.actionSelectWindow null");
            foreach (var action in ActionEditorWindowManager.instance.actions) {
                GUILayout.BeginHorizontal(GUILayout.Height(20));
                action.isSelected = GUILayout.Toggle(action.isSelected, "", GUILayout.Width(15));
                GUILayout.Label(action.action.name);
                GUILayout.EndHorizontal();
            }
            foreach (var action in ActionEditorWindowManager.instance.actions) {
                Debug.Log(action.action.name + " " + action.isSelected);
            }
            foreach (var obj in Selection.gameObjects) {
                var actions = obj.GetComponents<ScriptableAction>();
                for (int i = 0; i < actions.Count(); i++) {
                    GUILayout.BeginHorizontal(GUILayout.Height(20));
                    actions[i].isEditing = GUILayout.Toggle(actions[i].isEditing, "", GUILayout.Width(15));
                    GUILayout.Label(actions[i].name + "." + actions[i].GetType().ToString());
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndScrollView();

            GUI.skin = null;
        }

        public static void ReloadActionList() {
            if (!ActionEditorWindowManager.instance.actionSelectWindow) return;
            // Asset全検索
            var guids = AssetDatabase.FindAssets("*").Distinct();
            // 特定フォルダ
            // var keyPosesInFolder = AssetDatabase.FindAssets("t:KeyPoseInterpolationGroup", saveFolder);

            List<ActionStateMachineStatus> reloadedList = new List<ActionStateMachineStatus>();
            //ActionEditorWindowManager.instance.actions = new List<ActionStateMachineStatus>();

            foreach (var guid in guids) {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                var action = obj as ActionStateMachine;
                if (action != null && AssetDatabase.IsMainAsset(obj)) {
                    ActionStateMachineStatus actionStatus = new ActionStateMachineStatus();
                    actionStatus.action = action;
                    actionStatus.isSelected = false;
                    foreach(var existingAction in ActionEditorWindowManager.instance.actions) {
                        if(existingAction.action == action) {
                            actionStatus.isSelected = existingAction.isSelected;
                            actionStatus.templeteTransition = existingAction.templeteTransition;
                            continue;
                        }
                    }
                    reloadedList.Add(actionStatus);
                }
            }
            ActionEditorWindowManager.instance.actions = reloadedList;
        }

        void CreateAction() {

        }

        void DeleteAction() {

        }
    }

}