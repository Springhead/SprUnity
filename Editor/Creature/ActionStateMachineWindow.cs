using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine.EventSystems;

public class ActionStateMachineWindow : EditorWindow {

    // 個人的にはStateやTransitionの描画は各々に任せたい


    // Windowのインスタンス
    static ActionStateMachineWindow window;

    // 現在編集中のActionStateMachine
    ActionStateMachine editingAction;
    // 編集時に使うBody(これを使わないとBoneを座標の基準として編集できない)
    public InteraWare.Body bodyUsedEditing;


    // 以下のビジュアライズ関係の設定はシングルトンの設定用クラスを作るかも

    // Graph backgroundの設定
    const bool graphBackground = false;
    Graph actionGraph;
    GraphGUI actionGraphGUI;

    // ActionStateのビジュアル設定
    private GUIStyle defaultNodeStyle;
    private GUIStyle selectedNodeStyle;
    private GUIStyle entryNodeStyle;
    private GUIStyle exitNodeStyle;

    //
    static Texture2D transitionTexture;

	[MenuItem("Window/Action State Machine")]
    static void Open() {
        window = GetWindow<ActionStateMachineWindow>(typeof(SceneView));
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

        // EditorPrefsからウィンドウ情報取得？

        // ノードスタイル
        defaultNodeStyle = new GUIStyle();
        defaultNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
        defaultNodeStyle.alignment = TextAnchor.UpperCenter;
        defaultNodeStyle.border = new RectOffset(12, 12, 12, 12);

        selectedNodeStyle = new GUIStyle();
        selectedNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1 on.png") as Texture2D;
        selectedNodeStyle.border = new RectOffset(12, 12, 12, 12);

        if (editingAction) {
            Object[] subObjects = editingAction.GetSubAssets();
            foreach (var item in subObjects.Select((v, i) => new { Index = i, Value = v })) {
                ActionState state = item.Value as ActionState;
                if (state != null) {
                    state.SetDefaultNodeStyle(defaultNodeStyle);
                    state.SetSelectedNodeStyle(selectedNodeStyle);
                    continue;
                }
            }
        }
    }

    void OnDisable() {
        window = null;
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

        if (bodyUsedEditing) {
            GUILayout.Label("Body:" + bodyUsedEditing.gameObject.name);
        }

        BeginWindows();
        if (editingAction) {
            Object[] subObjects = editingAction.GetSubAssets();
            Debug.Log(subObjects.Length);
            editingAction.entryRect = GUI.Window(subObjects.Length, editingAction.entryRect, (i) => GUI.DragWindow(), "Entry");
            editingAction.exitRect = GUI.Window(subObjects.Length + 1, editingAction.exitRect, (i) => GUI.DragWindow(), "Exit");
            foreach (var item in subObjects.Select((v, i) => new { Index = i, Value = v })) {
                // EntryNode
                // ExitNode
                // ステートの表示
                ActionState state = item.Value as ActionState;
                if (state != null) {
                    //state.Draw(item.Index);
                    state.Draw(item.Index);
                    bool changed = state.ProcessEvents();
                    if (changed) GUI.changed = true;
                    continue;
                }
                // 遷移の表示
                ActionTransition transition = item.Value as ActionTransition;
                if (transition != null) {
                    transition.Draw();
                    continue;
                }
            }
        }
        EndWindows();
        //Debug.Log(GUIUtility.hotControl);
        
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
                if(DragAndDrop.objectReferences.Length == 1) {
                    var action = DragAndDrop.objectReferences[0] as ActionStateMachine;
                    if(action != null) {
                        editingAction = action;
                    }
                    var body = DragAndDrop.objectReferences[0] as InteraWare.Body;
                    if(body != null) {
                        bodyUsedEditing = body;
                    }
                }
                break;
        }
    }

    // Stateのノード各々のもの
    private void ProcessNodeEvents() {

    }

    // ----- ----- ----- ----- ----- -----

    // 
    ActionStateMachine GetEditingActionStateMachine() {
        return editingAction;
    }
    
    //
    void SetEditingActionStateMachine(ActionStateMachine s) {
        editingAction = s;
    }

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
        if (editingAction == null) return;
        editingAction.CreateState(mousePosition);
    }
}
