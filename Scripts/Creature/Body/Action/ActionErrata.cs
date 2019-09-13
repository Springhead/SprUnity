using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SprUnity;

namespace VGent{

    [CreateAssetMenu()]
    public class ActionErrata : MonoBehaviour{

        [System.Serializable]
        public class FromToPair {

            public string parentBoneLabel;
            private Bone parentBone = null;

            public bool considerPosition = true;
            public bool considerRotation = false;

            public Vector3 fromPos;
            public Collider fromPosRange;

            public Vector3 toPos;
            public Collider toPosRange;

            public ActionTargetOutputNode insertedNode;

            public Vector2 springDamper = Vector2.one;

            public float extensionRate = 1.0f; // 発行済みのサブムーブメントの倍率
            public float firstEndTimeRate = 0.6f;
            public float secondStartTimeRate = 0.4f;

            public bool IsInside(Pose from, Pose to) {
                if (fromPosRange.ClosestPoint(from.position) == from.position && toPosRange.ClosestPoint(to.position) == to.position) return true;
                else return false;
            }
        }

        public FromToPair[] erattas;
    }

}