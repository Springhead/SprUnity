using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CreateAssetMenu(menuName = "Action/Create ActionState Instance")]
public class ActionState : ScriptableObject {
    // member values
    public List<ActionKeyFrame> keyframes;
    public List<ActionTransition> transitions;
    [HideInInspector]
    public float timeFromEnter;

    [MenuItem("CONTEXT/ActionState/New Transition")]
    static void CreateState() {
        /*var fromState = Selection.activeObject as ActionState;

        if (fromState == null) {
            Debug.LogWarning("No ActionState object selected");
        }

        var transition = ScriptableObject.CreateInstance<ActionTransition>();

        AssetDatabase.AddObjectToAsset(transition, fromState);

        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(fromState));*/
    }

    // ----- ----- ----- ----- ----- -----
    // State events

    // Enter event of the state
    public void OnEnter(InteraWare.Body body) {
        timeFromEnter = 0.0f;
        foreach (var keyframe in keyframes) {
            keyframe.generateSubMovement(body);
        }
        Debug.Log("Enter state:" + name + " at time:" + Time.time);
    }

    // Exit event of the state
    public void OnExit() {

    }
}

#endif