using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif

namespace SprUnity {

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
                foreach (var flag in flags) {
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
        public TransitionFlagList Clone() {
            TransitionFlagList clone = new TransitionFlagList();
            foreach(var flag in this.flags) {
                clone.flags.Add(new TransitionFlag(flag.label, flag.enabled));
            }
            return clone;
        }
    }


    [System.Serializable]
    public class StateMachineParameter {
        public string label;
        public float param;
        public StateMachineParameter() {
            label = "";
            param = 0.0f;
        }
        public StateMachineParameter(string label, float initValue) {
            this.label = label;
            param = initValue;
        }
    }
    [System.Serializable]
    public class StateMachineParameters {
        public List<StateMachineParameter> parameters = new List<StateMachineParameter>();
        public float this[string key] {
            set {
                int l = parameters.Count;
                foreach (var p in parameters) {
                    if (p.label == key) {
                        p.param = value;
                    }
                }
            }
            get {
                foreach (var p in parameters) {
                    if (p.label == key) {
                        return p.param;
                    }
                }
                return 0.0f;
            }
        }
        public StateMachineParameters Clone() {
            StateMachineParameters clone = new StateMachineParameters();
            foreach (var param in this.parameters) {
                clone.parameters.Add(new StateMachineParameter(param.label, param.param));
            }
            return clone;
        }
    }

#if UNITY_EDITOR
    [CreateAssetMenu(menuName = "Action/ Create ActionStateMachine Instance")]
#endif
    public class ActionStateMachine : ScriptableObject {

        [SerializeField]
        //public List<ActionState> states;

        // ----- ----- ----- ----- -----
 
        // Bool型のフラグリスト
        public TransitionFlagList flags;

        public StateMachineParameters parameters;

        // ----- ----- ----- ----- -----
        // 実行用

        private ActionState currentState;
        public ActionState CurrentState { get { return currentState; } }

        private float stateMachineTime = 0;

        // 適用するBody
        // Start時に対応させる
        [HideInInspector]
        public Body body;

        //
        ActionLog actionLog;
        public ActionLog ActionLog{ get { return actionLog; } }
        private List<ActionTransition> futureTransitions = new List<ActionTransition>();
        static private int maxPredictLength = 5;

        private bool enabled = false;
        public bool Enabled {
            get {
                return enabled;
            }
            set {
                enabled = value;
            }
        }

        // ----- ----- ----- ----- -----
        // Editor関係

        public bool isChanged = false;

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

        // ----- ----- ----- ----- ----- -----
        // Create ActionStateMachine

#if UNITY_EDITOR
        [MenuItem("Action/Create ActionStateMachine")]
        static void CreateStateMachine() {
            CreateStateMachine("ActionStateMachine");
        }
#endif

        public static void CreateStateMachine(string newName) {
            var action = CreateInstance<ActionStateMachine>();
            string currrentDirectory = GetCurrentDirectory();
            AssetDatabase.CreateAsset(action, currrentDirectory + "/" + newName + ".asset");
            AssetDatabase.Refresh();
        }
        // ----- ----- ----- ----- ----- -----
        // Create/Delete ActionState

#if UNITY_EDITOR
        [MenuItem("CONTEXT/ActionStateMachine/New State")]
        static void CreateState() {
            var parentStateMachine = Selection.activeObject as ActionStateMachine;

            if (parentStateMachine == null) {
                Debug.LogWarning("No ActionStateMachine object selected");
                return;
            }

            var state = ScriptableObject.CreateInstance<ActionState>();
            state.name = "state";
            state.stateMachine = parentStateMachine;

            AssetDatabase.AddObjectToAsset(state, parentStateMachine);

            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(parentStateMachine));
        }
#endif

        public void CreateState(Vector2 pos = default(Vector2)) {
#if UNITY_EDITOR
            var state = ScriptableObject.CreateInstance<ActionState>();
            state.name = "new state";
            state.stateMachine = this;
            state.stateNodeRect.position = pos;

            AssetDatabase.AddObjectToAsset(state, this);

            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(this));
#endif
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

            if (entryTransitions.Count == 0) return;
            
            enabled = true;

            actionLog = new ActionLog();

            stateMachineTime = 0;
            this.body = body;
            futureTransitions = new List<ActionTransition>();
            if (body == null) this.body = GameObject.FindObjectOfType<Body>();// <!!> 遷移が1パターン!!
            currentState = entryTransitions[0].toState;
            var logs = currentState.OnEnter();
            if (logs != null) AddLog(logs, currentState.name);
            isChanged = true;
        }

        // Update StateMachine if it's enabled
        public void UpdateStateMachine() {
            if (!enabled) return;
            // Update timer
            stateMachineTime += Time.fixedDeltaTime;
            currentState.OnUpdate();

            int nTransition = currentState.transitions.Count;
            // 遷移の判定
            // リストで番号が若いものが優先される
            // なお、1回のUpdateでは1回しか遷移しない
            for (int i = 0; i < nTransition; i++) {
                if (currentState.transitions[i].IsTransitable(0)) {
                    currentState.OnExit();
                    currentState = currentState.transitions[i].toState;
                    if (currentState == null) {
                        End();
                    } else {
                        var logs = currentState.OnEnter();
                        if (logs != null) AddLog(logs, currentState.name);
                        PredictFutureTransition();
                    }
                    isChanged = true;
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
            isChanged = true;
            ResetStateMachine();
        }

        void ResetStateMachine() {
            for (int i = 0; i < flags.flags.Count; i++) {
                flags.flags[i].enabled = false;
            }
            for(int i = 0; i < states.Count; i++) {
                states[i].IsCurrent = false;
            }
        }

        public void AddLog(List<BoneSubMovementPair> logs, string s) {
            foreach (var log in logs) {
                float duration = log.subMovement.t1 - log.subMovement.t0;
                log.subMovement.t0 += stateMachineTime;
                log.subMovement.t1 += stateMachineTime;
                actionLog.AddLog(log, s);
            }
        }

        public void PredictFutureTransition() {
            futureTransitions.Clear();
            actionLog.ClearFuture();
            
            ActionState predicted = currentState;
            if (currentState == null) predicted = entryTransitions[0].toState;
            for(int i = 0; i < maxPredictLength; i++) {
                if (predicted) {
                    if (predicted.transitions.Count == 0) break;
                    var transitables = predicted.transitions.Where(value => value.IsTransitableOnlyFlag());
                    if (transitables.Count() == 0) break;
                    ActionTransition temp = transitables.Min(); ;
                    futureTransitions.Add(temp);
                    predicted = temp.toState;
                    if(predicted != null) {
                        var boneKeyPoses = predicted.keyframe.boneKeyPoses;
                    }
                } else {
                    break;
                }
            }
        }

        // ----- ----- ----- ----- -----
        // その他

        static string GetCurrentDirectory() {
#if UNITY_EDITOR
            // source : https://qiita.com/r-ngtm/items/13d609cbd6a30e39f83a
            var flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
            var asm = Assembly.Load("UnityEditor.dll");
            var typeProjectBrowser = asm.GetType("UnityEditor.ProjectBrowser");
            var projectBrowserWindow = EditorWindow.GetWindow(typeProjectBrowser);
            return (string)typeProjectBrowser.GetMethod("GetActiveFolderPath", flag).Invoke(projectBrowserWindow, null);
#else
        return ""; // <!!> Need Future Implementation
#endif
        }

        public Object[] GetSubAssets() {
#if UNITY_EDITOR
            string path = AssetDatabase.GetAssetPath(this);
            return AssetDatabase.LoadAllAssetsAtPath(path);
#else
        return null;
#endif
        }
    }

}