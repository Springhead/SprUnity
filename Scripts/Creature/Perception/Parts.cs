using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SprUnity {
#if UNITY_EDITOR
    [CustomEditor(typeof(Parts), true)]
    public class PartsEditor : Editor {
        public override void OnInspectorGUI() {
            Parts parts = (Parts)target;
            foreach (var field in target.GetType().GetFields()) {
                if (field.FieldType == typeof(PerceptionObject)) {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(field.Name);
                    EditorGUI.BeginChangeCheck();
                    var gameObject =
                        (GameObject)EditorGUILayout.ObjectField(((PerceptionObject)field.GetValue(parts)).gameObject, typeof(GameObject), true);
                    if (EditorGUI.EndChangeCheck()) {
                        Undo.RecordObject(parts, "PerceptionObject's gameObject set");
                        ((PerceptionObject)field.GetValue(parts)).gameObject = gameObject;
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            if (GUILayout.Button("Set Same Name GameObjects")) {
                Undo.RecordObject(parts, "PerceptionObject's gameObject set");
                parts.SetSameNameGameObject();
            }
        }
    }
#endif
    public class Parts : MonoBehaviour, IEnumerable<PerceptionObject> {

        // PerceptionObjectGroupが初期化されてからでないといけない
        public void Start() {
            var perceptionObjectGroup = GetComponentInParent<PerceptionObjectGroup>();
            if (perceptionObjectGroup == null) {
                Debug.LogError("There is no perceptionObjectGroup in the parents");
            } else {
                // 省略できるらしい
                perceptionObjectGroup.SetParts(this);
            }
        }

        public IEnumerator<PerceptionObject> GetEnumerator() {
            foreach (var field in this.GetType().GetFields()) {
                if (field.FieldType == typeof(PerceptionObject)) {
                    yield return (PerceptionObject)field.GetValue(this);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        public void SetSameNameGameObject() {
            var children = this.GetComponentsInChildren<Transform>();
            foreach (var field in this.GetType().GetFields()) {
                if (field.FieldType == typeof(PerceptionObject)) {
                    foreach(var child in children) {
                        if(child.name == field.Name) {
                            ((PerceptionObject)field.GetValue(this)).gameObject = child.gameObject;
                        }
                    }
                }
            }
        }
    }
}
