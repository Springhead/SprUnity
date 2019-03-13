using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SprUnity {

#if UNITY_EDITOR
    [CustomEditor(typeof(ActionTransition))]
    public class ActionTransitionEditor : Editor {
        public override void OnInspectorGUI() {
            EditorGUI.BeginChangeCheck();
            target.name = EditorGUILayout.TextField("Name", target.name);
            base.OnInspectorGUI();
            if (EditorGUI.EndChangeCheck()) {
                EditorUtility.SetDirty(target);
                string mainPath = AssetDatabase.GetAssetPath(this);
                //EditorUtility.SetDirty(AssetDatabase.LoadMainAssetAtPath(mainPath));
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath((ActionTransition)target));
            }
        }
    }
#endif

    public class ActionTransition : ScriptableObject {

        public ActionStateMachine stateMachine;

        [SerializeField]
        public ActionState toState;
        [SerializeField]
        public ActionState fromState;

        [HideInInspector]
        public int priority;
        /*
        public class TransitionCondition {
            public float time = 1.0f;
            public List<string> flags = new List<string>();
        }
        public List<TransitionCondition> conditions = new List<TransitionCondition>();
        */
        public float time = 1.0f;
        public List<string> flags = new List<string>();
        //
        [HideInInspector]
        public int transitionNumber = 0;
        [HideInInspector]
        public int transitionCountSamePairs = 1;
        //
        bool isSelected = false;
        Vector2 centerForMouseDetection;
        static Color defaultColor = Color.white;
        static Color selectedColor = Color.red;


        // ----- ----- ----- ----- ----- -----
        // 

        static void CreateTransition(ActionState from, ActionState to) {
#if UNITY_EDITOR
            var transition = ScriptableObject.CreateInstance<ActionTransition>();
            transition.name = "transition";
            transition.fromState = from;
            transition.toState = to;

            if (from != null) {
                AssetDatabase.AddObjectToAsset(transition, from);
                from.transitions.Add(transition);
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(from));
            } else {
                AssetDatabase.AddObjectToAsset(transition, to.stateMachine);
                to.stateMachine.entryTransitions.Add(transition);
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(to.stateMachine));
            }
