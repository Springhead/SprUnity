using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace VGent{
    [CreateNodeMenu("Coordinate/World")]
    public class WorldCoordinateNode : ActionTargetInputNodeBase {
        [Output] public PosRotScale posRotScale = new PosRotScale();

        // Use this for initialization
        protected override void Init() {
            base.Init();
        }

        // Return the correct value of an output port when requested
        public override object GetValue(NodePort port) {
            if (port.fieldName == "posRotScale") {
                return posRotScale;
            } else {
                return null;
            }
        }
    }
}