using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VGent{
    [CustomEditor(typeof(ActionStateTransition))]
    public class ActionStateTransitionEditor : Editor {

        ReorderableList conditionList;

        void OnEnable() {
            conditionList = new ReorderableList(serializedObject, serializedObject.FindProperty("conditions"));
        }

        public override void OnInspectorGUI() {
            bool textChangeComp = false;
            EditorGUI.BeginChangeCheck();
            Event e = Event.current;
            if (e.keyCode == KeyCode.Return && Input.eatKeyPressOnTextFieldFocus) {
                textChangeComp = true;
                Event.current.Use();
            }
            target.name = EditorGUILayout.TextField("Name", target.name);
            ActionStateTransition transition = (ActionStateTransition)target;
            //base.OnInspectorGUI();
            var serializedObject = new SerializedObject(target);
            var flagProperty = serializedObject.FindProperty("flags");
            serializedObject.Update();
            EditorGUILayout.PropertyField(flagProperty, true);
            transition.intervalMode = (ActionStateTransition.IntervalMode)EditorGUILayout.EnumPopup("Interval Mode", transition.intervalMode);
            switch (transition.intervalMode) {
                case ActionStateTransition.IntervalMode.StaticTimeFromPreviousSubMovementStart:
                case ActionStateTransition.IntervalMode.StaticTimeFromPreviousSubMovementEnd:
                    transition.time = EditorGUILayout.FloatField(new GUIContent("Time", "Static time interval value"), transition.time);
                    break;
                case ActionStateTransition.IntervalMode.RelativeTimeFromPreviousSubMovementStart:
                    transition.timeCoefficient = EditorGUILayout.FloatField(new GUIContent("Time Coefficient", "(For Relative mode) set interval time = last submovement duration * coefficient(this value)"), transition.timeCoefficient);
                    break;
                case ActionStateTransition.IntervalMode.ProportionalToFloatParam:
                    transition.floatParam = EditorGUILayout.TextField(new GUIContent("Float Param", "Float parameter to decide interval time"), transition.floatParam);
                    transition.minInterval = EditorGUILayout.FloatField(new GUIContent("Min Interval", "Min value of dynamically changing interval time"), transition.minInterval);
                    transition.maxInterval = EditorGUILayout.FloatField(new GUIContent("Max Interval", "Max value of dynamically changing interval time"), transition.maxInterval);
                    break;
                case ActionStateTransition.IntervalMode.Random:
                    transition.minInterval = EditorGUILayout.FloatField(new GUIContent("Min Interval", "Min value of dynamically changing interval time"), transition.minInterval);
                    transition.maxInterval = EditorGUILayout.FloatField(new GUIContent("Max Interval", "Max value of dynamically changing interval time"), transition.maxInterval);
                    break;
                case ActionStateTransition.IntervalMode.OuterTrigger:
                    GUILayout.Label("Sorry, this mode is not implemented");
                    break;

            }
            //conditionList.DoLayoutList();
            if (EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            }
            if (textChangeComp) {
                string mainPath = AssetDatabase.GetAssetPath(this);
                //EditorUtility.SetDirty(AssetDatabase.LoadMainAssetAtPath(mainPath));
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath((ActionStateTransition)target));
            }
        }
    }

}