using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(Finger))]
public class FingerEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        Finger finger = (Finger)target;

        EditorGUILayout.PrefixLabel("Close");
        finger.close = EditorGUILayout.Slider(finger.close, 0.0f, 1.0f);
    }
}
#endif

public class Finger : MonoBehaviour {

    public GameObject[] thumb = new GameObject[3];
    public GameObject[] index = new GameObject[3];
    public GameObject[] middle = new GameObject[3];

    public bool left = true;

    public bool usePositionValue = false;

    [HideInInspector]
    public float close = 0.5f;

    void Start () {
		
	}
	
	void FixedUpdate () {
        if (usePositionValue) {
            close = transform.localPosition.y;
        }

        // ----- ----- ----- ----- -----

        float lr = (left ? 1 : -1);

        if (close < 0.2f) {
            float c = (0.2f - close) / 0.2f;

            thumb[0].transform.localRotation = Quaternion.Euler(new Vector3(-40, 0, 0));
            thumb[1].transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
            thumb[2].transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));

            foreach (var finger in index) {
                finger.transform.localRotation = Quaternion.Euler(0, 0, -8 * c * lr);
            }
            foreach (var finger in middle) {
                finger.transform.localRotation = Quaternion.Euler(0, 0, -8 * c * lr);
            }

            middle[0].transform.localRotation = Quaternion.Euler(new Vector3(0, -5 * lr, -8 * c * lr));


        } else if (close < 0.4f) {
            float c = (0.4f - close) / 0.2f;

            thumb[0].transform.localRotation = Quaternion.Euler(new Vector3(-40, 0, 0) * c);
            thumb[1].transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0) * c);
            thumb[2].transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0) * c);

            foreach (var finger in index) {
                finger.transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
            foreach (var finger in middle) {
                finger.transform.localRotation = Quaternion.Euler(0, 0, 0);
            }

            middle[0].transform.localRotation = Quaternion.Euler(new Vector3(0, -5 * lr, 0) * c);

        } else {
            float c = (close - 0.4f) / 0.6f;

            thumb[0].transform.localRotation = Quaternion.Euler(new Vector3(21, 0, 0) * c);
            thumb[1].transform.localRotation = Quaternion.Euler(new Vector3(72, -63 * lr, -6 * lr) * c);
            thumb[2].transform.localRotation = Quaternion.Euler(new Vector3(4, -52 * lr, -6 * lr) * c);

            foreach (var finger in index) {
                finger.transform.localRotation = Quaternion.Euler(0, 0, 90 * c * lr);
            }
            foreach (var finger in middle) {
                finger.transform.localRotation = Quaternion.Euler(0, 0, 90 * c * lr);
            }
        }
    }
}
