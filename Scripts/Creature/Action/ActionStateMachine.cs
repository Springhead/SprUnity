using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;

public class TransitionFlag {
    public string label;
    public bool enabled;
    public TransitionFlag(string label, bool e = false) {
        this.label = label;
        this.enabled = e;
    }
}

public class TransitionFlagList : List<TransitionFlag> {
    public bool this[string key] {
        set {
            bool found = false;
            int l = this.Count;
            foreach(var flag in this) {
                if (flag.label == key) {
                    flag.enabled = value;
                    found = true;
                }
            }
            if (!found) this.Add(new TransitionFlag(key, value));
        }
        get {
            foreach (var flag in this) {
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
    public string CaptionText { get; set; }
    public string ButtonText { get; set; }
    public string NewName { get; set; }
    public System.Action<string> OnClickButtonDelegate { get; set; }

    void OnGUI() {

    }
}

[CreateAssetMenu(menuName = "Action/ Create ActionStateMachine Instance")]
public class ActionStateMachine : ScriptableObject {

    [SerializeField]
    List<ActionState> states;

    ActionState currentState;

    // どこからでも遷移できるグローバルステートいる？
    // 
    //public List<TransitionFlag> flags = new List<TransitionFlag>();
    //public Dictionary<string, TransitionFlag> flags = new Dictionary<string, TransitionFlag>();
    public TransitionFlagList flags;

    public bool enabled = false;

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
    // Create

	[MenuItem("Action/Create ActionStateMachine")]
    static void CreateAsset() {
        var action = CreateInstance<ActionStateMachine>();

        string currrentDirectory = GetCurrentDirectory();
        AssetDatabase.CreateAsset(action, currrentDirectory + "/ActionStateMachine.asset");
        AssetDatabase.Refresh();
    }

    [MenuItem("CONTEXT/ActionStateMachine/New State")]
    static void CreateState() {
        /*var parentStateMachine = Selection.activeObject as ActionStateMachine;

        if(parentStateMachine == null) {
            Debug.LogWarning("No ActionStateMachine object selected");
        }

        var state =  ScriptableObject.CreateInstance<ActionState>();

        AssetDatabase.AddObjectToAsset(state, parentStateMachine);

        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(parentStateMachine));*/
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
    // 

    // 
    public void Begin(InteraWare.Body body) {
        enabled = true;

        currentState = states[0];
        currentState.OnEnter(body);
    }

    // Update StateMachine if it's enabled
    public void UpdateStateMachine(InteraWare.Body body) {
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
                    currentState.OnEnter(body);
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

    }

    // ----- ----- ----- ----- -----
    
    static string GetCurrentDirectory() {
        // source : https://qiita.com/r-ngtm/items/13d609cbd6a30e39f83a
        var flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
        var asm = Assembly.Load("UnityEditor.dll");
        var typeProjectBrowser = asm.GetType("UnityEditor.ProjectBrowser");
        var projectBrowserWindow = EditorWindow.GetWindow(typeProjectBrowser);
        return (string)typeProjectBrowser.GetMethod("GetActiveFolderPath", flag).Invoke(projectBrowserWindow, null);
    }
}

#endif