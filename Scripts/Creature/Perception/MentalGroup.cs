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
        private IEnumerable<Type> partsTypes;
        private List<string> partsTypeStrings;
        // perceptionObjectGroup.partsListが外部のメンバでboolを持ったクラスにすべきでない
        // partsの型情報は保存されないのでstringを使用
        private Dictionary<string, bool> showPartsTypeStrings;
        private int partsTypeIndex;
        private GameObject aaa;
        public override void OnInspectorGUI() {
            MentalGroup MentalGroup = (MentalGroup)target;
            foreach (var parts in MentalGroup.partsList) {
                EditorGUILayout.LabelField(parts.GetType().ToString());
                EditorGUI.indentLevel++;
                foreach (var field in target.GetType().GetFields()) {
                    if (field.FieldType == typeof(MentalObject)) {
                        EditorGUILayout.LabelField(field.ToString());
                    }
                }
                EditorGUI.indentLevel--;
            }
            if (GUILayout.Button("Test")) {
                MentalGroup.Test();
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
            MentalGroup perceptionObjectGroup = (MentalGroup)target;
            partsTypes = Assembly.GetAssembly(typeof(MentalParts)).GetTypes().Where(t => {
                return t.IsSubclassOf(typeof(MentalParts)) && !t.IsAbstract;
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

    public class MentalGroup : MentalExistance {

        //private List<MentalAttribute> attributes = new List<MentalAttribute>();
        //実態はGameObjectに格納されている
        public override Type GetAttribute<Type>() {
            //if (attributes.ContainsKey(typeof(Type))) {
            //    return (attributes[typeof(Type)] as Type);
            //} else {
            //    Type newObj = new Type();
            //    attributes[typeof(Type)] = newObj;
            //    return newObj;
            //}
            return null;
        }

        // Dictionaryだとinspectorに表示が確実にできないpublicにまずしてはならない
        // inspectorに表示が絶対にあった穂がようい
        [NonSerialized]
        public List<MentalParts> partsList = new List<MentalParts>();
        public Type GetParts<Type>() where Type : MentalParts, new() {
            foreach (var parts in partsList) {
                if (parts.GetType() == typeof(Type)) {
                    return (parts as Type);
                }
            }
            // ここで作成したところでperceptionObjectとGameObjectの対応関係を決めれない
            return null;
        }

        public class test {

        }
        // <Type>は引数を制限するため
        public void SetParts<Type>(Type parts) where Type : MentalParts, new() {
            foreach (var part in partsList) {
                if (part.GetType() == typeof(Type)) {
                    return;
                }
            }

            partsList.Add(parts);
        }

        public void Test() {
            foreach(var parts in partsList) {
                foreach(var part in parts) {
                    if (part != null) {
                        if (part.gameObject != null) {
                            Debug.Log(part.gameObject.name);
                        }
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
