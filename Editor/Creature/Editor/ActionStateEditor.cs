using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace VGent{
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
            ActionState state = (ActionState)target;
            //base.OnInspectorGUI();
            var serializedObject = new SerializedObject(target);
            var nodeProperty = serializedObject.FindProperty("nodes");
            serializedObject.Update();
            EditorGUILayout.PropertyField(nodeProperty, true);
            state.durationMode = (ActionState.DurationMode)EditorGUILayout.EnumPopup("Duration Mode", state.durationMode);
            switch (state.durationMode) {
                case ActionState.DurationMode.Static:
                    state.duration = EditorGUILayout.FloatField(new GUIContent("Duration", "static duration value"), state.duration);
                    break;
                case ActionState.DurationMode.VelocityBase:
                    EditorGUILayout.LabelField("sorry, this mode isn't implemented yet.");
                    break;
                case ActionState.DurationMode.Fitts:
                    state.fittsA = EditorGUILayout.FloatField("Fitts A", state.fittsA);
                    state.fittsB = EditorGUILayout.FloatField("Fitts B", state.fittsB);
                    state.accuracy = EditorGUILayout.FloatField("Acuracy", state.accuracy);
                    break;
            }
            state.spring = EditorGUILayout.FloatField(new GUIContent("Spring", "Spring activation rate in end of submovement"), state.spring);
            state.damper = EditorGUILayout.FloatField(new GUIContent("Damper", "Damper activation rate in end of submovement"), state.damper);
            state.useFace = EditorGUILayout.Toggle("Use Face", state.useFace);
            if (state.useFace) {
                state.blend = EditorGUILayout.TextField("Blend", state.blend);
                state.blendv = EditorGUILayout.FloatField("Blendv", state.blendv);
                state.time = EditorGUILayout.FloatField("Time", state.time);
                state.interval = EditorGUILayout.FloatField("Interval", state.interval);
            }
            if (EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            }
            if (textChangeComp) {
                string mainPath = AssetDatabase.GetAssetPath(this);
                //EditorUtility.SetDirty(AssetDatabase.LoadMainAssetAtPath(mainPath));
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath((ActionState)target));
            }
        }
    }

}