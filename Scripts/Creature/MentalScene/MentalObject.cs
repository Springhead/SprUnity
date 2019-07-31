using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SprUnity {
    public class MentalObject : MentalExistence {
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
        private PosRot[] posrots; // 真ん中(length/2+1)を現在のPosRotとする
        private float[] deltaTimes;
        public float confidence;
        private int length = 3;
        private int count = 0;
        public Vector3 Position(float time = 0) {
            return gameObject.transform.position;
        }
        public Quaternion Rotation(float time = 0) {
            return gameObject.transform.rotation;
        }
        public Vector3 Velocity(float time = 0) {
            if (deltaTimes[length / 2 + 1] == 0) {
                return Vector3.zero;
            }
            return (posrots[length / 2 + 1].position - posrots[length / 2].position) / deltaTimes[length / 2 + 1];
        }
        public PosRot PosRot(float time = 0) {
            return posrots[0];
        }
        private void Start() {
            posrots = new PosRot[length];
            for (int i = 0; i < length; i++) {
                posrots[i] = new PosRot(gameObject);
            }
            deltaTimes = new float[length];
        }
        private void FixedUpdate() {
            for (int i = 0; i < length / 2 + 1; i++) {
                posrots[i].position = posrots[i + 1].position;
                posrots[i].rotation = posrots[i + 1].rotation;
                deltaTimes[i] = deltaTimes[i + 1];
            }
            posrots[length / 2 + 1].position = gameObject.transform.position;
            posrots[length / 2 + 1].rotation = gameObject.transform.rotation;
            deltaTimes[length / 2 + 1] = Time.deltaTime;
        }
    }
}
