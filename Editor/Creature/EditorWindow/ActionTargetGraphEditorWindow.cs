using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using XNode;
using XNodeEditor;
using SprUnity;
using System.Linq;

namespace VGent {

    public class ActionTargetGraphEditorWindow : XNodeEditor.NodeEditorWindow, IHasCustomMenu {
        private float handleSize = 0.05f;
        private float selectedHandleSize = 0.15f;
        private StaticBoneKeyPose selectedboneKeyPose; // マウスが上にあるKeyPoseだけハンドルを大きくする
        private List<ActionTargetOutputNode> editableBoneKeyPoseNodes = new List<ActionTargetOutputNode>();

        private bool showHandles = true;
        private bool showBones = true;

        private static bool guiInitialized = false;
        public static GUIStyle toolbarBase;
        public static GUIStyle toolbarButton;
        public static GUIStyle toolbarLabel;
        public static GUIStyle toolbarDropdown;
        public static GUIStyle toolbarPopup;

        private static List<string> actionTargetGraphNames;

        // SubWindow
        private bool showSubWindow = false;
        private float subWindowFirstSpaceNum = 8;
        private float subWindowWidth = 200;
        static Texture2D noneButtonTexture = null;

        private GUISkin myskin;
        private string skinpath = "GUISkins/SprGUISkin.guiskin";
        private string editableButtonpath = "pictures/te.png";
        private string editableLabelpath = "GUISkins/labelbackEditable.png";

        private Vector2 scrollPos;
        private Vector2 scrollPosParameterWindow;

        static float scrollWidth = 20;
        static float parameterHeight = 150;
        static float buttonHeight = 25;
        static float buttonWidth = 40;

        private ActionTargetGraph latestEditableKeyPose;
        private ActionTargetGraph latestVisibleKeyPose;
        private static Dictionary<ActionTargetGraphStatus, Rect> actionTargetGraphRectDict;
        private static Dictionary<ActionTargetGraphStatus, Rect> tempActionTargetGraphRectDict;

        private static ActionTargetGraph renameActionTargetGraph;
        private string renaming;

        static private float parameterWindowHeight = 160;

        protected override void OnEnable() {
            base.OnEnable();

            var modelpath = "Assets/Libraries/SprUnity/Editor/Creature/Models/";

            if (ActionEditorWindowManager.instance.editableMat == null) {
                ActionEditorWindowManager.instance.editableMat = AssetDatabase.LoadAssetAtPath(modelpath + "editable.mat", typeof(Material)) as Material;
            }
            if (ActionEditorWindowManager.instance.visibleMat == null) {
                ActionEditorWindowManager.instance.visibleMat = AssetDatabase.LoadAssetAtPath(modelpath + "visible.mat", typeof(Material)) as Material;
            }

            if (ActionEditorWindowManager.instance.leftHand == null) {
                ActionEditorWindowManager.instance.leftHand = AssetDatabase.LoadAssetAtPath(
                    modelpath + "LeftHand.fbx", typeof(Mesh)) as Mesh;
            }

            if (ActionEditorWindowManager.instance.rightHand == null) {
                ActionEditorWindowManager.instance.rightHand = AssetDatabase.LoadAssetAtPath(
                    modelpath + "RightHand.fbx", typeof(Mesh)) as Mesh;
            }

            if (ActionEditorWindowManager.instance.head == null) {
                ActionEditorWindowManager.instance.head = AssetDatabase.LoadAssetAtPath(
                    modelpath + "Head.fbx", typeof(Mesh)) as Mesh;
            }

            if (ActionEditorWindowManager.instance.leftFoot == null) {
                ActionEditorWindowManager.instance.leftFoot = AssetDatabase.LoadAssetAtPath(
                    modelpath + "LeftFoot.fbx", typeof(Mesh)) as Mesh;
            }

            if (ActionEditorWindowManager.instance.rightFoot == null) {
                ActionEditorWindowManager.instance.rightFoot = AssetDatabase.LoadAssetAtPath(
                    modelpath + "RightFoot.fbx", typeof(Mesh)) as Mesh;
            }
            // SubWindow
            if (myskin == null) {
                var mono = MonoScript.FromScriptableObject(this);
                var scriptpath = AssetDatabase.GetAssetPath(mono);
                scriptpath = scriptpath.Replace("EditorWindow/ActionTargetGraphEditorWindow.cs", "");
                myskin = AssetDatabase.LoadAssetAtPath<GUISkin>(scriptpath + skinpath);
            }
            if (ActionEditorWindowManager.instance.visibleButtonTexture == null) {
                ActionEditorWindowManager.instance.visibleButtonTexture = EditorGUIUtility.Load("ViewToolOrbit On") as Texture2D;
            }
            if (ActionEditorWindowManager.instance.editableLabelTexture == null) {
                ActionEditorWindowManager.instance.editableLabelTexture = GetEditableLabelTexture();
            }
            tempActionTargetGraphRectDict = new Dictionary<ActionTargetGraphStatus, Rect>();

            InitAtionTargetGraphRectDict();

            SceneView.onSceneGUIDelegate -= OnSceneGUI;
            SceneView.onSceneGUIDelegate += OnSceneGUI;
        }

