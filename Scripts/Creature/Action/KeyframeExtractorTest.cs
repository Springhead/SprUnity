using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(KeyframeExtractorTest))]
public class KeyframeExtractorTestEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        KeyframeExtractorTest test = (KeyframeExtractorTest)target;

        if (GUILayout.Button("Fitting"))
        {
            test.Fitting();
        }
    }
}

#endif

public class KeyframeExtractorTest : MonoBehaviour {

    public bool rec = false;
    public List<float> velocity;
    private Vector3 prePos;
    private float[] reconstructed;
    private bool display = false;

    List<int> maximul;
    List<int> minimul;
    List<int> inflection;

    // Use this for initialization
    void Start () {
        velocity = new List<float>();
        prePos = gameObject.transform.position;
        display = false;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        if (rec)
        {
            velocity.Add((gameObject.transform.position - prePos).magnitude / Time.fixedDeltaTime);
        }
        prePos = gameObject.transform.position;
	}

    public void Fitting()
    {
        KeyframeExtractor extractor = new KeyframeExtractor();

        List<SubMovement1D> subs = extractor.ExtractFeatures(velocity.ToArray(), out maximul, out minimul, out inflection);

        int velCount = velocity.Count;
        int subCount = subs.Count;
        reconstructed = new float[velocity.Count];
        for(int i = 0; i < velCount; i++)
        {
            for(int j = 0; j < subCount; j++)
            {
                reconstructed[i] += subs[j].GetCurrentVelocity(i);
            }
        }

        display = true;
    }

    void OnDrawGizmos()
    {
        if (display)
        {
            int velCount = velocity.Count;
            Gizmos.color = Color.red;
            for(int i = 0; i < velCount - 1; i++)
            {
                Gizmos.DrawLine(new Vector3(i * 0.01f, velocity[i], 0.0f), new Vector3((i + 1) * 0.01f, velocity[i + 1], 0.0f));
            }
            Gizmos.color = Color.blue;
            for (int i = 0; i < velCount - 1; i++)
            {
                Gizmos.DrawLine(new Vector3(i * 0.01f, reconstructed[i], 0.0f), new Vector3((i + 1) * 0.01f, reconstructed[i + 1], 0.0f));
            }
            Gizmos.color = Color.red;
            foreach(int i in maximul)
            {
                Gizmos.DrawSphere(new Vector3(i * 0.01f, velocity[i], 0.0f), 0.01f);
            }
            Gizmos.color = Color.yellow;
            foreach (int i in minimul)
            {
                Gizmos.DrawSphere(new Vector3(i * 0.01f, velocity[i], 0.0f), 0.01f);
            }
            Gizmos.color = Color.green;
            foreach (int i in inflection)
            {
                Gizmos.DrawSphere(new Vector3(i * 0.01f, velocity[i], 0.0f), 0.01f);
            }
        }
    }
}
