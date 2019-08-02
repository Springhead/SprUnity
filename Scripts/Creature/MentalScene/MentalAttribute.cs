using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace SprUnity {
    // Attributes
#if UNITY_EDITOR
    [CustomEditor(typeof(MentalAttribute))]
    public class MentalAttributeEditor : Editor {
    }
#endif

    public class MentalAttribute : MonoBehaviour {
        // Attributeを所持している親からAttributeやPartsの情報を取得する
        public MentalGroup mentalGroup;
        
        // 他のところでStartを定義されるとそちらが優先される
        void Start() {
            mentalGroup = GetComponentInParent<MentalGroup>();
            if(mentalGroup == null) {
                return;
            }
            mentalGroup.AddMentalAttribute(this);
        }
        void OnDestroy() {
            if (mentalGroup== null) {
                return;
            }
            mentalGroup.RemoveMentalAttribute(this);
        }
    }
}
