using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using SprUnity;
using SprCs;

[Serializable]
public class KeyPoseTiming {
    public KeyPose keyPose = null;
    public float start = 0;
    public float duration = 1;
    public Vector2 springDamper = new Vector2(1, 1);
}

[Serializable]
public class KeyPoseSequence {
    public string name = "";
    public List<KeyPoseTiming> keyPoseTimings = new List<KeyPoseTiming>();
}

public class ActionInvoker : MonoBehaviour {

    public Body body = null;
    public List<KeyPoseSequence> keyPoseSequences = new List<KeyPoseSequence>();

    [HideInInspector]
    public KeyPoseSequence inActionSequence = null;

    // ----- ----- ----- ----- -----

    private float time = 0.0f;
    private int index = 0;
        
    // ----- ----- ----- ----- -----

    void Start () {
	}
	
	void Update () {
        KeyCode[] hotKeys = { KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R, KeyCode.T, KeyCode.Y };

        for (int i=0; i<hotKeys.Count(); i++) {
            if (Input.GetKeyDown(hotKeys[i])) {
                if (keyPoseSequences.Count > i) {
                    time = 0.0f;
                    index = 0;
                    inActionSequence = keyPoseSequences[i];
                }
            }
        }
    }

    private void FixedUpdate() {
        if (body == null || body.initialized) {
            if (inActionSequence != null) {
                if (inActionSequence.keyPoseTimings[index].start <= time) {
                    var kp = inActionSequence.keyPoseTimings[index];
                    kp.keyPose.Action(
                        body: body,
                        startTime: 0,
                        duration: kp.duration,
                        spring: kp.springDamper.x,
                        damper: kp.springDamper.y
                        );
                    index++;
                }

                time += Time.fixedDeltaTime;

                if (inActionSequence.keyPoseTimings.Count() <= index) {
                    time = 0.0f;
                    index = 0;
                    inActionSequence = null;
                }
            }
        }
    }

    // ----- ----- ----- ----- -----

    public void Action(string name) {
        print("Action: " + name);
        foreach(var sequence in keyPoseSequences) {
            if (sequence.name == name) {
                inActionSequence = sequence;
                time = 0.0f;
                index = 0;
            }
        }
    }

}
