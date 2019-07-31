using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SprUnity {
    [CreateNodeMenu("Value/PosRotScale")]
    public class PosRotScaleNode : ActionTargetNodeBase {
        [Output] public PosRotScale output;
        [Input] public Vector3 pos = new Vector3();
        [Input] public Quaternion rot = Quaternion.identity;
        [Input] public Vector3 scale = new Vector3(1f, 1f, 1f);

        // Use this for initialization
        protected override void Init() {
            base.Init();

        }

        // Return the correct value of an output port when requested
        public override object GetValue(NodePort port) {
            if (port.fieldName == "output") {
                return new PosRotScale(GetInputValue<Vector3>("pos", pos), GetInputValue<Quaternion>("rot", rot), GetInputValue<Vector3>("scale", scale));
            }
            return new PosRotScale();
        }



        public override void OnSceneGUI(Body body = null) {
#if UNITY_EDITOR
            Vector3 tempPos = GetInputValue<Vector3>("pos", this.pos);
            Quaternion tempRot = GetInputValue<Quaternion>("rot", this.rot);
            Vector3 tempScale = GetInputValue<Vector3>("scale", this.scale);
            EditorGUI.BeginChangeCheck();
            Vector3 p = Vector3.zero;
            Quaternion r = Quaternion.identity;
            if (GetPort("pos").IsConnected) {
                //Handles.PositionHandle(tempPos, tempRot);
            } else {
                p = Handles.PositionHandle(tempPos, tempRot);
            }
            if (GetPort("rot").IsConnected) {

            } else {
                r = Handles.RotationHandle(tempRot, tempPos);
            }
            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(this, "Change PosRotScaleNode");
                if (!GetPort("pos").IsConnected) {
                    pos = p;
                }
                if (!GetPort("rot").IsConnected) {
                    rot = r.normalized;
                }
            }
#endif
        }
    }
}