        private void OnDestroy() {
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
        }

        [OnOpenAsset(0)]
        public static bool OnOpen(int instanceID, int line) {
            ActionTargetGraph nodeGraph = EditorUtility.InstanceIDToObject(instanceID) as ActionTargetGraph;
            //toolbarBase = GUI.skin.FindStyle("toolbar");
            //toolbarButton = GUI.skin.FindStyle("toolbarButton");
            //toolbarLabel = GUI.skin.FindStyle("toolbarButton");
            //toolbarDropdown = GUI.skin.FindStyle("toolbarDropdown");
            //toolbarPopup = GUI.skin.FindStyle("toolbarPopup");
            if (nodeGraph != null) {
                Open(nodeGraph);
                return true;
            }
            return false;
        }
        [MenuItem("Window/SprUnity Action/Action Target Graph Editor Window")]
        static void Open() {
            ReloadActionList();
            if (ActionEditorWindowManager.instance.actionTargetGraphStatuses != null) {
                ActionTargetGraphEditorWindow w = GetWindow(typeof(ActionTargetGraphEditorWindow), false, "ActionTargetGraph", true) as ActionTargetGraphEditorWindow;
                w.wantsMouseMove = true;
                w.graph = ActionEditorWindowManager.instance.actionTargetGraphStatuses[0].actionTargetGraph;
            }
        }
        public static void Open(ActionTargetGraph graph) {
            if (!graph) return;

            ActionTargetGraphEditorWindow w = GetWindow(typeof(ActionTargetGraphEditorWindow), false, "ActionTargetGraph", true) as ActionTargetGraphEditorWindow;
            w.wantsMouseMove = true;
            w.graph = graph;
        }

        public void AddItemsToMenu(GenericMenu menu) {
            menu.AddItem(new GUIContent("Reload"), false, () => {
                ReloadActionTargetGraphs();
            });
            menu.AddItem(new GUIContent("Reload list"), false, () => {
                ReloadActionList();
            });
        }

        public new void OnGUI() {
            base.OnGUI();

            if (!guiInitialized) {
                toolbarBase = GUI.skin.FindStyle("toolbar");
                toolbarButton = GUI.skin.FindStyle("toolbarButton");
                toolbarLabel = GUI.skin.FindStyle("toolbarButton");
                toolbarDropdown = GUI.skin.FindStyle("toolbarDropdown");
                toolbarPopup = GUI.skin.FindStyle("toolbarPopup");
                guiInitialized = true;
            }
            DrawToolBar();
            SubWindow();
        }

