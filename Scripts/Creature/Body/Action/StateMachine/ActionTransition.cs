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

    public class ActionTransition : ScriptableObject, System.IComparable<ActionTransition> {

        // ----- ----- ----- ----- -----

        [HideInInspector]
        public ActionStateMachine stateMachine;

        [HideInInspector]
        public ActionState toState;
        [HideInInspector]
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

        // ----- ----- ----- ----- -----
        // Editor関係
        [HideInInspector]
        public int transitionNumber = 0;
        [HideInInspector]
        public int transitionCountSamePairs = 1;
        //
        bool isSelected = false;
        Vector2 centerForMouseDetection;
        Rect selectRect = new Rect(10, 10, 0, 0);
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
        // 
        public int CompareTo(ActionTransition other) {
            if (this.time == other.time) return 0;
            else if (this.time < other.time) return -1;
            else return 1;
        }

        // ----- ----- ----- ----- ----- -----
        // State Machine Events

        public void Transit() {

        }

        public bool IsTransitable(float t) {
            if (stateMachine.CurrentState.TimeFromEnter < time) return false;
            foreach (var flag in flags) {
                if (!stateMachine.flags[flag]) return false;
            }
            return true;
        }

        public bool IsTransitableOnlyFlag() {
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
            if (isSelected) Handles.color = selectedColor;
            else Handles.color = defaultColor;
            if (toState != null && fromState != null) {
                if (toState == fromState) {
                    float diff = transitionNumber * 10;
                    Vector3 center = new Vector3(fromState.stateNodeRect.x - 20 - diff, fromState.stateNodeRect.center.y, 0);
                    Handles.DrawWireDisc(center, new Vector3(0, 0, 1), 30 + diff);
                    Handles.DrawSolidArc(center + new Vector3(-30 - diff, -10, 0), new Vector3(0, 0, 1), new Vector3(Mathf.Cos((Mathf.PI / 2) - 0.3f), Mathf.Sin((Mathf.PI / 2) - 0.3f), 0), 0.6f * Mathf.Rad2Deg, 15);
                    centerForMouseDetection = new Vector2(center.x - 30 - diff, center.y);
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
            selectRect.width = 10; selectRect.height = 10;
            selectRect.position = centerForMouseDetection - new Vector2(5, 5);
            GUI.Box(selectRect, "");
#endif
        }

        public bool ProcessEvents() {
#if UNITY_EDITOR
            Event e = Event.current;
            switch (e.type) {
                case EventType.MouseDown:
                    if (e.button == 0) {
                        if (selectRect.Contains(e.mousePosition)) {
                            isSelected = true;
                            Selection.activeObject = this;
                            GUI.changed = true;
                        } else {
                            GUI.changed = true;
                            isSelected = false;
                        }
                    }
                    if (e.button == 1) {
                        if (selectRect.Contains(e.mousePosition)) {
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
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent("Delete this transition"), false, () => OnDelete());
            genericMenu.ShowAsContext();
        }

        private void OnDelete() {
            var path = AssetDatabase.GetAssetPath(this.stateMachine);
            if(fromState == null) {
                stateMachine.entryTransitions.Remove(this);
            } else {
                fromState.transitions.Remove(this);
            }
            Object.DestroyImmediate(this, true);
            AssetDatabase.ImportAsset(path);
        }

        public void OnValidate() {
            stateMachine.isChanged = true;
        }

        // https://qiita.com/boiledorange73/items/bcd4e150e7caa0210ee6
        private bool Contains(Vector2 s, Vector2 f, Vector2 p) {
            // 直線からの距離で当たり判定
            float dist = 0;
            if (s == f) return false;
            float a = f.x - s.x;
            float b = f.y - s.y;
            float a2 = a * a;
            float b2 = b * b;
            float r2 = a2 + b2;
            float tt = -(a * (s.x - p.x) + b * (s.y - p.y));
            if(tt < 0) {
                dist = (s.x - p.x) * (s.x - p.x) + (s.y - p.y) * (s.y - p.y);
            }else if(tt > r2) {
                dist = (f.x - p.x) * (f.x - p.x) * (f.y - p.y) * (f.y - p.y);
            } else {
                float f1 = a * (s.y - p.y) - b * (s.x - p.x);
                dist = (f1 * f1) / r2;
            }
            // 判定
            float width = 5.0f;
            if (dist < width * width) return true;
            else return false;
        }
    }

}