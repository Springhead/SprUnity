using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using SprCs;

namespace SprUnity {

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
        public Person lookAt = null;
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

        void Start() {
        }

        void Update() {
            KeyCode[] hotKeys = { KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R, KeyCode.T, KeyCode.Y, KeyCode.U, KeyCode.I, KeyCode.O, KeyCode.P };

            for (int i = 0; i < hotKeys.Count(); i++) {
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
                if (inActionSequence != null && inActionSequence.keyPoseTimings.Count() > 0) {
                    if (inActionSequence.keyPoseTimings[index].start <= time) {
                        Quaternion rotate = Quaternion.identity;
                        if (inActionSequence.lookAt != null) {
                            Vector3 lookDir = inActionSequence.lookAt.head.transform.position - body["Hips"].transform.position;
                            lookDir.y = 0; lookDir.Normalize();
                            Quaternion lookRotation = Quaternion.FromToRotation(Vector3.forward, lookDir);
                            rotate = Quaternion.Slerp(Quaternion.identity, lookRotation, 1);
                        }

                        var kp = inActionSequence.keyPoseTimings[index];
                        kp.keyPose.Action(
                            body: body,
                            startTime: 0,
                            duration: kp.duration,
                            spring: kp.springDamper.x,
                            damper: kp.springDamper.y,
                            rotate: rotate
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

        public void Action(string name, Person lookAt = null) {
            foreach (var sequence in keyPoseSequences) {
                if (sequence.name == name) {
                    inActionSequence = sequence;
                    inActionSequence.lookAt = lookAt;
                    time = 0.0f;
                    index = 0;
                }
            }
        }

    }

}