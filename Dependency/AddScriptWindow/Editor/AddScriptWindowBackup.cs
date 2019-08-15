﻿// c.f.) https://forum.unity.com/threads/custom-add-component-like-button.439730/

using UnityEngine;
using System.Collections;
using System;

namespace UnityEditorAddon {
    [Serializable]
    class AddScriptWindowBackup : ScriptableObject {
        public string scriptPath;
        public bool addAsset;

        public static AddScriptWindowBackup Instance {
            get {
                var objs = Resources.FindObjectsOfTypeAll<AddScriptWindowBackup>();
                if (objs.Length == 0 || objs[0] == null) {
                    return ScriptableObject.CreateInstance<AddScriptWindowBackup>();
                }
                return objs[0];
            }
        }

        void OnEnable() {
            hideFlags = HideFlags.HideAndDontSave;
        }

        public void Reset() {
            addAsset = false;
            scriptPath = string.Empty;
        }
    }
}