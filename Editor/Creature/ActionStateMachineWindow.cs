using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine.EventSystems;
using SprUnity;


public class ActionStateMachineWindow : EditorWindow {

    //
    public static ActionStateMachineWindow window;

    // 編集時に使うBody(これを使わないとBoneを座標の基準として編集できない)
    // Play時にリンクが切れるという問題
    public Body body;
    
    // Graph backgroundの設定
    const bool graphBackground = false;
    Graph actionGraph;
    GraphGUI actionGraphGUI;

    // ActionStateのビジュアル設定
    private GUIStyle defaultNodeStyle;
    private GUIStyle selectedNodeStyle;
    private GUIStyle entryNodeStyle;
    private GUIStyle exitNodeStyle;

    private bool initialized = false;
    private ActionStateMachine lastEditedStateMachine;
    private List<List<int>> graphConnectionMatrix;
    // Stateにナンバリングしてソートして同じStateToStateのものをまとめる
    private List<List<ActionTransition>> transitionGraph;

	[MenuItem("Window/Action State Machine Window")]
    static void Open() {
        window = GetWindow<ActionStateMachineWindow>(typeof(SceneView));
        ActionEditorWindowManager.instance.stateMachineWindow = ActionStateMachineWindow.window;
        ActionTransitionWindowEditor.Initialize();
        ActionStateWindowEditor.Initialize();
    }

    void OnEnable() {
        //Open();
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
        ActionState.defaultStyle.normal.background = EditorGUIUtility.Load("flow node 0") as Texture2D;
        ActionState.defaultStyle.alignment = TextAnchor.MiddleCenter;
        //ActionState.defaultStyle.border = new RectOffset(12, 12, 12, 12);

        ActionState.selectedStyle = new GUIStyle();
        ActionState.selectedStyle.normal.background = EditorGUIUtility.Load("floaw node 2 on") as Texture2D;
        ActionState.selectedStyle.alignment = TextAnchor.MiddleCenter;
        //ActionState.selectedStyle.border = new RectOffset(12, 12, 12, 12);

        ActionState.currentStateStyle = new GUIStyle();
        ActionState.currentStateStyle.normal.background = EditorGUIUtility.Load("floaw node 5") as Texture2D;
        ActionState.currentStateStyle.alignment = TextAnchor.MiddleCenter;
        //ActionState.currentStateStyle.border = new RectOffset(12, 12, 12, 12);
    }

    void OnDisable() {
        window = null;
        ActionEditorWindowManager.instance.stateMachineWindow = null;
    }

    void OnGUI() {
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
        var actionSelectWindow = ActionEditorWindowManager.instance.actionSelectWindow;
        if (actionSelectWindow) {
            var actions = ActionEditorWindowManager.instance.selectedAction;
            if (actions.Count == 1) {
                var action = actions[0].action;
                if (lastEditedStateMachine != action) { initialized = false; }
                if (!initialized) {
                    InitializeGraphMatrix();
                    initialized = true;
                }
                Object[] subObjects = action.GetSubAssets();
                Debug.Log(subObjects.Length);
                action.entryRect = GUI.Window(subObjects.Length, action.entryRect, (i) => GUI.DragWindow(), "Entry");
                action.exitRect = GUI.Window(subObjects.Length + 1, action.exitRect, (i) => GUI.DragWindow(), "Exit");
                foreach (var item in subObjects.Select((v, i) => new { Index = i, Value = v })) {
                    // EntryNode
                    // ExitNode
                    // ステートの表示
                    ActionState state = item.Value as ActionState;
                    if (state != null) {
                        state.Draw(item.Index);
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
        }
        EndWindows();
        //Debug.Log(GUIUtility.hotControl);
        
    }

    public void InitializeGraphMatrix() {
        var action = ActionEditorWindowManager.instance.selectedAction[0].action;
        int nStates = action.nStates;
        graphConnectionMatrix = new List<List<int>>();
        for(int i = 0; i < nStates; i++) {
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
        for (int i = 0; i < transitions.Count; i++) {
            int from = transitions[i].fromState != null ? transitions[i].fromState.serialCount : (transitions[i].toState.serialCount + 1);
            int to = transitions[i].toState != null ? transitions[i].toState.serialCount : (transitions[i].fromState.serialCount + 2);
            if(from < to) {
                transitions[i].transitionNumber = graphConnectionMatrix[from][to];
                graphConnectionMatrix[from][to]++;
            } else {
                transitions[i].transitionNumber = graphConnectionMatrix[to][from];
                graphConnectionMatrix[to][from]++;
            }
        }
        for (int i = 0; i < transitions.Count; i++) {
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
        Debug.Log(Event.current.type);
        switch (e.type) {
            case EventType.MouseDown:
                if(e.button == 1) {
                    OnContextMenu(e.mousePosition);
                }
                break;
            case EventType.DragUpdated:
                break;
            case EventType.DragExited:
                break;
        }
    }

    // Stateのノード各々のもの
    private void ProcessNodeEvents() {

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
        if (ActionEditorWindowManager.instance.selectedAction.Count != 1) return;
        ActionEditorWindowManager.instance.selectedAction[0].action.CreateState();
    }
}
