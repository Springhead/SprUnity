﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using SprUnity;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VGent{
    [CreateNodeMenu("Coordinate/Body")]
    public class BodyCoordinateNode : ActionTargetInputNodeBase {
        [Output] public PosRotScale posRotScale = new PosRotScale();

        // Use this for initialization
        protected override void Init() {
            base.Init();
        }

        // Return the correct value of an output port when requested
        public override object GetValue(NodePort port) {
            if (port.fieldName == "posRotScale") {
                PosRotScale tempPosRotScale = new PosRotScale();
                Body body = (graph as ActionTargetGraph)?.body;
                if(body != null) {
                    tempPosRotScale.position = body.transform.position;
                    tempPosRotScale.rotation = body.transform.rotation;
                    tempPosRotScale.scale = body.height * Vector3.one;
                } else {
                    tempPosRotScale = posRotScale;
                }
                return tempPosRotScale;
            } else {
                return null;
            }
        }

        public override void OnSceneGUI(Body body = null) {
#if UNITY_EDITOR
            PosRotScale tempPosRotScale = GetInputValue<PosRotScale>("posRotScale", this.posRotScale);
            if(tempPosRotScale != null) {
                Handles.PositionHandle(tempPosRotScale.position, tempPosRotScale.rotation);
            }
#endif
        }
    }
}