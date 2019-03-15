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
        public List<KeyPose> keyposes = new List<KeyPose>();
    }
}
