using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif
public enum FacePart {
    Mouth = 1,
    Eyes = 2,
    Eyebrows = 3,
}
public class BlendShapeConflict : ScriptableObject {
    public string[] blendShapeNames;
    public FacePart[] faceParts;
    
    public static BlendShapeConflict GetBlendShapeConflict(string name) {
        var blendpath = "Assets/Actions/BlendShapeConflict/";
#if UNITY_EDITOR
        // Asset全検索
        var guids = AssetDatabase.FindAssets("*").Distinct();
#endif
        BlendShapeConflict bsc = null;
#if UNITY_EDITOR
        foreach (var guid in guids) {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
            bsc = obj as BlendShapeConflict;
            if (bsc != null) {
                if (name == bsc.name) {
                    break;
                }
            }
        }
        if (bsc == null) {
            bsc = CreateInstance<BlendShapeConflict>();
            AssetDatabase.CreateAsset(bsc, blendpath + name + ".asset");
            AssetDatabase.Refresh();
        }
#endif
        return bsc;
    }

    public Dictionary<string,FacePart> GetDictionary() {
        Dictionary<string, FacePart> dic = new Dictionary<string, FacePart>();

        if (blendShapeNames != null && faceParts != null) {
            dic = blendShapeNames.Zip(faceParts, (k, v) => new { k, v }).ToDictionary(a => a.k, a => a.v);
        }
        return dic;
    }
    public void Save(Dictionary<string,FacePart> dic) {
        blendShapeNames = new string[dic.Keys.Count];
        dic.Keys.CopyTo(blendShapeNames, 0);
        faceParts = new FacePart[dic.Values.Count];
        dic.Values.CopyTo(faceParts, 0);
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
#endif
    }
}
