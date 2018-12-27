using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(KeyFramePose))]
public class KeyFramePoseEditor: Editor {
    void OnEnable() {
        SceneView.onSceneGUIDelegate += OnSceneGUI;
    }

    void OnDisable() {
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
    }

    void OnSceneGUI(SceneView sceneView) {
        KeyFramePose kPose = (KeyFramePose)target;
        EditorGUI.BeginChangeCheck();
        Vector3 positionHandle = Handles.PositionHandle(kPose.localPosition, Quaternion.Euler(kPose.localEulerRotation));
        Quaternion rotationHandle = Handles.RotationHandle(Quaternion.Euler(kPose.localEulerRotation), kPose.localPosition);
        if (EditorGUI.EndChangeCheck()) {
            kPose.localPosition = positionHandle;
            kPose.localEulerRotation = rotationHandle.eulerAngles;
        }
    }
}

[CreateAssetMenu(menuName = "Action/Create KeyFrame Pose Object")]
public class KeyFramePose : ScriptableObject, IDropHandler {
    public Vector3 localPosition;
    public Vector3 localEulerRotation;

    public void OnDrop(PointerEventData eventData) {
        Transform droppedTransform = eventData.pointerDrag.GetComponent<Transform>();
        if (droppedTransform == null) return;
        // localといいつつworldを取得
        localPosition = droppedTransform.position;
        localEulerRotation = droppedTransform.rotation.eulerAngles;
    }
}

#endif