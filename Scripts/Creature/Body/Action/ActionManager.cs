using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using SprCs;

namespace SprUnity {

#if UNITY_EDITOR
    [CustomEditor(typeof(ActionManager))]
    public class ActionManagerEditor : Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            ActionManager manager = (ActionManager)target;
            for(int i = 0; i < manager.actions.Count; i++) {
                if(GUILayout.Button("Action " + i)) {
                    manager.Action(manager.actions[i].name);
                }
            }
        }
    }
#endif

    public class ActionStateMachineController {

        ActionStateMachine stateMachine;
        private float stateMachineTime = 0;

        private TransitionFlagList flagList;
        private StateMachineParameters parameters;
        // 
        private ActionState currentState;
        public ActionState CurrentState { get { return currentState; } }
        float timeInCurrentStateFromEnter;
        
        // 適用するBody
        // Start時に対応させる
        [HideInInspector]
        public Body body;

        //
        ActionLog actionLog;
        public ActionLog ActionLog { get { return actionLog; } }
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

        public string Name { get { return this.stateMachine.name; } }

        public ActionStateMachineController(ActionStateMachine stateMachine, Body body = null) {
            this.stateMachine = stateMachine;
            this.body = body;
            if (body == null) this.body = GameObject.FindObjectOfType<Body>();
        }

        // 
        public void Begin() {

            // Check entry transition 
            if (stateMachine.entryTransitions.Count == 0) return;

            // Init
            Init();

            // <!!> 遷移が1パターン!!
            currentState = stateMachine.entryTransitions[0].toState;
            var logs = OnEnter();
            if (logs != null) AddLog(logs, currentState.name);
        }

        // Update StateMachine if it's enabled
        public void UpdateStateMachine() {
            if (!enabled) return;
            // Update timer
            stateMachineTime += Time.fixedDeltaTime;
            OnUpdate();
            Transit();
        }

        // 
        public void End() {
            enabled = false;
            stateMachine.isChanged = true;
            Reset();
        }

        void Init() {
            enabled = true;
            stateMachineTime = 0;

            actionLog = new ActionLog();
            futureTransitions = new List<ActionTransition>();
        }

        void Reset() {
            /*
            for (int i = 0; i < flags.flags.Count; i++) {
                flags.flags[i].enabled = false;
            }
            for (int i = 0; i < states.Count; i++) {
                states[i].IsCurrent = false;
            }
            */
        }

        public void AddLog(List<BoneSubMovementPair> logs, string s) {
            foreach (var log in logs) {
                log.subMovement.t0 += stateMachineTime;
                log.subMovement.t1 += stateMachineTime;
                actionLog.AddLog(log, s);
            }
        }

        public void PredictFutureTransition() {
            futureTransitions.Clear();
            actionLog.ClearFuture();

            ActionState predicted = currentState;
            if (currentState == null) predicted = stateMachine.entryTransitions[0].toState;
            for (int i = 0; i < maxPredictLength; i++) {
                if (predicted) {
                    if (predicted.transitions.Count == 0) break;
                    var transitables = predicted.transitions.Where(value => value.IsTransitableOnlyFlag());
                    if (transitables.Count() == 0) break;
                    ActionTransition temp = transitables.Min(); ;
                    futureTransitions.Add(temp);
                    predicted = temp.toState;
                    if (predicted != null) {
                        var boneKeyPoses = predicted.keyframe.boneKeyPoses;
                    }
                } else {
                    break;
                }
            }
        }

        // Enter event of the state
        public List<BoneSubMovementPair> OnEnter() {
            timeInCurrentStateFromEnter = 0.0f;
            Debug.Log("Enter state:" + currentState.name + " at time:" + Time.time);
            if (body == null) { body = GameObject.FindObjectOfType<Body>(); }
            if (body != null) {
                // ターゲット位置による変換後のKeyPose
                return currentState.keyframe.Action(body, currentState.duration, 0, currentState.spring, currentState.damper);
            }
            stateMachine.isChanged = true;
            return null;
        }

        public void OnUpdate() {
            timeInCurrentStateFromEnter += Time.fixedDeltaTime;
        }

        // Exit event of the state
        public void OnExit() {
            stateMachine.isChanged = true;
        }

        public bool Transit() {
            if(currentState.transitions.Count == 0) {
                OnExit();
                End();
            }
            foreach (var transition in currentState.transitions) {
                if (timeInCurrentStateFromEnter < transition.time) continue;
                bool isFlagEnabledAll = true;
                foreach(var flag in transition.flags) {
                    if (!flagList[flag]) {
                        isFlagEnabledAll = false;
                        continue;
                    }
                }
                //OK. transit to next state
                if (isFlagEnabledAll) {
                    currentState.OnExit();
                    currentState = transition.toState;
                    if (currentState == null) {
                        End();
                    } else {
                        var logs = OnEnter();
                        if (logs != null) AddLog(logs, currentState.name);
                        PredictFutureTransition();
                    }
                    stateMachine.isChanged = true;
                }
            }
            return false;
        }
    }

    public class ActionManager : MonoBehaviour {

        public Body body = null;

        public List<ActionStateMachine> actions = new List<ActionStateMachine>();
        private List<ActionStateMachineController> controllers;

        [HideInInspector]
        public ActionStateMachineController inAction = null;

        // ----- ----- ----- ----- -----

        private float time = 0.0f;
        private int index = 0;

        public bool useClone = true;

        // ----- ----- ----- ----- -----

        public ActionStateMachine this[string key] {
            get {
                foreach (var action in actions) { if (action.name.Contains(key)) return action; }
                return null;
            }
        }

        void Start() {
            if (useClone) {
                for (int i = 0; i < actions.Count; i++) {
                    actions[i] = ScriptableObject.Instantiate<ActionStateMachine>(actions[i]);
                }
            }
            controllers = new List<ActionStateMachineController>();
            foreach(var action in actions) {
                controllers.Add(new ActionStateMachineController(action, body));
            }
        }

        void Update() {

        }

        private void FixedUpdate() {
            if(body != null && inAction != null) {
                inAction.UpdateStateMachine();
            }
        }

        // ----- ----- ----- ----- -----

        public void Action(string name) {
            if (inAction != null) {
                inAction.End();
            }
            print("Action: " + name);
            foreach (var action in controllers) {
                if (action.Name.Contains(name)) {
                    inAction = action;
                    inAction.Begin();
                }
            }
        }

        public void QuitAction() {
            inAction.End();
        }
    }

}