using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;
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

    public bool isEditing;
    public bool actionEnabled;

    protected CancellationTokenSource tokenSource;
    protected CancellationToken cancelToken;

    protected Body body;

    // 編集に関してはLogをとってそれを編集することで
    // submovement(class?)の一覧
    // waitTime(class?)の一覧

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		
	}

    void OnDisable() {
        EndAction();
    }

    public void BeginAction(Body body) {

    }
    public void UpdateAction() {

    }
    public void EndAction() {

    }
}
