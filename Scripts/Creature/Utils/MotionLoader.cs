using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(MotionLoader))]
public class MotionLoaderEditor : Editor {
    public float minVal = 0;
    public float maxVal = 999;
    public float maxLimit = 999;

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        MotionLoader motionLoader = (MotionLoader)target;
        motionLoader.editor = this;

        EditorGUILayout.LabelField((motionLoader.loading ? "Now Loading... " : "Loaded ") + motionLoader.endLimit + " frames.");

        minVal = motionLoader.begin;
        maxVal = (motionLoader.end > 0) ? motionLoader.end : 999;
        maxLimit = motionLoader.endLimit;
        EditorGUILayout.MinMaxSlider(ref minVal, ref maxVal, 0, maxLimit);
        motionLoader.begin = (int)minVal;
        motionLoader.end = (int)maxVal;

        motionLoader.idx = (int)(EditorGUILayout.Slider(motionLoader.idx, motionLoader.begin, motionLoader.end));

        if (GUILayout.Button("Key")) {
            float time = (motionLoader.idx - motionLoader.begin) * Time.fixedDeltaTime;
            float duration = 0.4f; // (motionLoader.idx - motionLoader.lastKey) * Time.fixedDeltaTime;
            if (motionLoader.lastKey < 0) { duration = 0; }
            motionLoader.lastKey = motionLoader.idx;

            foreach (var obj in motionLoader.objects) {
                FileInfo fileinfo = new FileInfo(Application.dataPath + "/../" + motionLoader.filename + "." + obj.name + "." + motionLoader.outputMotionName + ".txt");
                StreamWriter writer = fileinfo.AppendText();
                Vector3 p = obj.transform.position;
                Quaternion q = obj.transform.rotation;

                string str = "";
                str += (p.x + "," + p.y + "," + p.z + ",");
                str += (q.x + "," + q.y + "," + q.z + "," + q.w + ",");
                str += time + ",";
                str += duration;

                writer.WriteLine(str);
                writer.Close();
            }

            motionLoader.idx += 10;
        }
    }
}
#endif

public class MotionLoader : MonoBehaviour {
    public List<GameObject> objects = new List<GameObject>();
    public string filename = "";

    public bool rotationOnly = false;
    public bool timestamp = false;

    public bool usePosition = false;
    public bool useRotation = true;

    [HideInInspector]
    public int begin = 0;
    [HideInInspector]
    public int end = 0;
    [HideInInspector]
    public int endLimit = 0;

#if UNITY_EDITOR
    [HideInInspector]
    public MotionLoaderEditor editor = null;
#endif

    private List<List<Vector3>> position = new List<List<Vector3>>();
    private List<List<Quaternion>> rotation = new List<List<Quaternion>>();

    [HideInInspector]
    public int idx = 0;

    [HideInInspector]
    public bool loading = true;

    public bool run = false;

    private StreamReader reader = null;

    public string outputMotionName = "";
    public int lastKey = -1;

    // ----- ----- ----- ----- -----

    private List<Vector3> noise = new List<Vector3>();

    // ----- ----- ----- ----- -----

    // Use this for initialization
    void OnEnable () {
        position.Clear();
        rotation.Clear();
        FileInfo fileinfo = new FileInfo(Application.dataPath + "/../" + filename);
        reader = fileinfo.OpenText();
        reader.ReadLine(); // Skip Header Row

        foreach (var obj in objects) { noise.Add(new Vector3()); }
    }

    void Update() {
        for (int times = 0; times < 30; times++) {
            if (reader != null && !reader.EndOfStream) {
                string text = reader.ReadLine();
                var data = text.Split(',').Select(str => float.Parse(str));
                position.Add(new List<Vector3>());
                rotation.Add(new List<Quaternion>());

                int s = rotationOnly ? 4 : 7;
                for (int i = 0; i < objects.Count; i++) {
                    int n = timestamp ? 1 : 0;

                    if (!rotationOnly) {
                        Vector3 pos = new Vector3(data.ElementAt(i * s + 0), data.ElementAt(i * s + 1), data.ElementAt(i * s + 2));
                        position.Last().Add(pos);
                        n += 3;
                    }

                    Quaternion rot = new Quaternion(data.ElementAt(i * s + n + 0), data.ElementAt(i * s + n + 1), data.ElementAt(i * s + n + 2), data.ElementAt(i * s + n + 3));
                    rotation.Last().Add(rot);
                }

                end += 1;
                endLimit += 1;

                if (reader.EndOfStream) {
                    reader.Close();
                    reader = null;
                    loading = false;
                }
            }
        }
    }

    // Update is called once per frame
    public float noiseCoeff = 0.01f;
    void FixedUpdate () {
        /*
        if (editor != null) {
            editor.maxLimit = endLimit;
            begin = (int)(editor.minVal);
            end = (int)(editor.maxVal);
        }
        */

        for (int i = 0; i < objects.Count; i++) {
            var obj = objects[i];

            if (usePosition) {
                if (run && idx > 0) {
                    Vector3 vel = position[idx][i] - position[idx - 1][i];
                    noise[i] += (vel * GaussianRandom() * noiseCoeff);
                }

                obj.transform.position = position[idx][i] + noise[i];
            }

            if (useRotation) {
                obj.transform.rotation = gameObject.transform.rotation * rotation[idx][i];
            }
        }

        if (run) {
            idx++;
            if (idx >= rotation.Count || (end > 0 && idx >= end)) {
                idx = begin;
                for (int i = 0; i < objects.Count; i++) { noise[i] = new Vector3(); }
            }
        }
	}

    void OnValidate() {
#if UNITY_EDITOR
        if (editor != null) {
            editor.minVal = begin;
            editor.maxVal = end;
        }
#endif
    }

    // -----

    float GaussianRandom(float mu = 0.0f, float sigma = 1 / 2.0f) { // デフォルトだと95%で 0±1.0 に収まる（2σ）
        float rand = 0.0f;
        while ((rand = Random.value) == 0.0f) ;
        float rand2 = Random.value;
        float normrand = Mathf.Sqrt(-2.0f * Mathf.Log(rand)) * Mathf.Cos(2.0f * Mathf.PI * rand2);
        normrand = normrand * sigma + mu;
        return normrand;
    }
}
