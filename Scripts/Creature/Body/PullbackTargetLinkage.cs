using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SprCs;
using SprUnity;

#if UNITY_EDITOR
using UnityEditor;
[CanEditMultipleObjects]
[CustomEditor(typeof(PullbackTargetLinkage))]
public class PullbackTargetLinkageEditor : Editor {
}
#endif

public class PullbackTargetLinkage : MonoBehaviour {

    public GameObject linkTarget = null;
    public GameObject coordinateOrigin = null;
    public float linkRatio = 0.0f;

    public Vector3 offsetRot = new Vector3();

    private PHIKBallActuatorBehaviour ikActuator;

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

    void Start () {
        ikActuator = GetComponent<PHIKBallActuatorBehaviour>();
	}
	
	void FixedUpdate () {
        if (ikActuator != null) {
            Quaternion targetRot = linkTarget.transform.rotation;
            Quaternion ikPullback = Quaternion.Euler(0, 0, offsetRot.z) * Quaternion.Euler(offsetRot.x, 0, 0) * Quaternion.Slerp(coordinateOrigin.transform.rotation, targetRot, linkRatio);
            ikActuator.desc.pullbackTarget = ikPullback.ToQuaterniond();
            if (ikActuator.phIKBallActuator != null) {
                ikActuator.phIKBallActuator.SetPullbackTarget(ikPullback.ToQuaterniond());
            }
        } else {
            Debug.Log(gameObject.name + " does not have ikActuator");
        }
	}
}
