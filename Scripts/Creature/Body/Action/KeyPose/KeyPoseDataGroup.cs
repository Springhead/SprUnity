using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SprUnity {

#if UNITY_EDITOR
    [CustomEditor(typeof(KeyPoseDataGroup))]
    public class KeyPoseDataGroupEditor : Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            KeyPoseDataGroup keyPoseGroup = (KeyPoseDataGroup)target;
            Rect DDBox = EditorGUILayout.GetControlRect(GUILayout.Width(200), GUILayout.Height(50));
            List<Object> droppedObjects = CreateDragAndDropGUI(DDBox);
            if (droppedObjects.Count > 0) {
                Debug.Log("D&D!");
                string keyPoseGroupPath = AssetDatabase.GetAssetPath(keyPoseGroup);
                foreach (var obj in droppedObjects) {
                    KeyPoseInterpolationGroup keyPoseInterpolation = obj as KeyPoseInterpolationGroup;
                    if (keyPoseInterpolation != null) {
                        foreach (var keyposeInGroup in keyPoseInterpolation.keyposes) {
                            var clone = Instantiate(keyposeInGroup);
                            clone.name = clone.name.Split('(')[0];
                            AssetDatabase.AddObjectToAsset(clone, keyPoseGroupPath);
                        }
                        continue;
                    }
                    KeyPoseData keypose = obj as KeyPoseData;
                    if (keypose != null) {
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

            if (e == EventType.DragUpdated || e == EventType.DragPerform) {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (e == EventType.DragPerform) {
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
    public class KeyPoseDataGroup : ScriptableObject {
        public static void CreateKeyPoseDataGroupAsset() {
            // Asset全検索
            var guids = AssetDatabase.FindAssets("*").Distinct();

            List<string> nameList = new List<string>();
            foreach (var guid in guids) {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                var keyPoseGroup = obj as KeyPoseDataGroup;
                if (keyPoseGroup != null) {
                    nameList.Add(keyPoseGroup.name);
                }
            }
            var newAsset = CreateInstance<KeyPoseDataGroup>();
            bool exist = false;
            for (int i = 0; i < 100; i++) {
                exist = false;
                foreach (var name in nameList) {
                    if (name == "KeyPoseGroup" + i) {
                        exist = true;
                        break;
                    }
                }
                if (!exist) {
                    AssetDatabase.CreateAsset(newAsset, "Assets/Actions/KeyPoses/" + "KeyPoseGroup" + i + ".asset");
                    AssetDatabase.Refresh();
                    break;
                }
            }
            if (exist) {
                Debug.LogError("KeyPoseGroup's name is covered");
            }
        }
#if UNITY_EDITOR
        [MenuItem("Assets/Create/Action/Add New KeyPose")]
        static void CreateKeyPose() {
            var selected = Selection.activeObject as KeyPoseDataGroup;

            if (selected == null) {
                Debug.LogWarning("Null KeyPoseGroup");
                return;
            }

            var keypose = ScriptableObject.CreateInstance<KeyPoseData>();
            keypose.name = "keypose";
            keypose.InitializeByCurrentPose();
            AssetDatabase.AddObjectToAsset(keypose, selected);

            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(keypose));
        }
#endif
        public void CreateKeyPoseInWin() {
            CreateKeyPoseInWin("keypose");
        }
        public void CreateKeyPoseInWin(string name) {
#if UNITY_EDITOR
            if (this == null) {
                Debug.LogWarning("Null KeyPoseGroup");
                return;
            }

            var keypose = ScriptableObject.CreateInstance<KeyPoseData>();
            keypose.name = name;
            keypose.InitializeByCurrentPose();
            AssetDatabase.AddObjectToAsset(keypose, this);

            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(keypose));
#endif
        }

        // この関数ではSubAssetだけでなくKeyPoseGroupも含まれる
        public Object[] GetSubAssets() {
#if UNITY_EDITOR
            string path = AssetDatabase.GetAssetPath(this);
            return AssetDatabase.LoadAllAssetsAtPath(path);
#else
        return null;
#endif
        }
    }
}
