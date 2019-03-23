using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SprUnity {
    public class PullbackPoseGroupWindow : EditorWindow {

        //
        static PullbackPoseGroupWindow window;

        [MenuItem("Window/Pullback Pose Group Window")]
        static void Open() {
            window = GetWindow<PullbackPoseGroupWindow>();
            ActionEditorWindowManager.instance.pullbackPoseGroupWindow = window;
        }

        public void OnEnable() {

        }

        public void OnDisable() {
            window = null;
            ActionEditorWindowManager.instance.pullbackPoseGroupWindow = null;
        }
    }

}