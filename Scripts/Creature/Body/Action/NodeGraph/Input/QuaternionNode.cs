using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SprUnity {
    [CreateNodeMenu("Value/Quaternion")]
    public class QuaternionNode : ActionTargetNodeBase {
        [Output] public Quaternion output;
        [Input] public float x;
        [Input] public float y;
        [Input] public float z;
        [Input] public float w = 1;

        // Use this for initialization
        protected override void Init() {
            base.Init();

        }

        public override void SetInput<T>(T value) {
            if (value is Quaternion) {
                this.output = (Quaternion)(object)value;
                this.x = output.x;
                this.y = output.y;
                this.z = output.z;
                this.w = output.w;
            }
        }

        // Return the correct value of an output port when requested
        public override object GetValue(NodePort port) {
            if (port.fieldName == "output") {
                return new Quaternion(GetInputValue<float>("x", x), GetInputValue<float>("y", y), GetInputValue<float>("z", z), GetInputValue<float>("w", w));
            }
            return Quaternion.identity;
        }

        public override void OnSceneGUI(Body body = null) {
#if UNITY_EDITOR
            EditorGUI.BeginChangeCheck();
            Quaternion rot = Handles.RotationHandle(new Quaternion(x, y, z, w), Vector3.zero);
            Handles.PositionHandle(Vector3.zero, new Quaternion(x, y, z, w));
            if (EditorGUI.EndChangeCheck()) {
                x = rot.x;
                y = rot.y;
                z = rot.z;
                w = rot.w;
            }
#endif
        }
    }
}