﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace SprUnity {
    public class CreateActionStateMachineWindow : EditorWindow {
        public static CreateActionStateMachineWindow window;
        private string newName = "";
        private static List<string> pathList= new List<string>();
        private static int pathIndex = 0;

        public static void Open(Vector2 vec) {
            window = GetWindow<CreateActionStateMachineWindow>();
            window.titleContent = new GUIContent("CreateActionStateMachine");
            var position = window.position;
            //position.center = new Rect(0f, 0f, Screen.currentResolution.width, Screen.currentResolution.height).center;
            position.center = vec;
            window.position = position;
            window.minSize = new Vector2(300, 44);
            window.maxSize = new Vector2(300, 44);
            ReloadPathList();
        }
        void OnGUI() {
            pathIndex = EditorGUILayout.Popup(pathIndex, pathList.ToArray());
            GUILayout.BeginHorizontal();
            var label = GUI.skin.GetStyle("label");
            var backLabel = label.fontSize;
            label.fontSize = 15;
            GUILayout.Label("New Action Name", label);
            label.fontSize = backLabel;
            var textField = GUI.skin.GetStyle("textfield");
            var backTextField = textField.fontSize;
            textField.fontSize = 15;
            newName = GUILayout.TextField(newName, textField, GUILayout.Height(20));
            if (Event.current.keyCode == KeyCode.Return) {
                if (newName != "" && !existActionStateMachine(newName)) {
                    ActionStateMachine.CreateStateMachine(newName,pathList[pathIndex]);
                }
                textField.fontSize = backTextField;
                this.Close();
            }
            textField.fontSize = backTextField;
            GUILayout.EndHorizontal();
        }
        bool existActionStateMachine(string name) {
            // Asset全検索
            var guids = AssetDatabase.FindAssets("*").Distinct();
            foreach (var guid in guids) {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                var actionStateMachine = obj as ActionStateMachine;
                if (actionStateMachine) {
                    if (actionStateMachine.name == name) {
                        return true;
                    }
                }
            }
            return false;
        }
        public static void ReloadPathList() {
            pathList.Clear();
            // Asset全検索
            var guids = AssetDatabase.FindAssets("*").Distinct();

            foreach (var guid in guids) {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                var action = obj as ActionStateMachine;
                if (action != null && AssetDatabase.IsMainAsset(obj)) {
                    var newPath = path.Replace(action.name+".asset","");
                    if (!pathList.Contains(newPath)) {
                        pathList.Add(newPath);
                    }
                }
            }
        }
    }
}
