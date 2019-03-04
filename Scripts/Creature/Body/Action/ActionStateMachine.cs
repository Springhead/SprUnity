using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SprUnity;

#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;

[System.Serializable]
public class TransitionFlag {
    public string label;
    public bool enabled;
    public TransitionFlag(string label, bool e = false) {
        this.label = label;
        this.enabled = e;
    }
}

[System.Serializable]
public class TransitionFlagList {
    public List<TransitionFlag> flags = new List<TransitionFlag>();
    public bool this[string key] {
        set {
            bool found = false;
            int l = flags.Count;
            foreach(var flag in flags) {
                if (flag.label == key) {
                    flag.enabled = value;
                    found = true;
                }
            }
            if (!found) flags.Add(new TransitionFlag(key, value));
        }
        get {
            foreach (var flag in flags) {
                if (flag.label == key) {
                    return flag.enabled;
                }
            }
            return false;
        }
    }
}

// 参照：uGUIではじめるUnity UIデザインの教科書p244
public class RenameWindow : EditorWindow {
    public string captionText { get; set; }
    public string buttonText { get; set; }
    public string newName { get; set; }
    public System.Action<string> onClickButtonDelegate { get; set; }

    void OnGUI() {
        newName = EditorGUILayout.TextField(captionText, newName);
        if (GUILayout.Button(buttonText)) {
            if(onClickButtonDelegate != null) {
                onClickButtonDelegate.Invoke(newName.Trim());
            }

            Close();
            GUIUtility.ExitGUI();
        }
    }
}

public struct BoneTargetPair {
    public string boneLabel;
    public GameObject target;
}

[CreateAssetMenu(menuName = "Action/ Create ActionStateMachine Instance")]
public class ActionStateMachine : ScriptableObject {

    [SerializeField]
    //public List<ActionState> states;

    [HideInInspector]
    public ActionState currentState;

    // Bool型のフラグリスト
    public TransitionFlagList flags;

    public bool enabled = false;

    // 適用するBody
    // Start時に対応させる
    [HideInInspector]
    public Body body;

    // キャラクタ身体データ
    // 現在適用キャラクタ
    Vector3 headPositionHipBase;
    Vector3 footCenterPositionHipBase;
    Vector3 HandPositionHipBase;
    // 最終編集時に使用したキャラクタ
    Vector3 headPositionHipBaseInLastEdit;
    Vector3 footCenterPositionHipBaseInLastEdit;
    Vector3 HandPositionHipBaseInLastEdit;

    // アクションのターゲット
    //public List<BoneTargetPair> targetObjects = new List<BoneTargetPair>();
    public GameObject targetObject;

    //
    [HideInInspector]
    public Rect entryRect = new Rect(0, 0, 100, 50);
    public List<ActionTransition> entryTransitions = new List<ActionTransition>();

    [HideInInspector]
    public Rect exitRect = new Rect(0, 100, 100, 50);

    // ----- ----- ----- ----- -----
    public int nStates {
        get {
            return this.GetSubAssets().Where(value => value as ActionState != null).Count();
        }
    }

    public int nTransitions {
        get {
            return this.GetSubAssets().Where(value => value as ActionTransition != null).Count();
        }
    }

    public List<ActionState> states {
        get {
            return this.GetSubAssets().OfType<ActionState>().ToList();
        }
    }

    public List<ActionTransition> transitions {
        get {
            return this.GetSubAssets().OfType<ActionTransition>().ToList();
        }
    }

    // ----- ----- ----- ----- -----
    /*
    public bool this[string key] {
        set {
            bool found = false;
            foreach(var flag in flags) {
                if(flag.label == key) {
                    flag.isEnabled = value;
                    found = true;
                }
            }
            if (!found) flags.Add(new TransitionFlag(key, value));
        }
    }
    */

    // ----- ----- ----- ----- ----- -----
    // Create ActionStateMachine

