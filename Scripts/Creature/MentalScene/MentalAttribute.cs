using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace VGent{
    // Attributes
#if UNITY_EDITOR
    [CustomEditor(typeof(MentalAttribute))]
    public class MentalAttributeEditor : Editor {
    }
#endif

    public class MentalAttribute : MonoBehaviour {
        // Attributeを所持している親からAttributeやPartsの情報を取得する
        [HideInInspector]
        public MentalGroup mentalGroup;
        // GetAttributeした場合はGetAttribute内でmentalAttributeListOnPlayingに追加
        [HideInInspector]
        public bool initialized = false;

        // 他のところでStartを定義されるとそちらが優先される
        void Start() {
            if (!initialized) {
                mentalGroup = GetComponentInParent<MentalGroup>();
                if (mentalGroup == null) {
                    return;
                }
                mentalGroup.AddMentalAttribute(this);
                initialized = true;
            }
        }
        void OnDestroy() {
            if (mentalGroup == null) {
                return;
            }
            mentalGroup.RemoveMentalAttribute(this);
        }
    }
}
