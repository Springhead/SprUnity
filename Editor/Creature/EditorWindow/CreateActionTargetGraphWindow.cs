using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace SprUnity {
    public class CreateActionTargetGraphWindow : EditorWindow {
        public static CreateActionTargetGraphWindow window;
        private string newName = "";
        public static void Open(Vector2 vec) {
            window = GetWindow<CreateActionTargetGraphWindow>();
            window.titleContent = new GUIContent("CreateActionTargetGraph");
            var position = window.position;
            //position.center = new Rect(0f, 0f, Screen.currentResolution.width, Screen.currentResolution.height).center;
            position.center = vec;
            window.position = position;
            window.minSize = new Vector2(300, 22);
            window.maxSize = new Vector2(300, 22);
        }
        void OnGUI() {
            GUILayout.BeginHorizontal();
            var label = GUI.skin.GetStyle("label");
            var backLabel = label.fontSize;
            label.fontSize = 15;
            GUILayout.Label("New Graph Name", label);
            label.fontSize = backLabel;
            var textField = GUI.skin.GetStyle("textfield");
            var backTextField = textField.fontSize;
            textField.fontSize = 15;
            newName = GUILayout.TextField(newName, textField, GUILayout.Height(20));
            if (Event.current.keyCode == KeyCode.Return) {
                if (newName != "" && !existActionTargetGraph(newName)) {
                    var graph = ActionTargetGraph.CreateActionTargetGraph(newName);
                    AssetDatabase.CreateAsset(graph, "Assets/Actions/KeyPoses/" + newName + ".asset");
                    AssetDatabase.Refresh();
                    ActionTargetGraphEditorWindow.ReloadActionList();
                }
                textField.fontSize = backTextField;
                this.Close();
            }
            textField.fontSize = backTextField;
            GUILayout.EndHorizontal();
        }
        bool existActionTargetGraph(string name) {
            // Asset全検索
            var guids = AssetDatabase.FindAssets("*").Distinct();
            foreach (var guid in guids) {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                var actionStateMachine = obj as ActionTargetGraph;
                if (actionStateMachine) {
                    if (actionStateMachine.name == name) {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
