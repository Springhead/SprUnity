using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using InteraWare;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(KeyFrame))]
[CanEditMultipleObjects]
public class KeyFrameEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        KeyFrame keyframe = (KeyFrame)target;

        if (GUILayout.Button("Action")) {
            keyframe.Action();
        }
    }
}
#endif

public class KeyFrame : MonoBehaviour {

    public ReachController reachController = null;
    public Vector2 springDamper = new Vector2(1, 1);
    public float duration = 0.5f;

    public bool autoReturn = false;
    public float returnStartTime = 0;
    public float returnDuration = 0;

    public KeyCode hotKey = KeyCode.F1;

    // ----- ----- ----- ----- -----

    private float waitTimer = 0;

    void Start () {
	}
	
	void FixedUpdate () {
        if (waitTimer > 0) {
            waitTimer -= Time.fixedDeltaTime;
        } else {
            if (Input.GetKey(hotKey)) {
                Action();
                waitTimer = 0.1f;
            }
        }
	}

    public void Action() {
        reachController.AddSubMovement(
            new PosRot(gameObject.transform),
            springDamper,
            duration,
            duration
            );

        if (autoReturn && returnDuration > 1e-5 && returnStartTime > 1e-5) {
            reachController.AddSubMovement(
                new PosRot(reachController.transform),
                springDamper,
                returnStartTime + returnDuration,
                returnDuration
                );
        }
    }

}
