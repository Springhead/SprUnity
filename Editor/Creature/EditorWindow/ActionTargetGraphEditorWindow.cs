using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using XNode;
using XNodeEditor;
using System.Linq;

namespace SprUnity {

    public class ActionTargetGraphEditorWindow : XNodeEditor.NodeEditorWindow, IHasCustomMenu {
        private Material editableMat, visibleMat;
        private Mesh leftHand;
        private Mesh rightHand;
        private Mesh head;
        private Mesh leftFoot;
        private Mesh rightFoot;

        private float handleSize = 0.05f;
        private float selectedHandleSize = 0.15f;
        private StaticBoneKeyPose selectedboneKeyPose; // マウスが上にあるKeyPoseだけハンドルを大きくする
        private List<BoneKeyPoseNode> editableBoneKeyPoseNodes = new List<BoneKeyPoseNode>();

        private static bool guiInitialized = false;
        public static GUIStyle toolbarBase;
        public static GUIStyle toolbarButton;
        public static GUIStyle toolbarLabel;
        public static GUIStyle toolbarDropdown;
        public static GUIStyle toolbarPopup;

        private static List<string> actionTargetGraphNames;
        private static int actionTargetGraphIndex = 0;

        protected override void OnEnable() {
            base.OnEnable();

            var modelpath = "Assets/Libraries/SprUnity/Editor/Creature/Models/";

            editableMat = AssetDatabase.LoadAssetAtPath(modelpath + "editable.mat", typeof(Material)) as Material;
            visibleMat = AssetDatabase.LoadAssetAtPath(modelpath + "visible.mat", typeof(Material)) as Material;
            if (editableMat == null) {
                Debug.Log("mat null");
            }
            if (visibleMat == null) {
                Debug.Log("mat null");
            }

            leftHand = AssetDatabase.LoadAssetAtPath(
                modelpath + "LeftHand.fbx", typeof(Mesh)) as Mesh;
            if (leftHand == null) {
                Debug.Log("fbx null");
            }

            rightHand = AssetDatabase.LoadAssetAtPath(
                modelpath + "RightHand.fbx", typeof(Mesh)) as Mesh;
            if (rightHand == null) {
                Debug.Log("fbx null");
            }

            head = AssetDatabase.LoadAssetAtPath(
                modelpath + "Head.fbx", typeof(Mesh)) as Mesh;
            if (head == null) {
                Debug.Log("fbx null");
            }

            leftFoot = AssetDatabase.LoadAssetAtPath(
                modelpath + "LeftFoot.fbx", typeof(Mesh)) as Mesh;
            if (leftFoot == null) {
                Debug.Log("fbx null");
            }

            rightFoot = AssetDatabase.LoadAssetAtPath(
                modelpath + "RightFoot.fbx", typeof(Mesh)) as Mesh;
            if (rightFoot == null) {
                Debug.Log("fbx null");
            }

            SceneView.onSceneGUIDelegate -= OnSceneGUI;
            SceneView.onSceneGUIDelegate += OnSceneGUI;
        }

        private void OnDestroy() {
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
        }

        [OnOpenAsset(0)]
        public static bool OnOpen(int instanceID, int line) {
            ActionTargetGraph nodeGraph = EditorUtility.InstanceIDToObject(instanceID) as ActionTargetGraph;
            toolbarBase = GUI.skin.FindStyle("toolbar");
            toolbarButton = GUI.skin.FindStyle("toolbarButton");
            toolbarLabel = GUI.skin.FindStyle("toolbarButton");
            toolbarDropdown = GUI.skin.FindStyle("toolbarDropdown");
            toolbarPopup = GUI.skin.FindStyle("toolbarPopup");
            if (nodeGraph != null) {
                Open(nodeGraph);
                return true;
            }
            return false;
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
        }

        public new void OnGUI() {
            base.OnGUI();

            if (!guiInitialized) {
                ReloadActionList();
                toolbarBase = GUI.skin.FindStyle("toolbar");
                toolbarButton = GUI.skin.FindStyle("toolbarButton");
                toolbarLabel = GUI.skin.FindStyle("toolbarButton");
                toolbarDropdown = GUI.skin.FindStyle("toolbarDropdown");
                toolbarPopup = GUI.skin.FindStyle("toolbarPopup");
                guiInitialized = true;
            }
            DrawToolBar();
        }

        void DrawToolBar() {
            int toolBarHeight = 17;
            //Vector2 pos = GridToWindowPosition(Vector2.zero);
            Rect rect = new Rect(0, 19 * zoom, this.position.width, toolBarHeight);

            GUILayout.BeginArea(rect, toolbarBase);
            GUILayout.BeginHorizontal();

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
            }

            GUILayout.Space(10);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Menu", toolbarButton, GUILayout.Width(50))) {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Create"), false, null);
                menu.AddItem(new GUIContent("Home"), true, () => { this.zoom = 1.0f; this.panOffset = Vector2.zero; });

                menu.DropDown(new Rect(0, -20, 50, 40));
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
        private void OnSceneGUI(SceneView sceneView) {
            Body body = ActionEditorWindowManager.instance.body;
            editableBoneKeyPoseNodes.Clear();
            foreach (var obj in Selection.objects) {
                ActionTargetNodeBase node = obj as ActionTargetNodeBase;
                if (node != null) {
                    var editor = NodeEditor.GetEditor(node, this) as ActionTargetNodeBaseEditor;
                    if (editor != null) {
                        editor.OnSceneGUI(body);
                    }
                    //node.OnSceneGUI(body);
                    AddBoneKeyPoseNode(node, editableBoneKeyPoseNodes);
                }
            }
            foreach (var editableBoneKeyPoseNode in editableBoneKeyPoseNodes) {
                DrawHumanBone(editableBoneKeyPoseNode);
            }
        }
        void AddBoneKeyPoseNode(Node node, List<BoneKeyPoseNode> boneKeyPoseNodes) {
            foreach (var output in node.Outputs) {
                foreach (var connection in output.GetConnections()) {
                    var newBoneKeyPoseNode = connection.node as BoneKeyPoseNode;
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
        void DrawHumanBone(BoneKeyPoseNode boneKeyPoseNode) {
            PosRotScale r = boneKeyPoseNode.GetInputValue<PosRotScale>("posRotScale");
            if (boneKeyPoseNode.usePosition || boneKeyPoseNode.useRotation) {
                // 調整用の手などを表示
                editableMat.SetPass(0); // 1だと影しか見えない？ 
                if (boneKeyPoseNode.boneId == HumanBodyBones.LeftHand) {
                    Graphics.DrawMeshNow(leftHand, r.position, r.rotation.normalized, 0);
                } else if (boneKeyPoseNode.boneId == HumanBodyBones.RightHand) {
                    Graphics.DrawMeshNow(rightHand, r.position, r.rotation.normalized, 0);
                } else if (boneKeyPoseNode.boneId == HumanBodyBones.Head) {
                    Graphics.DrawMeshNow(head, r.position, r.rotation.normalized, 0);
                } else if (boneKeyPoseNode.boneId == HumanBodyBones.LeftFoot) {
                    Graphics.DrawMeshNow(leftFoot, r.position, r.rotation.normalized, 0);
                } else if (boneKeyPoseNode.boneId == HumanBodyBones.RightFoot) {
                    Graphics.DrawMeshNow(rightFoot, r.position, r.rotation.normalized, 0);
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
                }
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
    }

}
