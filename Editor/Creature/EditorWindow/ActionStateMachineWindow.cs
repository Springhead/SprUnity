using System.Collections;
using System.Collections.Generic;
using UnityEditor.Callbacks;
using System.Linq;
using UnityEngine;
using UnityEditor;
using VGent;

using UnityEditor.Graphs;
using UnityEngine.EventSystems;

namespace SprUnity {

    public class ActionStateMachineWindow : EditorWindow {

        //
        public static ActionStateMachineWindow window;

        public enum EditStatus { Idle, StateDrag, TransitionDrag, GridDrag }
        private EditStatus currentEditStatus = EditStatus.Idle;

        private bool initialized = false;
        private List<List<int>> graphConnectionMatrix;
        // Stateにナンバリングしてソートして同じStateToStateのものをまとめる
        private List<List<ActionStateTransition>> transitionGraph;

        // GUI
        //private Vector2 scrollPos;
        private static List<string> actionNames;

        private float zoom = 1.0f;
        private Vector2 panOffset;

        private Texture2D gridTexture;
        public Texture2D GridTexture {
            get {
                if (gridTexture == null) gridTexture = GenerateGridTexture();
                return gridTexture;
            }
        }

        private static bool guiInitialized = false;
        public static GUIStyle toolbarBase;
        public static GUIStyle toolbarButton;
        public static GUIStyle toolbarLabel;
        public static GUIStyle toolbarDropdown;
        public static GUIStyle toolbarPopup;

        [MenuItem("Window/SprUnity Action/Action State Machine Window")]
        static void Open() {
            window = GetWindow<ActionStateMachineWindow>();
            window.titleContent = new GUIContent("ActionStateMachine");
            ActionEditorWindowManager.instance.stateMachineWindow = ActionStateMachineWindow.window;
            ActionEditorWindowManager.instance.selectedAction = ActionEditorWindowManager.instance.actions[0];
        }

        [OnOpenAsset(0)]
        public static bool OnOpen(int instanceID, int line) {
            ActionStateMachine nodeGraph = EditorUtility.InstanceIDToObject(instanceID) as ActionStateMachine;
            if (nodeGraph != null) {
                Open();
                ActionEditorWindowManager.instance.selectedAction = nodeGraph;
                ActionEditorWindowManager.instance.actionIndex = ActionEditorWindowManager.instance.actions.IndexOf(nodeGraph);
                return true;
            }
            return false;
        }

        void OnDisable() {
            window = null;
            ActionEditorWindowManager.instance.stateMachineWindow = null;
        }

        static void Init() {
            toolbarBase = GUI.skin.FindStyle("toolbar");
            toolbarButton = GUI.skin.FindStyle("toolbarButton");
            toolbarLabel = GUI.skin.FindStyle("toolbarButton");
            toolbarDropdown = GUI.skin.FindStyle("toolbarDropdown");
            toolbarPopup = GUI.skin.FindStyle("toolbarPopup");
        }

        void OnGUI() {
            if (window == null) {
                Open();
            }
            if (!guiInitialized) {
                ReloadActionList();
                Init();
            }

            DrawBackGround();

            ProcessEvents();

            var action = ActionEditorWindowManager.instance.selectedAction;
            BeginWindows();
            if (action) {
                if (!initialized) {
                    InitializeGraphMatrix();
                    initialized = true;
                }

                Object[] subObjects = action.GetSubAssets();
                action.entryRect = GUI.Window(subObjects.Length, action.entryRect, (i) => GUI.DragWindow(), "Entry", "flow node 5");
                action.exitRect = GUI.Window(subObjects.Length + 1, action.exitRect, (i) => GUI.DragWindow(), "Exit", "flow node 1");
                var states = action.states;
                var transitions = action.transitions;
                foreach (var item in subObjects.Select((v, i) => new { Index = i, Value = v })) {
                    // EntryNode
                    // ExitNode
                    // ステートの表示
                    ActionState state = item.Value as ActionState;
                    if (state != null) {
                        state.Draw(item.Index, false, position, zoom, panOffset);
                        bool changed = state.ProcessEvents();
                        if (changed) { GUI.changed = true; initialized = false; }
                        continue;
                    }
                    // 遷移の表示
                    ActionStateTransition transition = item.Value as ActionStateTransition;
                    if (transition != null) {
                        transition.Draw();
                        bool changed = transition.ProcessEvents();
                        if (changed) { GUI.changed = true; initialized = false; }
                        continue;
                    }
                }
                DrawStates();
                DrawTransitions();
            }
            if (GUI.changed) {
                Repaint();
            }
            EndWindows();
            
            DrawToolBar();
            DrawLeftWindow();
        }

