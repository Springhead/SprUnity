using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using XNode;
using XNodeEditor;

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

        public void ReloadActionTargetGraphs() {
            var guids = AssetDatabase.FindAssets("*").Distinct();
            Debug.Log(guids.Count());

            foreach (var guid in guids) {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                var action = obj as NodeGraph;
                if (action != null && AssetDatabase.IsMainAsset(obj)) {
                    foreach(Node node in action.nodes) {
                        node.graph = action;
                    }
                }
            }
        }
    }

}
