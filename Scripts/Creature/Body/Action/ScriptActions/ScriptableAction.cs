using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using SprUnity;

// SubMovementの発行条件クラス
[Serializable]
public class SubmovementBlock {
    public float duration;
    public Vector2 springDamper;

}

// SubMovement発行までのWaitTime管理保持クラス
[Serializable]
public class WaitTimeBlock {
    public int waitTime;
}

public class ScriptableAction : MonoBehaviour {

    // 編集に関してはLogをとってそれを編集することで
    // submovement(class?)の一覧
    // waitTime(class?)の一覧

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		
	}

    public void BeginAction(Body body) {

    }
    public void UpdateAction() {

    }
    public void EndAction() {

    }
}
