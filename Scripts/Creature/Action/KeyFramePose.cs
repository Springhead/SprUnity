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
        //if (EditorWindow.GetWindow(typeof(ActionStateMachineWindow)) != null) {
            EditorGUI.BeginChangeCheck();
            Vector3 positionHandle = Handles.PositionHandle(kPose.worldPosition, Quaternion.Euler(kPose.worldEulerRotation));
            Quaternion rotationHandle = Handles.RotationHandle(Quaternion.Euler(kPose.worldEulerRotation), kPose.worldPosition);
            if (EditorGUI.EndChangeCheck()) {
                kPose.worldPosition = positionHandle;
                kPose.worldEulerRotation = rotationHandle.eulerAngles;
            }
        //}
    }

    public override void OnInspectorGUI() {
        KeyFramePose kPose = (KeyFramePose)target;
        base.OnInspectorGUI();
        var dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "Set from Object");
        int id = GUIUtility.GetControlID(FocusType.Passive);
        if (dropArea.Contains(Event.current.mousePosition)) {
            switch (Event.current.type) {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    DragAndDrop.activeControlID = id;
                    DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                    if(Event.current.type == EventType.DragPerform) {
                        if (DragAndDrop.objectReferences.Length == 1) {
                            DragAndDrop.AcceptDrag();
                            var gameObject = DragAndDrop.objectReferences[0] as GameObject;
                            if (gameObject != null) {
                                kPose.worldPosition = gameObject.transform.position;
                                kPose.worldEulerRotation = gameObject.transform.rotation.eulerAngles;
                            }
                            var keypose = DragAndDrop.objectReferences[0] as KeyFramePose;
                            if (keypose != null) {
                                kPose.worldPosition = keypose.worldPosition;
                                kPose.worldEulerRotation = keypose.worldEulerRotation;
                            }
                            HandleUtility.Repaint();
                        }
                        DragAndDrop.activeControlID = 0;
                        HandleUtility.Repaint();
                        Debug.Log("perform");
                    }
                    break;
                    /*
                case EventType.DragExited:
                    Debug.Log("exit");
                    if (DragAndDrop.objectReferences.Length == 1) {
                        var transform = DragAndDrop.objectReferences[0] as Transform;
                        if (transform != null) {
                            kPose.localPosition = transform.position;
                            kPose.localEulerRotation = transform.rotation.eulerAngles;
                            HandleUtility.Repaint();
                        }
                    }
                    break;*/
            }
        }
    }
}

[CreateAssetMenu(menuName = "Action/Create KeyFrame Pose Object")]
public class KeyFramePose : ScriptableObject, IDropHandler {
    /**
     * ActionKeyFrameの基本的な姿勢情報
     * そのため、複数のActionKeyFrameが参照する可能性がある
     * <!!> Bodyがないと編集できない!
     * 解決案１：明示的なセーブで同期
     * 解決案２：なんとかEditingBodyを設定
     * 
     */ 
    public Vector3 worldPosition;
    public Vector3 worldEulerRotation;

    //[HideInInspector]
    public Vector3 localPosition;
    //[HideInInspector]
    public Vector3 localEulerRotation;

    public void OnDrop(PointerEventData eventData) {
        Transform droppedTransform = eventData.pointerDrag.GetComponent<Transform>();
        if (droppedTransform == null) return;
        // localといいつつworldを取得
        worldPosition = droppedTransform.position;
        worldEulerRotation = droppedTransform.rotation.eulerAngles;
    }
}

#endif