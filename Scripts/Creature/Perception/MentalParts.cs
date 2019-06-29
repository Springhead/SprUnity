using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SprUnity {
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
                Undo.RecordObject(mentalParts, "PerceptionObject's gameObject set");
                mentalParts.SetSameNameGameObject();
            }
        }
    }
#endif
    public class MentalParts : MonoBehaviour, IEnumerable<MentalObject> {

        // PerceptionObjectGroupが初期化されてからでないといけない
        public void Start() {
            var mentalGroup = GetComponentInParent<MentalGroup>();
            if (mentalGroup == null) {
                Debug.LogError("There is no perceptionObjectGroup in the parents");
            } else {
                // 省略できるらしい
                mentalGroup.SetParts(this);
            }
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
            var children = this.GetComponentsInChildren<MentalObject>();
            foreach (var field in this.GetType().GetFields()) {
                if (field.FieldType == typeof(MentalObject)) {
                    foreach(var child in children) {
                        if(child.name == field.Name) {
                            field.SetValue(this,child);
                        }
                    }
                }
            }
        }
    }
}
