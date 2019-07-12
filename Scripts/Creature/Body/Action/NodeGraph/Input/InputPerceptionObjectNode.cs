using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SprUnity {
    [CreateNodeMenu("Input/PerceptionObject")]
    public class InputPerceptionObjectNode : VGentNodeBase {
        public GameObject perceptionObj;
        [Output] public PosRotScale posRotScale;
        [Output] public Vector3 pos;
        [Output] public Quaternion rot;

        // Use this for initialization
        protected override void Init() {
            base.Init();
        }

        // Return the correct value of an output port when requested
        public override object GetValue(NodePort port) {
            PosRotScale tempPosRotScale;
            if (perceptionObj != null) {
                tempPosRotScale = new PosRotScale(perceptionObj.transform);
            } else {
                tempPosRotScale = posRotScale;
            }
            switch (port.fieldName){
                case "posRotScale":
                    return tempPosRotScale;
                case "pos":
                    return tempPosRotScale.position;
                case "rot":
                    return tempPosRotScale.rotation;
                default:
                    return null;
            }
        }

        public override void SetInput<T>(T value) {
            if (value is GameObject) {
                perceptionObj = value as GameObject;
            }
        }

        public override void OnSceneGUI(Body body = null) {
#if UNITY_EDITOR
            if (perceptionObj == null) {
                EditorGUI.BeginChangeCheck();
                Vector3 pos = Handles.PositionHandle(posRotScale.position, posRotScale.rotation);
                Quaternion rot = Handles.RotationHandle(posRotScale.rotation, posRotScale.position);
                if (EditorGUI.EndChangeCheck()) {
                    posRotScale.position = pos;
                    posRotScale.rotation = rot;
                }
            }
#endif
        }
    }

}