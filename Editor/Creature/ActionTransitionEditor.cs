using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SprUnity {
    [CustomEditor(typeof(ActionTransition))]
    public class ActionTransitionEditor : Editor {

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
            base.OnInspectorGUI();
            ActionTransition transition = (ActionTransition)target;
            conditionList.DoLayoutList();
            if (EditorGUI.EndChangeCheck()) {
                EditorUtility.SetDirty(target);
            }
            if (textChangeComp) {
                string mainPath = AssetDatabase.GetAssetPath(this);
                //EditorUtility.SetDirty(AssetDatabase.LoadMainAssetAtPath(mainPath));
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath((ActionTransition)target));
            }
        }
    }

}