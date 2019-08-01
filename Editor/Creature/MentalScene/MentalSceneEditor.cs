using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SprUnity {
    [CustomEditor(typeof(MentalScene))]
    public partial class MentalSceneEditor : Editor {
        public override void OnInspectorGUI() {
            MentalScene mentalScene = (MentalScene)target;
            foreach (var mentalGroup in mentalScene.mentalGroups) {
                EditorGUILayout.LabelField(mentalGroup.name);
                EditorGUI.indentLevel++;
                foreach (var parts in mentalGroup.GetAllParts()) {
                    EditorGUILayout.LabelField(parts.GetType().ToString());
                    EditorGUI.indentLevel++;
                    foreach (var field in target.GetType().GetFields()) {
                        if (field.FieldType == typeof(MentalObject)) {
                            EditorGUILayout.LabelField(field.ToString());
                        }
                    }
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.LabelField("");
                foreach (var attribute in mentalGroup.GetAllAttribute()) {
                    EditorGUILayout.LabelField(attribute.GetType().ToString());
                    EditorGUI.indentLevel++;
                    foreach (var field in target.GetType().GetFields()) {
                        if (field.FieldType == typeof(MentalObject)) {
                            EditorGUILayout.LabelField(field.ToString());
                        }
                    }
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }
            base.OnInspectorGUI();
        }
        private void OnSceneGUI() {
            Handles.PositionHandle(new Vector3(1, 0, 0), Quaternion.identity);
        }
    }
}
