using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;

namespace SprUnity {
    public class ActionEditorWindowManager : ScriptableSingleton<ActionEditorWindowManager> {

        // マネージャーのインスタンス
        // シングルトン化のため
        //ActionEditorWindowManager instance;

        // 
        public Body[] bodiesInScene; 
        public Body body;

        //
        public SceneView scene;

        //
        public ActionStateMachineWindow stateMachineWindow;
        public KeyPoseWindow keyPoseWindow;
        public ActionTimelineWindow timelineWindow;

        //
        public KeyPoseBoneWindow keyPoseBoneWindow;

        //
        public BodyParameterWindow bodyParameterWindow;
        
        
        // KeyPoseWindow関係
        public List<KeyPoseStatus> keyPoseStatuses;

        // public ActionSelectWindow関係
        public List<ActionStateMachine> actions;
        public ActionStateMachine selectedAction;
        public int actionIndex = 0;

        public ActionStateMachine lastSelectedStateMachine;

        // KeyPoseBoneWindow関係
        public bool showKeyPoseBoneWindow;

        // Management flags
        public bool actionSelectChanged = false;
        public bool actionUpdated = false;

        // ActionTragetGraphEditorWindow関係
        public List<ActionTargetGraph> actionTargetGraphs;
        public ActionTargetGraph selectedActionTargetGraph;

        public List<ActionTargetGraphStatus> actionTargetGraphStatuses;
        // Log data

        ActionEditorWindowManager() {
            keyPoseStatuses = new List<KeyPoseStatus>();
            actions = new List<ActionStateMachine>();
            actionTargetGraphs = new List<ActionTargetGraph>();
            actionTargetGraphStatuses = new List<ActionTargetGraphStatus>();

            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            EditorApplication.projectChanged -= OnProjectChanged;
            EditorApplication.projectChanged += OnProjectChanged;
            EditorApplication.update -= Update;
            EditorApplication.update += Update;

            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            EditorApplication.pauseStateChanged -= OnPauseChanged;
            EditorApplication.pauseStateChanged += OnPauseChanged;

            Selection.selectionChanged -= OnSelectionChanged;
            Selection.selectionChanged += OnSelectionChanged;

            Debug.Log("Manager constructed");

            SceneView.onSceneGUIDelegate -= OnSceneGUI;
            SceneView.onSceneGUIDelegate += OnSceneGUI;
        }

        ~ActionEditorWindowManager() {
            Debug.Log("Manager destructed");
        }

        void OnEnable() {
            if (body == null) {
                body = GameObject.FindObjectOfType<Body>();
            }
            Debug.Log("Manager OnEnable");
        }

        void OnDisable() {
            Debug.Log("Manager OnDisable");
        }

        void Reload() {
            KeyPoseWindow.ReloadKeyPoseList();
            ActionStateMachineWindow.ReloadActionList();
            body = GameObject.FindObjectOfType<Body>();
            if (stateMachineWindow) stateMachineWindow.InitializeGraphMatrix();
        }

        public void SearchBody() {
            body = GameObject.FindObjectOfType<Body>();
            bodiesInScene = GameObject.FindObjectsOfType<Body>();
        }

        #region EventDelegates

        void OnHierarchyChanged() {
            // <!!> ループしてる？
            //ActionStateMachineWindow.ReloadActionList();
            SearchBody();
        }

        void OnProjectChanged() {
            KeyPoseWindow.ReloadKeyPoseList();
            ActionStateMachineWindow.ReloadActionList();
        }

        void Update() {
            if (actionSelectChanged) {
                if (instance.timelineWindow != null) instance.timelineWindow.Repaint();
                if (instance.stateMachineWindow != null) instance.stateMachineWindow.Repaint();
                actionSelectChanged = false;
            }
            if(instance.body == null) {
                bodiesInScene = GameObject.FindObjectsOfType<Body>();
                body = GameObject.FindObjectOfType<Body>();
            }
        }

        void OnPlayModeChanged(PlayModeStateChange state) {
            if(state == PlayModeStateChange.ExitingEditMode) {

            }else if(state == PlayModeStateChange.EnteredPlayMode) {

            }else if(state == PlayModeStateChange.ExitingPlayMode) {

            }else if(state == PlayModeStateChange.EnteredEditMode) {

            }
        }

        void OnPauseChanged(PauseState state) {
            if(state == PauseState.Paused) {

            }else if(state == PauseState.Unpaused) {

            }
        }

        void OnSelectionChanged() {

        }

        void OnSceneGUI(SceneView sceneView) {
            if (bodiesInScene == null) bodiesInScene = GameObject.FindObjectsOfType<Body>();

            var sceneCamera = sceneView.camera;

            Handles.BeginGUI();
            GUILayout.BeginVertical();
            GUILayout.Label("Bodies:" + bodiesInScene.Length);
            foreach(var bodyInScene in bodiesInScene) {
                bool enable = !(bodyInScene == body);
                if (bodyInScene) {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button(bodyInScene.gameObject.name, GUILayout.Width(100))) {
                        body = bodyInScene;
                    }
                    GUILayout.Label(bodyInScene.height.ToString());
                    GUILayout.EndHorizontal();
                }

            }
            GUILayout.EndVertical();
            Handles.EndGUI();
        }

        #endregion // EventDelegates
    }

}