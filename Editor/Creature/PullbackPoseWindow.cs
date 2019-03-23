using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SprUnity {

    public class PullbackPoseWindow : EditorWindow {

        //
        static PullbackPoseWindow window;

        [MenuItem("Window/Pullback Pose Window")]
        static void Open() {
            window = GetWindow<PullbackPoseWindow>();
            ActionEditorWindowManager.instance.pullbackPoseWindow = window;
        }

        public void OnEnable() {

        }

        public void OnDisable() {
            window = null;
            ActionEditorWindowManager.instance.pullbackPoseWindow = null;
        }
    }

}