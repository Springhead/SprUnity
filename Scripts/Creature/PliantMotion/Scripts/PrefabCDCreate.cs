using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

using System.IO;
using SprCs;
using SprUnity;

using UnityEngine;
using InteraWare;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(PrefabCDCreate))]
public class PrefabCDEditor : Editor {
    public override void OnInspectorGUI() {
        PrefabCDCreate prefab = (PrefabCDCreate)target;

        // ----- ----- ----- ----- -----

        DrawDefaultInspector();

        // ----- ----- ----- ----- -----

        if (GUILayout.Button("Setup From Bones")) {
            prefab.SetupFromBones();
        }
        if (GUILayout.Button("Save RoundCone parameter")) {
            if (prefab.read_file) {
                prefab.Save();
            }
        }
        if (GUILayout.Button("Load RoundCone parameter")) {
            if (prefab.read_file) {
                prefab.Load();
            }
        }
        if (GUILayout.Button("Mesh Renderer On")) {
            prefab.EnMeshRendereron();
        }
        if (GUILayout.Button("Mesh Renderer Off")) {
            prefab.EnMeshRendereroff();
        }
        if (GUILayout.Button("SetupFromLeftBody")) {
            prefab.SetupFromLeftBody();
        }
    }
}
#endif
public class PrefabCDCreate : MonoBehaviour {
    private Bone[] Bones;
    private static List<MeshRoundCone> MeshRoundCones;
    public bool is_destory = false;
    static Material default_material = null;
    public string filename = "MeshRoundCones.txt";
    public bool read_file = true;
    // Use this for initialization
    void Start() {
    }

