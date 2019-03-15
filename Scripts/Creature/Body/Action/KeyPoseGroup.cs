using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SprUnity
{
#if UNITY_EDITOR
    [CreateAssetMenu(menuName = "Action/Create KeyPoseGroup")]
#endif
    public class KeyPoseGroup : ScriptableObject
    {
        [MenuItem("Assets/Create/Action/Add New KeyPose")]
        public static void AddKeyPose() {
            KeyPoseGroup selectedAsset = Selection.activeObject as KeyPoseGroup;
            if (selectedAsset == null) {
                Debug.LogWarning("Null KeyPoseGroup Asset");
                return;
            }

            var keyPose = ScriptableObject.CreateInstance<KeyPose>();
            keyPose.name = "New Key Pose";
            AssetDatabase.AddObjectToAsset(keyPose, selectedAsset);
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(selectedAsset));
        }
        public List<KeyPose> keyposes = new List<KeyPose>();
    }
}
