using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SprUnity {
    [CreateNodeMenu("Transform/Relative")]
    [NodeTint("#bae1ff")]
    public class RelativePosRotScaleNode : ActionTargetTransformNodeBase {
        [Input] public PosRotScale origin = new PosRotScale();
        [Input] public PosRotScale relative = new PosRotScale();
        [Output] public PosRotScale result = new PosRotScale();

        // Use this for initialization
        protected override void Init() {
            base.Init();

        }

        // Return the correct value of an output port when requested
        public override object GetValue(NodePort port) {
            PosRotScale tempOrigin = GetInputValue<PosRotScale>("origin", this.origin);
            PosRotScale tempRelative = GetInputValue<PosRotScale>("relative", this.relative);
            if(port.fieldName == "result") {
                return tempOrigin.TransformPosRotScale(tempRelative);
            } else {
                return new PosRotScale();
            }
        }

        public override void OnSceneGUI(Body body = null) {
#if UNITY_EDITOR
            PosRotScale tempOrigin = GetInputValue<PosRotScale>("origin");
            if (GetPort("origin").IsConnected) {
                Handles.PositionHandle(tempOrigin.position, tempOrigin.rotation);
            }
            PosRotScale tempRelative = GetInputValue<PosRotScale>("relative", this.relative);
            PosRotScale r = tempOrigin.TransformPosRotScale(tempRelative);
            if (Tools.pivotRotation == PivotRotation.Local) {
                EditorGUI.BeginChangeCheck();
                Vector3 pos = Handles.PositionHandle(r.position, r.rotation);
                Quaternion rot = Handles.RotationHandle(r.rotation, r.position);
                if (EditorGUI.EndChangeCheck() && !GetPort("relative").IsConnected) {
                    Undo.RecordObject(this, "Change RelativePosRotScaleNode");
                    relative = tempOrigin.InverseTransformPosRotScale(new PosRotScale(pos, rot, r.scale));
                }
            } else {
                EditorGUI.BeginChangeCheck();
                Vector3 pos = Handles.PositionHandle(r.position, Quaternion.identity);
                if (EditorGUI.EndChangeCheck() && !GetPort("relative").IsConnected) {
                    Undo.RecordObject(this, "Change RelativePosRotScaleNode");
                    relative = tempOrigin.InverseTransformPosRotScale(new PosRotScale(pos, r.rotation, r.scale));
                }
            }
#endif
        }
    }

}