using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SprUnity {
#if UNITY_EDITOR
    [CustomEditor(typeof(MentalGroup))]
    public class MentalGroupEditor : Editor {
        public override void OnInspectorGUI() {
            MentalGroup mentalGroup = (MentalGroup)target;
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
            base.OnInspectorGUI();
        }
    }
#endif

    public class MentalGroup : MentalExistance {

        public override Type GetAttribute<Type>() {
            var mentalAttribute = GetComponentInChildren<Type>();
            if (mentalAttribute?.GetComponentInParent<MentalGroup>() == this) {
                return mentalAttribute;
            }
            return this.gameObject.AddComponent<Type>();
        }
        public List<MentalAttribute> GetAllAttribute() {
            var mentalAttributeList = GetComponentsInChildren<MentalAttribute>();
            List<MentalAttribute> newMentalAttributeList = new List<MentalAttribute>();
            foreach (var mentalParts in mentalAttributeList) {
                if (mentalParts.GetComponentInParent<MentalGroup>() == this) {
                    newMentalAttributeList.Add(mentalParts);
                }
            }
            return newMentalAttributeList;
        }

        public Type GetParts<Type>() where Type : MentalParts, new() {
            var mentalParts = GetComponentInChildren<Type>();
            if (mentalParts.GetComponentInParent<MentalGroup>() == this) {
                return mentalParts;
            }
            // ここで作成したところでMentalObjectとGameObjectの対応関係を決めれない
            return null;
        }

        public List<MentalParts> GetAllParts() {
            var mentalPartsList = GetComponentsInChildren<MentalParts>();
            List<MentalParts> newMentalPartsList = new List<MentalParts>();
            foreach (var mentalParts in mentalPartsList) {
                if (mentalParts.GetComponentInParent<MentalGroup>() == this) {
                    newMentalPartsList.Add(mentalParts);
                }
            }
            return newMentalPartsList;
        }
    }
}
