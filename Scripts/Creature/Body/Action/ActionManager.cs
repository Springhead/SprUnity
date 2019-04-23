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
            if (GUILayout.Button("Stop")) {
                manager.QuitAction();
            }
        }
    }
#endif

    public class ActionStateMachineController {

        ActionStateMachine stateMachine;
        public ActionStateMachine StateMachine { get { return stateMachine; } }
        private float stateMachineTime = 0;
        private float timeOfLastEnter = 0.0f;
        //
        private TransitionFlagList flagList;
        private StateMachineParameters parameters;
        // 
        private ActionState currentState;
        public ActionState CurrentState { get { return currentState; } }
        float timeInCurrentStateFromEnter;
        
        // 
        [HideInInspector]
        public Body body;

        //
        ActionLog actionLog;
        public ActionLog ActionLog { get { return actionLog; } }
        public List<ActionTransition> futureTransitions = new List<ActionTransition>();
        public List<ActionTransition> specifiedTransitions = new List<ActionTransition>();
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

        public bool isChanged = false;

        public string Name { get { return this.stateMachine.name; } }

        public ActionStateMachineController(ActionStateMachine stateMachine, Body body = null) {
            this.stateMachine = stateMachine;
            this.body = body;
            if (body == null) this.body = GameObject.FindObjectOfType<Body>();
            actionLog = new ActionLog(body);
            specifiedTransitions = new List<ActionTransition>();
            specifiedTransitions.Add(stateMachine.entryTransitions[0]);
        }

        // 
        public void Begin() {

            // Check entry transition 
            if (stateMachine.entryTransitions.Count == 0) return;

            // Init
            Init();

            // <!!> 遷移が1パターン!!
            currentState = specifiedTransitions[0].toState;
            specifiedTransitions.RemoveAt(0);
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
            isChanged = true;
            Reset();
        }

        void Init() {
            enabled = true;
            stateMachineTime = 0;

            flagList = stateMachine.flags.Clone();
            parameters = stateMachine.parameters.Clone();

            actionLog = new ActionLog(body);
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
            specifiedTransitions.Clear();
            specifiedTransitions.Add(stateMachine.entryTransitions[0]);
        }

        public void AddLog(List<BoneSubMovementPair> logs, string s) {
            foreach (var log in logs) {
                log.subMovement.t0 += stateMachineTime;
                log.subMovement.t1 += stateMachineTime;
                actionLog.AddLog(log, s);
            }
        }

        public void PredictFutureTransition() {
            Debug.LogWarning("Predict called");
            futureTransitions.Clear();
            actionLog.ClearFuture();

            float startTime = timeOfLastEnter;

            // Specified transitions
            foreach(var specified in specifiedTransitions) {
                startTime += specified.time;
                float duration = specified.toState.duration;
                float spring = specified.toState.spring;
                float damper = specified.toState.damper;
                foreach(var boneKeyPose in specified.toState.keyframe.boneKeyPoses) {
                    actionLog.AddFuture(boneKeyPose, specified.toState.name, startTime, duration, spring, damper, body);
                }
            }

            ActionState predicted = specifiedTransitions.Count > 0 ? specifiedTransitions.Last().toState : currentState;
            if (currentState == null && specifiedTransitions.Count == 0) {
                predicted = stateMachine.entryTransitions[0].toState;
                futureTransitions.Add(stateMachine.entryTransitions[0]);
            }
            for (int i = futureTransitions.Count; i < maxPredictLength; i++) {
                if (predicted) {
                    if (predicted.transitions.Count == 0) break;
                    var transitables = predicted.transitions.Where(value => value.IsTransitableOnlyFlag());
                    if (transitables.Count() == 0) break;
                    ActionTransition temp = transitables.Min(); ;
                    futureTransitions.Add(temp);
                    predicted = temp.toState;
                    if (predicted != null) {
                        startTime += temp.time;
                        float duration = predicted.duration;
                        float spring = predicted.spring;
                        float damper = predicted.damper;
                        foreach (var boneKeyPose in predicted.keyframe.boneKeyPoses) {
                            actionLog.AddFuture(boneKeyPose, predicted.name, startTime, duration, spring, damper, body);
                        }
                    }
                } else {
                    break;
                }
            }
        }

        // Enter event of the state
        public List<BoneSubMovementPair> OnEnter() {
            timeInCurrentStateFromEnter = 0.0f;
            timeOfLastEnter = stateMachineTime;
            //Debug.Log("Enter state:" + currentState.name + " at time:" + Time.time);
            if (body == null) { body = GameObject.FindObjectOfType<Body>(); }
            if (body != null) {
                // ターゲット位置による変換後のKeyPose
                return currentState.keyframe.Action(body, currentState.duration, 0, currentState.spring, currentState.damper);
            }
            isChanged = true;
            return null;
        }

        public void OnUpdate() {
            timeInCurrentStateFromEnter += Time.fixedDeltaTime;
        }

        // Exit event of the state
        public void OnExit() {
            isChanged = true;
        }

        public bool Transit() {
            if(currentState.transitions.Count == 0) {
                OnExit();
                End();
            }
            if(specifiedTransitions.Count > 0) {
                if (currentState.transitions.Contains(specifiedTransitions[0])) {
                    if (IsTransitable(specifiedTransitions[0])) {
                        currentState.OnExit();
                        currentState = specifiedTransitions[0].toState;
                        specifiedTransitions.RemoveAt(0);
                        if (currentState == null) {
                            End();
                        } else {
                            var logs = OnEnter();
                            if (logs != null) AddLog(logs, currentState.name);
                            PredictFutureTransition();
                        }
                        stateMachine.isChanged = true;
                    } else {
                        return false;
                    }
                }
            }
            foreach (var transition in currentState.transitions) {
                //OK. transit to next state
                if (IsTransitable(transition)) {
                    //Debug.Log("Transit");
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
                    break;
                }
            }
            return false;
        }

        public bool IsTransitable(ActionTransition transition) {
            if (timeInCurrentStateFromEnter < transition.time) return false;
            foreach (var flag in transition.flags) {
                if (!flagList[flag]) {
                    return false;
                }
            }
            return true;
        }
    }

    public class ActionManager : MonoBehaviour {

        public Body body = null;

        public List<ActionStateMachine> actions = new List<ActionStateMachine>();
        private List<ActionStateMachineController> controllers = new List<ActionStateMachineController>();

        [HideInInspector]
        public ActionStateMachineController inAction = null;

        // ----- ----- ----- ----- -----

        private float time = 0.0f;
        private int index = 0;

        public bool useClone = true;

        // ----- ----- ----- ----- -----

        public ActionStateMachineController this[string key] {
            get {
                foreach (var action in controllers) { if (action.Name.Contains(key)) return action; }
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
            inAction = null;
        }
    }

}