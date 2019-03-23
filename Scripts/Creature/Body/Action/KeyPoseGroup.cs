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
        public void CreateKeyPose(string name) {
#if UNITY_EDITOR
            if (this == null) {
                Debug.LogWarning("Null KeyPoseGroup");
                return;
            }

            var keypose = ScriptableObject.CreateInstance<KeyPose>();
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
