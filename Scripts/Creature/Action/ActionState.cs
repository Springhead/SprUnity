using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(ActionState))]
public class ActionStateEditor : Editor {
    public override void OnInspectorGUI() {
        EditorGUI.BeginChangeCheck();
        target.name = EditorGUILayout.TextField("Name", target.name);
        base.OnInspectorGUI();
        if (EditorGUI.EndChangeCheck()) {
            EditorUtility.SetDirty(target);
            string mainPath = AssetDatabase.GetAssetPath(this);
            //EditorUtility.SetDirty(AssetDatabase.LoadMainAssetAtPath(mainPath));
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath((ActionState)target));
        }
    }
}

[CreateAssetMenu(menuName = "Action/Create ActionState Instance")]
public class ActionState : ScriptableObject {
    // member values
    public ActionStateMachine stateMachine;
    public List<ActionKeyFrame> keyframes = new List<ActionKeyFrame>();
    public List<ActionTransition> transitions = new List<ActionTransition>();
    [HideInInspector]
    public float timeFromEnter;

    //[HideInInspector]
    public Rect stateNodeRect = new Rect(0, 0, 200, 50);
    public bool isDragged;
    public bool isSelected;

    public GUIStyle currentStyle = new GUIStyle();
    public GUIStyle defaultStyle = new GUIStyle();
    public GUIStyle selectedStyle = new GUIStyle();

    // ----- ----- ----- ----- ----- -----
    // Setter/Getter

    // ----- ----- ----- ----- ----- -----
    // Creator

    [MenuItem("CONTEXT/ActionState/New Transition")]
    static void CreateTransition() {
        var fromState = Selection.activeObject as ActionState;

        if (fromState == null) {
            Debug.LogWarning("No ActionState object selected");
        }

        CreateTransition(fromState, null);
    }

    static ActionTransition CreateTransition(ActionState from, ActionState to) {
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
        }
        else {
            // Entryからの遷移
            AssetDatabase.AddObjectToAsset(transition, to.stateMachine);
            to.stateMachine.entryTransitions.Add(transition);
            transition.stateMachine = to.stateMachine;
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(to.stateMachine));
        }

        return transition;
    }

    static void CreateKeyFrame(ActionState state) {
        var keyframe = ScriptableObject.CreateInstance<ActionKeyFrame>();
        keyframe.name = "keyframe";

        AssetDatabase.AddObjectToAsset(keyframe, state);
        state.keyframes.Add(keyframe);
        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(state.stateMachine));
    }

    // ----- ----- ----- ----- ----- -----
    // State Machine Events
    // Enter
    // Exit

    // Enter event of the state
    public void OnEnter() {
        timeFromEnter = 0.0f;
        foreach (var keyframe in keyframes) {
            keyframe.GenerateSubMovement(stateMachine.body);
        }
        Debug.Log("Enter state:" + name + " at time:" + Time.time);
    }

    // Exit event of the state
    public void OnExit() {

    }


    // ----- ----- ----- ----- ----- -----
    // Editor

    public void DrawStateNode(int id) {
        //GUI.DragWindow();
        int nTransitions = transitions.Count;
        for(int i = 0; i < nTransitions; i++) {
            Rect rect = new Rect(new Vector2(0, i * 20 + 15), new Vector2(stateNodeRect.width, 20));
            transitions[i].priority = i;
            GUI.Box(rect, transitions[i].name);
        }
        stateNodeRect.height = Mathf.Max(20 + 20 * nTransitions, 50);
    }

    public void Drag(Vector2 delta) {
        stateNodeRect.position += delta;
    }

    public void Draw(int id) {
        //GUI.Box(stateNodeRect, name, currentStyle);
        GUI.Window(id, stateNodeRect, DrawStateNode, name);
    }

    public bool ProcessEvents() {
        Event e = Event.current;
        switch (e.type) {
            case EventType.MouseDown:
                if(e.button == 0) {
                    if (stateNodeRect.Contains(e.mousePosition)) {
                        isDragged = true;
                        isSelected = true;
                        Selection.activeObject = this;
                        currentStyle = selectedStyle;
                        GUI.changed = true;
                    } else {
                        GUI.changed = true;
                        isSelected = false;
                        currentStyle = defaultStyle;
                    }
                }
                if(e.button == 1) {
                    if (stateNodeRect.Contains(e.mousePosition)) {
                        OnContextMenu(e.mousePosition);
                    }
                }
                break;

            case EventType.MouseUp:
                isDragged = false;
                break;

            case EventType.MouseDrag:
                if(e.button == 0 && isDragged) {
                    Drag(e.delta);
                    e.Use();
                    return true;
                }
                break;
        }

        return false;
    }

    private void OnContextMenu(Vector2 mousePosition) {
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
        genericMenu.AddItem(new GUIContent("Add KeyFrame"), false, () => OnClickAddKeyFrame());
        int nKeyframes = keyframes.Count;
        for (int i = 0; i < nKeyframes; i++) {
            ActionKeyFrame keyframe = keyframes[i]; // 
            genericMenu.AddItem(new GUIContent("Remove KeyFrame/" + keyframe.name), false, () => OnRemoveKeyFrame(keyframe));
        }
        genericMenu.AddItem(new GUIContent("Remove State"), false, () => OnRemoveState());
        genericMenu.ShowAsContext();
    }

    private void OnClickAddTransition(ActionState from, ActionState to) {
        CreateTransition(from, to);
    }

    private void OnClickAddKeyFrame() {
        CreateKeyFrame(this);
    }

    private void OnRemoveState() {
        // ステートマシンの関係する遷移を全部消す
        // キーフレームは
    }

    private void OnRemoveTransition(ActionTransition transition) {
        transitions.Remove(transition);
        Object.DestroyImmediate(transition, true);
        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(this.stateMachine));
    }

    private void OnRemoveKeyFrame(ActionKeyFrame keyframe) {
        keyframes.Remove(keyframe);
        Object.DestroyImmediate(keyframe, true);
        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(this.stateMachine));
    }

    public void SetDefaultNodeStyle(GUIStyle style) {
        defaultStyle = style;
    }
    public void SetSelectedNodeStyle(GUIStyle style) {
        selectedStyle = style;
    }
}

#endif