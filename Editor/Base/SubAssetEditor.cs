using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


/**
 *  参考：『uGUIではじめるUnity UIデザインの教科書』 p245
 */
public class SubAssetEditor : MonoBehaviour {

    [MenuItem("Assets/Delete Sub Asset")]
    public static void Delete() {
        Object[] selectedAssets = Selection.objects;
        if(selectedAssets.Length < 1) {
            Debug.LogWarning("No sub asset selected.");
            return;
        }

        foreach(var asset in selectedAssets) {
            if (AssetDatabase.IsSubAsset(asset)) {
                string path = AssetDatabase.GetAssetPath(asset);
                DestroyImmediate(asset, true);
                AssetDatabase.ImportAsset(path);
            }
        }
    } 
}
