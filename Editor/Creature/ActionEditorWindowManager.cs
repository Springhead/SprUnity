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
        public Body body;

        //
        public SceneView scene;

        //
        public ActionStateMachineWindow stateMachineWindow;
        public KeyPoseWindow keyPoseWindow;
        public KeyPoseInterpolationWindow interpolationWindow;
        public ActionTimelineWindow timelineWindow;

        // 
        public PullbackPoseWindow pullbackPoseWindow;
        public PullbackPoseGroupWindow pullbackPoseGroupWindow;

        //
        public KeyPoseBoneWindow keyPoseBoneWindow;
        public PullbackPoseBoneWindow pullbackPoseBoneWindow;

        //
        public BodyParameterWindow bodyParameterWindow;

        //
        public KeyPoseNodeGraphEditorWindow keyPoseNodeGraphWindow;
        
        
        // KeyPoseWindow関係
        public List<KeyPoseGroupStatus> keyPoseGroupStatuses;

        // public ActionSelectWindow関係
        public List<ActionStateMachine> actions;
        public ActionStateMachine selectedAction;

        public ActionStateMachine lastSelectedStateMachine;
        public ActionManager lastSelectedActionManager;

        // PullbackPoseWindow関係
        // public PullbackPoseGroupWindow関係

        // KeyPoseBoneWindow関係
        public bool showKeyPoseBoneWindow;
        // PullbackPoseBoneWindow関係
        public bool showPullbackPoseBoneWindow;


        //
        public string actionSaveFolder;
        public string KeyPoseSaveFolder;

        // Management flags
        public bool actionSelectChanged = false;
        public bool actionUpdated = false;

        // Log data


        ActionEditorWindowManager() {
            keyPoseGroupStatuses = new List<KeyPoseGroupStatus>();
            actions = new List<ActionStateMachine>();

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
        }

        ~ActionEditorWindowManager() {
            Debug.Log("Manager destructed");
        }

        void OnEnable() {
            if (body == null) {
                body = GameObject.FindObjectOfType<Body>();
            }
            actionSaveFolder = Application.dataPath + "/Actions/Actions";
            KeyPoseSaveFolder = Application.dataPath + "/Actions/KeyPoses";
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
        }

        #region EventDelegates

        void OnHierarchyChanged() {
            ActionStateMachineWindow.ReloadActionList();
            if (Selection.activeGameObject?.GetComponent<ActionManager>()) {
                instance.lastSelectedActionManager = Selection.activeGameObject.GetComponent<ActionManager>();
            }
        }

        void OnProjectChanged() {
            KeyPoseWindow.ReloadKeyPoseList();
            ActionStateMachineWindow.ReloadActionList();
        }

        void Update() {
            if (actionSelectChanged) {
                Debug.LogWarning(lastSelectedActionManager?[selectedAction.name]);
                if (lastSelectedActionManager?[selectedAction.name] != null) {
                    instance.lastSelectedActionManager[selectedAction.name].PredictFutureTransition();
                }
                if (instance.timelineWindow != null) instance.timelineWindow.Repaint();
                if (instance.stateMachineWindow != null) instance.stateMachineWindow.Repaint();
                actionSelectChanged = false;
            }
            if (selectedAction) {
                if (selectedAction.isChanged) {
                if (lastSelectedActionManager?[selectedAction.name] != null) {
                        instance.lastSelectedActionManager[selectedAction.name].PredictFutureTransition();
                        instance.timelineWindow?.Repaint();
                        instance.lastSelectedActionManager[selectedAction.name].isChanged = false;
                }
                    instance.stateMachineWindow?.Repaint();
                    selectedAction.isChanged = false;
                }
            }
            if(instance.body == null) {
                body = GameObject.FindObjectOfType<Body>();
            }
        }

        void OnPlayModeChanged(PlayModeStateChange state) {
            
        }

        void OnPauseChanged(PauseState state) {

        }

        void OnSelectionChanged() {
            if (Selection.activeGameObject?.GetComponent<ActionManager>()) {
                instance.lastSelectedActionManager = Selection.activeGameObject.GetComponent<ActionManager>();
            }
        }

        #endregion // EventDelegates
    }

}