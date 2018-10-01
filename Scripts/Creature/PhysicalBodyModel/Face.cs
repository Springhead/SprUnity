using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InteraWare {

    public class Face : MonoBehaviour {
        public Body body = null;
        public GameObject eye = null;

        public GameObject leftLowerEye = null;
        public GameObject leftUpperEye = null;
        public GameObject rightLowerEye = null;
        public GameObject rightUpperEye = null;

        // ----- ----- ----- ----- -----

        public Vector3 lowerCoeff = new Vector3(0.0002f, 0.0003f, 0.44f);
        public Vector3 lowerLimitMin = new Vector3(-0.05f, -0.05f, 0);
        public Vector3 lowerLimitMax = new Vector3(+0.05f, +0.05f, 0.01f);

        public Vector3 upperCoeff = new Vector3(0.0002f, 0.0002f, 0.44f);
        public Vector3 upperLimitMin = new Vector3(-0.05f, -0.05f, 0);
        public Vector3 upperLimitMax = new Vector3(+0.05f, +0.05f, 0.01f);

        // ----- ----- ----- ----- -----

        public float blinkClose = 0.0f;
        public float smileClose = 0.0f;

        // ----- ----- ----- ----- -----

        public GameObject eyeBlinkTarget = null;
        public GameObject eyeSmileTarget = null;

        // ----- ----- ----- ----- -----

        private Vector3 leftLowerEyeBasePos = new Vector3();
        private Vector3 leftUpperEyeBasePos = new Vector3();
        private Vector3 rightLowerEyeBasePos = new Vector3();
        private Vector3 rightUpperEyeBasePos = new Vector3();

        void Start() {
            leftLowerEyeBasePos = leftLowerEye.transform.localPosition;
            leftUpperEyeBasePos = leftUpperEye.transform.localPosition;
            rightLowerEyeBasePos = rightLowerEye.transform.localPosition;
            rightUpperEyeBasePos = rightUpperEye.transform.localPosition;
        }

        void FixedUpdate() {
            MoveEyelidByEyeMovement();
        }

        void MoveEyelidByEyeMovement() {
            if (body != null && eye != null) {
                var eyeRotationL = Quaternion.Inverse(body["Head"].gameObject.transform.rotation) * body["LeftEye"].gameObject.transform.rotation;
                var eyeDirectionL = eyeRotationL * Vector3.forward;
                var eyeDirectionLHoriz = eyeDirectionL; eyeDirectionLHoriz.y = 0; eyeDirectionLHoriz.Normalize();
                var eyeDirectionLVerti = eyeDirectionL; eyeDirectionLVerti.x = 0; eyeDirectionLVerti.Normalize();
                var angleL = new Vector2(Vector3.SignedAngle(eyeDirectionLHoriz, Vector3.forward, Vector3.up), Vector3.SignedAngle(eyeDirectionLVerti, Vector3.forward, Vector3.right));

                var eyeRotationR = Quaternion.Inverse(body["Head"].gameObject.transform.rotation) * body["RightEye"].gameObject.transform.rotation;
                var eyeDirectionR = eyeRotationL * Vector3.forward;
                var eyeDirectionRHoriz = eyeDirectionR; eyeDirectionRHoriz.y = 0; eyeDirectionRHoriz.Normalize();
                var eyeDirectionRVerti = eyeDirectionR; eyeDirectionRVerti.x = 0; eyeDirectionRVerti.Normalize();
                var angleR = new Vector2(Vector3.SignedAngle(eyeDirectionRHoriz, Vector3.forward, Vector3.up), Vector3.SignedAngle(eyeDirectionRVerti, Vector3.forward, Vector3.right));

                // ----- ----- ----- ----- -----

                if (eyeBlinkTarget != null) {
                    blinkClose = eyeBlinkTarget.transform.localPosition.y;
                }
                if (eyeSmileTarget != null) {
                    smileClose = eyeSmileTarget.transform.localPosition.y;
                }

                // ----- ----- ----- ----- -----

                var lowerBlinkCloseOffset = new Vector3(0, 0.01f, 0);
                var lowerSmileCloseOffset = new Vector3(0, 0.025f, 0.00375f);

                var upperBlinkCloseOffset = new Vector3(0, -0.03f, 0);
                var upperSmileCloseOffset = new Vector3(0, -0.015f, 0.00375f);

                float s = 1.0f;
                if (blinkClose + smileClose > 1) {
                    s = (blinkClose + smileClose);
                }

                Vector3 lowerCloseOffset = (blinkClose * lowerBlinkCloseOffset + smileClose * lowerSmileCloseOffset) * (1 / s);
                Vector3 upperCloseOffset = (blinkClose * upperBlinkCloseOffset + smileClose * upperSmileCloseOffset) * (1 / s);

                Vector3 leftLowerEyeOffset = new Vector3(
                    Mathf.Clamp(-angleL.x * lowerCoeff.x, lowerLimitMin.x, lowerLimitMax.x),
                    Mathf.Clamp(+angleL.y * lowerCoeff.y, lowerLimitMin.y, lowerLimitMax.y),
                    Mathf.Clamp(+angleL.y * lowerCoeff.y * lowerCoeff.z, lowerLimitMin.z, lowerLimitMax.z)
                    );
                leftLowerEye.transform.localPosition = leftLowerEyeBasePos + leftLowerEyeOffset + lowerCloseOffset;

                Vector3 leftUpperEyeOffset = new Vector3(
                    Mathf.Clamp(-angleL.x * upperCoeff.x, upperLimitMin.x, upperLimitMax.x),
                    Mathf.Clamp(+angleL.y * upperCoeff.y, upperLimitMin.y, upperLimitMax.y),
                    Mathf.Clamp(+angleL.y * upperCoeff.y * upperCoeff.z, upperLimitMin.z, upperLimitMax.z)
                    );
                leftUpperEye.transform.localPosition = leftUpperEyeBasePos + leftUpperEyeOffset + upperCloseOffset;

                Vector3 rightLowerEyeOffset = new Vector3(
                    Mathf.Clamp(-angleR.x * lowerCoeff.x, lowerLimitMin.x, lowerLimitMax.x),
                    Mathf.Clamp(+angleR.y * lowerCoeff.y, lowerLimitMin.y, lowerLimitMax.y),
                    Mathf.Clamp(+angleR.y * lowerCoeff.y * lowerCoeff.z, lowerLimitMin.z, lowerLimitMax.z)
                    );
                rightLowerEye.transform.localPosition = rightLowerEyeBasePos + rightLowerEyeOffset + lowerCloseOffset;

                Vector3 rightUpperEyeOffset = new Vector3(
                    Mathf.Clamp(-angleR.x * upperCoeff.x, upperLimitMin.x, upperLimitMax.x),
                    Mathf.Clamp(+angleR.y * upperCoeff.y, upperLimitMin.y, upperLimitMax.y),
                    Mathf.Clamp(+angleR.y * upperCoeff.y * upperCoeff.z, upperLimitMin.z, upperLimitMax.z)
                    );
                rightUpperEye.transform.localPosition = rightUpperEyeBasePos + rightUpperEyeOffset + upperCloseOffset;
            }
        }
    }

}