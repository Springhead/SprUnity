using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SprUnity {
#if UNITY_EDITOR
    [CustomEditor(typeof(Parts),true)]
    public class PartsEditor : Editor {
        private GameObject aaa;
        public override void OnInspectorGUI() {
            foreach (var property in target.GetType().GetProperties()) {
                if (property.PropertyType == typeof(PerceptionObject)) {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(property.Name);
                    EditorGUILayout.ObjectField(aaa, typeof(GameObject), true);
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
    }
#endif
    public class Parts : MonoBehaviour {
    public List<PerceptionObject> parts;
    public int partsSize; //これを毎回走らせる
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
