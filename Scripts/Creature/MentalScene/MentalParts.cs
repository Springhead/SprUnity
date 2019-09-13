using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VGent{
#if UNITY_EDITOR
    [CustomEditor(typeof(MentalParts), true)]
    public class MentalPartsEditor : Editor {
        public override void OnInspectorGUI() {
            MentalParts mentalParts = (MentalParts)target;
            foreach (var field in target.GetType().GetFields()) {
                if (field.FieldType == typeof(MentalObject)) {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(field.Name);
                    EditorGUI.BeginChangeCheck();
                    var mentalObject =
                        (MentalObject)EditorGUILayout.ObjectField(((MentalObject)field.GetValue(mentalParts)), typeof(MentalObject), true);
                    if (EditorGUI.EndChangeCheck()) {
                        Undo.RecordObject(mentalParts, "PerceptionObject's gameObject set");
                        field.SetValue(mentalParts,mentalObject);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            if (GUILayout.Button("Set Same Name GameObjects")) {
                Undo.RecordObject(mentalParts, "MentalObject's gameObject set");
                mentalParts.SetSameNameGameObject();
            }
            if (GUILayout.Button("Remove all MentalObjects")) {
                Undo.RecordObject(mentalParts, "MentalnObject's gameObject set");
                mentalParts.DeleteMental();
            }
        }
    }
#endif
    public class MentalParts : MonoBehaviour, IEnumerable<MentalObject> {
        public MentalGroup mentalGroup;
        // 他のところでStartを定義されるとそちらが優先される
        void Start() {
            mentalGroup = GetComponentInParent<MentalGroup>();
            if(mentalGroup == null) {
                return;
            }
            mentalGroup.AddMentalParts(this);
        }
        void OnDestroy() {
            if (mentalGroup== null) {
                return;
            }
            mentalGroup.RemoveMentalParts(this);
        }

        public IEnumerator<MentalObject> GetEnumerator() {
            foreach (var field in this.GetType().GetFields()) {
                if (field.FieldType == typeof(MentalObject)) {
                    yield return (MentalObject)field.GetValue(this);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        public void SetSameNameGameObject() {
#if UNITY_EDITOR
            var children = this.GetComponentsInChildren<Transform>();
            foreach (var field in this.GetType().GetFields()) {
                if (field.FieldType == typeof(MentalObject)) {
                    foreach(var child in children) {
                        if(child.name == field.Name) {
                            var mentalObject = child.GetComponent<MentalObject>();
                            if( mentalObject == null) {
                                Undo.AddComponent(child.gameObject, typeof(MentalObject));
                                mentalObject = child.gameObject.GetComponent<MentalObject>();
                            }
                            field.SetValue(this,mentalObject);
                        }
                    }
                }
            }
#endif
        }
        public void DeleteMental() {
            var children = this.GetComponentsInChildren<MentalObject>();
            foreach (var field in this.GetType().GetFields()) {
                if (field.FieldType == typeof(MentalObject)) {
                    field.SetValue(this,null);
                }
            }
            foreach(var child in children) {
#if UNITY_EDITOR
                Undo.DestroyObjectImmediate(child);
#endif
            }
        }
    }
}
