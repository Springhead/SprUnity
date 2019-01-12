using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SprCs;

namespace SprUnity {

    public class Bone : MonoBehaviour {

        // Label to identify the role of this bone
        public string label = "";

        // Body which is owner of this bone
        public Body body = null;

        // Parent and Children of this bone in bone tree
        public Bone parent = null;
        public List<Bone> children = new List<Bone>();

        // Avatar bone related to this bone
        public GameObject avatarBone = null;

        // Springhead objects which belongs to this bone
        public PHSolidBehaviour solid = null;
        public CDShapeBehaviour shape = null;
        public PHJointBehaviour joint = null;
        public PHIKEndEffectorBehaviour ikEndEffector = null;
        public PHIKActuatorBehaviour ikActuator = null;

        // Mode of pose synchronize
        public bool syncPosition = false; // shold be true for some bones e.g.) Hips, Leg, Foot
        public bool syncRotation = true;

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        // Relative Rotation between PHSolid and Avatar Bone
        private Quaternion relativeRotSolidAvatar = Quaternion.identity;

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        // -> Move to Bone Controller
        // private double initialSpring = 0.0f;
        // private double initialDamper = 0.0f;

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        void Start() {
            // -> Move to Bone Controller
            /*
            if (joint != null) {
                PHBallJointBehaviour bj = joint as PHBallJointBehaviour;
                if (bj != null) {
                    initialSpring = bj.phBallJoint.GetSpring();
                    initialDamper = bj.phBallJoint.GetDamper();
                }

                PHHingeJointBehaviour hj = joint as PHHingeJointBehaviour;
                if (hj != null) {
                    initialSpring = hj.phHingeJoint.GetSpring();
                    initialDamper = hj.phHingeJoint.GetDamper();
                }
            }
            */
        }

        void OnDrawGizmos() {
            Gizmos.color = Color.red;

            // Draw Line
            if (parent != null && solid != null) {
                var from = solid.transform.position;
                var to = parent.solid.transform.position;
                Gizmos.DrawLine(from, to);
            }

            // Draw Joint
            if (solid != null) {
                Gizmos.DrawWireSphere(solid.transform.position, 0.01f);
            } else {
                Gizmos.DrawWireSphere(transform.position, 0.01f);
            }

            // Draw Solid
            {
                Gizmos.color = Color.blue;
                Vector3 pos;
                if (solid != null) {
                    pos = solid.transform.position + solid.transform.rotation * ((Vec3d)(solid.desc.center)).ToVector3();
                } else {
                    pos = transform.position;
                }
                Gizmos.DrawWireSphere(pos, 0.02f);
#if UNITY_EDITOR
                UnityEditor.Handles.Label(pos, label);
#endif
            }
        }

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        public void RecordRelativeRotSolidAvatar() {
            if (avatarBone != null && solid != null) {
                var so = solid.transform.rotation;
                var av = avatarBone.transform.rotation;
                relativeRotSolidAvatar = Quaternion.Inverse(so) * av;
            }
        }

        public void SyncAvatarBoneFromSolid() {
            if (avatarBone != null && solid != null) {
                if (syncPosition) {
                    avatarBone.transform.position = solid.transform.position;
                }
                if (syncRotation) {
                    avatarBone.transform.rotation = solid.transform.rotation * relativeRotSolidAvatar;
                }
            }
        }

        // -> Move to Bone Controller
        /*
        public void SetSpringDamperInRatio(Vector2 springDamperInRatio) {
            var spring = initialSpring * springDamperInRatio[0];
            var damper = initialDamper * springDamperInRatio[1];

            if (joint != null) {
                PHBallJointBehaviour bj = joint as PHBallJointBehaviour;
                if (bj != null) {
                    bj.desc.spring = spring;
                    bj.phBallJoint.SetSpring(spring);

                    bj.desc.damper = damper;
                    bj.phBallJoint.SetDamper(damper);
                }

                PHHingeJointBehaviour hj = joint as PHHingeJointBehaviour;
                if (hj != null) {
                    hj.desc.spring = spring;
                    hj.phHingeJoint.SetSpring(spring);

                    hj.desc.damper = damper;
                    hj.phHingeJoint.SetDamper(damper);
                }
            }
        }
        */

    }

}
