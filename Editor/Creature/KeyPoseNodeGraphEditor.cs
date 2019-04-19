using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Graphs;

namespace SprUnity {

    public class NodeComponent {

    }

    public class InputNodeComponent : NodeComponent {

    }

    public class OutputNodeComponent : NodeComponent {

    }

    public class BaseNode {
        public Rect nodeRect = new Rect();
    }

    public class KeyPoseNode {

    }

    public class GameObjectNode {

    }

    public class KeyPoseNodeGraphEditorWindow : EditorWindow {

        static KeyPoseNodeGraphEditorWindow window;
        //
        static bool graphBackground;
        static Graph actionGraph;
        static GraphGUI actionGraphGUI;

        [MenuItem("Window/KeyPoseNodeGraph Window")]
        static void Open() {
            window = GetWindow<KeyPoseNodeGraphEditorWindow>();
            ActionEditorWindowManager.instance.keyPoseNodeGraphWindow = KeyPoseNodeGraphEditorWindow.window;
            window.minSize = new Vector2(200, 300);
            if (false) {
                if (actionGraph == null) {
                    actionGraph = ScriptableObject.CreateInstance<Graph>();
                    actionGraph.hideFlags = HideFlags.HideAndDontSave;
                }
                if (actionGraphGUI == null) {
                    actionGraphGUI = (GetEditor(actionGraph));
                }
            }
        }

        public void OnEnable() {
            Open();
            SceneView.onSceneGUIDelegate += OnSceneGUI;
        }

        public void OnDisable() {
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
            ActionEditorWindowManager.instance.keyPoseWindow = null;
        }

        public void OnGUI() {
            if (false) {
                if (window && actionGraphGUI != null) {
                    actionGraphGUI.BeginGraphGUI(window, new Rect(0, 0, window.position.width, window.position.height));
                    //Debug.Log("called");
                    actionGraphGUI.EndGraphGUI();
                }
            }

        }

        public void OnSceneGUI(SceneView sceneView) {

        }

        // ----- ----- ----- ----- ----- -----


        static GraphGUI GetEditor(Graph graph) {
            GraphGUI graphGUI = CreateInstance("GraphGUI") as GraphGUI;
            graphGUI.graph = graph;
            graphGUI.hideFlags = HideFlags.HideAndDontSave;
            return graphGUI;
        }
    }

}