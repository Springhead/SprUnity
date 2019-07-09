using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

namespace SprUnity {

    public class KeyPoseNodeGraphEditorWindow : XNodeEditor.NodeEditorWindow {

        protected override void OnEnable() {
            base.OnEnable();
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
            SceneView.onSceneGUIDelegate += OnSceneGUI;
        }

        private void OnDestroy() {
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
        }

        [OnOpenAsset(0)]
        public static bool OnOpen(int instanceID, int line) {
            KeyPoseNodeGraph nodeGraph = EditorUtility.InstanceIDToObject(instanceID) as KeyPoseNodeGraph;
            if (nodeGraph != null) {
                Open(nodeGraph);
                return true;
            }
            return false;
        }

        public static void Open(KeyPoseNodeGraph graph) {
            if (!graph) return;

            KeyPoseNodeGraphEditorWindow w = GetWindow(typeof(KeyPoseNodeGraphEditorWindow), false, "KeyPoseNodeGraph", true) as KeyPoseNodeGraphEditorWindow;
            w.wantsMouseMove = true;
            w.graph = graph;
        }

        private void OnSceneGUI(SceneView sceneView) {
            Body body = ActionEditorWindowManager.instance.body;
            foreach (var obj in Selection.objects) {
                SprUnity.VGentNodeBase node = obj as SprUnity.VGentNodeBase;
                if (node != null) node.OnSceneGUI(body);
            }
        }
    }

}