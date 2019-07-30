using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using System.Linq;

namespace SprUnity {
    [CreateAssetMenu]
    public class KeyPoseNodeGraph : NodeGraph {

        private bool isKeyPoseData = false;

        [System.NonSerialized]
        public KeyPoseNodeGraph original = null;

        public ActionManager manager;
        public Body body {
            get {
                if (/*original != null && */manager != null) {
                    return manager.body;
                }
                return null;
            }
        }
        /*
        [System.NonSerialized]
        public Dictionary<ActionManager, KeyPoseNodeGraph> instances = new Dictionary<ActionManager, KeyPoseNodeGraph>();
        public KeyPoseNodeGraph GetInstance(ActionManager manager) {
            if (instances.ContainsKey(manager)) return instances[manager];
            else {
                if (this.original == null) {
                    KeyPoseNodeGraph instance = this.Copy() as KeyPoseNodeGraph;
                    instances.Add(manager, instance);
                    instance.manager = manager;
                    instance.original = this;
                    return instance;
                } else return manager == this.manager ? this : null;
            }
        }*/
        public override NodeGraph Copy() {
            KeyPoseNodeGraph graph = base.Copy() as KeyPoseNodeGraph;
            graph.isKeyPoseData = this.isKeyPoseData;
            return graph as NodeGraph;
        }

        public IEnumerable<Node> inputNodes { get { foreach (var node in nodes) { if (IsInputNode(node)) yield return node; } } }
        private bool IsInputNode(Node node){
            if (node == null) return false;
            if (!node.Inputs.Any()) {
                return true;
            } else {
                foreach (var inputPort in node.Inputs) {
                    if (inputPort.IsConnected) {
                        return false;
                    }
                }return true;
            }
        }
        public IEnumerable<BoneKeyPoseNode> boneKeyPoseNodes { get { foreach (var node in nodes) { if (node is BoneKeyPoseNode) yield return (BoneKeyPoseNode)node; } } }
        public IEnumerable<BoneKeyPose> boneKeyPoses { get { foreach (var boneKeyPoseNode in boneKeyPoseNodes) yield return boneKeyPoseNode.GetBoneKeyPose(); } }

        public List<BoneSubMovementPair> Action(Body body = null, float duration = -1, float startTime = -1, float spring = -1, float damper = -1, Quaternion? rotate = null) {
            return null;
        }
    }
}