    [MenuItem("Action/Create ActionStateMachine")]
    static void CreateStateMachine() {
        var action = CreateInstance<ActionStateMachine>();

        string currrentDirectory = GetCurrentDirectory();
        AssetDatabase.CreateAsset(action, currrentDirectory + "/ActionStateMachine.asset");
        AssetDatabase.Refresh();
    }


    // ----- ----- ----- ----- ----- -----
    // Create/Delete ActionState

    [MenuItem("CONTEXT/ActionStateMachine/New State")]
    static void CreateState() {
        var parentStateMachine = Selection.activeObject as ActionStateMachine;

        if(parentStateMachine == null) {
            Debug.LogWarning("No ActionStateMachine object selected");
            return;
        }

        var state =  ScriptableObject.CreateInstance<ActionState>();
        state.name = "state";
        state.stateMachine = parentStateMachine;

        AssetDatabase.AddObjectToAsset(state, parentStateMachine);

        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(parentStateMachine));
    }

    public void CreateState(Vector2 pos = default(Vector2)) {
        var state = ScriptableObject.CreateInstance<ActionState>();
        state.name = "new state";
        state.stateMachine = this;
        state.stateNodeRect.position = pos;

        AssetDatabase.AddObjectToAsset(state, this);

        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(this));
    }

    static void DeleteState(ActionState state) {

    }

    // ----- ----- ----- ----- ----- -----
    // 

    // 
    void Awake() {

    }

    void OnDestroy() {

    }

    void OnEnable() {

    }

    void OnDisable() {

    }

    // ----- ----- ----- ----- ----- -----
    // ステートマシン関係のイベント

    // 
    public void Begin(Body body = null, GameObject targets = null) {
        enabled = true;

        this.body = body;
        if (body == null) this.body = GameObject.FindObjectOfType<Body>();
        /*
        foreach(var target in targets) {
            for(int i = 0; i < targetObjects.Count; i++) {
                if(targetObjects[i].boneLabel == target.boneLabel) {

                }
            }
        }
        */

        if (entryTransitions.Count == 0) return;
        currentState = entryTransitions[0].toState;
        currentState.OnEnter();
    }

    // Update StateMachine if it's enabled
    public void UpdateStateMachine() {
        if (!enabled) return;
        // Update timer
        currentState.timeFromEnter += Time.fixedDeltaTime;

        int nTransition = currentState.transitions.Count;
        // 遷移の処理優先度が問題
        // リストで番号が若いものが優先される
        for(int i = 0; i < nTransition; i++) {
            if (currentState.transitions[i].IsTransitable()) {
                currentState.OnExit();
                currentState = currentState.transitions[i].toState;
                if (currentState == null) {
                    End();
                } else {
                    currentState.OnEnter();
                }
                break;
            }
        }
        if (nTransition == 0) {
            currentState.OnExit();
            End();
        }
    }

    // 
    public void End() {
        enabled = false;
        ResetStateMachine();
    }

    void ResetStateMachine() {
        for(int i = 0; i < flags.flags.Count; i++) {
            flags.flags[i].enabled = false;
        }
    }

    // ----- ----- ----- ----- -----
    // その他
    
    static string GetCurrentDirectory() {
        // source : https://qiita.com/r-ngtm/items/13d609cbd6a30e39f83a
        var flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
        var asm = Assembly.Load("UnityEditor.dll");
        var typeProjectBrowser = asm.GetType("UnityEditor.ProjectBrowser");
        var projectBrowserWindow = EditorWindow.GetWindow(typeProjectBrowser);
        return (string)typeProjectBrowser.GetMethod("GetActiveFolderPath", flag).Invoke(projectBrowserWindow, null);
    }

    public Object[] GetSubAssets() {
        string path = AssetDatabase.GetAssetPath(this);
        return AssetDatabase.LoadAllAssetsAtPath(path);
    }
}

#endif