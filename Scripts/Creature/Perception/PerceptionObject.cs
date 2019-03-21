using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(PerceptionObject))]
public class PerceptionObjectEditor : Editor {
    public bool showAttributes = true;

    public override void OnInspectorGUI() {
        PerceptionObject perceptionObject = (PerceptionObject)target;

        DrawDefaultInspector();

        showAttributes = EditorGUILayout.Foldout(showAttributes, "Attributes");
        if (showAttributes) {
            foreach (var attribute in perceptionObject.attributes) {
                EditorGUILayout.BeginHorizontal();

                attribute.name = EditorGUILayout.TextField(attribute.name);
                attribute.type = (PerceptionAttribute.Type)(EditorGUILayout.EnumPopup(attribute.type));

                EditorGUILayout.EndHorizontal();
            }

            // ----- ----- ----- ----- -----

            if (GUILayout.Button("Add Attribute")) {
                perceptionObject.attributes.Add(new PerceptionAttribute());
            }
        }
    }
}
#endif

public class PerceptionObject : MonoBehaviour {

    public string name = "";

    public List<PerceptionObject> children = new List<PerceptionObject>();

    [HideInInspector]
    public List<PerceptionAttribute> attributes = new List<PerceptionAttribute>();

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

    private void Start() {
        PerceptionScene.GetInstance().objects.Add(this);
    }

    private void FixedUpdate() {
    }

    private void OnDrawGizmos() {
    }

    private void OnDestroy() {
        PerceptionScene.GetInstance().objects.Remove(this);
    }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

    public PerceptionAttribute this[string attributeName] {
        get { return attributes.Find(item => item.name == attributeName); }
    }

}
