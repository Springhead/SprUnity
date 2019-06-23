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
            for (int i = 0; i < target.GetType().GetProperties().Length; i++) {
                var property = target.GetType().GetProperties()[i];
                if (property.PropertyType == typeof(PerceptionObject)) {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(property.Name);
                    EditorGUI.BeginChangeCheck();
                    var gameObject =
                        (GameObject)EditorGUILayout.ObjectField(parts.parts[i].gameObject, typeof(GameObject), true);
                    if (EditorGUI.EndChangeCheck()) {
                        Undo.RecordObject(parts, "PerceptionObject's gameObject set");
                        parts.parts[i].gameObject = gameObject;
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        public void OnEnable() {
            // ここでpartsのpartsの初期化や大きさ調整をする
            Parts parts = (Parts)target;
            int listSize = 0;
            foreach (var property in target.GetType().GetProperties()) {
                if (property.PropertyType == typeof(PerceptionObject)) {
                    //var isExist = false;
                    //foreach(var partsProperty in parts.properties) {
                    //    if(property.Name == partsProperty) {
                    //        isExist = true;
                    //    }
                    //}
                    
                    //if (!isExist) {
                    //    parts.properties.Add(property.Name);
                    //}
                    listSize++;
                }
            }
            var partsCount = parts.parts.Count;
            if (partsCount < listSize) {
                for (int i = 0; i < listSize - partsCount; i++) {
                    parts.parts.Add(new PerceptionObject());
                }
            } else if (partsCount > listSize) {
                parts.parts.RemoveRange(partsCount - listSize, partsCount - 1);
            }
        }
    }
#endif
    public class Parts : MonoBehaviour {
        // <!!> これprivateにしたいが実装が困難とりあえずpublicでやる
        public List<PerceptionObject> parts = new List<PerceptionObject>();
        //public List<string> properties = new List<string>();

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
        public PerceptionObject GetPerceptionObject(int i) {
            if (0 <= i && i < parts.Count) {
                return parts[i];
            } else {
                return null;
            }
        }

        // staticだと自分のクラスの型がわからないため,これ外部ではPartsにキャストされてしまうから使えない
        public int GetPartsCount() {
            int count = 0;
            foreach (var property in this.GetType().GetProperties()) {
                if (property.GetType() == typeof(PerceptionObject)) {
                    count++;
                }
            }
            return count;
        }
    }

}
