using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class KeyPoseInterpolationWindow : EditorWindow {

    //
    static KeyPoseInterpolationWindow window;

    [MenuItem("Window/KeyPose Interpolation Window")]
    static void Open() {
        window = GetWindow<KeyPoseInterpolationWindow>();
        ActionEditorWindowManager.instance.interpolationWindow = KeyPoseInterpolationWindow.window;
    }

    public void OnEnable() {

    }

    public void OnDisable() {
        window = null;
        ActionEditorWindowManager.instance.interpolationWindow = null;
    }
}
