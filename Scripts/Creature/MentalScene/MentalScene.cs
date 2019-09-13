using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SprUnity;
using System;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VGent{
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
            RepaintInspector();
        }
        public void RemoveMentalGroup(MentalGroup mentalGroup) {
            mentalGroupList.Remove(mentalGroup);
        }
        public void RepaintInspector() {
#if UNITY_EDITOR
            var assembly = Assembly.Load("UnityEditor");
            var type = assembly.GetType("UnityEditor.InspectorWindow");
            var inspector = EditorWindow.GetWindow(type);

            inspector.Repaint();
#endif
        }
    }
}
