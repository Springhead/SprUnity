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
            foreach (var parts in mentalGroup.mentalPartsList) {
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
            foreach (var attribute in mentalGroup.mentalAttributeList) {
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

    [DefaultExecutionOrder(2)]
    public class MentalGroup : MentalExistence {
        private MentalScene mentalScene;
        private List<MentalAttribute> mentalAttributeListOnPlaying = new List<MentalAttribute>();
        private List<MentalParts> mentalPartsListOnPlaying = new List<MentalParts>();
        public List<MentalAttribute> mentalAttributeList {
            get {
                if (Application.isPlaying) {
                    return mentalAttributeListOnPlaying;
                } else {
                    var tempMentalAttributeList = GetComponentsInChildren<MentalAttribute>();
                    List<MentalAttribute> newMentalAttributeList = new List<MentalAttribute>();
                    foreach (var mentalAttribute in tempMentalAttributeList) {
                        if (mentalAttribute?.GetComponentInParent<MentalGroup>() == this) {
                            newMentalAttributeList.Add(mentalAttribute);
                        }
                    }
                    return newMentalAttributeList;
                }
            }
        }
        public List<MentalParts> mentalPartsList {
            get {
                if (Application.isPlaying) {
                    return mentalPartsListOnPlaying;
                } else {
                    var tempMentalPartsList = GetComponentsInChildren<MentalParts>();
                    List<MentalParts> newMentalPartsList = new List<MentalParts>();
                    foreach (var mentalParts in tempMentalPartsList) {
                        if (mentalParts?.GetComponentInParent<MentalGroup>() == this) {
                            newMentalPartsList.Add(mentalParts);
                        }
                    }
                    return newMentalPartsList;
                }
            }
        }
        public void AddMentalAttribute(MentalAttribute mentalAttribute) {
            mentalAttributeListOnPlaying.Add(mentalAttribute);
        }
        public void RemoveMentalAttribute(MentalAttribute mentalAttribute) {
            mentalAttributeListOnPlaying.Remove(mentalAttribute);
        }

        public void AddMentalParts(MentalParts mentalParts) {
            mentalPartsListOnPlaying.Add(mentalParts);
        }
        public void RemoveMentalParts(MentalParts mentalParts) {
            mentalPartsListOnPlaying.Remove(mentalParts);
        }

        void Start() {
            mentalScene = FindObjectOfType<MentalScene>();
            if (mentalScene == null) {
                return;
            }
            mentalScene.AddMentalGroup(this);
        }
        void OnDestroy() {
            if (mentalScene == null) {
                return;
            }
            mentalScene.RemoveMentalGroup(this);
        }
        // AttributeをStartでmentalAttributeListOnPlayingに追加すると二つ付いてしまうことがある
        public override Type GetAttribute<Type>() {
            foreach(var mentalAttribute in mentalAttributeList) {
                if(mentalAttribute.GetType() == typeof(Type)) {
                    return mentalAttribute as Type;
                }
            }
            var attribute = this.gameObject.AddComponent<Type>();
            attribute.mentalGroup = this;
            attribute.initialized = true;
            AddMentalAttribute(attribute);
            return attribute;
        }

        public Type GetParts<Type>() where Type : MentalParts, new() {
            foreach(var mentalParts in mentalPartsList) {
                if(mentalParts.GetType() == typeof(Type)) {
                    return mentalParts as Type;
                }
            }
            // ここで作成したところでMentalObjectとGameObjectの対応関係を決めれない
            return null;
        }
    }
}