        void DrawStates() {

        }

        void DrawTransitions() {

        }

        void DrawBackGround() {
            Rect rect = position;
            rect.position = Vector2.zero;
            Vector2 center = position.size / 2f;

            // Offset from origin in tile units
            float xOffset = -(center.x * zoom + panOffset.x) / GridTexture.width;
            float yOffset = ((center.y - rect.size.y) * zoom + panOffset.y) / GridTexture.height;

            Vector2 tileOffset = new Vector2(xOffset, yOffset);

            // Amount of tiles
            float tileAmountX = Mathf.Round(rect.size.x * zoom) / GridTexture.width;
            float tileAmountY = Mathf.Round(rect.size.y * zoom) / GridTexture.height;

            Vector2 tileAmount = new Vector2(tileAmountX, tileAmountY);

            // Draw tiled background
            GUI.DrawTextureWithTexCoords(rect, GridTexture, new Rect(tileOffset, tileAmount));
        }

        void DrawToolBar() {
            int toolBarHeight = 17;
            Rect rect = new Rect(0, 0, this.position.width, toolBarHeight);

            GUILayout.BeginArea(rect, toolbarBase);
            GUILayout.BeginHorizontal();

            GUILayout.Label("Current Action", toolbarLabel, GUILayout.Width(100));

            //actionIndex = ActionEditorWindowManager.instance.actions.IndexOf(ActionEditorWindowManager.instance.selectedAction);
            ActionEditorWindowManager.instance.actionIndex = EditorGUILayout.Popup(ActionEditorWindowManager.instance.actionIndex, actionNames.ToArray(), toolbarPopup, GUILayout.Width(120));
            foreach (var act in ActionEditorWindowManager.instance.actions) {
                if (act.name == actionNames[ActionEditorWindowManager.instance.actionIndex]) {
                    if (ActionEditorWindowManager.instance.selectedAction != act) ActionEditorWindowManager.instance.actionSelectChanged = true;
                    ActionEditorWindowManager.instance.selectedAction = act;
                }
            }

            if(GUILayout.Button("Create", toolbarButton, GUILayout.Width(70))) {
                CreateActionStateMachineWindow.Open(position.center);
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

        void DrawLeftWindow() {

        }

        

        public void InitializeGraphMatrix() {
            var action = ActionEditorWindowManager.instance.selectedAction;
            int nStates = action.states.Count;
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
        }

        Texture2D GenerateGridTexture() {
            Texture2D tex = new Texture2D(64, 64);
            Color[] cols = new Color[64 * 64];
            Color back = Color.black;
            Color line = Color.gray;
            for (int y = 0; y < 64; y++) {
                for (int x = 0; x < 64; x++) {
                    Color col = back;
                    if (y % 16 == 0 || x % 16 == 0) col = Color.Lerp(line, back, 0.65f);
                    if (y == 63 || x == 63) col = Color.Lerp(line, back, 0.35f);
                    cols[(y * 64) + x] = col;
                }
            }
            tex.SetPixels(cols);
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Bilinear;
            tex.name = "Grid";
            tex.Apply();
            return tex;
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
                case EventType.MouseUp:
                    break;
                case EventType.DragUpdated:
                    break;
                case EventType.MouseDrag:
                    if (e.button == 0) {
                        //e.Use();
                    } else {
                        panOffset += e.delta;
                        Repaint();
                    }
                    break;
                case EventType.DragExited:
                    break;
                case EventType.ScrollWheel:
                    break;
                case EventType.KeyDown:
                    break;
            }
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
            ActionEditorWindowManager.instance.selectedAction.CreateState(mousePosition);
        }

        public static void ReloadActionList() {
            actionNames = new List<string>();
            // Asset全検索
            var guids = AssetDatabase.FindAssets("*").Distinct();

            ActionEditorWindowManager.instance.actions.Clear();
            actionNames.Clear();

            foreach (var guid in guids) {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                var action = obj as ActionStateMachine;
                if (action != null && AssetDatabase.IsMainAsset(obj)) {
                    ActionEditorWindowManager.instance.actions.Add(action);
                    actionNames.Add(action.name);
                }
            }
        }
    }

}