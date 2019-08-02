using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SprUnity;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SprUnity {
    [DefaultExecutionOrder(1)]
    public class MentalScene : MonoBehaviour {
        // MentalScene関連はプレイ中以外でも見れるようにするために二つ実装する 
        private List<MentalGroup> mentalGroupList = new List<MentalGroup>();
        public MentalGroup[] mentalGroups {
            get {
                if (Application.isPlaying) {
                    return mentalGroupList.ToArray();
                } else {
                    return FindObjectsOfType<MentalGroup>();
                }
            }
        }
        public MentalObject[] mentalObjects {
            get {
                return FindObjectsOfType<MentalObject>();
            }
        }
        public void AddMentalGroup(MentalGroup mentalGroup) {
            mentalGroupList.Add(mentalGroup);
        }
        public void RemoveMentalGroup(MentalGroup mentalGroup) {
            mentalGroupList.Remove(mentalGroup);
        }
        public void OnValidate() {
            
        }
    }
}
