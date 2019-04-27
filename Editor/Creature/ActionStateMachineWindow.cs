using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine.EventSystems;

namespace SprUnity {

    public class ActionStateMachineWindow : EditorWindow {

        //
        public static ActionStateMachineWindow window;

        // Graph backgroundの設定
        const bool graphBackground = false;
        Graph actionGraph;
        GraphGUI actionGraphGUI;

        private bool initialized = false;
        private ActionStateMachine lastEditedStateMachine;
        private List<List<int>> graphConnectionMatrix;
        // Stateにナンバリングしてソートして同じStateToStateのものをまとめる
        private List<List<ActionTransition>> transitionGraph;

        // GUI
        private Vector2 scrollPos;
        private static List<string> actionNames;
        private int index;

        [MenuItem("Window/SprUnity Action/Action State Machine Window")]
        static void Open() {
            window = GetWindow<ActionStateMachineWindow>();
            ActionEditorWindowManager.instance.stateMachineWindow = ActionStateMachineWindow.window;
            ActionTransitionWindowEditor.Initialize();
            ActionStateWindowEditor.Initialize();
            ReloadActionList();
        }

        void OnEnable() {
            if (graphBackground) {
                if (actionGraph == null) {
                    actionGraph = ScriptableObject.CreateInstance<Graph>();
                    actionGraph.hideFlags = HideFlags.HideAndDontSave;
                }
                if (actionGraphGUI == null) {
                    actionGraphGUI = (GetEditor(actionGraph));
                }
            }
            ActionState.defaultStyle = new GUIStyle();
            //ActionState.defaultStyle.normal.background = EditorGUIUtility.Load("flow node 0") as Texture2D;
            ActionState.defaultStyle.alignment = TextAnchor.MiddleCenter;
            //ActionState.defaultStyle.border = new RectOffset(12, 12, 12, 12);

            ActionState.selectedStyle = new GUIStyle();
            ActionState.selectedStyle.normal.background = EditorGUIUtility.Load("flow node 2 on") as Texture2D;
            ActionState.selectedStyle.alignment = TextAnchor.MiddleCenter;
            //ActionState.selectedStyle.border = new RectOffset(12, 12, 12, 12);

            ActionState.currentStateStyle = new GUIStyle();
            ActionState.currentStateStyle.normal.background = EditorGUIUtility.Load("flow node 5") as Texture2D;
            ActionState.currentStateStyle.alignment = TextAnchor.MiddleCenter;
            //ActionState.currentStateStyle.border = new RectOffset(12, 12, 12, 12);
        }

        void OnDisable() {
            window = null;
            ActionEditorWindowManager.instance.stateMachineWindow = null;
        }

