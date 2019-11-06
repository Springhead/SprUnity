using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
namespace VGent{
    public class CreateActionTargetGraphWindow : EditorWindow {
        public static CreateActionTargetGraphWindow window;
        private string newName = "";
        private string templateName = "Template";
        // private static List<string> pathList= new List<string>();
        private static int pathIndex = 0;

        public static void Open(Vector2 vec) {
            window = GetWindow<CreateActionTargetGraphWindow>();
            window.titleContent = new GUIContent("CreateActionTargetGraph");
            var position = window.position;
            //position.center = new Rect(0f, 0f, Screen.currentResolution.width, Screen.currentResolution.height).center;
            position.center = vec;
            window.position = position;
            window.minSize = new Vector2(300, 88);
            window.maxSize = new Vector2(300, 88);
        }
        void OnGUI() {
            var pathList = ActionManager.TargetGraphFolders();
            pathIndex = EditorGUILayout.Popup(pathIndex, pathList.ToArray());

            GUILayout.BeginHorizontal();
            GUILayout.Label("Template Name");
            templateName = GUILayout.TextField(templateName);
            GUILayout.EndHorizontal();

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
                if (newName != "" && ActionManager.FindTargetGraph(newName) == null) {
                    createGraphFromTemplate(newName, pathList[pathIndex], templateName);
                    //var graph = ActionTargetGraph.CreateActionTargetGraph(newName);
                    //AssetDatabase.CreateAsset(graph, pathList[pathIndex] + newName + ".asset");
                    //AssetDatabase.Refresh();
                    //ActionTargetGraphEditorWindow.ReloadActionList();
                }
                textField.fontSize = backTextField;
                this.Close();
            }
            textField.fontSize = backTextField;
            GUILayout.EndHorizontal();
        }


        // Addする機能はいらない
        void createGraphFromTemplate(string name, string newPath, string templateName) {
            var templateObject = AssetDatabase.LoadAssetAtPath<Object>(newPath + templateName + ".asset");
            ActionTargetGraph templateActionTargetGraph = templateObject as ActionTargetGraph;

            Debug.Log(newPath + templateName + ".asset");
            Debug.Log("Template : " + templateActionTargetGraph);

            if (templateActionTargetGraph != null) {
                if (ActionManager.FindTargetGraph(name) == null) {
                    var newActionTargetGraph = templateActionTargetGraph.Copy();

                    // この処理がなくても描画されるがProjectWindowでnodeが見えなくなる
                    AssetDatabase.CreateAsset(newActionTargetGraph, newPath + name + ".asset");
                    foreach (var node in newActionTargetGraph.nodes) {
                        node.name = node.name.Replace("(Clone)", "");
                        node.name = node.name.Replace("Template", name);
                        AssetDatabase.AddObjectToAsset(node, newActionTargetGraph);
                    }

                    AssetDatabase.Refresh();
                    ActionTargetGraphEditorWindow.ReloadActionList();
                    Repaint();
                }
            }
        }
    }
}
