using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SprUnity {

#if UNITY_EDITOR
    [CustomEditor(typeof(SampleAction))]
    public class SampleActionEditor : Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            SampleAction action = (SampleAction)target;
            if (GUILayout.Button("Start")) {
                action.Begin();
            }
            if (GUILayout.Button("End")) {
                action.End();
            }
        }
    }
#endif

    public class SampleAction : ScriptableAction {

        // KeyPoses
        public KeyPoseData sample1, sample2;
        public KeyPoseData[] sample3;

        public KeyPoseTimePair _sample1, _sample2, _sample3;

        // PerceptionGenerator

        // ----- ----- ----- ----- ----- -----
        public void Start() {
            _sample1.keyPose.ParserSpecifiedParts(sample1, new HumanBodyBones[] { HumanBodyBones.RightHand});
            _sample2.keyPose.ParserSpecifiedParts(sample2, new HumanBodyBones[] { HumanBodyBones.RightHand });
            _sample3.keyPose.ParserSpecifiedParts(sample3[0], new HumanBodyBones[] { HumanBodyBones.RightHand });
            base.Start();
        }
        
        // ----- ----- ----- ----- ----- -----
        // ScriptableActionを継承したものが実装するメソッド

        // Generate keyposes actually applied as motion 
        public override void GenerateMovement() {
            if (!_sample1.isUsed) {
                _sample1.keyPose[HumanBodyBones.RightHand].position = sample1[HumanBodyBones.RightHand].position;
                _sample1.keyPose[HumanBodyBones.RightHand].rotation = sample1[HumanBodyBones.RightHand].rotation;
                //Debug.Log("Add _sample1");
                generatedKeyPoses.Add(_sample1);
            }
            if (!_sample2.isUsed) {
                _sample2.keyPose[HumanBodyBones.RightHand].position = sample2[HumanBodyBones.RightHand].position;
                _sample2.keyPose[HumanBodyBones.RightHand].rotation = sample2[HumanBodyBones.RightHand].rotation;
                //Debug.Log("Add _sample2");
                generatedKeyPoses.Add(_sample2);
            }
        }

        public override void ResetAction() {
            _sample1.isUsed = false;
            _sample2.isUsed = false;
            _sample3.isUsed = false;
        }
    }

}