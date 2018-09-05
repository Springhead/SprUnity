using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InteraWare {

    public class EyeByTexture : MonoBehaviour {
        public Body body = null;
        public GameObject eye = null;
        public int eyeMaterialId = 0;
        public List<string> textures = new List<string>();
        public Vector2 uvRatio = new Vector2(0.3f, 0.3f);

        // Use this for initialization
        void Start() {
            if (textures.Count == 0) { textures.Add("_MainTex"); }
        }

        // Update is called once per frame
        void FixedUpdate() {
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

                var angle = (angleL + angleR) * 0.5f;

                float horiz = ((angle.x > 180.0f) ? (angle.x - 360.0f) : angle.x) / 90.0f;
                float verti = ((angle.y > 180.0f) ? (angle.y - 360.0f) : angle.y) / 90.0f;

                horiz = Mathf.Clamp(horiz, -1, 1);
                verti = Mathf.Clamp(verti, -1, 1);

                foreach (var textureName in textures) {
                    eye.GetComponent<Renderer>().materials[eyeMaterialId].SetTextureOffset(textureName, new Vector2(-horiz * uvRatio.x, -verti * uvRatio.y));
                }
            }
        }
    }

}