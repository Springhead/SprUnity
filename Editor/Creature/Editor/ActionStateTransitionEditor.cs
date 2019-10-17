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
                case ActionStateTransition.IntervalMode.StaticTimeFromPreviousKeyPoseStart:
                case ActionStateTransition.IntervalMode.StaticTimeFromPreviousKeyPoseEnd:
                    transition.time = EditorGUILayout.FloatField("Time", transition.time);
                    break;
                case ActionStateTransition.IntervalMode.RelativeTimeFromPreviousKeyPoseStart:
                    transition.timeCoefficient = EditorGUILayout.FloatField("Time Coefficient", transition.timeCoefficient);
                    break;
                case ActionStateTransition.IntervalMode.ProportionalToFloatParam:
                    transition.floatParam = EditorGUILayout.TextField("Float Param", transition.floatParam);
                    transition.minInterval = EditorGUILayout.FloatField("Min Interval", transition.minInterval);
                    transition.maxInterval = EditorGUILayout.FloatField("Max Interval", transition.maxInterval);
                    break;
                case ActionStateTransition.IntervalMode.Random:
                    transition.minInterval = EditorGUILayout.FloatField("Min Interval", transition.minInterval);
                    transition.maxInterval = EditorGUILayout.FloatField("Max Interval", transition.maxInterval);
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