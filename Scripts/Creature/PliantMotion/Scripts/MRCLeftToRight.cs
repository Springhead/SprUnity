using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace InteraWare {
#if UNITY_EDITOR
    [CustomEditor(typeof(MRCLeftToRight))]
    public class MRCLeftToRightEditor : Editor {
        public override void OnInspectorGUI() {
            MRCLeftToRight mrcltr = (MRCLeftToRight)target;

            // ----- ----- ----- ----- -----

            DrawDefaultInspector();

            // ----- ----- ----- ----- -----

            if (GUILayout.Button("Setup From Left Body")) {
                mrcltr.SetupFromLeftBody();
            }
        }
    }
#endif
    public class MRCLeftToRight : MonoBehaviour {
        private MeshRoundCone MRCs;
        public string filename;
        public List<MeshRoundCone> MeshRoundCones;
        // Use this for initialization
        void Start() {

        }

        // Update is called once per frame
        void Update() {

        }

        public void SetupFromLeftBody() {
            FileInfo fileInfo = new FileInfo(Application.dataPath + "/../Settings/" + filename);
            StreamReader reader = fileInfo.OpenText();
            MeshRoundCones = new List<MeshRoundCone>(this.GetComponentsInChildren<MeshRoundCone>());

            string name = "";
            string Length = "";
            string R1 = "";
            string R2 = "";
            string line = "";
            Vector3 position = new Vector3();
            Quaternion rotation = new Quaternion();

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
                foreach (MeshRoundCone o in MeshRoundCones) {
                    if (o.GetComponent<MeshRoundCone>() == null) {
                        Debug.Log("MeshRoundConeがアタッチされていないMeshRoundCones");
                        continue;
                    }
                    if (o.name == name) {
                        o.GetComponent<MeshRoundCone>().length = float.Parse(Length);
                        o.GetComponent<MeshRoundCone>().r1 = float.Parse(R1);
                        o.GetComponent<MeshRoundCone>().r2 = float.Parse(R2);

                        if (lines.Length >= 10) {
                            o.GetComponent<MeshRoundCone>().transform.position = position;
                            o.GetComponent<MeshRoundCone>().transform.rotation = rotation;
                        }
                        o.GetComponent<MeshRoundCone>().Reshape();
                    }
                }
            }
        }
    }
}
