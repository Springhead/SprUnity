using System.Collections;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MeshRoundCone : MonoBehaviour {

    public float length = 2.0f;
    public float r1 = 0.5f;
    public float r2 = 1.0f;
    public enum Pivot { Center, R1, R2 };
    public Pivot pivot = Pivot.Center;

    public bool usePositionR1 = false;
    public Vector3 positionR1 = new Vector3();
    public bool usePositionR2 = false;
    public Vector3 positionR2 = new Vector3();

    public void UpdateR1R2HandlePosition() {
        if (pivot == Pivot.Center) {
            positionR1 = transform.TransformPoint(new Vector3(length * -0.5f, 0, 0));
            positionR2 = transform.TransformPoint(new Vector3(length * +0.5f, 0, 0));
        } else if (pivot == Pivot.R1) {
            positionR1 = transform.position;
            positionR2 = transform.TransformPoint(new Vector3(+length, 0, 0));
        } else {
            positionR1 = transform.TransformPoint(new Vector3(-length, 0, 0));
            positionR2 = transform.position;
        }
    }

    public void Reposition() {
        transform.rotation = Quaternion.FromToRotation(Vector3.right, (positionR2 - positionR1).normalized);
        if (pivot == Pivot.Center) {
            transform.position = (positionR1 + positionR2) * 0.5f;
        } else if (pivot == Pivot.R1) {
            transform.position = positionR1;
        } else if (pivot == Pivot.R2) {
            transform.position = positionR2;
        }
        float scale = (transform.lossyScale.x + transform.lossyScale.y + transform.lossyScale.z) / 3.0f;
        length = (positionR1 - positionR2).magnitude / scale;
    }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

    [HideInInspector]
    public int split = 32;
    [HideInInspector]
    public int slice = 16;

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

    private static Material default_material = null;

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

    private void Reset() {
        CreateMesh();
        UpdateR1R2HandlePosition();
    }

    private void OnValidate() {
        /*
        if (!usePositionR1) {
            if (pivot == Pivot.Center) {
                positionR1 = transform.TransformPoint(new Vector3(length * -0.5f, 0, 0));
            }
        }
        if (!usePositionR2) {
            if (pivot == Pivot.Center) {
                positionR2 = transform.TransformPoint(new Vector3(length * +0.5f, 0, 0));
            }
        }
        */
        Reshape();
    }

#if UNITY_EDITOR
    [MenuItem("GameObject/3D Object/Round Cone")]
    public static void OnMenu() {
        GameObject roundCone = new GameObject("RoundCone");
        roundCone.AddComponent<MeshRoundCone>();
        Selection.activeGameObject = roundCone;
    }
#endif

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

    public void CreateMesh() {
        int sphereVtxs = split * (slice + 1);
        int sphereTris = 2 * split * slice;

        int coneVtxs = split * 2;
        int coneTris = split * 2;

        Vector3[] vertices = new Vector3[2 * sphereVtxs + coneVtxs];
        int[] triangles = new int[(2 * sphereTris + coneTris) * 3];

        // Make Triangles
        /// -- Sphere
        int cnt = 0;
        for (int n = 0; n < 2; n++) {
            for (int i = 0; i < split; i++) {
                for (int j = 0; j < slice; j++) {
                    int i0 = i;
                    int i1 = (i == split - 1) ? 0 : i + 1;

                    triangles[cnt + 0] = split * (j + 0) + i1 + n * sphereVtxs;
                    triangles[cnt + 1] = split * (j + 1) + i0 + n * sphereVtxs;
                    triangles[cnt + 2] = split * (j + 0) + i0 + n * sphereVtxs;
                    cnt += 3;

                    triangles[cnt + 0] = split * (j + 0) + i1 + n * sphereVtxs;
                    triangles[cnt + 1] = split * (j + 1) + i1 + n * sphereVtxs;
                    triangles[cnt + 2] = split * (j + 1) + i0 + n * sphereVtxs;
                    cnt += 3;
                }
            }
        }
        /// -- Cone
        for (int i = 0; i < split; i++) {
            int i0 = i;
            int i1 = (i == split - 1) ? 0 : i + 1;

            triangles[cnt + 0] = split * 0 + i0 + 2 * sphereVtxs;
            triangles[cnt + 1] = split * 1 + i0 + 2 * sphereVtxs;
            triangles[cnt + 2] = split * 0 + i1 + 2 * sphereVtxs;
            cnt += 3;

            triangles[cnt + 0] = split * 1 + i0 + 2 * sphereVtxs;
            triangles[cnt + 1] = split * 1 + i1 + 2 * sphereVtxs;
            triangles[cnt + 2] = split * 0 + i1 + 2 * sphereVtxs;
            cnt += 3;
        }

        // Create Mesh and Related Components

        Mesh mesh = new Mesh();
        mesh.name = "RoundCone";

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
        if (meshRenderer == null) { meshRenderer = gameObject.AddComponent<MeshRenderer>(); }

        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
        if (meshFilter == null) { meshFilter = gameObject.AddComponent<MeshFilter>(); }

        meshFilter.sharedMesh = mesh;

        if (default_material == null) {
            foreach (var i in Resources.FindObjectsOfTypeAll<Material>()) {
                if (i.name == "Default-Material") { default_material = i; break; }
            }
        }

        meshRenderer.sharedMaterial = default_material;
    }

    public void Reshape() {
        // Avoid Invalid Value
        if (length <= 0) { length = 1e-5f; }
        if (r1 < 0) { r1 = 0; }
        if (r2 < 0) { r2 = 0; }

        // ----- ----- ----- ----- -----

        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
        if (meshFilter == null) {
            CreateMesh();
            meshFilter = gameObject.GetComponent<MeshFilter>();
        }

        Mesh mesh = meshFilter.sharedMesh;
        if (mesh == null) {
            CreateMesh();
            mesh = meshFilter.sharedMesh;
        }

        // ----- ----- ----- ----- -----

        Vector3[] vertices = mesh.vertices;

        int sphereVtxs = split * (slice + 1);
        int sphereTris = 2 * split * slice;

        int coneVtxs = split * 2;
        int coneTris = split * 2;

        float offset1, offset2;
        if (pivot == Pivot.Center) {
            offset1 = -0.5f * length;
            offset2 = +0.5f * length;
        } else if (pivot == Pivot.R1) {
            offset1 = 0;
            offset2 = length;
        } else {
            offset1 = -length;
            offset2 = 0;
        }

        // ----- ----- ----- ----- -----

        // Make Vertices
        /// -- Sphere
        for (int n = 0; n < 2; n++) {
            float r = (n == 0) ? r1 : r2;
            float c = (n == 0) ? offset1 : offset2;
            for (int j = 0; j < slice + 1; j++) {
                for (int i = 0; i < split; i++) {
                    vertices[(split * j + i) + n * sphereVtxs].x = r * Mathf.Cos(Mathf.Deg2Rad * 180.0f * j / slice) + c;
                    vertices[(split * j + i) + n * sphereVtxs].y = r * Mathf.Sin(Mathf.Deg2Rad * 360.0f * i / split) * Mathf.Sin(Mathf.Deg2Rad * 180.0f * j / slice);
                    vertices[(split * j + i) + n * sphereVtxs].z = r * Mathf.Cos(Mathf.Deg2Rad * 360.0f * i / split) * Mathf.Sin(Mathf.Deg2Rad * 180.0f * j / slice);
                }
            }
        }
        /// -- Cone
        float cr1 = r1, cr2 = r2;
        float cx1 = offset1, cx2 = offset2;
        if (r1 > r2) {
            float cos = (r1 - r2) / length;
            cx1 += r1 * cos;
            cx2 += r2 * cos;
            cr1 = r1 * Mathf.Sqrt(1 - cos * cos);
            cr2 = r2 * Mathf.Sqrt(1 - cos * cos);
        } else {
            float cos = (r2 - r1) / length;
            cx1 -= r1 * cos;
            cx2 -= r2 * cos;
            cr1 = r1 * Mathf.Sqrt(1 - cos * cos);
            cr2 = r2 * Mathf.Sqrt(1 - cos * cos);
        }
        for (int n = 0; n < 2; n++) {
            float r = (n == 0) ? cr1 : cr2;
            float c = (n == 0) ? cx1 : cx2;
            for (int i = 0; i < split; i++) {
                vertices[(split * n + i) + (2 * sphereVtxs)].x = c;
                vertices[(split * n + i) + (2 * sphereVtxs)].y = r * Mathf.Sin(Mathf.Deg2Rad * 360.0f * i / split);
                vertices[(split * n + i) + (2 * sphereVtxs)].z = r * Mathf.Cos(Mathf.Deg2Rad * 360.0f * i / split);
            }
        }

        // ----- ----- ----- ----- -----

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

}