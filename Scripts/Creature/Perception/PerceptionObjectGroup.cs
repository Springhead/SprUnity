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
    [CustomEditor(typeof(PerceptionObjectGroup))]
    public class PerceptionObjectGroupEditor : Editor {
        private IEnumerable<Type> partsTypes;
        private List<string> partsTypeStrings;
        // perceptionObjectGroup.partsListが外部のメンバでboolを持ったクラスにすべきでない
        // partsの型情報は保存されないのでstringを使用
        private Dictionary<string, bool> showPartsTypeStrings;
        private int partsTypeIndex;
        private GameObject aaa;
        public override void OnInspectorGUI() {
            PerceptionObjectGroup perceptionObjectGroup = (PerceptionObjectGroup)target;
            foreach (var parts in perceptionObjectGroup.partsList) {
                EditorGUILayout.LabelField(parts.GetType().ToString());
                EditorGUI.indentLevel++;
                foreach (var field in target.GetType().GetFields()) {
                    if (field.FieldType == typeof(PerceptionObject)) {
                        EditorGUILayout.LabelField(field.ToString());
                    }
                }
                EditorGUI.indentLevel--;
            }
            if (GUILayout.Button("Test")) {
                perceptionObjectGroup.Test();
            }
            //var iterator = serializedObject.GetIterator();
            //while (iterator.NextVisible(true)) {
            //    EditorGUILayout.PropertyField(iterator);
            //}
            /*
            var partsSerializedObject = serializedObject.FindProperty("parts"); //PartNamePair
            for (int i = 0; i < partsSerializedObject.arraySize; i++) {
                EditorGUILayout.BeginHorizontal();
                var partNamePairSerializeObject = partsSerializedObject.GetArrayElementAtIndex(i);
                var PartSerializeObject = partNamePairSerializeObject.FindPropertyRelative("Part"); //List<PerceptionObject>
                EditorGUILayout.LabelField(PartSerializeObject.propertyType.ToString());
                for (int j = 0; j < PartSerializeObject.arraySize; j++) {
                    var perceptionObjectSerializedObject = PartSerializeObject.GetArrayElementAtIndex(j);
                    EditorGUILayout.LabelField(perceptionObjectSerializedObject.
                        FindPropertyRelative("confidence").floatValue.ToString());
                }
                EditorGUILayout.EndHorizontal();
                //if (GUILayout.Button("Create Part")) {
                //    partsSerializedObject.InsertArrayElementAtIndex(partsSerializedObject.arraySize);
                //}
            }
            if (GUILayout.Button("Create Parts")) {
                partsSerializedObject.InsertArrayElementAtIndex(partsSerializedObject.arraySize);
            }
            //parts.objectReferenceValue;
            //foreach(var pair in perceptionObjectGroup.GetParts)
            //EditorGUILayout.Popup();
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.LabelField(partsSerializedObject.arraySize.ToString());
            //var parts = (PerceptionObjectGroup.PartNamePair)partsSerializedObject.GetArrayElementAtIndex(1);
            */
            base.OnInspectorGUI();

            //game = new List<GameObject>();
            //game = (GameObject)EditorGUILayout.ObjectField(game, typeof(GameObject), true);
        }

        public void OnEnable() {
            PerceptionObjectGroup perceptionObjectGroup = (PerceptionObjectGroup)target;
            partsTypes = Assembly.GetAssembly(typeof(Parts)).GetTypes().Where(t => {
                return t.IsSubclassOf(typeof(Parts)) && !t.IsAbstract;
            });
            partsTypeStrings = new List<string>();
            foreach (var skillType in partsTypes) {
                partsTypeStrings.Add(skillType.Name);
            }
            //var iterator = serializedObject.GetIterator();
            //while (iterator.NextVisible(true)) {
            //    Debug.Log(iterator.propertyPath);
            //}
            showPartsTypeStrings = new Dictionary<string, bool>();

        }
    }
#endif
    [Serializable]
    public class PerceptionObject {
        // ここの構造どうしようか..PosRotConfを作るか？
        public List<PosRot> posrots = new List<PosRot>();
        public float confidence;
        public GameObject gameObject;
        public Vector3 Position(float time = 0) {
            return gameObject.transform.position;
        }
        public Quaternion Rotation(float time = 0) {
            return gameObject.transform.rotation;
        }
        public PosRot PosRot(float time = 0) {
            return posrots[0];
        }
        // ここで時間の更新する？どうする？VirtualSensorがやる？
        public void UpdatePerception() {
        }
    }

    public class PerceptionObjectGroup : MonoBehaviour {
        // Attributes
        public class Attribute {
            public virtual void StartPerc(PerceptionObjectGroup perceptionObjectGroup) { }
            public virtual void UpdatePerc(PerceptionObjectGroup perceptionObjectGroup) { }
            public virtual void OnDrawGizmos(PerceptionObjectGroup perceptionObjectGroup) { }
        }

        private Dictionary<Type, Attribute> attributes = new Dictionary<Type, Attribute>();
        public Type GetAttribute<Type>() where Type : Attribute, new() {
            if (attributes.ContainsKey(typeof(Type))) {
                return (attributes[typeof(Type)] as Type);
            } else {
                Type newObj = new Type();
                attributes[typeof(Type)] = newObj;
                attributes[typeof(Type)].StartPerc(this);
                return newObj;
            }
        }

        // <!!> 違和感のある実装PartNamePair.PartがList<PerceptionObject>であるためにTypeにキャスとしているのが違和感がある
        // ここをprivateにしているとEditor側でparts.part = new CupPart()観たいのができない←SerializedProperty.objectReferenceValueを使えばできるかも
        // Dictionaryだとinspectorに表示が確実にできないpublicにまずしてはならない
        // inspectorに表示が絶対にあった穂がようい
        [NonSerialized]
        public List<Parts> partsList = new List<Parts>();
        public Type GetParts<Type>() where Type : Parts, new() {
            foreach (var part in partsList) {
                if (part.GetType() == typeof(Type)) {
                    return (part as Type);
                }
            }
            // ここで作成したところでperceptionObjectとGameObjectの対応関係を決めれない
            return null;
        }

        // <Type>は引数を制限するため
        public void SetParts<Type>(Type parts) where Type : Parts, new() {
            foreach (var part in partsList) {
                if (part.GetType() == typeof(Type)) {
                    return;
                }
            }
            partsList.Add(parts);
        }
        public void UpdatePerception() {
            foreach (var attr in attributes) {
                attr.Value.UpdatePerc(this);
            }
        }
        void OnDrawGizmos() {
            foreach (var kv in attributes) {
                kv.Value.OnDrawGizmos(this);
            }
            //foreach (var container in containers) {
            //    container.Value.OnDrawGizmos();
            //}
        }

        public void Test() {
            foreach(var parts in partsList) {
                foreach(var part in parts) {
                    if (part.gameObject != null) {
                        Debug.Log(part.gameObject.name);
                    }
                }
            }
        }
        //// Testように
        //List<Parts> test;
        //public void Start() {
        //    PersonParts ppc = new PersonParts();

        //    test = new List<Parts>();
        //    test.Add(new CupParts());
        //    foreach (var tes in test) {
        //        Debug.Log(tes.GetType());
        //    }
        //    //foreach (var pp in ppc) {
        //    //    Debug.Log("posrot = " + pp.PosRot().position);
        //    //}
        //}
    }
}