    // Update is called once per frame
    void Update() {

    }
// これ複数に対応できない
    public void SetupFromBones() {
        Bones = GetComponentsInChildren<Bone>();
        GameObject child = null;
        MeshRoundCones = new List<MeshRoundCone>();
        List<CDRoundConeBehavior> deletelist = new List<CDRoundConeBehavior>();
        for (int i = 0; i < Bones.Length; i++) {
            foreach (CDRoundConeBehavior cb in Bones[i].GetComponents<CDRoundConeBehavior>()) { //CDRoundConeBehaiviour削除
                deletelist.Add(cb);
            }
        }

        foreach (var obj in deletelist) { //MissingReferenceExceptionが消せない
            DestroyImmediate(obj);
        }

        for (int i = 0; i < Bones.Length; i++) {
            for (int j = 0; j < Bones[i].transform.childCount; j++) {
                if (Bones[i].GetComponent<PHJointBehaviour>() != null) {
                    Bones[i].GetComponent<PHJointBehaviour>().disableCollision = true;
                }
                child = Bones[i].transform.GetChild(j).gameObject; //おそらく人の構造が腰から膝に直接行くのではなくワンクッションはサムのので大丈夫
                if (child == null || child.GetComponent<Bone>() == null) {
                    continue;
                }
                //MeshRoundCones[i] = Instantiate(RoundCone, Bones[i].transform, false); //複製すると片方動かしたらもう片方も変わる、なぜか一つしか作れん
                var cdMeshRoundCones= GetComponentsInChildren<MeshRoundCone>();
                foreach (var cone in cdMeshRoundCones) {
                    if(cone.gameObject.name == Bones[i].name + child.name + "RoundConeMesh") {
                        DestroyImmediate(cone.gameObject);
                    }
                }
                if ((Bones[i].gameObject.name == "Head" && child.name == "LeftEye") ||
                    (Bones[i].gameObject.name == "Head" && child.name == "RightEye") ||
                    (Bones[i].gameObject.name == "UpperChest" && child.name == "Neck") ||
                    (Bones[i].gameObject.name == "Hips" && child.name == "LeftUpperLeg") ||
                    (Bones[i].gameObject.name == "Hips" && child.name == "RightUpperLeg") ||
                    (Bones[i].gameObject.name == "UpperChest" && child.name == "LeftShoulder") ||
                    (Bones[i].gameObject.name == "UpperChest" && child.name == "RightShoulder") ||
                    Bones[i].gameObject.name == "LeftHand"||
                    Bones[i].gameObject.name == "RightHand"
                    ) //無視されるボーンたち
                {
                    continue;
                }
                if (!is_destory) {
                    GameObject newMeshRoundCone = new GameObject();
                    newMeshRoundCone.name = Bones[i].name + child.name + "RoundConeMesh";
                    newMeshRoundCone.transform.parent = Bones[i].transform;
                    newMeshRoundCone.transform.localPosition = new Vector3(0, 0, 0);
                    CDRoundConeBehavior cdrcb = Bones[i].gameObject.AddComponent<CDRoundConeBehavior>();
                    cdrcb.shapeObject = newMeshRoundCone;
                    MeshRenderer mr = newMeshRoundCone.AddComponent<MeshRenderer>();
                    MeshFilter mf = newMeshRoundCone.AddComponent<MeshFilter>();
                    MeshRoundCone mrc = newMeshRoundCone.AddComponent<MeshRoundCone>();
                    newMeshRoundCone.GetComponent<MeshRoundCone>().r1 = 0.03f; //R1が始点(Bones[i]のtransform
                    newMeshRoundCone.GetComponent<MeshRoundCone>().r2 = 0.03f; //R2が終点(Bones[i].children(ボーンあり)のtransform
                    foreach(var bone in newMeshRoundCone.GetComponentsInParent<Bone>()) {
                        if(bone.name == "LeftHand" || bone.name == "RightHand") {
                            newMeshRoundCone.GetComponent<MeshRoundCone>().r1 = 0.007f; //R1が始点(Bones[i]のtransform
                            newMeshRoundCone.GetComponent<MeshRoundCone>().r2 = 0.007f; //R1が始点(Bones[i]のtransform
                        }
                    }
                    newMeshRoundCone.GetComponent<MeshRoundCone>().pivot = MeshRoundCone.Pivot.R1;
                    newMeshRoundCone.GetComponent<MeshRoundCone>().length = Vector3.Distance(Bones[i].transform.position, child.transform.position);
                    newMeshRoundCone.GetComponent<MeshRoundCone>().Reshape();
                    newMeshRoundCone.transform.rotation = Quaternion.FromToRotation(new Vector3(1, 0, 0), child.transform.position - Bones[i].transform.position);

                    Mesh mesh = new Mesh();

                    int split = mrc.split, slice = mrc.slice;

                    int sphereVtxs = split * (slice + 1);
                    int sphereTris = 2 * split * slice;

                    int coneVtxs = split * 2;
                    int coneTris = split * 2;

                    Vector3[] vertices = new Vector3[2 * sphereVtxs + coneVtxs];
                    int[] triangles = new int[(2 * sphereTris + coneTris) * 3];

                    // 頂点位置の設定は後でMeshRoundCone.Reshape()が行う

                    // Make Triangles
                    /// -- Sphere
                    int cnt = 0;
                    for (int n = 0; n < 2; n++) {
                        for (int x = 0; x < split; x++) {
                            for (int y = 0; y < slice; y++) {
                                int i0 = x;
                                int i1 = (x == split - 1) ? 0 : x + 1;

                                triangles[cnt + 0] = split * (y + 0) + i1 + n * sphereVtxs;
                                triangles[cnt + 1] = split * (y + 1) + i0 + n * sphereVtxs;
                                triangles[cnt + 2] = split * (y + 0) + i0 + n * sphereVtxs;
                                cnt += 3;

                                triangles[cnt + 0] = split * (y + 0) + i1 + n * sphereVtxs;
                                triangles[cnt + 1] = split * (y + 1) + i1 + n * sphereVtxs;
                                triangles[cnt + 2] = split * (y + 1) + i0 + n * sphereVtxs;
                                cnt += 3;
                            }
                        }
                    }
                    /// -- Cone
                    for (int z = 0; z < split; z++) {
                        int i0 = z;
                        int i1 = (z == split - 1) ? 0 : z + 1;

                        triangles[cnt + 0] = split * 0 + i0 + 2 * sphereVtxs;
                        triangles[cnt + 1] = split * 1 + i0 + 2 * sphereVtxs;
                        triangles[cnt + 2] = split * 0 + i1 + 2 * sphereVtxs;
                        cnt += 3;

                        triangles[cnt + 0] = split * 1 + i0 + 2 * sphereVtxs;
                        triangles[cnt + 1] = split * 1 + i1 + 2 * sphereVtxs;
                        triangles[cnt + 2] = split * 0 + i1 + 2 * sphereVtxs;
                        cnt += 3;
                    }

                    mesh.vertices = vertices;
                    mesh.triangles = triangles;

                    mesh.RecalculateNormals();
                    mesh.RecalculateBounds();

                    mf.sharedMesh = mesh;

                    if (default_material == null) {
                        foreach (var q in Resources.FindObjectsOfTypeAll<Material>()) {
                            if (q.name == "Default-Material") { default_material = q; break; }
                        }
                    }

                    mr.sharedMaterial = default_material;

                    // 頂点位置の設定
                    mrc.Reshape();
                    //MeshRoundCones[i].GetComponent<CDRoundConeBehavior>()

                    MeshRoundCones.Add(newMeshRoundCone.GetComponent<MeshRoundCone>());
                }
            }
        }
    }
    public void Load() {
        FileInfo fileInfo = new FileInfo(Application.dataPath + "/../Settings/" + filename);
        string name = "";
        string Length = "";
        string R1 = "";
        string R2 = "";
        string line = "";
        Vector3 position = new Vector3();
        Quaternion rotation = new Quaternion();
        MeshRoundCones = new List<MeshRoundCone>(this.GetComponentsInChildren<MeshRoundCone>());

        if (fileInfo.Exists) {
            StreamReader reader = fileInfo.OpenText();

            while (reader.Peek() >= 0) {
                line = reader.ReadLine();
                string[] lines = line.Split(' ');
                if (lines.Length < 4) {
                    Debug.LogError("Collision.txtファイルおかしい");
                    return;
                }
                name = lines[0];
                Length = lines[1];
                R1 = lines[2];
                R2 = lines[3];
                if (lines.Length >= 10) {
                    position = new Vector3(float.Parse(lines[4]), float.Parse(lines[5]), float.Parse(lines[6]));
                    rotation = new Quaternion(float.Parse(lines[7]), float.Parse(lines[8]), float.Parse(lines[9]), float.Parse(lines[10]));
                }
                foreach (MeshRoundCone c in MeshRoundCones) {
                    if (c == null) {
                        Debug.Log("MeshRoundConeがアタッチされていないMeshRoundCones");
                        continue;
                    }
                    if (c.name == name) {
                        Undo.RecordObject(c, "Load MeshRoundCone");
                        Undo.RecordObject(c.gameObject, "Load MeshRoundCone GameObject");
                        c.length = float.Parse(Length);
                        c.r1 = float.Parse(R1);
                        c.r2 = float.Parse(R2);

                        if (lines.Length >= 10) {
                            c.transform.position = position;
                            c.transform.rotation = rotation;
                        }
                        c.Reshape();
                    }
                }
            }
            reader.Close();
        }
    }
    public void Save() {
        FileInfo fileInfo = new FileInfo(Application.dataPath + "/../Settings/" + filename);
        StreamWriter writer = fileInfo.CreateText();
        MeshRoundCones = new List<MeshRoundCone>(this.GetComponentsInChildren<MeshRoundCone>());

        if(MeshRoundCones != null) {
            foreach (MeshRoundCone c in MeshRoundCones) {
                Debug.Log("MeshRoundCones name = " + c.name);
                if (c == null) {
                    Debug.Log("MeshRoundConeがアタッチされていないMeshRoundCones");
                    continue;
                }
                writer.WriteLine(c.name + " " + c.length + " " +
                    c.r1 + " " + c.r2 + " " +
                    c.transform.position.x + " " + c.transform.position.y + " " + c.transform.position.z + " " +
                    c.transform.rotation.x + " " + c.transform.rotation.y + " " + c.transform.rotation.z + " " + c.transform.rotation.w);
            }
        } else {
                Debug.Log("MeshRoundConesがnull,SetupFromBonesをし直してLoadしてからSave");
        }
        writer.Close();
    }
    public void EnMeshRendereron() {
        FileInfo fileInfo = new FileInfo(Application.dataPath + "/../Settings/" + filename);
        string name = "";
        string line = "";
        List<MeshRenderer> meshRenderers = new List<MeshRenderer>(GetComponentsInChildren<MeshRenderer>());
        if (fileInfo.Exists) {
            StreamReader reader = fileInfo.OpenText();

            while (reader.Peek() >= 0) {
                line = reader.ReadLine();
                string[] lines = line.Split(' ');
                if (lines.Length < 4) {
                    Debug.LogError("Collision.txtファイルおかしい");
                    return;
                }
                name = lines[0];
                foreach(var meshRenderer in meshRenderers) {
                    if(meshRenderer.name == name) {
                        meshRenderer.enabled = true;
                    }
                }
            }
            reader.Close();
        }
        //頭と手の甲用
        List<SphereCollider> spherecollider = new List<SphereCollider>(this.GetComponentsInChildren<SphereCollider>());
        foreach(SphereCollider s in spherecollider) {
            MeshRenderer mesh = s.GetComponent<MeshRenderer>();
            if(mesh != null) {
                mesh.enabled = true;
            }
        }
    }
    public void EnMeshRendereroff() {
        FileInfo fileInfo = new FileInfo(Application.dataPath + "/../Settings/" + filename);
        string name = "";
        string line = "";
        List<MeshRenderer> meshRenderers = new List<MeshRenderer>(GetComponentsInChildren<MeshRenderer>());
        if (fileInfo.Exists) {
            StreamReader reader = fileInfo.OpenText();

            while (reader.Peek() >= 0) {
                line = reader.ReadLine();
                string[] lines = line.Split(' ');
                if (lines.Length < 4) {
                    Debug.LogError("Collision.txtファイルおかしい");
                    return;
                }
                name = lines[0];
                foreach(var meshRenderer in meshRenderers) {
                    if(meshRenderer.name == name) {
                        meshRenderer.enabled = false;
                    }
                }
            }
            reader.Close();
        }
        //頭と手の甲用
        List<SphereCollider> spherecollider = new List<SphereCollider>(this.GetComponentsInChildren<SphereCollider>());
        foreach(SphereCollider s in spherecollider) {
            MeshRenderer mesh = s.GetComponent<MeshRenderer>();
            if(mesh != null) {
                mesh.enabled = false;
            }
        }
    }
    public void SetupFromLeftBody() {
        MeshRoundCones = new List<MeshRoundCone>(this.GetComponentsInChildren<MeshRoundCone>());
        Vector3 r1_position,r2_position;
        foreach(var lmrc in MeshRoundCones) {
            if (lmrc.name.Contains("Left")) {
                foreach(var rmrc in MeshRoundCones) {
                    if(lmrc.name.Replace("Left","Right") == rmrc.name){
                        rmrc.length = lmrc.length;
                        rmrc.r1 = lmrc.r1;
                        rmrc.r2 = lmrc.r2;
                        r1_position = lmrc.transform.position;
                        r1_position.x = -r1_position.x;
                        rmrc.transform.position = r1_position;
                        r2_position = lmrc.transform.position + lmrc.length * (lmrc.transform.rotation * Vector3.right);
                        r2_position.x = -r2_position.x;
                        rmrc.transform.rotation = Quaternion.FromToRotation(Vector3.right, r2_position - r1_position);
                        rmrc.Reshape();
                    }
                }
            }
            //r1_position = mrc.transform.position;
            //r2_position = mrc.transform.position + mrc.length * (mrc.transform.rotation * Vector3.right);
            //mrc.transform.rotation = Quaternion.FromToRotation(Vector3.right, r2_position - r1_position);
        }
    }
}
