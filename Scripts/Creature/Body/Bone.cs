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

        // Bone Controller
        public BoneController controller = null;

        // Spring and Damper Ratio
        public float springRatio = 1.0f;
        public float damperRatio = 1.0f;

        // Mode of pose synchronize
        public bool syncPosition = false; // shold be true for some bones e.g.) Hips, Leg, Foot
        public bool syncRotation = true;

        // Automatically remove this if corresponding avatar bone is missing (by Body)
        public bool removeIfNotInAvatar = false;

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        // Relative Rotation between PHSolid and Avatar Bone

        // SerializeFieldされていない変数が破棄される
        [SerializeField]
        private Quaternion relativeRotSolidAvatar = Quaternion.identity;

        // Initial Spring and Damper
        private double initialSpring = 0.0f;
        private double initialDamper = 0.0f;

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        void Start() {
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

            if (parent != null && ikActuator?.phIKActuator != null && parent?.ikActuator?.phIKActuator != null) {
                {
                    Gizmos.color = Color.white;
                    var from = ikActuator.phIKActuator.GetSolidPullbackPose().Pos().ToVector3();
                    var to = parent.ikActuator.phIKActuator.GetSolidPullbackPose().Pos().ToVector3();
                    Gizmos.DrawLine(from, to);
                }
                {
                    Gizmos.color = Color.green;
                    var from = ikActuator.phIKActuator.GetSolidTempPose().Pos().ToVector3();
                    var to = parent.ikActuator.phIKActuator.GetSolidTempPose().Pos().ToVector3();
                    Gizmos.DrawLine(from, to);
                }
            }

            // Draw Solid
            {
                Gizmos.color = Color.blue;
                Vector3 pos;
                if (solid != null && solid.desc != null) {
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

        void FixedUpdate() {
            if (!body.initialized) { return; }

            if (controller != null && controller.bone == null) { controller.bone = this; }

            // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

            // Change Spring and Damper
            var spring = initialSpring * springRatio;
            var damper = initialDamper * damperRatio;

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

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        public void RecordRelativeRotSolidAvatar() {
            if (avatarBone != null) {
                if (solid != null) {
                    var so = solid.transform.rotation;
                    var av = avatarBone.transform.rotation;
                    relativeRotSolidAvatar = Quaternion.Inverse(so) * av;
                } else {
                    var so = transform.rotation;
                    var av = avatarBone.transform.rotation;
                    relativeRotSolidAvatar = Quaternion.Inverse(so) * av;
                }
            }
        }

        public void SaveInitialSpringDamper() {
            if (solid != null && solid.phSolid != null) {
                // Get Initial Spring and Damper
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
            }
        }

        public void InitializeController() {
            // Initialize Bone Controller
            if (controller != null) {
                controller.Initialize();
            }
        }

        public void SyncAvatarBoneFromSolid() {
            if (avatarBone != null) {
                if (solid != null) {
                    if (syncPosition) {
                        avatarBone.transform.position = solid.transform.position;
                    }
                    if (syncRotation) {
                        avatarBone.transform.rotation = solid.transform.rotation * relativeRotSolidAvatar;
                    }
                } else {
                    if (syncPosition) {
                        avatarBone.transform.position = transform.position;
                    }
                    if (syncRotation) {
                        avatarBone.transform.rotation = transform.rotation * relativeRotSolidAvatar;
                    }
                }
            }
        }

        public void SyncSolidFromAvatarBone() {
            if (avatarBone != null) {
                if(joint != null) { 
                }
            }
        }

    }

}
