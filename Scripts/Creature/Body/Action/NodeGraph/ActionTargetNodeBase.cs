using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using SprUnity;

namespace VGent {
    public abstract class ActionTargetNodeBase : Node {

        public bool visualizable = false;

        public virtual void OnSceneGUI(Body body = null) { }
        public virtual void SetInput<T>(T value) { }

        public virtual void OnValidate() {

        }
    }
}
