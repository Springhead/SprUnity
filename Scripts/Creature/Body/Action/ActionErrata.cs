using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SprUnity {

    public class ActionErrata {

        public class FromToPair {
            public Vector3 fromPos;
            public Quaternion fromRot;
            public Collider fromPosRange;
            public float fromRotRange;

            public Vector3 toPos;
            public Quaternion toRot;
            public Collider toPosRange;
            public float toRotRange;  //姿勢差分角

            public Vector3 delta;

            public float delay; // 発行済みのサブムーブメントの遅延量
        }

        public void InsertErrataSubMovement(SubMovement sub) {
            
        }
    }

}