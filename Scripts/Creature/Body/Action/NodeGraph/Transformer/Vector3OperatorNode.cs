using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace SprUnity {
    [CreateNodeMenu("Transform/Vector3Operator")]
    public class Vector3OperatorNode : ActionTargetNodeBase {
        [Output] public Vector3 result;
        [Input] public Vector3 input1;
        [Input] public Vector3 input2;
        public enum OperatorType {
            Add,
            Subtract,
            DotProduct,
            CrossProduct
        }
        public OperatorType type = OperatorType.Add;

        // Use this for initialization
        protected override void Init() {
            base.Init();
        }

        // Return the correct value of an output port when requested
        public override object GetValue(NodePort port) {
            Vector3 tempInput1 = GetInputValue<Vector3>("input1", this.input1);
            Vector3 tempInput2 = GetInputValue<Vector3>("input2", this.input2);
            switch (type) {
                case OperatorType.Add:
                    return tempInput1 + tempInput2;
                case OperatorType.Subtract:
                    return tempInput1 - tempInput2;
                case OperatorType.DotProduct:
                    return Vector3.Dot(tempInput1, tempInput2);
                case OperatorType.CrossProduct:
                    return Vector3.Cross(tempInput1, tempInput2);
            }
            return null;
        }
    }
}