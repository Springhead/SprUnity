using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PullbackPoseBoneWindow : EditorWindow {

    //
    static PullbackPoseBoneWindow window;

    [MenuItem("Window/Pullback Pose Bone Window")]
    static void Open() {
        window = GetWindow<PullbackPoseBoneWindow>();
        ActionEditorWindowManager.instance.pullbackPoseBoneWindow = window;
    }

    public void OnEnable() {

    }

    public void OnDisable() {
        window = null;
        ActionEditorWindowManager.instance.pullbackPoseBoneWindow = null;
    }
}
