using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SprUnity;

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
    public List<KeyPoseStatus> singleKeyPoses;
    public List<KeyPoseStatus> pluralKeyPoses;
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
    //public GameObject targetObject;

    ActionEditorWindowManager() {
        singleKeyPoses = new List<KeyPoseStatus>();
        pluralKeyPoses = new List<KeyPoseStatus>();
        actions = new List<ActionStateMachineStatus>();
        EditorApplication.hierarchyChanged += Reload;
        EditorApplication.projectChanged += Reload;
        // nullになってしまうためやめとく
        // body = GameObject.FindObjectOfType<Body>();
        Debug.Log("Manager constructed");
    }

    ~ActionEditorWindowManager() {
        Debug.Log("Manager destructed");
    }

    void OnEnable() {
        if (body == null) {
            body = GameObject.FindObjectOfType<Body>();
        }
        /*
        singleKeyPoses = new List<KeyPoseStatus>();
        pluralKeyPoses = new List<KeyPoseStatus>();
        actions = new List<ActionStateMachineStatus>();
        */
        KeyPoseWindow.GetKeyPoses();
        ActionSelectWindow.GetActions();
        actionSaveFolder = Application.dataPath + "/Actions/Actions";
        KeyPoseSaveFolder = Application.dataPath + "/Actions/KeyPoses";
        Debug.Log("Manager OnEnable");
    }

    void OnDisable() {
        Debug.Log("Manager OnDisable");
    }

    void Reload() {
        KeyPoseWindow.GetKeyPoses();
        ActionSelectWindow.GetActions();
        body = GameObject.FindObjectOfType<Body>();
        if(stateMachineWindow) stateMachineWindow.InitializeGraphMatrix();
    }

    public void SearchBody() {
        body = GameObject.FindObjectOfType<Body>();
    }
}
