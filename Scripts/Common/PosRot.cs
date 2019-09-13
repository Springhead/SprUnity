using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VGent{
    [System.Serializable]
    public class PosRot {
        public PosRot() { }
        public PosRot(GameObject g) { position = g.transform.position; rotation = g.transform.rotation; }
        public PosRot(Transform t) { position = t.position; rotation = t.rotation; }
        public PosRot(Vector3 p, Quaternion r) { position = p; rotation = r; }
        public PosRot(PosRot p) { position = p.position; rotation = p.rotation; }
        public PosRot(float px, float py, float pz, float rx, float ry, float rz) { position = new Vector3(px, py, pz); rotation = Quaternion.Euler(rx, ry, rz); }

        public Vector3 position = new Vector3();
        public Quaternion rotation = Quaternion.identity;

        public void SetTo(GameObject g) { g.transform.position = position; g.transform.rotation = rotation; }
        public void SetTo(Transform t) { t.position = position; t.rotation = rotation; }

        public Vector3 InverseTransformPoint(Vector3 point) {
            point -= position;
            point = Quaternion.Inverse(rotation) * point;
            return point;
        }

        public Vector3 TransformPoint(Vector3 point) {
            point = rotation * point;
            point += position;
            return point;
        }

        public PosRot TransformPosRot(PosRot posrot) {
            PosRot result = new PosRot();
            result.position = position + rotation * posrot.position;
            result.rotation = rotation * posrot.rotation;
            return result;
        }

        public PosRot Inverse() {
            PosRot result = new PosRot();
            result.rotation = Quaternion.Inverse(rotation);
            result.position = -(result.rotation * position);
            return result;
        }

        // ----- ----- ----- ----- -----

        public override string ToString() {
            string str = "";
            str += (position.x + "," + position.y + "," + position.z + ",");
            str += (rotation.x + "," + rotation.y + "," + rotation.z + "," + rotation.w);
            return str;
        }

        public static PosRot Parse(string line) {
            var data = line.Split(',').Select(s => float.Parse(s));
            PosRot pose = new PosRot();
            pose.position = new Vector3(data.ElementAt(0), data.ElementAt(1), data.ElementAt(2));
            pose.rotation = new Quaternion(data.ElementAt(3), data.ElementAt(4), data.ElementAt(5), data.ElementAt(6));
            return pose;
        }

        // ----- ----- ----- ----- -----

        public static PosRot Interpolate(PosRot px0, PosRot px1, float x) {
            return new PosRot(
                    px1.position * x + px0.position * (1 - x),
                    Quaternion.Slerp(px0.rotation, px1.rotation, x)
                );
        }

        public static PosRot Interpolate(PosRot px0y0, PosRot px1y0, PosRot px0y1, PosRot px1y1, float x, float y) {
            PosRot py0 = Interpolate(px0y0, px1y0, x);
            PosRot py1 = Interpolate(px0y1, px1y1, x);
            return Interpolate(py0, py1, y);
        }

        public static PosRot Rotate(PosRot px0, Quaternion rotation, Vector3 center) {
            PosRot px1 = new PosRot();
            px1.position = rotation * (px0.position - center) + center;
            px1.rotation = rotation * px0.rotation;
            return px1;
        }
    }

    [System.Serializable]
    public class PosRotScale {
        public PosRotScale() { }
        public PosRotScale(GameObject g) { position = g.transform.position; rotation = g.transform.rotation; scale = g.transform.lossyScale; }
        public PosRotScale(Transform t) { position = t.position; rotation = t.rotation; scale = t.lossyScale; }
        public PosRotScale(Vector3 p, Quaternion r, Vector3 s) { position = p; rotation = r; scale = s; }
        public PosRotScale(PosRotScale p) { position = p.position; rotation = p.rotation; scale = p.scale; }
        public PosRotScale(float px, float py, float pz, float rx, float ry, float rz, float sx, float sy, float sz) { position = new Vector3(px, py, pz); rotation = Quaternion.Euler(rx, ry, rz); scale = new Vector3(sx, sy, sz); }

        public Vector3 position = new Vector3();
        public Quaternion rotation = Quaternion.identity;
        public Vector3 scale = new Vector3(1, 1, 1);
        public Vector3 scaleInv {
            get { return new Vector3(1 / scale.x, 1 / scale.y, 1 / scale.z); }
        }

        //<!!>
        public void SetTo(GameObject g) { g.transform.position = position; g.transform.rotation = rotation; }
        public void SetTo(Transform t) { t.position = position; t.rotation = rotation; }

        public Vector3 InverseTransformPoint(Vector3 point) {
            point -= position;
            point = Quaternion.Inverse(rotation) * Vector3.Scale(scaleInv, point);
            return point;
        }

        public Vector3 TransformPoint(Vector3 point) {
            point = rotation * Vector3.Scale(scale, point);
            point += position;
            return point;
        }

        public PosRotScale TransformPosRotScale(PosRotScale posrotscale) {
            PosRotScale result = new PosRotScale();
            result.position = position + rotation * Vector3.Scale(scale, posrotscale.position);
            result.rotation = (rotation * posrotscale.rotation).normalized;
            result.scale = Vector3.Scale(scale, posrotscale.scale);
            return result;
        }

        public PosRotScale InverseTransformPosRotScale(PosRotScale posrotscale) {
            PosRotScale result = new PosRotScale();
            result.position = Quaternion.Inverse(rotation) * Vector3.Scale(scaleInv, (posrotscale.position - position));
            result.rotation = (Quaternion.Inverse(rotation) * posrotscale.rotation).normalized;
            result.scale = Vector3.Scale(scaleInv, posrotscale.scale);
            return result;
        }

        public PosRotScale Inverse() {
            PosRotScale result = new PosRotScale();
            result.rotation = Quaternion.Inverse(rotation);
            result.scale = scaleInv;
            result.position = -(rotation * Vector3.Scale(scale, position));
            return result;
        }

        // ----- ----- ----- ----- -----

        public override string ToString() {
            string str = "";
            str += (position.x + "," + position.y + "," + position.z + ",");
            str += (rotation.x + "," + rotation.y + "," + rotation.z + "," + rotation.w + ",");
            str += (scale.x + "," + scale.y + "," + scale.z);
            return str;
        }

        public static PosRotScale Parse(string line) {
            var data = line.Split(',').Select(s => float.Parse(s));
            PosRotScale pose = new PosRotScale();
            pose.position = new Vector3(data.ElementAt(0), data.ElementAt(1), data.ElementAt(2));
            pose.rotation = new Quaternion(data.ElementAt(3), data.ElementAt(4), data.ElementAt(5), data.ElementAt(6));
            pose.scale = new Vector3(data.ElementAt(7), data.ElementAt(8), data.ElementAt(9));
            return pose;
        }

        // ----- ----- ----- ----- -----

        public static PosRotScale Interpolate(PosRotScale px0, PosRotScale px1, float x) {
            return new PosRotScale(
                    px1.position * x + px0.position * (1 - x),
                    Quaternion.Slerp(px0.rotation, px1.rotation, x),
                    px1.scale * x + px0.scale * (1 - x)
                );
        }
    }

    
}