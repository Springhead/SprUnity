using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Linq;

namespace SprUnity {
    public class PerceptionObject {
        // ここの構造どうしようか..PosRotConfを作るか？
        public List<PosRot> posrots = new List<PosRot>();
        public float confidence;
        public GameObject gameObject;
        public Vector3 Position(float time = 0) {
            return gameObject.transform.position;
        }
        public Quaternion Rotation(float time = 0) {
            return gameObject.transform.rotation;
        }
        public PosRot PosRot(float time = 0) {
            return posrots[0];
        }
        // ここで時間の更新する？どうする？VirtualSensorがやる？
        public void UpdatePerception() {

        }
    }

    public class PerceptionObjectGroup : MonoBehaviour {
        // Attributes
        public class Attribute {
            public virtual void StartPerc(PerceptionObjectGroup perceptionObjectGroup) { }
            public virtual void UpdatePerc(PerceptionObjectGroup perceptionObjectGroup) { }
            public virtual void OnDrawGizmos(PerceptionObjectGroup perceptionObjectGroup) { }
        }
        // <!!> 後回し
        public abstract class Container : List<PerceptionObject> {
            public abstract void OnDrawGizmos();
            public PerceptionObject GetPerceptionObject(int i) {
                if(i < this.Count) {
                    return this[i];
                } else {
                    return null;
                }
            }
        }
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
        private Dictionary<Type, Container> containers = new Dictionary<Type, Container>();
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
            var ContainerTypes = Assembly.GetAssembly(typeof(Container)).GetTypes().Where(t => {
                return t.IsSubclassOf(typeof(Container)) && !t.IsAbstract;
            });
            foreach(var skillType in ContainerTypes) {
                Debug.Log(skillType);
            }
            //foreach (var pp in ppc) {
            //    Debug.Log("posrot = " + pp.PosRot().position);
            //}
        }
    }
    public class PersonPartsContainer : PerceptionObjectGroup.Container {
        public override void OnDrawGizmos() {
        }
        public PerceptionObject Head { get { return this[0]; } set { this[0] = value; } }
        public PerceptionObject LeftHand { get { return this[1]; } set { this[1] = value; } }
        public PerceptionObject RightHand { get { return this[2]; } set { this[2] = value; } }
        public PerceptionObject RightArm => GetPerceptionObject(3);
    }
}
