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
        public IEnumerable<Type> partsTypes;
        public List<string> partsTypeStrings;

        // perceptionObjectGroup.partsListが外部のメンバでboolを持ったクラスにすべきでない
        // partsの型情報は保存されないのでstringを使用
        private Dictionary<string, bool> showPartsTypeStrings;
        private int partsTypeIndex;
        private GameObject aaa;
        public override void OnInspectorGUI() {
            PerceptionObjectGroup perceptionObjectGroup = (PerceptionObjectGroup)target;

            foreach (var pair in perceptionObjectGroup.partsNamePairs) {
                showPartsTypeStrings[pair.name] =
                    EditorGUILayout.Foldout(showPartsTypeStrings[pair.name], pair.name);
                if (showPartsTypeStrings[pair.name]) {
                    EditorGUI.indentLevel++;
                    foreach (var partType in partsTypes) {
                        if (partType.Name == pair.name) {
                            foreach (var property in partType.GetProperties()) {
                                if (property.PropertyType == typeof(PerceptionObject)){
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.LabelField(property.Name);
                                    EditorGUILayout.ObjectField(aaa, typeof(GameObject), true);
                                    EditorGUILayout.EndHorizontal();
                                }
                            }
                        }
                    }
                    EditorGUI.indentLevel--;
                }
            }

            EditorGUILayout.BeginHorizontal();
            partsTypeIndex = EditorGUILayout.Popup(partsTypeIndex, partsTypeStrings.ToArray());
            if (GUILayout.Button("Create")) {
                foreach (var partsType in partsTypes) {
                    if (partsType.Name == partsTypeStrings[partsTypeIndex]) {
                        // partsTypeは同じ名前を二つ含まない
                        var isExist = false;
                        foreach (var pair in perceptionObjectGroup.partsNamePairs) {
                            if (pair.name == partsType.Name) {
                                isExist = true;
                            }
                        }
                        if (isExist) {
                            break;
                        }
                        Debug.Log("aaa");
                        Undo.RecordObject(perceptionObjectGroup, "Change PerceptionObjectGroup");
                        var newPartsNamePair = new PerceptionObjectGroup.PartsNamePair();
                        newPartsNamePair.parts = new PerceptionObjectGroup.Parts();
                        newPartsNamePair.name = partsType.Name;
                        perceptionObjectGroup.partsNamePairs.Add(newPartsNamePair);
                        showPartsTypeStrings.Add(newPartsNamePair.name, true);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

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
            partsTypes = Assembly.GetAssembly(typeof(PerceptionObjectGroup.Parts)).GetTypes().Where(t => {
                return t.IsSubclassOf(typeof(PerceptionObjectGroup.Parts)) && !t.IsAbstract;
            });
            partsTypeStrings = new List<string>();
            foreach (var skillType in partsTypes) {
                partsTypeStrings.Add(skillType.Name);
                Debug.Log(skillType.Name);
            }
            //var iterator = serializedObject.GetIterator();
            //while (iterator.NextVisible(true)) {
            //    Debug.Log(iterator.propertyPath);
            //}
            showPartsTypeStrings = new Dictionary<string, bool>();

            foreach (var pair in perceptionObjectGroup.partsNamePairs) {
                showPartsTypeStrings.Add(pair.name, true);
            }
            foreach(var pair in perceptionObjectGroup.partsNamePairs) {
                foreach(var partsType in partsTypes) {
                    if(pair.name == partsType.Name) {
                        int count = 0;
                        foreach (var property in partsType.GetProperties()) {
                            // propertyのTypeはGetTypeではなくPropertyType
                            if (property.PropertyType == typeof(PerceptionObject)) {
                                count++;
                            }
                        }
                        //pair.parts.parts = new List<PerceptionObject>
                    }
                }
            }
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
        // <!!> ContainerクラスをSerializeableにしようとしたが継承元のSerializeableが反映されないため無理
        // Listが出てこない
        //[Serializable]
        public abstract class Container : List<PerceptionObject> {
            public abstract void OnDrawGizmos();
            public PerceptionObject GetPerceptionObject(int i) {
                if (0 <= i && i < this.Count) {
                    return this[i];
                } else {
                    return null;
                }
            }
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

        [Serializable]
        public class PartsNamePair {
            public string name;
            public Parts parts;
        }
        // これTypeNameがいらないからこれをContainerにならないだろうか？
        [Serializable]
        public class Parts {
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

        // <!!> 違和感のある実装PartNamePair.PartがList<PerceptionObject>であるためにTypeにキャスとしているのが違和感がある
        // ここをprivateにしているとEditor側でparts.part = new CupPart()観たいのができない
        [SerializeField]
        public List<PartsNamePair> partsNamePairs = new List<PartsNamePair>();
        public Type GetPart<Type>() where Type : Parts, new() {
            foreach (var pair in partsNamePairs) {
                if (pair.name == typeof(Type).Name) {
                    return (pair.parts as Type);
                }
            }
            PartsNamePair newPartsNamePair = new PartsNamePair();
            newPartsNamePair.parts = new Type();
            newPartsNamePair.name = typeof(Type).Name;
            partsNamePairs.Add(newPartsNamePair);
            return (newPartsNamePair.parts as Type);
        }
        //public Type GetContainer<Type>() where Type : Container, new() {
        //    if (containers.ContainsKey(typeof(Type))) {
        //        return (containers[typeof(Type)] as Type);
        //    } else {
        //        Type newObj = new Type();
        //        containers[typeof(Type)] = newObj;
        //        return newObj;
        //    }
        //}
        public void UpdatePerc() {
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

        // Testように
        List<Parts> test;
        public void Start() {
            PersonPartsContainer ppc = new PersonPartsContainer();
            var ContainerTypes = Assembly.GetAssembly(typeof(Container)).GetTypes().Where(t => {
                return t.IsSubclassOf(typeof(Container)) && !t.IsAbstract;
            });
            foreach (var skillType in ContainerTypes) {
                Debug.Log(skillType);
            }

            test = new List<Parts>();
            test.Add(new CupParts());
            foreach (var tes in test) {
                Debug.Log(tes.GetType());
            }
            //foreach (var pp in ppc) {
            //    Debug.Log("posrot = " + pp.PosRot().position);
            //}
        }
    }
    [Serializable]
    public class CupParts : PerceptionObjectGroup.Parts {
        public PerceptionObject Body => GetPerceptionObject(0);
        public PerceptionObject Handle => GetPerceptionObject(1);
    }
    [Serializable]
    public class PersonPartsContainer : PerceptionObjectGroup.Parts {
        public PerceptionObject Head => GetPerceptionObject(0);
        public PerceptionObject LeftHand => GetPerceptionObject(1);
        public PerceptionObject RightHand => GetPerceptionObject(2);
        public PerceptionObject RightArm => GetPerceptionObject(3);
    }

    ///// <summary>
    ///// ジェネリックを隠すために継承してしまう
    ///// [System.Serializable]を書くのを忘れない
    ///// </summary>
    [System.Serializable]
    public class SampleTable : TableBase<Type, PerceptionObjectGroup.Container, SamplePair> {
    }
    /// <summary>
    /// ジェネリックを隠すために継承してしまう
    /// [System.Serializable]を書くのを忘れない
    /// </summary>
    [System.Serializable]
    public class SamplePair : KeyAndValue<Type, PerceptionObjectGroup.Container> {
        public SamplePair(Type key, PerceptionObjectGroup.Container value) : base(key, value) {

        }
    }
    //private Dictionary<Type, Container> containers = new Dictionary<Type, Container>();
}
