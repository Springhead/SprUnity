using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ActionKeyFrame))]
public class KeyFrameEditor : Editor {
    // EditingBodyをとってくるためのステートマシンウィンドウ
    static ActionStateMachineWindow stateMachineWindow;

    void OnEnable() {
        SceneView.onSceneGUIDelegate += OnSceneGUI;
    }

    void OnDisable() {
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
    }
    
    void OnSceneGUI(SceneView sceneView) {
        ActionKeyFrame keyframe = (ActionKeyFrame)target;
        if (stateMachineWindow != null && keyframe.pose != null) {
            EditorGUI.BeginChangeCheck();
            KeyFramePose pose = keyframe.pose;
            //InteraWare.Body body = (EditorWindow.GetWindow(typeof(ActionStateMachineWindow)) as ActionStateMachineWindow).bodyUsedEditing;
            InteraWare.Body body = stateMachineWindow.bodyUsedEditing;
            Vector3 positionHandle = Handles.PositionHandle(pose.worldPosition, Quaternion.Euler(pose.worldEulerRotation));
            Quaternion rotationHandle = Handles.RotationHandle(Quaternion.Euler(pose.worldEulerRotation), pose.worldPosition);
            if (EditorGUI.EndChangeCheck()) {
                pose.worldPosition = positionHandle;
                pose.worldEulerRotation = rotationHandle.eulerAngles;
                pose.localPosition = body[keyframe.coordinateBaseBone].transform.InverseTransformPoint(positionHandle);
                pose.localEulerRotation = (rotationHandle * Quaternion.Inverse(body[keyframe.coordinateBaseBone].transform.rotation)).eulerAngles;
            }
        }
    }
    
    public override void OnInspectorGUI() {
        EditorGUI.BeginChangeCheck();
        target.name = EditorGUILayout.TextField("Name", target.name);
        base.OnInspectorGUI();
        if (EditorGUI.EndChangeCheck()) {
            EditorUtility.SetDirty(target);
            string mainPath = AssetDatabase.GetAssetPath(this);
            //EditorUtility.SetDirty(AssetDatabase.LoadMainAssetAtPath(mainPath));
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath((ActionKeyFrame)target));
        }
        if (GUILayout.Button("SceneSearch")) { SearchStateMachineWindow(); }
    }

    public bool SearchStateMachineWindow() {
        stateMachineWindow = EditorWindow.GetWindow(typeof(ActionStateMachineWindow)) as ActionStateMachineWindow;
        if (stateMachineWindow) return true;
        else return false;
    }
}
