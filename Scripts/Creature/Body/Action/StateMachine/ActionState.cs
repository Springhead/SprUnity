using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SprUnity;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VGent{

    public class ActionState : ScriptableObject {
        // ----- ----- ----- ----- -----

        [HideInInspector]
        public ActionStateMachine stateMachine;
        public ActionTargetOutputNode[] nodes;
        [HideInInspector]
        public List<ActionStateTransition> transitions = new List<ActionStateTransition>();
        public List<string> useParams;

        public enum DurationMode {
            Static,           // 一定
            VelocityBase,     // 平均速度と距離から決定
            Fitts,            // フィッツ則による決定
        };
        public DurationMode durationMode;
        public float duration = 0.5f;
        public float fittsA = 0.5f;
        public float fittsB;
        public float accuracy = 0.01f;
        public float spring = 1.0f;
        public float damper = 1.0f;
        public bool durationNoise;

        public bool useFace = false;
        public string blend = "";
        public float blendv = 1f;
        public float time = 0.3f;
        public float interval = 0f;

        // ----- ----- ----- ----- -----
        // 実行時
        [HideInInspector]
        private float timeFromEnter;
        public float TimeFromEnter {
            get {
                return timeFromEnter;
            }
        }

        // ----- ----- ----- ----- -----
        // Editor関係
        [HideInInspector]
        public Rect stateNodeRect = new Rect(0, 0, 200, 50);
        private bool isDragged;
        private bool isSelected;
        private bool isCurrent = false;
        [HideInInspector]
        public int serialCount;

        private GUIStyle appliedStyle = new GUIStyle();
        static public GUIStyle defaultStyle = new GUIStyle();
        static public GUIStyle selectedStyle = new GUIStyle();
        static public GUIStyle currentStateStyle = new GUIStyle();

        // ----- ----- ----- ----- ----- -----
        // Setter/Getter
        public bool IsCurrent {
            get {
                return isCurrent;
            }
            set {
                isCurrent = value;
            }
        }

        // ----- ----- ----- ----- ----- -----
        // Creator

        static ActionStateTransition CreateTransition(ActionState from, ActionState to) {
#if UNITY_EDITOR
            var transition = ScriptableObject.CreateInstance<ActionStateTransition>();
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
                transition.time = 0.0f;
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
        public List<BoneSubMovementPair> OnEnter(ActionStateMachine aStateMachine, out float d) {
            isCurrent = true;
            timeFromEnter = 0.0f;
            Body body = aStateMachine.Body;
            KeyPose kp = new KeyPose();
            var manager = aStateMachine.manager;
            if (nodes != null) {
                aStateMachine.ApplyParameters();
                foreach (var boneKeyPose in nodes) {
                    //var nodeInstance = (boneKeyPose.graph as ActionTargetGraph).GetInstance(manager).nodes[index] as BoneKeyPoseNode;
                    kp.boneKeyPoses.Add(boneKeyPose.GetBoneKeyPose());
                }
            } 
            switch (durationMode) {
                case DurationMode.Static:
                    d = duration;
                    break;
                case DurationMode.VelocityBase:
                    d = duration * accuracy;
                    break;
                case DurationMode.Fitts:
                    float dis = 1.0f;
                    if (kp.boneKeyPoses.Count == 1) {
                        dis = Vector3.Magnitude(body[kp.boneKeyPoses[0].boneId].transform.position - kp.boneKeyPoses[0].position);
                    }
                    d = fittsA + fittsB * Mathf.Log((1 + (dis /Mathf.Max(accuracy, 0.00001f))), 2);
                    break;
                default:
                    d = duration;
                    break;
            }
            if (durationNoise) {
                d *= 1.05f * GaussianRandom.random();
            }
            Debug.Log("OnEnter");
            if (body != null && kp != null) {
                return kp.Action(body, duration, 0, spring, damper);
            }
            if (useFace) {
                if (stateMachine.blendController != null) {
                    stateMachine.blendController.BlendSet(interval, blend, blendv, time);
                }
            }
            return null;
        }

        public void OnUpdate() {
            timeFromEnter += Time.fixedDeltaTime;
        }

        // Exit event of the state
        public void OnExit() {
            
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

        public void Draw(int id, bool isCurrent, Rect position, float zoom, Vector2 panOffset) {
            //GUI.Box(stateNodeRect, name, GUI.skin.box);
            Vector2 center = position.size * 0.5f;
            float xOffset = Mathf.Round(center.x * zoom + (panOffset.x + stateNodeRect.x));
            float yOffset = Mathf.Round(center.y * zoom + (panOffset.y + stateNodeRect.y));
            Rect pos = stateNodeRect;//new Rect(new Vector2(xOffset, yOffset), stateNodeRect.size);

            if (isCurrent) {
                if (isSelected || isDragged) {
                    GUI.Window(id, pos, DrawStateNode, name, "flow node 6 on");
                } else {
                    GUI.Window(id, pos, DrawStateNode, name, "flow node 6");
                }
            }else if (isSelected || isDragged) {
                GUI.Window(id, pos, DrawStateNode, name, "flow node 0 on");
            } else {
                GUI.Window(id, pos, DrawStateNode, name, "flow node 0");
            }
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
                            GUI.changed = true;
                        } else {
                            GUI.changed = true;
                            isSelected = false;
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
                genericMenu.AddItem(new GUIContent("Add Transition to../" + states[i].name), false, () => { OnClickAddTransition(this, state); stateMachine.isChanged = true; });
            }
            genericMenu.AddItem(new GUIContent("Add Transition to../" + "Exit"), false, () => { OnClickAddTransition(this, null); stateMachine.isChanged = true; });
            genericMenu.AddItem(new GUIContent("Add Transition from../" + "Entry"), false, () => { OnClickAddTransition(null, this); stateMachine.isChanged = true; });
            int nTransitions = transitions.Count;
            for (int i = 0; i < nTransitions; i++) {
                ActionStateTransition transition = transitions[i]; // 
                genericMenu.AddItem(new GUIContent("Remove Transition/" + transition.name + i), false, () => { OnRemoveTransition(transition); stateMachine.isChanged = true; });
            }
            genericMenu.AddItem(new GUIContent("Remove State"), false, () => { OnRemoveState(); stateMachine.isChanged = true; });
            genericMenu.ShowAsContext();
#endif
        }

        private void OnClickAddTransition(ActionState from, ActionState to) {
            CreateTransition(from, to);
        }

        private void OnRemoveState() {
            // ステートマシンの関係する遷移を全部消す
            List<ActionStateTransition> deleteList = new List<ActionStateTransition>();
            List<ActionStateTransition> removeListEntry = new List<ActionStateTransition>();
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
            List<ActionStateTransition> removeList = new List<ActionStateTransition>();
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

        private void OnRemoveTransition(ActionStateTransition transition) {
#if UNITY_EDITOR
            transitions.Remove(transition);
            Object.DestroyImmediate(transition, true);
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(this.stateMachine));
#endif
        }

        public void OnValidate() {

        }
    }

}