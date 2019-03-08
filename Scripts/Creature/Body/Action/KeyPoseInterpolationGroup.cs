using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SprUnity;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(KeyPoseInterpolationGroup))]
public class KeyPoseInterpolationGroupEditor : Editor {
    void OnEnable() {
        SceneView.onSceneGUIDelegate += OnSceneGUI;
    }
    void OnDisable() {
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
    }
    public override void OnInspectorGUI() {
        bool textChangeComp = false;
        EditorGUI.BeginChangeCheck();
        Event e = Event.current;
        if (e.keyCode == KeyCode.Return && Input.eatKeyPressOnTextFieldFocus) {
            textChangeComp = true;
            Event.current.Use();
        }
        target.name = EditorGUILayout.TextField("Name", target.name);
        base.OnInspectorGUI();
        if (EditorGUI.EndChangeCheck()) {
            EditorUtility.SetDirty(target);
        }
        if (textChangeComp) {
            string mainPath = AssetDatabase.GetAssetPath(this);
            //EditorUtility.SetDirty(AssetDatabase.LoadMainAssetAtPath(mainPath));
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath((KeyPoseInterpolationGroup)target));
        }
    }
    public void OnSceneGUI(SceneView sceneView) {
        KeyPoseInterpolationGroup keyPoseGroup = (KeyPoseInterpolationGroup)target;
        if(keyPoseGroup.keyposes.Count == 1) {
            /*
            var keyPoseEditor = CreateEditor(keyPoseGroup.keyposes[0]);
            if (keyPoseEditor == null) return;
            ((KeyPoseEditor)keyPoseEditor).OnSceneGUI(sceneView);
            */
        }
    }
}

#endif

public class KeyPoseInterpolationGroup : ScriptableObject {

    // keyposes.Count == 1 : ただのコンテナ
    // keyposes.Count > 1  : パラメータによる補間で姿勢が決まるKeyPose

    public List<KeyPose> keyposes = new List<KeyPose>();

    public List<float> parameters = new List<float>();

    public float testDuration = 1.0f;
    public float testSpring = 1.0f;
    public float testDamper = 1.0f;

    public KeyCode hotKey;

    public void Action(Body body, float duration = -1, float startTime = -1, float spring = -1, float damper = -1, List<float> givenParam = null) {
        if (duration < 0) { duration = testDuration; }
        if (startTime < 0) { startTime = 0; }
        if (spring < 0) { spring = testSpring; }
        if (damper < 0) { damper = testDamper; }

        if (body == null) { body = GameObject.FindObjectOfType<Body>(); }
        if (body != null) {
            // 仮定:パラメータはN次元立方体の範囲内のみ
            // 　　 M個のサンプルを使用
            foreach (var boneKeyPose in keyposes) {
                // 未実装
            }
            if (keyposes.Count == 1 || givenParam.Count == 0) {
                keyposes[0].Action(body, duration, startTime, spring, damper);
            }
            else if(keyposes.Count == 2 && givenParam.Count >= 1) {
                List<BoneKeyPose> keyposes0 = keyposes[0].GetBoneKeyPoses(body);
                List<BoneKeyPose> keyposes1 = keyposes[1].GetBoneKeyPoses(body);
                List<BoneKeyPose> appliedKeypose = new List<BoneKeyPose>();
                foreach(var keypose0 in keyposes0) {
                    foreach(var keypose1 in keyposes1) {
                        if(keypose0.boneId == keypose1.boneId) {
                            Pose pose = new Pose();
                            bool onSubmovement = false;
                            if (keypose0.usePosition && keypose1.usePosition) {
                                pose.position = (1 - givenParam[0]) * keypose0.position + givenParam[0] * keypose1.position;
                                onSubmovement = true;
                            }
                            if (keypose0.useRotation && keypose1.useRotation) {
                                pose.rotation = Quaternion.Lerp(keypose0.rotation, keypose1.rotation, givenParam[0]);
                                onSubmovement = true;
                            }
                            if (onSubmovement) {
                                body[keypose0.boneId].controller.AddSubMovement(pose, new Vector2(spring, damper), duration, duration);
                            }
                        }
                    }
                }
            }
        }
    }

    public List<BoneKeyPose> GetBaseKeyPose(Body body) {
        // 現状 SingleKeyPoseにしか対応していない
        var keyPose = keyposes[0];
        if (body == null) { body = GameObject.FindObjectOfType<Body>(); }
        if (body == null) { return null; }
        List<BoneKeyPose> boneKeyPoses = new List<BoneKeyPose>();
        //return boneKeyPoses;
        return keyPose.GetBoneKeyPoses(body);
    }

    public List<BoneKeyPose> GetBaseKeyPose(Body body, GameObject target, out float duration, out float spring, out float damper) {
        // 現状 SingleKeyPoseにしか対応していない
        var keyPose = keyposes[0];
        duration = keyPose.testDuration;
        spring = keyPose.testSpring;
        damper = keyPose.testDamper;
        return GetBaseKeyPose(body);
    }

