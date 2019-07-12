using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SprUnity {

#if UNITY_EDITOR
    [CreateAssetMenu(menuName = "Action/Create KeyPoseGroup")]
#endif
    public class KeyPoseDataGroup : ScriptableObject {
        public static string path = "Assets/Actions/KeyPoses/";
#if UNITY_EDITOR
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
                    AssetDatabase.CreateAsset(newAsset, path + "KeyPoseGroup" + i + ".asset");
                    AssetDatabase.Refresh();
                    break;
                }
            }
            if (exist) {
                Debug.LogError("KeyPoseGroup's name is covered");
            }
        }
        [MenuItem("Assets/Create/Action/Add New KeyPose")]
        static void CreateKeyPose() {
            var selected = Selection.activeObject as KeyPoseDataGroup;

            if (selected == null) {
                Debug.LogWarning("Null KeyPoseGroup");
                return;
            }

            var keypose = ScriptableObject.CreateInstance<KeyPoseNodeGraph>();
            keypose.name = "keypose";
            //keypose.InitializeByCurrentPose();
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

            var keypose = ScriptableObject.CreateInstance<KeyPoseNodeGraph>();
            keypose.name = name;
            //keypose.InitializeByCurrentPose();
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
