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
        public bool finish = false;
        public bool abort = false;
    }

    public class ActionInvoker : MonoBehaviour {

        private static ActionInvoker instance = null;
        public static ActionInvoker GetInstance() { return instance; }

        public Body body = null;
        public List<KeyPoseSequence> keyPoseSequences = new List<KeyPoseSequence>();

        [HideInInspector]
        public KeyPoseSequence inActionSequence = null;

        // ----- ----- ----- ----- -----

        private float time = 0.0f;
        private int index = 0;

        // ----- ----- ----- ----- -----

        void Awake() {
            instance = this;
        }

        void Start() {
        }

        void Update() {
            KeyCode[] hotKeys = { KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R, KeyCode.T, KeyCode.Y, KeyCode.U, KeyCode.I, KeyCode.O, KeyCode.Z, KeyCode.X, KeyCode.C };

            for (int i = 0; i < hotKeys.Count(); i++) {
                if (Input.GetKeyDown(hotKeys[i])) {
                    if (keyPoseSequences.Count > i) {
                        Action(keyPoseSequences[i].name);
                    }
                }
            }
        }

        public float timeRatio = 0.5f;
        private void FixedUpdate() {
            if (body == null || body.initialized) {
                if (inActionSequence != null && inActionSequence.keyPoseTimings.Count() > 0) {
                    if (index < inActionSequence.keyPoseTimings.Count()) {
                        if ((inActionSequence.keyPoseTimings[index].start * timeRatio) <= time) {
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
                                duration: kp.duration * timeRatio,
                                spring: kp.springDamper.x,
                                damper: kp.springDamper.y,
                                rotate: rotate
                                );
                            index++;
                        }
                    }

                    time += Time.fixedDeltaTime;

                    if (inActionSequence.keyPoseTimings.Count() <= index && GetRemainTime() <= 0) {
                        inActionSequence.finish = true;
                        time = 0.0f;
                        index = 0;
                        inActionSequence = null;
                    }
                }
            }
        }

        // ----- ----- ----- ----- -----

        public KeyPoseSequence Action(string name, Person lookAt = null) {
            foreach (var sequence in keyPoseSequences) {
                if (sequence.name == name) {
                    if (inActionSequence != null) {
                        inActionSequence.abort = true;
                        inActionSequence.finish = true;
                    }
                    inActionSequence = sequence;
                    inActionSequence.lookAt = lookAt;
                    inActionSequence.abort = false;
                    inActionSequence.finish = false;
                    time = 0.0f;
                    index = 0;
                    return inActionSequence;
                }
            }
            return null;
        }

        public float GetRemainTime() {
            if (inActionSequence == null) { return -1e-3f; }

            var lastKeyPose = inActionSequence.keyPoseTimings.Last();
            var endTime = (lastKeyPose.start * timeRatio) + (lastKeyPose.duration * timeRatio);
            return (endTime - time);
        }

    }

}