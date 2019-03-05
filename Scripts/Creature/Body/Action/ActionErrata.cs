using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        public KeyPoseInterpolationGroup keyframe; // 差し込むキーフレーム

        public float delay; // 発行済みのサブムーブメントの遅延量
    }

    public void InsertErrataSubMovement() {
        // エラッタ発行条件のチェック
        // エラッタ挿入のために直近発行したサブムーブメントを遅延、必須ではない？
        // エラッタ挿入
    }
}
