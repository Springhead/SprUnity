using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using InteraWare;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(BlinkController))]
public class BlinkControllerEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        BlinkController action = (BlinkController)target;

        if (GUILayout.Button("Blink")) {
            action.Action();
        }
    }
}
#endif

public class BlinkController : MonoBehaviour {
    public Body body;
    public SkinnedMeshRenderer face;
    public int eyeCloseMorph = 3;

    // ----- ----- ----- ----- -----

    private float timeFromBlink = 0;
    private float waitTimer = 0;

    private float blinkTimer = 0;
    private SubMovement openMovement;
    private SubMovement closeMovement;

    // ----- ----- ----- ----- -----

    void Start () {
        openMovement = new SubMovement();
        openMovement.t0 = 0.07f - 0.07f;
        openMovement.t1 = 0.07f;

        closeMovement = new SubMovement();
        closeMovement.t0 = 0.20f - 0.13f;
        closeMovement.t1 = 0.13f;
    }

    void FixedUpdate () {
        timeFromBlink += Time.fixedDeltaTime;

        // Blink Timing Control
        if (waitTimer > 0) {
            waitTimer -= Time.fixedDeltaTime;
        } else {
            waitTimer = 0.1f; // Interval

            float prob = Mathf.Min(Mathf.Max(0, timeFromBlink / 15), 1);
            prob = prob * prob;
            if (Random.value < prob) {
                Action();
            }
        }

        // Blink Interpolation
        if (blinkTimer < 0.21f) {
            blinkTimer += Time.fixedDeltaTime;
        }

        // Update BlendShape
        if (body != null) {
            float eyelidClose = 100.0f * (1.0f - ((openMovement.GetCurrentActiveness(blinkTimer) - closeMovement.GetCurrentActiveness(blinkTimer))));

            if (face.GetBlendShapeWeight(0) > 30) { eyelidClose = 0; }

            face.SetBlendShapeWeight(eyeCloseMorph, eyelidClose);
        }
    }

    public void Action() {
        if (blinkTimer > 0.20f) {
            blinkTimer = 0;
            timeFromBlink = 0;
        }
    }
}
