using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SprUnity {
    public class PerceptionObject {
        public PosRot posrot;
        public float confidence;
    }

    public class PerceptionObjectGroup : MonoBehaviour {
        // Attributes
        public class Attribute {
            public virtual void StartPerc(PerceptionObjectGroup perceptionObjectGroup) { }
            public virtual void UpdatePerc(PerceptionObjectGroup perceptionObjectGroup) { }
            public virtual void OnDrawGizmos(PerceptionObjectGroup perceptionObjectGroup) { }
        }
        // <!!> 後回し
        public abstract class Container : List<PerceptionObject>{
            public abstract void OnDrawGizmos();
        }
        bool isAgent;
        private Dictionary<Type, Attribute> attributes = new Dictionary<Type, Attribute>();
        public Type GetAttribute<Type>() where Type : Attribute, new() {
            if (attributes.ContainsKey(typeof(Type))) {
                return (attributes[typeof(Type)] as Type);
            } else {
                Type newObj = new Type();
                attributes[typeof(Type)] = newObj;
                attributes[typeof(Type)].StartPerc(this);
                return newObj;
            }
        }
        private Dictionary<Type,Container> containers = new Dictionary<Type, Container>();
        public Type GetContainer<Type>() where Type : Container, new() {
            if (containers.ContainsKey(typeof(Type))) {
                return (containers[typeof(Type)] as Type);
            } else {
                Type newObj = new Type();
                containers[typeof(Type)] = newObj;
                return newObj;
            }
        }
        public void UpdatePerc() {
            foreach (var attr in attributes) {
                attr.Value.UpdatePerc(this);
            }
        }
        void OnDrawGizmos() {
            foreach (var kv in attributes) {
                kv.Value.OnDrawGizmos(this);
            }
            foreach (var container in containers) {
                container.Value.OnDrawGizmos();
            }
        }

        // Testように
        public void Start() {
            PersonPartsContainer ppc = new PersonPartsContainer();
            foreach(var pp in ppc) {
                Debug.Log("posrot = "+ pp.posrot);
            }
        }
    }
    public class PersonPartsContainer : PerceptionObjectGroup.Container {
        public override void OnDrawGizmos() {
        }
        public PerceptionObject Head { get { return this[0]; } set { this[0] = value; } }
        public PerceptionObject LeftHand;
        public PerceptionObject RightHand;
    }
}