        void OnGUI() {
            if (window == null) Open();

            // Actionのセレクト用
            scrollPos = GUILayout.BeginScrollView(scrollPos);

            //if (window == null) GUILayout.Label("window null");
            //foreach (var action in ActionEditorWindowManager.instance.actions) {
            //    GUILayout.BeginHorizontal(GUILayout.Height(20));
            //action.isSelected = GUILayout.Toggle(action.isSelected, "", GUILayout.Width(15));
            //if (action.isSelected) {
            //    foreach(var act in ActionEditorWindowManager.instance.actions) {
            //        if (act != action) {
            //            act.isSelected = false;
            //        }
            //    }
            //}
            //GUILayout.Label(action.action.name);
            //GUILayout.EndHorizontal();
            //}

            GUILayout.BeginHorizontal(GUILayout.Height(40));
            GUILayout.Label("Actions",GUILayout.Width(50));
            index = EditorGUILayout.Popup(index, actionNames.ToArray(),GUILayout.Width(100));
            foreach (var act in ActionEditorWindowManager.instance.actions) {
                if(act.name == actionNames[index]) {
                    if (ActionEditorWindowManager.instance.selectedAction != act) ActionEditorWindowManager.instance.actionSelectChanged = true;
                    ActionEditorWindowManager.instance.selectedAction = act;
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndScrollView();

            var action = ActionEditorWindowManager.instance.selectedAction;
            var controller = ActionEditorWindowManager.instance.lastSelectedActionManager?[action.name];
            if (controller == null) {
                // normal
            } else {
                // controller depend
            }

            // Draw Flags
            GUILayout.BeginVertical();
            if (controller != null) {
                foreach(var flag in controller.flagList.flags) {
                    flag.enabled = GUILayout.Toggle(flag.enabled, flag.label);
                }
            } else {
                foreach (var flag in action.flags.flags) {
                    GUILayout.BeginHorizontal();
                    flag.enabled = GUILayout.Toggle(flag.enabled,"", GUILayout.Width(10));
                    flag.label = GUILayout.TextField(flag.label);
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();
            //

            if (graphBackground) {
                if (window && actionGraphGUI != null) {
                    actionGraphGUI.BeginGraphGUI(window, new Rect(0, 0, window.position.width, window.position.height));
                    Debug.Log("called");
                    actionGraphGUI.EndGraphGUI();
                }
            }

            ProcessNodeEvents();
            //ProcessEntryNodeEvents();
            ProcessEvents();


            BeginWindows();
            if (action) {
                if (lastEditedStateMachine != action) { initialized = false; }
                if (!initialized) {
                    InitializeGraphMatrix();
                    initialized = true;
                }

                Object[] subObjects = action.GetSubAssets();
                action.entryRect = GUI.Window(subObjects.Length, action.entryRect, (i) => GUI.DragWindow(), "Entry", "flow node 5");
                action.exitRect = GUI.Window(subObjects.Length + 1, action.exitRect, (i) => GUI.DragWindow(), "Exit", "flow node 1");
                foreach (var item in subObjects.Select((v, i) => new { Index = i, Value = v })) {
                    // EntryNode
                    // ExitNode
                    // ステートの表示
                    ActionState state = item.Value as ActionState;
                    if (state != null) {
                        if (controller == null) {
                            state.Draw(item.Index, false);
                        }else if(controller.CurrentState == state) {
                            state.Draw(item.Index, true);
                        } else {
                            state.Draw(item.Index, false);
                        }
                        bool changed = state.ProcessEvents();
                        if (changed) { GUI.changed = true; initialized = false; }
                        continue;
                    }
                    // 遷移の表示
                    ActionTransition transition = item.Value as ActionTransition;
                    if (transition != null) {
                        transition.Draw();
                        bool changed = transition.ProcessEvents();
                        if (changed) { GUI.changed = true; initialized = false; }
                        continue;
                    }
                }
            }
            if (GUI.changed) {
                Repaint();
            }
            EndWindows();
        }

        public void InitializeGraphMatrix() {
            var action = ActionEditorWindowManager.instance.selectedAction;
            int nStates = action.nStates;
            graphConnectionMatrix = new List<List<int>>();
            for (int i = 0; i < nStates; i++) {
                action.states[i].serialCount = i;
            }
            for (int i = 0; i < nStates; i++) {
                List<int> list = new List<int>();
                for (int j = 0; j < (i + 3); j++) {
                    list.Add(0);
                }
                graphConnectionMatrix.Add(list);
            }
            var transitions = action.transitions;
            for (int i = 0; i < transitions.Count(); i++) {
                int from = transitions[i].fromState != null ? transitions[i].fromState.serialCount : (transitions[i].toState.serialCount + 1);
                int to = transitions[i].toState != null ? transitions[i].toState.serialCount : (transitions[i].fromState.serialCount + 2);
                if (from < to) {
                    transitions[i].transitionNumber = graphConnectionMatrix[from][to];
                    graphConnectionMatrix[from][to]++;
                } else {
                    transitions[i].transitionNumber = graphConnectionMatrix[to][from];
                    graphConnectionMatrix[to][from]++;
                }
            }
            for (int i = 0; i < transitions.Count(); i++) {
                int from = transitions[i].fromState != null ? transitions[i].fromState.serialCount : (transitions[i].toState.serialCount + 1);
                int to = transitions[i].toState != null ? transitions[i].toState.serialCount : (transitions[i].fromState.serialCount + 2);
                if (from < to) {
                    transitions[i].transitionCountSamePairs = graphConnectionMatrix[from][to];
                } else {
                    transitions[i].transitionCountSamePairs = graphConnectionMatrix[to][from];
                }
            }
            /*
            transitionGraph = new List<List<ActionTransition>>();
            for(int i = 0; i < nStates; i++) {
                List<ActionTransition> list = new List<ActionTransition>();
                for(int j = 0; j < (i + 2); j++) {

                }
            }
            */
        }

        // ----- ----- ----- ----- ----- -----
        // ユーザ入力の処理

        // ActionStateMachine全体にかかわるもの
        void ProcessEvents() {
            Event e = Event.current;
            //Debug.Log(Event.current.type);
            switch (e.type) {
                case EventType.MouseDown:
                    if (e.button == 1) {
                        OnContextMenu(e.mousePosition);
                    }
                    break;
                case EventType.DragUpdated:
                    break;
                case EventType.MouseDrag:
                    if (e.button == 2) {
                        Drag(e.delta);
                        e.Use();
                    }
                    break;
                case EventType.DragExited:
                    break;
            }
        }

        // Stateのノード各々のもの
        private void ProcessNodeEvents() {

        }

        void Drag(Vector2 delta) {
            ;
        }

        // ----- ----- ----- ----- ----- -----


        GraphGUI GetEditor(Graph graph) {
            GraphGUI graphGUI = CreateInstance("GraphGUI") as GraphGUI;
            graphGUI.graph = graph;
            graphGUI.hideFlags = HideFlags.HideAndDontSave;
            return graphGUI;
        }


        // ----- ----- ----- ----- ----- -----
        // Context Menu

        private void OnContextMenu(Vector2 mousePosition) {
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent("Add State"), false, () => OnClickAddState(mousePosition));
            genericMenu.ShowAsContext();

        }

        private void OnClickAddState(Vector2 mousePosition) {
            if (ActionEditorWindowManager.instance.selectedAction == null) return;
            ActionEditorWindowManager.instance.selectedAction.CreateState();
        }

        public static void ReloadActionList() {
            actionNames = new List<string>();
            // Asset全検索
            var guids = AssetDatabase.FindAssets("*").Distinct();
            // 特定フォルダ
            // var keyPosesInFolder = AssetDatabase.FindAssets("t:KeyPoseInterpolationGroup", saveFolder);

            ActionEditorWindowManager.instance.actions.Clear();
            actionNames.Clear();
            //string selectedActionName = ActionEditorWindowManager.instance.selectedAction.name;
            //ActionEditorWindowManager.instance.actions = new List<ActionStateMachineStatus>();

            foreach (var guid in guids) {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                var action = obj as ActionStateMachine;
                if (action != null && AssetDatabase.IsMainAsset(obj)) {
                    ActionEditorWindowManager.instance.actions.Add(action);
                    actionNames.Add(action.name);
                    Debug.Log("Add " + action.name);
                    /*
                    if(action.name == selectedActionName) {
                        ActionEditorWindowManager.instance.selectedAction = action;
                    }
                    */
                }
            }
        }
    }

}