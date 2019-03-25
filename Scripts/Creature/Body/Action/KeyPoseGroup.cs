using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SprUnity {

#if UNITY_EDITOR
    [CustomEditor(typeof(KeyPoseGroup))]
    public class KeyPoseGroupEditor : Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            KeyPoseGroup keyPoseGroup = (KeyPoseGroup)target;
            Rect DDBox = EditorGUILayout.GetControlRect(GUILayout.Width(200), GUILayout.Height(50));
            List<Object> droppedObjects = CreateDragAndDropGUI(DDBox);
            if(droppedObjects.Count > 0) {
                Debug.Log("D&D!");
                string keyPoseGroupPath = AssetDatabase.GetAssetPath(keyPoseGroup);
                foreach (var obj in droppedObjects) {
                    KeyPoseInterpolationGroup keyPoseInterpolation = obj as KeyPoseInterpolationGroup;
                    if(keyPoseInterpolation != null) {
                        foreach(var keyposeInGroup in keyPoseInterpolation.keyposes) {
                            var clone = Instantiate(keyposeInGroup);
                            clone.name = clone.name.Split('(')[0];
                            AssetDatabase.AddObjectToAsset(clone, keyPoseGroupPath);
                        }
                        continue;
                    }
                    KeyPose keypose = obj as KeyPose;
                    if(keypose != null) {
                        var clone = Instantiate(keypose);
                        clone.name = clone.name.Split('(')[0];
                        AssetDatabase.AddObjectToAsset(clone, keyPoseGroupPath);
                    }
                }
                AssetDatabase.ImportAsset(keyPoseGroupPath);
            }
        }

        private List<Object> CreateDragAndDropGUI(Rect rect) {
            List<Object> list = new List<Object>();
            GUI.Box(rect, "D&D KeyPose");

            if (!rect.Contains(Event.current.mousePosition)) {
                return list;
            }

            EventType e = Event.current.type;

            if(e == EventType.DragUpdated || e == EventType.DragPerform) {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if(e == EventType.DragPerform) {
                    list = new List<Object>(DragAndDrop.objectReferences);
                    DragAndDrop.AcceptDrag();
                }
                Event.current.Use();
            }
            return list;
        }
    }
#endif

#if UNITY_EDITOR
    [CreateAssetMenu(menuName = "Action/Create KeyPoseGroup")]
#endif
    public class KeyPoseGroup : ScriptableObject {

#if UNITY_EDITOR
        [MenuItem("Assets/Create/Action/Add New KeyPose")]
        static void CreateKeyPose() {
            var selected = Selection.activeObject as KeyPoseGroup;

            if (selected == null) {
                Debug.LogWarning("Null KeyPoseGroup");
                return;
            }

            var keypose = ScriptableObject.CreateInstance<KeyPose>();
            keypose.name = "keypose";
            keypose.InitializeByCurrentPose();
            AssetDatabase.AddObjectToAsset(keypose, selected);

            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(keypose));
        }
#endif
        void CreateKeyPose(string name) {

            if (this == null) {
                Debug.LogWarning("Null KeyPoseGroup");
                return;
            }

            var keypose = ScriptableObject.CreateInstance<KeyPose>();
            keypose.name = name;
            keypose.InitializeByCurrentPose();
            AssetDatabase.AddObjectToAsset(keypose, this);

            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(keypose));
        }
#if UNITY_EDITOR

#endif
    }
}
