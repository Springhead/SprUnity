using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

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

[CreateAssetMenu(menuName = "Action/ Create ActionTransition Instance")]
public class ActionTransition : ScriptableObject {

    public ActionStateMachine stateMachine;

    [SerializeField]
    public ActionState toState;
    public ActionState fromState;

    [HideInInspector]
    public int priority;

    public float time = 1.0f;
    public List<string> flags = new List<string>();


    // ----- ----- ----- ----- ----- -----
    // 

    static void CreateTransition(ActionState from, ActionState to) {
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
    }

    // ----- ----- ----- ----- ----- -----
    // State Machine Events

    public void Transit() {

    }

    public bool IsTransitable() {
        if (stateMachine.currentState.timeFromEnter < time) return false;
        foreach(var flag in flags) {
            if (!stateMachine.flags[flag]) return false;
        }
        return true;
    }

    // ----- ----- ----- ----- ----- -----
    // Editor

    //
    public void Draw() {
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
    }

    public bool ProcessEvents() {
        return false;
    }
}

#endif