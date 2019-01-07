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
        if (selectedAssets.Length < 1) {
            Debug.LogWarning("No sub asset selected.");
            return;
        }

        foreach (var asset in selectedAssets) {
            if (AssetDatabase.IsSubAsset(asset)) {
                string path = AssetDatabase.GetAssetPath(asset);
                DestroyImmediate(asset, true);
                AssetDatabase.ImportAsset(path);
            }
        }
    }

    [MenuItem("Assets/Set to HideFlags.None")]
    static void SetHideFlagsNone() {
        //AnimatorController を選択した状態でメニューを実行
        var path = AssetDatabase.GetAssetPath(Selection.activeObject);

        //サブアセット含めすべて取得
        foreach (var item in AssetDatabase.LoadAllAssetsAtPath(path)) {
            //フラグをすべて None にして非表示設定を解除
            item.hideFlags = HideFlags.None;
        }
        //再インポートして最新にする
        AssetDatabase.ImportAsset(path);
    }

    [MenuItem("Assets/Set to HideFlags.HideInHierarchy")]
    static void SetHideFlagsHide() {
        //AnimatorController を選択した状態でメニューを実行
        var path = AssetDatabase.GetAssetPath(Selection.activeObject);

        //サブアセット含めすべて取得
        foreach (var item in AssetDatabase.LoadAllAssetsAtPath(path)) {
            //フラグをすべて None にして非表示設定を解除
            item.hideFlags = HideFlags.HideInHierarchy;
        }
        //再インポートして最新にする
        AssetDatabase.ImportAsset(path);
    }
}