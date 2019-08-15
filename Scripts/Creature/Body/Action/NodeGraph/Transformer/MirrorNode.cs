using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace SprUnity {
    [CreateNodeMenu("Transform/Mirror")]
    public class MirrorNode : ActionTargetTransformNodeBase {

        [Output] public PosRotScale output;
        [Input] public PosRotScale input;
        [Input] public Vector3 mirrorPos = Vector3.zero;
        [Input] public Vector3 mirrorNormal = new Vector3(1, 0, 0);
        public bool mirrorPosition;
        public bool mirrorRotation;

        // Use this for initialization
        protected override void Init() {
            base.Init();
        }

        // Return the correct value of an output port when requested
        public override object GetValue(NodePort port) {
            if(port.fieldName == "output") {
                PosRotScale tempInput = GetInputValue<PosRotScale>("input", this.input);
                Vector3 tempMirrorPos = GetInputValue<Vector3>("mirrorPos", this.mirrorPos);
                Vector3 tempMirrorNormal = GetInputValue<Vector3>("mirrorNormal", this.mirrorNormal);
                Quaternion mirrorRot = Quaternion.FromToRotation(Vector3.right, tempMirrorNormal);
                // 一度中心原点、x軸法線に戻す
                var pos1 = Quaternion.Inverse(mirrorRot) * (tempInput.position - tempMirrorPos);
                var rot1 = Quaternion.Inverse(mirrorRot) * tempInput.rotation;
                // ミラー
                var pos2 = pos1; pos2.x *= -1;
                var rot2 = Quaternion.Inverse(rot1);
                // 元のミラー座標へ
                var outputPos = mirrorRot * pos2 + tempMirrorPos;
                var outputRot = mirrorRot * rot2;

                return new PosRotScale(outputPos, outputRot, tempInput.scale);
            }
            return null;
        }
    }
}