    public static KeyPoseInterpolationGroup CreateKeyPoseGroup(string folderPath = null, string name = null) {
        // KeyPose一つをSubassetにもつInterpolationGroupを作成
        // KeyPoseとKeyPoseInterpolationGroupは同名なので片方残っていればある程度復元できるはず
        var keyPoseGroup = ScriptableObject.CreateInstance<KeyPoseInterpolationGroup>();
        var keyPose = ScriptableObject.CreateInstance<KeyPose>();
        if (name != null) {
            keyPoseGroup.name = name;
            keyPose.name = name;
        } else {
            keyPose.name = "new keypose";
            keyPoseGroup.name = "new keypose";
        }
        keyPoseGroup.keyposes.Add(keyPose);

#if UNITY_EDITOR
        if (folderPath != null) {
            AssetDatabase.CreateAsset(keyPoseGroup, folderPath + "/" + keyPoseGroup.name + ".asset");
            AssetDatabase.AddObjectToAsset(keyPose, keyPoseGroup);
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(keyPoseGroup));
        } else {
            var filePath = EditorUtility.SaveFilePanelInProject("Save KeyPose", "new keypose", "asset", "");
            if (!string.IsNullOrEmpty(filePath)) {
                var splitedPath = filePath.Split('/');
                var assetName = splitedPath[splitedPath.Length - 1].Split('.')[0];
                keyPose.name = assetName;
                keyPoseGroup.name = assetName;
                AssetDatabase.AddObjectToAsset(keyPose, filePath);
                AssetDatabase.CreateAsset(keyPoseGroup, filePath);
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(keyPoseGroup));
            }
        }
#endif
        return keyPoseGroup;
    }

    void DeleteKeyPoseGroup() {
        // Delete
    }

    void AbsorbKeyPoseGroup(KeyPoseInterpolationGroup keyPoseGroup) {
#if UNITY_EDITOR
        // 他のKeyPoseGroupを吸収する
        var assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(keyPoseGroup));
        foreach(var asset in assets) {
            if (AssetDatabase.IsSubAsset(asset)) {
                var clone = Instantiate(asset);
                clone.name = clone.name.Split('(')[0];
                keyposes.Add((KeyPose)clone);
                AssetDatabase.AddObjectToAsset(clone, this);
            }
        }
        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(keyPoseGroup));
#endif
    }

    public static KeyPoseInterpolationGroup IntegrateKeyPoseGroup(List<KeyPoseInterpolationGroup> keyPoseGroups) {
#if UNITY_EDITOR
        // 複数のGroupをひとつにまとめる
        // まとめた後はパラメータによって中身が変化するGroupになる
        if (keyPoseGroups.Count < 2) return null;
        var filePath = AssetDatabase.GetAssetPath(keyPoseGroups[0]);
        var splitedPath = filePath.Split('/');
        string folderPath = System.String.Empty;
        for (int i = 0; i < splitedPath.Length - 1; i++) {
            folderPath += (splitedPath[i] + "/");
        }
        var newKeyPoseGroup = ScriptableObject.CreateInstance<KeyPoseInterpolationGroup>();
        newKeyPoseGroup.name = "new Group";
        string newKeyPoseGroupPath = folderPath + "/" + newKeyPoseGroup.name + ".asset";
        Debug.Log(newKeyPoseGroupPath);
        foreach (var keyPoseGroup in keyPoseGroups) {
            var clone = Instantiate(keyPoseGroup.keyposes[0]);
            clone.name = clone.name.Split('(')[0];
            newKeyPoseGroup.keyposes.Add(clone);
            AssetDatabase.AddObjectToAsset(clone, newKeyPoseGroupPath);
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(keyPoseGroup.keyposes[0]));
        }
        AssetDatabase.CreateAsset(newKeyPoseGroup, folderPath + "/" + newKeyPoseGroup.name + ".asset");
        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newKeyPoseGroup));
        return newKeyPoseGroup;
#else
        return null;
#endif
    }

    public List<KeyPoseInterpolationGroup> SeparateKeyPoseGroup() {
#if UNITY_EDITOR
        // 複数の要素からなるGroupを単一KeyPoseのみをもつGroupに分割
        if (keyposes.Count < 2) return null;
        var filePath = AssetDatabase.GetAssetPath(this);
        var splitedPath = filePath.Split('/');
        string folderPath = System.String.Empty;
        for(int i = 0; i < splitedPath.Length - 1; i++) {
            folderPath += (splitedPath[i] + "/");
        }
        List<KeyPoseInterpolationGroup> newKeyPoses = new List<KeyPoseInterpolationGroup>();
        foreach(var keypose in keyposes) {
            var newKeyPoseGroup = ScriptableObject.CreateInstance<KeyPoseInterpolationGroup>();
            var clone = Instantiate(keypose);
            clone.name = clone.name.Split('(')[0];
            newKeyPoseGroup.name = clone.name;
            newKeyPoseGroup.keyposes.Add(clone);
            AssetDatabase.AddObjectToAsset(clone, folderPath + "/" + newKeyPoseGroup.name + ".asset");
            AssetDatabase.CreateAsset(newKeyPoseGroup, folderPath + "/" + newKeyPoseGroup.name + ".asset");
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newKeyPoseGroup));
            newKeyPoses.Add(newKeyPoseGroup);
        }
        //Object.DestroyImmediate(this);
        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(this));
        return newKeyPoses;
#else
        return null;
#endif
    }

    // KeyPose AssetをDrag&DropしたらそれをSubAssetに設定する機能ほしい

}