#endif
        }

        // ----- ----- ----- ----- ----- -----
        // State Machine Events

        public void Transit() {

        }

        public bool IsTransitable() {
            if (stateMachine.CurrentState.TimeFromEnter < time) return false;
            foreach (var flag in flags) {
                if (!stateMachine.flags[flag]) return false;
            }
            return true;
        }

        // ----- ----- ----- ----- ----- -----
        // Other

        // ----- ----- ----- ----- ----- -----
        // Editor

        //
        public void Draw() {
#if UNITY_EDITOR
            /*
            if (toState != null && fromState != null) {
                Vector3 startPos = new Vector3(fromState.stateNodeRect.xMax, fromState.stateNodeRect.yMin + 25 + priority * 20, 0);
                Vector3 endPos = new Vector3(toState.stateNodeRect.xMin, toState.stateNodeRect.y, 0);
                Vector3 startTangent = startPos + new Vector3(100f, 0f, 0f);
                Vector3 endTangent = endPos + new Vector3(-100f, 0f, 0f);
                Handles.DrawBezier(startPos, endPos, startTangent, endTangent, Color.red, null, 4f);
            } else if(fromState == null && toState != null) {
                Vector3 startPos = new Vector3(stateMachine.entryRect.xMax, stateMachine.entryRect.yMin + 25, 0);
                Vector3 endPos = new Vector3(toState.stateNodeRect.xMin, toState.stateNodeRect.y, 0);
                Vector3 startTangent = startPos + new Vector3(100f, 0f, 0f);
                Vector3 endTangent = endPos + new Vector3(-100f, 0f, 0f);
                Handles.DrawBezier(startPos, endPos, startTangent, endTangent, Color.red, null, 4f);
            } else if(toState == null && fromState != null) {
                Vector3 startPos = new Vector3(fromState.stateNodeRect.xMax, fromState.stateNodeRect.yMin + 25 + priority * 20, 0);
                Vector3 endPos = new Vector3(stateMachine.exitRect.xMin, stateMachine.exitRect.y, 0);
                Vector3 startTangent = startPos + new Vector3(100f, 0f, 0f);
                Vector3 endTangent = endPos + new Vector3(-100f, 0f, 0f);
                Handles.DrawBezier(startPos, endPos, startTangent, endTangent, Color.red, null, 4f);
            }
            */
            if (isSelected) Handles.color = selectedColor;
            else Handles.color = defaultColor;
            if (toState != null && fromState != null) {
                if (toState == fromState) {
                    Vector3 center = new Vector3(fromState.stateNodeRect.x - 20, fromState.stateNodeRect.center.y, 0);
                    Handles.DrawWireDisc(center, new Vector3(0, 0, 1), 30);
                    Handles.DrawSolidArc(center + new Vector3(-30, -10, 0), new Vector3(0, 0, 1), new Vector3(Mathf.Cos((Mathf.PI / 2) - 0.3f), Mathf.Sin((Mathf.PI / 2) - 0.3f), 0), 0.6f * Mathf.Rad2Deg, 15);
                    centerForMouseDetection = new Vector2(center.x - 30, center.y);
                } else {
                    //float width = Mathf.Min(50, (transitionCountSamePairs - 1) * 10);
                    float width = (transitionCountSamePairs - 1) * 10;
                    float diff = 10 * transitionNumber - width / 2;
                    Vector3 startPos = new Vector3(fromState.stateNodeRect.center.x, fromState.stateNodeRect.center.y, 0);
                    Vector3 endPos = new Vector3(toState.stateNodeRect.center.x, toState.stateNodeRect.center.y, 0);
                    float angle = Mathf.Acos(Vector3.Dot((startPos - endPos).normalized, new Vector3(1, 0, 0)));
                    Vector3 diffVec = new Vector3(-Mathf.Sin(angle), Mathf.Cos(angle), 0) * diff;
                    //Vector3 diffVec = new Vector3(Mathf.Cos(angle - Mathf.PI / 2), Mathf.Sin(angle - Mathf.PI / 2), 0) * diff;
                    startPos += diffVec;
                    endPos += diffVec;
                    if ((startPos - endPos).y < 0) angle *= -1;
                    Handles.DrawLine(startPos, endPos);
                    Handles.DrawSolidArc((startPos + endPos) / 2, new Vector3(0, 0, 1), new Vector3(Mathf.Cos(angle - 0.2f), Mathf.Sin(angle - 0.2f), 0), 0.4f * Mathf.Rad2Deg, 20);
                    centerForMouseDetection = new Vector2(((startPos + endPos) / 2).x, ((startPos + endPos) / 2).y);
                }
            } else if (fromState == null && toState != null) {
                Vector3 startPos = new Vector3(stateMachine.entryRect.center.x, stateMachine.entryRect.center.y, 0);
                Vector3 endPos = new Vector3(toState.stateNodeRect.center.x, toState.stateNodeRect.center.y, 0);
                Handles.DrawLine(startPos, endPos);
                float angle = Mathf.Acos(Vector3.Dot((startPos - endPos).normalized, new Vector3(1, 0, 0)));
                if ((startPos - endPos).y < 0) angle *= -1;
                Handles.DrawSolidArc((startPos + endPos) / 2, new Vector3(0, 0, 1), new Vector3(Mathf.Cos(angle - 0.2f), Mathf.Sin(angle - 0.2f), 0), 0.4f * Mathf.Rad2Deg, 20);
                centerForMouseDetection = new Vector2(((startPos + endPos) / 2).x, ((startPos + endPos) / 2).y);
            } else if (toState == null && fromState != null) {
                Vector3 startPos = new Vector3(fromState.stateNodeRect.center.x, fromState.stateNodeRect.center.y, 0);
                Vector3 endPos = new Vector3(stateMachine.exitRect.center.x, stateMachine.exitRect.center.y, 0);
                Handles.DrawLine(startPos, endPos);
                float angle = Mathf.Acos(Vector3.Dot((startPos - endPos).normalized, new Vector3(1, 0, 0)));
                if ((startPos - endPos).y < 0) angle *= -1;
                Handles.DrawSolidArc((startPos + endPos) / 2, new Vector3(0, 0, 1), new Vector3(Mathf.Cos(angle - 0.2f), Mathf.Sin(angle - 0.2f), 0), 0.4f * Mathf.Rad2Deg, 20);
                centerForMouseDetection = new Vector2(((startPos + endPos) / 2).x, ((startPos + endPos) / 2).y);
            }
#endif
        }

        public bool ProcessEvents() {
#if UNITY_EDITOR
            Event e = Event.current;
            switch (e.type) {
                case EventType.MouseDown:
                    if (e.button == 0) {
                        if ((centerForMouseDetection - e.mousePosition).magnitude < 10) {
                            isSelected = true;
                            Selection.activeObject = this;
                            GUI.changed = true;
                        } else {
                            GUI.changed = true;
                            isSelected = false;
                        }
                    }
                    if (e.button == 1) {
                        if ((centerForMouseDetection - e.mousePosition).magnitude < 10) {
                            OnContextMenu(e.mousePosition);
                        }
                    }
                    break;

                case EventType.MouseUp:
                    break;
            }
#endif
            return false;
        }

        private void OnContextMenu(Vector2 mousePosition) {

        }
    }

}