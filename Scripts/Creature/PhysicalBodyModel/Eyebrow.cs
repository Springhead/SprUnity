using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(Eyebrow))]
public class EyebrowEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Eyebrow eyebrow = (Eyebrow)target;

        EditorGUILayout.PrefixLabel("Facial Emotion");
        eyebrow.emotion = EditorGUILayout.Slider(eyebrow.emotion, -1.0f, 1.0f);
    }
}
#endif

public class Eyebrow : MonoBehaviour
{

    public GameObject[] left = new GameObject[2];
    public GameObject[] right = new GameObject[2];

    public bool usePositionValue = false;

    public Pose[] neutral = new Pose[2]{
        new Pose(new Vector3(-0.0214f, 0.0840f, 0.0902f), Quaternion.Euler(0.0f, 0.0f, -15.0f)), // LeftEyebrow1
        new Pose(new Vector3(-0.0435f, 0.0844f, 0.0841f), Quaternion.Euler(0.0f, 0.0f, -4.4f))  // LeftEyebrow2
    };
    public Pose[] smile = new Pose[2]{
        new Pose(new Vector3(-0.0214f, 0.0824f, 0.0902f), Quaternion.Euler(0.0f, 0.0f, 0.0f)), // LeftEyebrow1
        new Pose(new Vector3(-0.0435f, 0.0886f, 0.0841f), Quaternion.Euler(0.0f, 0.0f, 0.0f))  // LeftEyebrow2
    };
    public Pose[] sad = new Pose[2]{
        new Pose(new Vector3(-0.0239f, 0.0906f, 0.0902f), Quaternion.Euler(0.0f, 0.0f, 0.0f)), // LeftEyebrow1
        new Pose(new Vector3(-0.0435f, 0.0788f, 0.0841f), Quaternion.Euler(0.0f, 0.0f, 0.0f))  // LeftEyebrow2
    };

    [HideInInspector]
    public float emotion = 0.0f;

    void Start()
    {

    }

    void FixedUpdate()
    {
        if (usePositionValue)
        {
            emotion = transform.localPosition.y;
        }

        // ----- ----- ----- ----- -----

        // float lr = (left ? 1 : -1);

        if (emotion < 0.0f)
        {
            float c = - emotion;
            for (int i = 0; i < 2; i++)
            {
                left[i].transform.localPosition = c * sad[i].position + (1.0f - c) * neutral[i].position;
                left[i].transform.localRotation = Quaternion.Lerp(sad[i].rotation, neutral[i].rotation, c);
            }
            for (int i = 0; i < 2; i++)
            {
                right[i].transform.localPosition = Mirror(c * sad[i].position + (1.0f - c) * neutral[i].position);
                right[i].transform.localRotation = Mirror(Quaternion.Lerp(sad[i].rotation, neutral[i].rotation, c));
            }
        }
        else if(0.0f <= emotion)
        {
            float c = emotion;
            for (int i = 0; i < 2; i++)
            {
                left[i].transform.localPosition = c * smile[i].position + (1.0f - c) * neutral[i].position;
                left[i].transform.localRotation = Quaternion.Lerp(smile[i].rotation, neutral[i].rotation, c);
            }
            for (int i = 0; i < 2; i++)
            {
                right[i].transform.localPosition = Mirror(c * smile[i].position + (1.0f - c) * neutral[i].position);
                right[i].transform.localRotation = Mirror(Quaternion.Lerp(smile[i].rotation, neutral[i].rotation, c));
            }
        }
    }

    public Vector3 Mirror(Vector3 v)
    {
        return new Vector3(-v.x, v.y, v.z);
    }
    public Quaternion Mirror(Quaternion q)
    {
        Vector3 euler = q.eulerAngles;
        return Quaternion.Euler(euler.x, -euler.y, - euler.z);
    }
}
