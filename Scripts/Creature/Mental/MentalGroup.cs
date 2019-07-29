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
        // perceptionObjectGroup.partsListが外部のメンバでboolを持ったクラスにすべきでない
        // partsの型情報は保存されないのでstringを使用
        private int partsTypeIndex;
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
            if (GUILayout.Button("Test")) {
                mentalGroup.Test();
            }
            base.OnInspectorGUI();
        }
    }
#endif

    public class MentalGroup : MentalExistance {

        //private List<MentalAttribute> attributes = new List<MentalAttribute>();
        //実態はGameObjectに格納されている
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

        // Dictionaryだとinspectorに表示が確実にできないpublicにまずしてはならない
        // inspectorに表示が絶対にあった穂がようい
        public Type GetParts<Type>() where Type : MentalParts, new() {
            // 子供の中を探し見つかったpartsがこのMentalGroupのものであれば返す
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

        public void Test() {
            foreach (var parts in GetAllParts()) {
                foreach (var part in parts) {
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
