using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using XNodeEditor;
using SprUnity;
namespace VGent{
    [CustomNodeEditor(typeof(ActionTargetNodeBase))]
    public class ActionTargetNodeBaseEditor : NodeEditor {
        public virtual void OnSceneGUI(Body body = null) { }
        public override void OnHeaderGUI() {
            GUILayout.BeginHorizontal();
            ActionTargetNodeBase node = (ActionTargetNodeBase)target;
            node.visualizable = GUILayout.Toggle(node.visualizable, "", GUILayout.Width(10));
            GUILayout.Label(target.name, NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
            GUILayout.EndHorizontal();
        }
    }
}