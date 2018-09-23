using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public LookController lookController;
    public float relativeMoveRatio = 0.0f;

    public ReachController reachController = null;
    public Vector2 springDamper = new Vector2(1, 1);
    public float duration = 0.5f;

    public bool autoReturn = false;
    public float returnStartTime = 0;
    public float returnDuration = 0;

    public KeyCode hotKey = KeyCode.F1; // <!!> will be removed

    // ----- ----- ----- ----- -----

    private PosRot relativeOriginInitialPosRot = new PosRot();

    // ----- ----- ----- ----- -----

    void Start () {
	}
	
	void FixedUpdate () {
    }

    public void Action() {
        PosRot moveTo = new PosRot(gameObject);
        PosRot autoReturnTo = new PosRot(reachController.trajectory.Last().p1, reachController.trajectory.Last().q1);

        // ----- ----- -----

        PosRot relativeKeyPosRot = new PosRot(lookController.body["Base"].transform).Inverse().TransformPosRot(moveTo);
        Vector3 originPos = lookController.body["Base"].transform.position;
        Quaternion originRot = Quaternion.Slerp(lookController.body["Base"].transform.rotation, lookController.currentTargetPose.rotation, relativeMoveRatio);
        moveTo = new PosRot(originPos, originRot).TransformPosRot(relativeKeyPosRot);

        // ----- ----- -----

        reachController.AddSubMovement(
            moveTo,
            springDamper,
            duration,
            duration
            );

        // ----- ----- -----

        if (autoReturn && returnDuration > 1e-5 && returnStartTime > 1e-5) {
            reachController.AddSubMovement(
                autoReturnTo,
                springDamper,
                returnStartTime + returnDuration,
                returnDuration
                );
        }
    }

}
