using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using XNodeEditor;

namespace SprUnity {
    [CustomNodeEditor(typeof(ActionTargetNodeBase))]
    public class ActionTargetNodeBaseEditor : NodeEditor {
        public virtual void OnSceneGUI(Body body = null) { } 
    }
}