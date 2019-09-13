using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
namespace VGent{
    public class CreateActionTargetGraphWindow : EditorWindow {
        public static CreateActionTargetGraphWindow window;
        private string newName = "";
        private static List<string> pathList= new List<string>();
        private static int pathIndex = 0;

        public static void Open(Vector2 vec) {
            window = GetWindow<CreateActionTargetGraphWindow>();
            window.titleContent = new GUIContent("CreateActionTargetGraph");
            var position = window.position;
            //position.center = new Rect(0f, 0f, Screen.currentResolution.width, Screen.currentResolution.height).center;
            position.center = vec;
            window.position = position;
            window.minSize = new Vector2(300, 66);
            window.maxSize = new Vector2(300, 66);
            ReloadPathList();
        }
        void OnGUI() {
            pathIndex = EditorGUILayout.Popup(pathIndex, pathList.ToArray());
            if(GUILayout.Button("Add")) {
                var addFullPath = EditorUtility.OpenFolderPanel("Add Path", Application.dataPath, "");
                if (addFullPath != "") {
                    // Assetsの下のみ選べるようにする
                    if (!addFullPath.Contains(Application.dataPath)) {
                        Debug.LogError("Wrong path. Please choose in Assets/");
                    } else {
                        var addShortPath = addFullPath.Replace(Application.dataPath.Replace("Assets", ""), "");
                        if (!pathList.Contains(addShortPath)) {
                            pathList.Add(addShortPath.Replace("/", "\\")+"\\");
                            pathIndex = pathList.Count - 1;
                        } else {
                            pathIndex = pathList.IndexOf(addShortPath);
                        }
                    }
                }
            }
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
                    createGraphFromTemplate(newName, pathList[pathIndex], "Assets/Actions/KeyPoses/Template.asset");
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
        // Addする機能はいらない
        void createGraphFromTemplate(string name,string newPath,string templatePath) {
            //KeyPoseDataGroup.CreateKeyPoseDataGroupAsset();
            // Asset全検索
            var guids = AssetDatabase.FindAssets("*").Distinct();
            List<string> nameList = new List<string>();
            ActionTargetGraph templateActionTargetGraph = null;
            var templateObject = AssetDatabase.LoadAssetAtPath<Object>(templatePath);
            templateActionTargetGraph = templateObject as ActionTargetGraph;
            if (templateActionTargetGraph != null) {
                bool exist = false;
                foreach (var guid in guids) {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                    var actionTargetGraph = obj as ActionTargetGraph;
                    if (actionTargetGraph != null) {
                        if (actionTargetGraph.name == name) {
                            exist = true;
                            break;
                        }
                    }
                }
                if (!exist) {
                    var newActionTargetGraph = templateActionTargetGraph.Copy();
                    // この処理がなくても描画されるがProjectWindowでnodeが見えなくなる
                    //AssetDatabase.CreateAsset(newActionTargetGraph, "Assets/Actions/KeyPoses/" + "testtest.asset");
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
        public static void ReloadPathList() {
            pathList.Clear();
            // Asset全検索
            var guids = AssetDatabase.FindAssets("*").Distinct();

            foreach (var guid in guids) {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                var action = obj as ActionTargetGraph;
                if (action != null && AssetDatabase.IsMainAsset(obj)) {
                    var newPath = path.Replace(action.name+".asset","");
                    // "/"があるとPopUpでサブメニューになってしまうため"\"に変更
                    newPath = newPath.Replace("/", "\\");
                    if (!pathList.Contains(newPath)) {
                        pathList.Add(newPath);
                    }
                }
            }
        }
    }
}
