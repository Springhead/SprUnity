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
        public ActionStateMachine stateMachineAction;
        public bool isSelected = false;
        public List<ActionTransition> specifiedTransition = new List<ActionTransition>();
        //
        public string name {
            get {
                if (stateMachineAction != null) return stateMachineAction.name;
                return "";
            }
        }
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
            ReloadActionList();
        }

        public void AddItemsToMenu(GenericMenu menu) {
            menu.AddItem(new GUIContent("Reload"), false, () => {
                Open();
            });
        }

        public void OnEnable() {
        }

        public void OnDisable() {
            window = null;
        }

        public void OnGUI() {
        }

        public static void ReloadActionList() {
            // Asset全検索
            var guids = AssetDatabase.FindAssets("*").Distinct();
            // 特定フォルダ
            // var keyPosesInFolder = AssetDatabase.FindAssets("t:KeyPoseInterpolationGroup", saveFolder);

            List<ActionStateMachine> reloadedList = new List<ActionStateMachine>();
            //ActionEditorWindowManager.instance.actions = new List<ActionStateMachineStatus>();

            foreach (var guid in guids) {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                var action = obj as ActionStateMachine;
                if (action != null && AssetDatabase.IsMainAsset(obj)) {
                    reloadedList.Add(action);
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