using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SprUnity {
    public class MentalObject : MentalExistance {
        public override Type GetAttribute<Type>() {
            //if (attributes.ContainsKey(typeof(Type))) {
            //    return (attributes[typeof(Type)] as Type);
            //} else {
            //    Type newObj = new Type();
            //    attributes[typeof(Type)] = newObj;
            //    return newObj;
            //}
            return null;
        }
        // ここの構造どうしようか..PosRotConfを作るか？
        public List<PosRot> posrots = new List<PosRot>();
        public float confidence;
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
}
