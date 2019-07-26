using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace SprUnity {
    public abstract class VGentNodeBase : Node {

        public virtual void OnSceneGUI(Body body = null) { }
        public virtual void SetInput<T>(T value) { }

        public virtual void OnValidate() {

        }
    }
}
