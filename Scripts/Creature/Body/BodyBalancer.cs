using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SprCs;

namespace SprUnity {

    public class BodyBalancer : MonoBehaviour {

        public Body body;

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        private Vector3 upperBodyCoM = new Vector3();
        private Vector3 targHipsPosLPF = new Vector3();

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        void Start() {
            targHipsPosLPF = body["Hips"].transform.position; // <!!>
        }

        void FixedUpdate() {
            upperBodyCoM = CompUpperBodyCoM();

            Vector3 footCenterPos = (body["LeftFoot"].transform.position + body["RightFoot"].transform.position) * 0.5f;
            Vector3 upperBodyCoMFloor = upperBodyCoM; upperBodyCoMFloor.y = footCenterPos.y;
            Vector3 hipsMove = footCenterPos - upperBodyCoMFloor;

            Vector3 currHipsPos = body["Hips"].transform.position;
            Vector3 targHipsPos = currHipsPos + hipsMove;

            float alpha = 0.3f;
            targHipsPosLPF = ((1 - alpha) * targHipsPosLPF) + (alpha * targHipsPos);

            if (body["Hips"].ikEndEffector != null && body["Hips"].ikEndEffector.phIKEndEffector != null) {
                body["Hips"].ikEndEffector.phIKEndEffector.SetTargetPosition(targHipsPosLPF.ToVec3d());
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