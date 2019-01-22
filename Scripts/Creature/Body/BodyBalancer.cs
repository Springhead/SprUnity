using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SprCs;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SprUnity {

//#if UNITY_EDITOR
    [CustomEditor(typeof(BodyBalancer))]
    public class BodyBalancerEditor : Editor {
        
        public override void OnInspectorGUI() {
            BodyBalancer bodyBalancer = (BodyBalancer)target;

            DrawDefaultInspector();

            bodyBalancer.hipsHeight = EditorGUILayout.Slider("Hips Height", bodyBalancer.hipsHeight, -1.0f, 0.5f);
        }

    }
//#endif

    public class BodyBalancer : MonoBehaviour {

        public Body body;

        [HideInInspector]
        public float hipsHeight = 0.0f; // Relative height from initialHipsHeight

        [HideInInspector]
        public float initialHipsHeight = 0.0f;

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        private bool initialized = false;

        private Vector3 upperBodyCoM = new Vector3();
        private Vector3 targHipsPosLPF = new Vector3();

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        void Start() {
        }

        void FixedUpdate() {
            if (!initialized) {
                // <!!>
                if (body["Hips"].solid.phSolid != null) {
                    targHipsPosLPF = body["Hips"].transform.position;
                    initialHipsHeight = body["Hips"].transform.position.y;
                    body.bodyBalancer = this;
                    initialized = true;
                }
            } else {

                upperBodyCoM = CompUpperBodyCoM();

                Vector3 leftFootPos = (body["LeftFoot"].transform.ToPosed() * body["LeftFoot"].solid.desc.center).ToVector3();
                Vector3 rightFootPos = (body["RightFoot"].transform.ToPosed() * body["RightFoot"].solid.desc.center).ToVector3();
                Vector3 footCenterPos = (leftFootPos + rightFootPos) * 0.5f;
                Vector3 upperBodyCoMFloor = upperBodyCoM; upperBodyCoMFloor.y = footCenterPos.y;
                Vector3 hipsMove = footCenterPos - upperBodyCoMFloor;

                Vector3 currHipsPos = body["Hips"].transform.position;
                Vector3 targHipsPos = currHipsPos + hipsMove;

                targHipsPos.y = initialHipsHeight + hipsHeight; // <!!>

                float alpha = 0.3f;
                targHipsPosLPF = ((1 - alpha) * targHipsPosLPF) + (alpha * targHipsPos);

                if (body["Hips"].ikEndEffector != null && body["Hips"].ikEndEffector.phIKEndEffector != null) {
                    body["Hips"].ikEndEffector.phIKEndEffector.SetTargetPosition(targHipsPosLPF.ToVec3d());
                }

            }
        }

        void OnDrawGizmos() {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(upperBodyCoM, 0.1f);
        }

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        private Vector3 CompUpperBodyCoM() {
            string[] upperBodyBones = {
                "Spine",
                "Chest",
                "UpperChest",
                "Neck",
                "Head",
                "LeftShoulder",
                "LeftUpperArm",
                "LeftLowerArm",
                "LeftHand",
                "RightShoulder",
                "RightUpperArm",
                "RightLowerArm",
                "RightHand",
            };

            Vector3 CoM = new Vector3(0, 0, 0);
            float mass = 0.0f;

            foreach (var boneLabel in upperBodyBones) {
                Bone bone = body[boneLabel];
                if (bone != null) {
                    CoM += (((float)bone.solid.desc.mass) * (bone.transform.ToPosed() * bone.solid.desc.center).ToVector3());
                    mass += ((float)bone.solid.desc.mass);
                }
            }

            CoM = CoM * (1.0f / mass);

            return CoM;
        }

    }

}