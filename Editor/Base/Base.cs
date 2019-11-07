using UnityEngine;
using System.Collections;
using UnityEditor;
using SprCs;
using System.Reflection;
using System;

[CustomPropertyDrawer(typeof(Vec3fStruct))]
public class Vec3fDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        float w = (position.width - 10) / 3;
        Rect rectX = new Rect(position.x + (w + 5) * 0, position.y, w, position.height);
        Rect rectY = new Rect(position.x + (w + 5) * 1, position.y, w, position.height);
        Rect rectZ = new Rect(position.x + (w + 5) * 2, position.y, w, position.height);

        EditorGUI.PropertyField(rectX, property.FindPropertyRelative("x"), GUIContent.none);
        EditorGUI.PropertyField(rectY, property.FindPropertyRelative("y"), GUIContent.none);
        EditorGUI.PropertyField(rectZ, property.FindPropertyRelative("z"), GUIContent.none);

        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }
}


[CustomPropertyDrawer(typeof(Vec3dStruct))]
public class Vec3dDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty (position, label, property);
		position = EditorGUI.PrefixLabel (position, GUIUtility.GetControlID (FocusType.Passive), label);

		int indent = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;

		float w = (position.width-10)/3;
		Rect rectX = new Rect (position.x + (w+5)*0, position.y, w, position.height);
		Rect rectY = new Rect (position.x + (w+5)*1, position.y, w, position.height);
		Rect rectZ = new Rect (position.x + (w+5)*2, position.y, w, position.height);

		EditorGUI.PropertyField (rectX, property.FindPropertyRelative ("x"), GUIContent.none);
		EditorGUI.PropertyField (rectY, property.FindPropertyRelative ("y"), GUIContent.none);
		EditorGUI.PropertyField (rectZ, property.FindPropertyRelative ("z"), GUIContent.none);

		EditorGUI.indentLevel = indent;

		EditorGUI.EndProperty ();
	}
}

[CustomPropertyDrawer(typeof(Matrix3dStruct))]
public class Matrix3dDrawer : PropertyDrawer {
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		EditorGUI.BeginProperty (position, label, property);
		position = EditorGUI.PrefixLabel (position, GUIUtility.GetControlID (FocusType.Passive), label);
		
		int indent = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;

		float w = (position.width-10)/3;
		float h = (position.height-8)/3;
		Rect rectXX = new Rect (position.x + (w+5)*0, position.y + (h+2)*0, w, h);
		Rect rectXY = new Rect (position.x + (w+5)*1, position.y + (h+2)*0, w, h);
		Rect rectXZ = new Rect (position.x + (w+5)*2, position.y + (h+2)*0, w, h);

		Rect rectYX = new Rect (position.x + (w+5)*0, position.y + (h+2)*1, w, h);
		Rect rectYY = new Rect (position.x + (w+5)*1, position.y + (h+2)*1, w, h);
		Rect rectYZ = new Rect (position.x + (w+5)*2, position.y + (h+2)*1, w, h);

		Rect rectZX = new Rect (position.x + (w+5)*0, position.y + (h+2)*2, w, h);
		Rect rectZY = new Rect (position.x + (w+5)*1, position.y + (h+2)*2, w, h);
		Rect rectZZ = new Rect (position.x + (w+5)*2, position.y + (h+2)*2, w, h);

		EditorGUI.PropertyField (rectXX, property.FindPropertyRelative ("xx"), GUIContent.none);
		EditorGUI.PropertyField (rectXY, property.FindPropertyRelative ("xy"), GUIContent.none);
		EditorGUI.PropertyField (rectXZ, property.FindPropertyRelative ("xz"), GUIContent.none);
		
		EditorGUI.PropertyField (rectYX, property.FindPropertyRelative ("yx"), GUIContent.none);
		EditorGUI.PropertyField (rectYY, property.FindPropertyRelative ("yy"), GUIContent.none);
		EditorGUI.PropertyField (rectYZ, property.FindPropertyRelative ("yz"), GUIContent.none);

		EditorGUI.PropertyField (rectZX, property.FindPropertyRelative ("zx"), GUIContent.none);
		EditorGUI.PropertyField (rectZY, property.FindPropertyRelative ("zy"), GUIContent.none);
		EditorGUI.PropertyField (rectZZ, property.FindPropertyRelative ("zz"), GUIContent.none);

		EditorGUI.indentLevel = indent;
		
		EditorGUI.EndProperty ();
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
		return base.GetPropertyHeight (property, label) * 3 + 8;
	}
}


// Source : https://gist.github.com/ProGM/226204b2a7f99998d84d755ffa1fb39a
public class Vector3WithPositionHandle : PropertyAttribute { }
public class QuaternionWithRotationHandle : PropertyAttribute {
    public Vector3 handlePos = new Vector3();
    public QuaternionWithRotationHandle(Vector3 pos = default(Vector3)) {
        this.handlePos = pos;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(MonoBehaviour), true)]
public class MemberHandleDrawer : Editor {

    readonly GUIStyle style = new GUIStyle();

    void OnEnable() {
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.black;
    }

    public void OnSceneGUI() {
        var property = serializedObject.GetIterator();
        while (property.Next(true)) {
            if (property.propertyType == SerializedPropertyType.Vector3) {
                var field = serializedObject.targetObject.GetType().GetField(property.name);
                if (field == null) {
                    continue;
                }
                var vector3WithHandle = field.GetCustomAttributes(typeof(Vector3WithPositionHandle), false);
                if (vector3WithHandle.Length > 0) {
                    Handles.Label(property.vector3Value, property.name);
                    property.vector3Value = Handles.PositionHandle(property.vector3Value, Quaternion.identity);
                    serializedObject.ApplyModifiedProperties();
                }
            }
            if (property.propertyType == SerializedPropertyType.Quaternion) {
                var field = serializedObject.targetObject.GetType().GetField(property.name);
                if (field == null) {
                    continue;
                }
                var quaternionWithHandle = field.GetCustomAttributes(typeof(QuaternionWithRotationHandle), false);
                if (quaternionWithHandle.Length > 0) {
                    var handlePos = field.GetCustomAttribute<QuaternionWithRotationHandle>().handlePos;
                    Handles.Label(handlePos, property.name);
                    property.quaternionValue = Handles.RotationHandle(property.quaternionValue, handlePos);
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }
    }
}
#endif