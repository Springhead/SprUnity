using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(MeshRoundCone))]
[CanEditMultipleObjects]
public class RoundConeEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
    }

    void OnSceneGUI () {
        MeshRoundCone mrc = target as MeshRoundCone;
        Transform trans = mrc.gameObject.transform;
        float scale = (trans.lossyScale.x + trans.lossyScale.y + trans.lossyScale.z) / 3.0f;

        mrc.UpdateR1R2HandlePosition();

        float defLeng = HandleUtility.GetHandleSize(Vector3.left);

        EditorGUI.BeginChangeCheck();
        float r1 = Handles.RadiusHandle(trans.rotation, mrc.positionR1, mrc.r1 * scale);
        if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(target, "Change Radius 1");
            mrc.r1 = r1 / scale;
            mrc.Reshape();
        }

        EditorGUI.BeginChangeCheck();
        float r2 = Handles.RadiusHandle(trans.rotation, mrc.positionR2, mrc.r2 * scale);
        if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(target, "Change Radius 2");
            mrc.r2 = r2 / scale;
            mrc.Reshape();
        }

        EditorGUI.BeginChangeCheck();
        float size = Mathf.Max(mrc.length * scale, Mathf.Max(mrc.r1 * scale, mrc.r2 * scale));
        float length = Handles.ScaleSlider(mrc.length * scale, trans.position, trans.rotation * new Vector3(0, 1, 0), trans.rotation, size, 0.5f);
        if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(target, "Change Length");
            mrc.length = length / scale;
            mrc.Reshape();
        }

        if (mrc.pivot != MeshRoundCone.Pivot.R1) {
            EditorGUI.BeginChangeCheck();
            Vector3 positionR1 = Handles.PositionHandle(mrc.positionR1, Quaternion.identity);
            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(target, "Change R1 Position");
                mrc.positionR1 = positionR1;
                mrc.Reposition();
                mrc.Reshape();
            }
        }

        if (mrc.pivot != MeshRoundCone.Pivot.R2) {
            EditorGUI.BeginChangeCheck();
            Vector3 positionR2 = Handles.PositionHandle(mrc.positionR2, Quaternion.identity);
            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(target, "Change R2 Position");
                mrc.positionR2 = positionR2;
                mrc.Reposition();
                mrc.Reshape();
            }
        }
    }
}
