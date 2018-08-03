using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SprCs;
using SprUnity;

public class PullbackTargetLinkage : MonoBehaviour {

    public GameObject linkTarget = null;
    public GameObject coordinateOrigin = null;
    public float linkRatio = 0.0f;

    private PHIKBallActuatorBehaviour ikActuator;

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

    void Start () {
        ikActuator = GetComponent<PHIKBallActuatorBehaviour>();
	}
	
	void FixedUpdate () {
        Quaterniond ikPullback = Quaternion.Slerp(coordinateOrigin.transform.rotation, linkTarget.transform.rotation, linkRatio).ToQuaterniond();
        ikActuator.desc.pullbackTarget = ikPullback;
        ikActuator.phIKBallActuator.SetPullbackTarget(ikPullback);
	}
}
