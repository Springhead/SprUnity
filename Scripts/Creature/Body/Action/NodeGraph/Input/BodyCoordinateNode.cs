﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace SprUnity {
    [CreateNodeMenu("Coordinate/Body")]
    public class BodyCoordinateNode : Node {
        [Output] public PosRotScale posRotScale = new PosRotScale();

        // Use this for initialization
        protected override void Init() {
            base.Init();
        }

        // Return the correct value of an output port when requested
        public override object GetValue(NodePort port) {
            if (port.fieldName == "posRotScale") {
                PosRotScale tempPosRotScale = new PosRotScale();
                Body body = (graph as KeyPoseNodeGraph)?.body;
                if(body != null) {
                    tempPosRotScale.position = body.transform.position;
                    tempPosRotScale.rotation = body.transform.rotation;
                    tempPosRotScale.scale = body.height * Vector3.one;
                }
                return tempPosRotScale;
            } else {
                return null;
            }
        }
    }
}