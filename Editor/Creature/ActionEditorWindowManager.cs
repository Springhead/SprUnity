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
        public ActionSelectWindow actionSelectWindow;
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


        // ActionStateMachineWindow関係
        // KeyPoseWindow関係
        public List<KeyPoseGroupStatus> keyPoseGroupStatuses;
        // KeyPoseInterpolationWindow関係
        // public ActionSelectWindow関係
        public List<ActionStateMachineStatus> actions;
        public List<ActionStateMachineStatus> selectedAction {
            get {
                var selected = new List<ActionStateMachineStatus>();
                foreach (var action in actions) {
                    if (action.isSelected) {
                        selected.Add(action);
                    }
                }
                return selected;
            }
        }
        public ActionStateMachine lastSelectedStateMachine;
        // public ActionTimelineWindow関係
        public bool showSpring;
        public bool showDamper;

        // PullbackPoseWindow関係
        // public PullbackPoseGroupWindow関係

        // KeyPoseBoneWindow関係
        public bool showKeyPoseBoneWindow;
        // PullbackPoseBoneWindow関係
        public bool showPullbackPoseBoneWindow;


        //
        public string actionSaveFolder;
        public string KeyPoseSaveFolder;

        //
        public GameObject targetObject;


        // Management flags
        public bool actionSelectChanged = false;


        // Log data


        ActionEditorWindowManager() {
            keyPoseGroupStatuses = new List<KeyPoseGroupStatus>();
            actions = new List<ActionStateMachineStatus>();

            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            EditorApplication.projectChanged -= OnProjectChanged;
            EditorApplication.projectChanged += OnProjectChanged;
            EditorApplication.update -= Update;
            EditorApplication.update += Update;

            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;

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
            ActionSelectWindow.ReloadActionList();
            body = GameObject.FindObjectOfType<Body>();
            if (stateMachineWindow) stateMachineWindow.InitializeGraphMatrix();
        }

        public void SearchBody() {
            body = GameObject.FindObjectOfType<Body>();
        }

        #region EventDelegates

        void OnHierarchyChanged() {
            ActionSelectWindow.ReloadActionList();
        }

        void OnProjectChanged() {
            KeyPoseWindow.ReloadKeyPoseList();
            ActionSelectWindow.ReloadActionList();
        }

        void Update() {
            if (actionSelectChanged) {
                if (instance.timelineWindow != null) instance.timelineWindow.Repaint();
                if (instance.stateMachineWindow != null) instance.stateMachineWindow.Repaint();
                actionSelectChanged = false;
            }
            if (selectedAction.Count == 1) {
                if (selectedAction[0].stateMachineAction.isChanged) {
                    instance.stateMachineWindow.Repaint();
                }
            }
            if(instance.body == null) {
                body = GameObject.FindObjectOfType<Body>();
            }
        }

        void OnPlayModeChanged(PlayModeStateChange state) {

        }

        #endregion // EventDelegates
    }

}