using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SprUnity;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SprUnity {

#if UNITY_EDITOR
    [CustomEditor(typeof(ActionState))]
    public class ActionStateEditor : Editor {
        public override void OnInspectorGUI() {
            bool textChangeComp = false;
            EditorGUI.BeginChangeCheck();
            Event e = Event.current;
            if (e.keyCode == KeyCode.Return && Input.eatKeyPressOnTextFieldFocus) {
                textChangeComp = true;
                Event.current.Use();
            }
            target.name = EditorGUILayout.TextField("Name", target.name);
            base.OnInspectorGUI();
            if (EditorGUI.EndChangeCheck()) {
                EditorUtility.SetDirty(target);
            }
            if (textChangeComp) {
                string mainPath = AssetDatabase.GetAssetPath(this);
                //EditorUtility.SetDirty(AssetDatabase.LoadMainAssetAtPath(mainPath));
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath((ActionState)target));
            }
        }
    }
#endif

    public class ActionState : ScriptableObject {
        // member values
        public ActionStateMachine stateMachine;
        public KeyPoseInterpolationGroup keyframe;
        public List<ActionTransition> transitions = new List<ActionTransition>();
        public List<string> useParams;

        public float duration = 0.5f;
        public float spring = 1.0f;
        public float damper = 1.0f;

        [HideInInspector]
        private float timeFromEnter;
        public float TimeFromEnter {
            get {
                return timeFromEnter;
            }
        }

        // ベースキーポーズと対称依存軌道上成分の内分比
        // baseKeyPose 0.0 <--> 1.0 referencePointOnTrajectory
        public float internalRatio = 0.0f;

        //[HideInInspector]
        public Rect stateNodeRect = new Rect(0, 0, 200, 50);
        public bool isDragged;
        public bool isSelected;
        public int serialCount;

        static public GUIStyle appliedStyle = new GUIStyle();
        static public GUIStyle defaultStyle = new GUIStyle();
        static public GUIStyle selectedStyle = new GUIStyle();
        static public GUIStyle currentStateStyle = new GUIStyle();

        // ----- ----- ----- ----- ----- -----
        // Setter/Getter

        // ----- ----- ----- ----- ----- -----
        // Creator

        static ActionTransition CreateTransition(ActionState from, ActionState to) {
#if UNITY_EDITOR
            var transition = ScriptableObject.CreateInstance<ActionTransition>();
            transition.name = "transition";
            transition.fromState = from;
            transition.toState = to;

            if (from != null) {
                // ActionStateからの遷移
                AssetDatabase.AddObjectToAsset(transition, from);
                from.transitions.Add(transition);
                transition.stateMachine = from.stateMachine;
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(from));
            } else {
                // Entryからの遷移
                AssetDatabase.AddObjectToAsset(transition, to.stateMachine);
                to.stateMachine.entryTransitions.Add(transition);
                transition.stateMachine = to.stateMachine;
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(to.stateMachine));
            }

            return transition;
#else
        return null;