        void DrawToolBar() {
            int toolBarHeight = 17;
            // <!!> まだ微妙にずれてるけどこれで上に表示できる
            Rect rect = new Rect(0, 19 * zoom, this.position.width, toolBarHeight);

            GUILayout.BeginArea(rect, toolbarBase);
            GUILayout.BeginHorizontal();
            /*
            GUILayout.Label("Current Graph", toolbarLabel, GUILayout.Width(100));

            //actionIndex = ActionEditorWindowManager.instance.actions.IndexOf(ActionEditorWindowManager.instance.selectedAction);
            actionTargetGraphIndex = EditorGUILayout.Popup(actionTargetGraphIndex, actionTargetGraphNames.ToArray(), toolbarPopup, GUILayout.Width(120));
            foreach (var act in ActionEditorWindowManager.instance.actionTargetGraphs) {
                if (act.name == actionTargetGraphNames[actionTargetGraphIndex]) {
                    //if (ActionEditorWindowManager.instance.selectedActionTargetGraph != act) ActionEditorWindowManager.instance.actionSelectChanged = true;
                    this.graph = act;
                }
            }

            if (GUILayout.Button("Create", toolbarButton, GUILayout.Width(70))) {
                CreateActionTargetGraphWindow.Open(position.center);
            }

            GUILayout.Space(10);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Menu", toolbarButton, GUILayout.Width(50))) {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Create"), false, null);
                menu.AddItem(new GUIContent("Home"), true, () => { this.zoom = 1.0f; this.panOffset = Vector2.zero; });

                menu.DropDown(new Rect(0, -20, 50, 40));
            }
            */
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
        void SubWindow() {
            Event e = Event.current;
            GUISkin defaultSkin = GUI.skin;
            Color defaultFoldoutTextColor = EditorStyles.foldout.onNormal.textColor;
            if (myskin != null) {
                GUI.skin = myskin;
                EditorStyles.foldout.onNormal.textColor = Color.white;
            }

            //for (int i = 0; i < subWindowFirstSpaceNum; i++) {
            GUILayout.Space(19 * zoom + 20);
            //}
            showSubWindow = EditorGUILayout.Foldout(showSubWindow, "SubWidnow");
            var showSubWindowRect = GUILayoutUtility.GetLastRect();
            if (!showSubWindow) {
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.Height(position.height - parameterHeight));

                var actionTargetGraphStatuses = ActionEditorWindowManager.instance.actionTargetGraphStatuses;

                GUILayout.Label("ActionTargetGraphs", GUILayout.Width(subWindowWidth - scrollWidth));
                //if (window == null) {
                //    Open(); // なぜかOnEnableに書くと新しくwindowが生成される
                //    // 選択が消えてしまうので残っている情報からフラグを正しくする
                //    // latest系がstaticにできないのでReloadKeyPoseList内に書けない(staticにするとプレイすると初期化される)
                //    foreach (var keyPoseStatus in keyPoseStatuses) {
                //        if (keyPoseStatus.keyPose == latestEditableKeyPose) {
                //            keyPoseStatus.isEditable = true;
                //        }
                //        if (keyPoseStatus.keyPose == latestVisibleKeyPose) {
                //            keyPoseStatus.isVisible = true;
                //        }
                //    }
                //}
                EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(subWindowWidth - scrollWidth));
                tempActionTargetGraphRectDict.Clear();
                foreach (var actionTargetGraphStatus in actionTargetGraphStatuses) {
                    //Rect singleRect = GUILayoutUtility.GetRect(windowWidth, 30);
                    //GUILayout.BeginArea(singleRect);
                    GUILayout.BeginHorizontal();
                    Texture2D currentTexture = noneButtonTexture;
                    if (actionTargetGraphStatus.isVisible) {
                        currentTexture = ActionEditorWindowManager.instance.visibleButtonTexture;
                    } else {
                        currentTexture = noneButtonTexture;
                    }
                    if (GUILayout.Button(currentTexture, GUILayout.Width(buttonHeight), GUILayout.Height(buttonHeight))) {
                        if (!actionTargetGraphStatus.isVisible) {
                            actionTargetGraphStatus.isVisible = true;
                            latestVisibleKeyPose = actionTargetGraphStatus.actionTargetGraph;
                            foreach (var keyPoseStatus2 in actionTargetGraphStatuses) {
                                if (keyPoseStatus2.isVisible && keyPoseStatus2.actionTargetGraph != latestVisibleKeyPose) {
                                    keyPoseStatus2.isVisible = false;
                                }
                            }
                            SceneView.RepaintAll();
                        } else {
                            actionTargetGraphStatus.isVisible = false;
                            if (latestVisibleKeyPose == actionTargetGraphStatus.actionTargetGraph) {
                                latestVisibleKeyPose = null;
                            }
                            SceneView.RepaintAll();
                        }
                    }
                    if (Application.isPlaying) {
                        if (GUILayout.Button("Play", GUILayout.Width(buttonWidth), GUILayout.Height(buttonHeight))) {
                            KeyPose keyPose = new KeyPose();
                            foreach (var obj in actionTargetGraphStatus.actionTargetGraph.nodes) {
                                ActionTargetOutputNode actionTargetOutputNode = obj as ActionTargetOutputNode;
                                if (actionTargetOutputNode != null) {
                                    if (actionTargetOutputNode.enablePlay) {
                                        // ここでActionを呼びたい
                                        foreach (var boneKeyPose in keyPose.boneKeyPoses) {
                                            if (boneKeyPose.boneId == actionTargetOutputNode.boneKeyPose.boneId) {
                                                continue;
                                            }
                                        }
                                        keyPose.boneKeyPoses.Add(actionTargetOutputNode.GetBoneKeyPose());
                                    }
                                }
                            }
                            keyPose.Action(actionTargetGraphStatus.actionTargetGraph.body);
                        }
                    }
                    var defaultback = GUI.skin.label.normal.background;
                    if (actionTargetGraphStatus.actionTargetGraph == this.graph) {
                        GUI.skin.label.normal.background = ActionEditorWindowManager.instance.editableLabelTexture;
                    }
                    if (actionTargetGraphStatus.actionTargetGraph == renameActionTargetGraph) {
                        renaming = GUILayout.TextField(renaming, GUILayout.Height(buttonHeight));
                        if (Event.current.keyCode == KeyCode.Return) {
                            Undo.RecordObject(actionTargetGraphStatus.actionTargetGraph, "Change KeyPose Name");
                            renameActionTargetGraph.name = renaming;
                            renameActionTargetGraph = null;
                            renaming = "";
                            EditorUtility.SetDirty(actionTargetGraphStatus.actionTargetGraph);
                            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(actionTargetGraphStatus.actionTargetGraph));
                            Repaint();
                        }
                    } else {
                        GUILayout.Label(actionTargetGraphStatus.actionTargetGraph.name, GUILayout.Height(buttonHeight));
                    }
                    GUI.skin.label.normal.background = defaultback;
                    // <!!>毎回呼ぶのか..
                    //GUI.Box(GUILayoutUtility.GetLastRect(), actionTargetGraphStatus.actionTargetGraph.name);
                    LeftClick(GUILayoutUtility.GetLastRect(), actionTargetGraphStatus.actionTargetGraph);

                    tempActionTargetGraphRectDict[actionTargetGraphStatus] = GUILayoutUtility.GetLastRect();
                    GUILayout.EndHorizontal();
                }
                if (GUILayout.Button("Add")) {
                    CreateActionTargetGraphWindow.Open(position.center);
                }
                //if (GUILayout.Button("Play")) {
                //    foreach (var obj in graph.nodes) {
                //        ActionTargetOutputNode actionTargetOutputNode = obj as ActionTargetOutputNode;
                //        if (actionTargetOutputNode != null) {
                //            // ここでActionを呼びたい
                //            KeyPose keyPose = new KeyPose();
                //            keyPose.boneKeyPoses.Add(actionTargetOutputNode.GetBoneKeyPose());
                //            var actionTargetGraph = graph as ActionTargetGraph;
                //            if (actionTargetGraph != null) {
                //                keyPose.Action(actionTargetGraph.body);
                //            }
                //        }
                //    }
                //}
                showHandles = GUILayout.Toggle(showHandles, "Show Handles");
                showBones = GUILayout.Toggle(showBones, "Show Bone Meshes");
                EditorGUILayout.EndVertical();

