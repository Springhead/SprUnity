using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionAnimator : MonoBehaviour {

    public InteraWare.Body body;
    public ActionStateMachine stateMachine;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        stateMachine.UpdateStateMachine(body);
	}
}