#endif
        }

        // ----- ----- ----- ----- ----- -----
        // State Machine Events

        // Enter event of the state
        public void OnEnter() {
            isSelected = true;
            timeFromEnter = 0.0f;
            appliedStyle = currentStateStyle;
            //keyframe.Action(stateMachine.body);
            Body body = stateMachine.body;
            GameObject target = stateMachine.targetObject;
            if (body == null) { body = GameObject.FindObjectOfType<Body>(); }
            if (body != null) {
                List<float> parameters = new List<float>();
                foreach (var l in useParams) {
                    parameters.Add(stateMachine.parameters[l]);
                }
                // ターゲット位置による変換後のKeyPose
                keyframe.Action(body, duration, 0, spring, damper, parameters);
            }
            Debug.Log("Enter state:" + name + " at time:" + Time.time);
        }

        public void OnUpdate() {
            timeFromEnter += Time.fixedDeltaTime;
        }

        // Exit event of the state
        public void OnExit() {
            isSelected = false;
            appliedStyle = defaultStyle;
        }


        // ----- ----- ----- ----- ----- -----
        // Editor

        public void DrawStateNode(int id) {
            //GUI.DragWindow();
            /*
            int nTransitions = transitions.Count;
            for(int i = 0; i < nTransitions; i++) {
                Rect rect = new Rect(new Vector2(0, i * 20 + 15), new Vector2(stateNodeRect.width, 20));
                transitions[i].priority = i;
                GUI.Box(rect, transitions[i].name);
            }
            stateNodeRect.height = Mathf.Max(20 + 20 * nTransitions, 50);
            */
        }

        public void Drag(Vector2 delta) {
            stateNodeRect.position += delta;
        }

        public void Draw(int id) {
            //GUI.Box(stateNodeRect, name, appliedStyle);
            GUI.Window(id, stateNodeRect, DrawStateNode, name);
        }

        public bool ProcessEvents() {
#if UNITY_EDITOR
            Event e = Event.current;
            switch (e.type) {
                case EventType.MouseDown:
                    if (e.button == 0) {
                        if (stateNodeRect.Contains(e.mousePosition)) {
                            isDragged = true;
                            isSelected = true;
                            Selection.activeObject = this;
                            appliedStyle = selectedStyle;
                            GUI.changed = true;
                        } else {
                            GUI.changed = true;
                            isSelected = false;
                            appliedStyle = defaultStyle;
                        }
                    }
                    if (e.button == 1) {
                        if (stateNodeRect.Contains(e.mousePosition)) {
                            OnContextMenu(e.mousePosition);
                        }
                    }
                    break;

                case EventType.MouseUp:
                    isDragged = false;
                    break;

                case EventType.MouseDrag:
                    if (e.button == 0 && isDragged) {
                        Drag(e.delta);
                        e.Use();
                        return true;
                    }
                    break;
            }
#endif
            return false;
        }

        private void OnContextMenu(Vector2 mousePosition) {
#if UNITY_EDITOR
            GenericMenu genericMenu = new GenericMenu();
            List<ActionState> states = stateMachine.states;
            int nStates = states.Count;
            for (int i = 0; i < nStates; i++) {
                ActionState state = states[i]; // 
                genericMenu.AddItem(new GUIContent("Add Transition to../" + states[i].name), false, () => OnClickAddTransition(this, state));
            }
            genericMenu.AddItem(new GUIContent("Add Transition to../" + "Exit"), false, () => OnClickAddTransition(this, null));
            genericMenu.AddItem(new GUIContent("Add Transition from../" + "Entry"), false, () => OnClickAddTransition(null, this));
            int nTransitions = transitions.Count;
            for (int i = 0; i < nTransitions; i++) {
                ActionTransition transition = transitions[i]; // 
                genericMenu.AddItem(new GUIContent("Remove Transition/" + transition.name), false, () => OnRemoveTransition(transition));
            }
            genericMenu.AddItem(new GUIContent("Remove State"), false, () => OnRemoveState());
            genericMenu.ShowAsContext();
#endif
        }

        private void OnClickAddTransition(ActionState from, ActionState to) {
            CreateTransition(from, to);
        }

        private void OnRemoveState() {
            // ステートマシンの関係する遷移を全部消す
            List<ActionTransition> deleteList = new List<ActionTransition>();
            List<ActionTransition> removeListEntry = new List<ActionTransition>();
            var entryTransitions = stateMachine.entryTransitions;
            for (int i = 0; i < entryTransitions.Count; i++) {
                if (entryTransitions[i].toState == this) {
                    removeListEntry.Add(entryTransitions[i]);
                    deleteList.Add(entryTransitions[i]);
                }
            }
            foreach (var transition in removeListEntry) {
                entryTransitions.Remove(transition);
            }
            var states = stateMachine.states;
            List<ActionTransition> removeList = new List<ActionTransition>();
            for (int i = 0; i < states.Count; i++) {
                removeList.Clear();
                foreach (var transition in states[i].transitions) {
                    if (transition.fromState == this || transition.toState == this) {
                        removeList.Add(transition);
                        deleteList.Add(transition);
                    }
                }
                foreach (var transition in removeList) {
                    states[i].transitions.Remove(transition);
                }
            }
            foreach (var deleteTransition in deleteList) {
                Object.DestroyImmediate(deleteTransition, true);
            }
#if UNITY_EDITOR
            var path = AssetDatabase.GetAssetPath(this.stateMachine);
            Object.DestroyImmediate(this, true);
            AssetDatabase.ImportAsset(path);
#endif
        }

        private void OnRemoveTransition(ActionTransition transition) {
#if UNITY_EDITOR
            transitions.Remove(transition);
            Object.DestroyImmediate(transition, true);
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(this.stateMachine));
#endif
        }

        public void SetDefaultNodeStyle(GUIStyle style) {
            defaultStyle = style;
        }
        public void SetSelectedNodeStyle(GUIStyle style) {
            selectedStyle = style;
        }
    }

}