                EditorGUILayout.EndScrollView();
            }

            foreach (var temp in tempActionTargetGraphRectDict) {
                if (actionTargetGraphRectDict.ContainsKey(temp.Key)) {
                    actionTargetGraphRectDict[temp.Key] = temp.Value;
                }
            }
            // Foldoutによる描画のずれを修正
            foreach (var actionTargetGraphRect in actionTargetGraphRectDict) {
                var rect = actionTargetGraphRect.Value;
                rect.y += showSubWindowRect.y + showSubWindowRect.height;
                //GUI.Box(rect, actionTargetGraphRect.Key.actionTargetGraph.name);
                RightClickMenu(rect, actionTargetGraphRect.Key.actionTargetGraph);
            }
            GUI.skin = defaultSkin; // 他のwindowに影響が出ないように元に戻す
            EditorStyles.foldout.onNormal.textColor = defaultFoldoutTextColor;
        }
        void LeftClick(Rect rect, ActionTargetGraph actionTargetGraph) {
            if (rect.Contains(Event.current.mousePosition) &&
                Event.current.type == EventType.MouseDown &&
                Event.current.button == 0) {
                if (this.graph == actionTargetGraph) {
                    this.graph = null;
                } else {
                    this.graph = actionTargetGraph;
                }
            }
        }
        void RightClickMenu(Rect rect, ActionTargetGraph actionTargetGraph) {
            if (rect.Contains(Event.current.mousePosition) &&
                Event.current.type == EventType.MouseDown &&
                Event.current.button == 1) {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Rename"), false,
                    () => {
                        renameActionTargetGraph = actionTargetGraph;
                        renaming = actionTargetGraph.name;
                        Repaint();
                    });
                menu.AddItem(new GUIContent("Delete"), false,
                    () => {
                        RemoveActionTargetGraph(actionTargetGraph);
                        Repaint();
                        SceneView.RepaintAll();
                    });
                menu.ShowAsContext();
            }
        }
        void RemoveActionTargetGraph(ActionTargetGraph actionTargetGraph) {
            if (actionTargetGraph == null) {
                Debug.LogWarning("No asset.");
                return;
            }
            bool removeSelectedGraph = false;
            if (actionTargetGraph == this.graph) {
                removeSelectedGraph = true;
            }
            string path = AssetDatabase.GetAssetPath(actionTargetGraph);
            AssetDatabase.MoveAssetToTrash(path);
            ReloadActionList();
            if (removeSelectedGraph) {
                if (ActionEditorWindowManager.instance.actionTargetGraphStatuses.Count != 0) {
                    this.graph = ActionEditorWindowManager.instance.actionTargetGraphStatuses[0].actionTargetGraph;
                } else {
                    this.graph = null;
                }
            }
        }
        private void OnSceneGUI(SceneView sceneView) {
            Body body = ActionEditorWindowManager.instance.body;
            editableBoneKeyPoseNodes.Clear();
            if (graph != null) {
                foreach (var obj in graph.nodes) {
                    ActionTargetNodeBase node = obj as ActionTargetNodeBase;
                    if (node != null && node.visualize) {
                        var editor = NodeEditor.GetEditor(node, this) as ActionTargetNodeBaseEditor;
                        if (editor != null && showHandles) {
                            editor.OnSceneGUI(body);
                        }
                        //node.OnSceneGUI(body);
                        AddBoneKeyPoseNode(node, editableBoneKeyPoseNodes);
                    }
                }
            }
            if (showBones) {
                foreach (var editableBoneKeyPoseNode in editableBoneKeyPoseNodes) {
                    DrawHumanBone(editableBoneKeyPoseNode);
                }
            }
        }
        // nodeの次のノードを見てそれがOutPutNodeならAddする
        void AddBoneKeyPoseNode(Node node, List<ActionTargetOutputNode> boneKeyPoseNodes) {
            foreach (var output in node.Outputs) {
                foreach (var connection in output.GetConnections()) {
                    var newBoneKeyPoseNode = connection.node as ActionTargetOutputNode;
                    if (newBoneKeyPoseNode != null) {
                        if (!boneKeyPoseNodes.Contains(newBoneKeyPoseNode)) {
                            boneKeyPoseNodes.Add(newBoneKeyPoseNode);
                        }
                    } else {
                        AddBoneKeyPoseNode(connection.node, boneKeyPoseNodes);
                    }
                }
            }
        }
        void DrawHumanBone(ActionTargetOutputNode boneKeyPoseNode) {
            PosRotScale r = boneKeyPoseNode.GetInputValue<PosRotScale>("posRotScale");
            if (boneKeyPoseNode.usePosition || boneKeyPoseNode.useRotation) {
                // 調整用の手などを表示
                ActionEditorWindowManager.instance.editableMat.SetPass(0); // 1だと影しか見えない？ 
                if (boneKeyPoseNode.boneId == HumanBodyBones.LeftHand) {
                    Graphics.DrawMeshNow(ActionEditorWindowManager.instance.leftHand, r.position + r.rotation * new Vector3(0.05f, 0, 0), r.rotation.normalized, 0);
                } else if (boneKeyPoseNode.boneId == HumanBodyBones.RightHand) {
                    Graphics.DrawMeshNow(ActionEditorWindowManager.instance.rightHand, r.position + r.rotation * new Vector3(-0.05f, 0, 0), r.rotation.normalized, 0);
                } else if (boneKeyPoseNode.boneId == HumanBodyBones.Head) {
                    Graphics.DrawMeshNow(ActionEditorWindowManager.instance.head, r.position, r.rotation.normalized, 0);
                } else if (boneKeyPoseNode.boneId == HumanBodyBones.LeftFoot) {
                    Graphics.DrawMeshNow(ActionEditorWindowManager.instance.leftFoot, r.position, r.rotation.normalized, 0);
                } else if (boneKeyPoseNode.boneId == HumanBodyBones.RightFoot) {
                    Graphics.DrawMeshNow(ActionEditorWindowManager.instance.rightFoot, r.position, r.rotation.normalized, 0);
                }
            }
            // visible
            //if (boneKeyPose.usePosition || boneKeyPose.useRotation) {
            //    // 調整用の手などを表示
            //    visibleMat.SetPass(0); // 1だと影しか見えない？ 
            //    if (boneKeyPose.boneId == HumanBodyBones.LeftHand) {
            //        Graphics.DrawMeshNow(leftHand, boneKeyPose.position, boneKeyPose.rotation.normalized, 0);
            //    } else if (boneKeyPose.boneId == HumanBodyBones.RightHand) {
            //        Graphics.DrawMeshNow(rightHand, boneKeyPose.position, boneKeyPose.rotation.normalized, 0);
            //    } else if (boneKeyPose.boneId == HumanBodyBones.Head) {
            //        Graphics.DrawMeshNow(head, boneKeyPose.position, boneKeyPose.rotation.normalized, 0);
            //    } else if (boneKeyPose.boneId == HumanBodyBones.LeftFoot) {
            //        Graphics.DrawMeshNow(leftFoot, boneKeyPose.position, boneKeyPose.rotation.normalized, 0);
            //    } else if (boneKeyPose.boneId == HumanBodyBones.RightFoot) {
            //        Graphics.DrawMeshNow(rightFoot, boneKeyPose.position, boneKeyPose.rotation.normalized, 0);
            //    }
            //}
        }

        public static void ReloadActionList() {
            actionTargetGraphNames = new List<string>();
            List<ActionTargetGraph> actionTargetGraphsInAsset = new List<ActionTargetGraph>();
            // Asset全検索
            var guids = AssetDatabase.FindAssets("*").Distinct();
            // 特定フォルダ
            // var keyPosesInFolder = AssetDatabase.FindAssets("t:KeyPoseInterpolationGroup", saveFolder);
            ActionEditorWindowManager.instance.actionTargetGraphs.Clear();
            actionTargetGraphNames.Clear();

            foreach (var guid in guids) {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                var actionTargetGraph = obj as ActionTargetGraph;
                if (actionTargetGraph != null && AssetDatabase.IsMainAsset(obj)) {
                    ActionEditorWindowManager.instance.actionTargetGraphs.Add(actionTargetGraph);
                    actionTargetGraphNames.Add(actionTargetGraph.name);
                    var actionTargetGraphStatus = new ActionTargetGraphStatus(actionTargetGraph);
                    // ActionEditorWindowManagerにない場合のみ追加
                    actionTargetGraphsInAsset.Add(actionTargetGraph);
                    bool isExist = false;
                    foreach (var existActionTargetGraphStatus in ActionEditorWindowManager.instance.actionTargetGraphStatuses) {
                        if (actionTargetGraph == existActionTargetGraphStatus.actionTargetGraph) {
                            isExist = true;
                            break;
                        }
                    }
                    if (!isExist) {
                        ActionEditorWindowManager.instance.actionTargetGraphStatuses.Add(actionTargetGraphStatus);
                    }
                }
            }

            // 残っているものを削除
            List<ActionTargetGraphStatus> deleteList = new List<ActionTargetGraphStatus>();
            foreach (var existActionTargetGraphStatus in ActionEditorWindowManager.instance.actionTargetGraphStatuses) {
                bool isExist = false;
                foreach (var actionTargetGraph in actionTargetGraphsInAsset) {
                    if (actionTargetGraph == existActionTargetGraphStatus.actionTargetGraph) {
                        isExist = true;
                    }
                }
                if (!isExist) {
                    deleteList.Add(existActionTargetGraphStatus);
                }
            }
            foreach (var delete in deleteList) {
                ActionEditorWindowManager.instance.actionTargetGraphStatuses.Remove(delete);
            }

            //ソート
            ActionEditorWindowManager.instance.actionTargetGraphStatuses.Sort((a, b) => a.actionTargetGraph.name.CompareTo(b.actionTargetGraph.name));

            InitAtionTargetGraphRectDict();
        }
        public static void InitAtionTargetGraphRectDict() {
            actionTargetGraphRectDict = new Dictionary<ActionTargetGraphStatus, Rect>();
            foreach (var actionTargetGraphStatus in ActionEditorWindowManager.instance.actionTargetGraphStatuses) {
                actionTargetGraphRectDict.Add(actionTargetGraphStatus, new Rect());
            }
        }
        public void ReloadActionTargetGraphs() {
            var guids = AssetDatabase.FindAssets("*").Distinct();
            Debug.Log(guids.Count());

            foreach (var guid in guids) {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                var action = obj as NodeGraph;
                if (action != null && AssetDatabase.IsMainAsset(obj)) {
                    foreach (Node node in action.nodes) {
                        node.graph = action;
                    }
                }
            }
        }

        Texture2D GetEditableLabelTexture() {
            Texture2D editableLabelTexture = null;
            var mono = MonoScript.FromScriptableObject(this);
            var scriptpath = AssetDatabase.GetAssetPath(mono);
            scriptpath = scriptpath.Replace("EditorWindow/ActionTargetGraphEditorWindow.cs", "");
            var bytes = System.IO.File.ReadAllBytes(scriptpath + editableLabelpath);
            if (bytes != null) {
                editableLabelTexture = new Texture2D(1, 1);
                editableLabelTexture.LoadImage(System.IO.File.ReadAllBytes(scriptpath + editableLabelpath));
                editableLabelTexture.filterMode = FilterMode.Bilinear;
            } else {
                Debug.Log("picture null");
            }
            return editableLabelTexture;
        }
    }
    [System.Serializable]
    public class ActionTargetGraphStatus {
        public ActionTargetGraph actionTargetGraph;
        public bool isVisible;
        public ActionTargetGraphStatus() {
            this.actionTargetGraph = null;
            this.isVisible = false;
        }
        public ActionTargetGraphStatus(ActionTargetGraph keyPose) {
            this.actionTargetGraph = keyPose;
            this.isVisible = false;
        }
    }

}
