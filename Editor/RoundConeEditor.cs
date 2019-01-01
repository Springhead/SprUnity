using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(MeshRoundCone))]
[CanEditMultipleObjects]
public class RoundConeEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
    }

    void OnEnable() { //デフォルトのトランスフォームをオフにする
        Tools.hidden = true;
    }

    void OnDisable() {
        Tools.hidden = false;
    }

    void OnSceneGUI() {
        MeshRoundCone mrc = target as MeshRoundCone;
        Transform trans = mrc.gameObject.transform;

        float defLeng = HandleUtility.GetHandleSize(Vector3.left);
        Vector3 pos = trans.position;
        Vector3 scale = trans.localScale;
        Quaternion rot = trans.rotation;

        float offset1, offset2;
        if (mrc.pivot == MeshRoundCone.Pivot.Center) {
            offset1 = -0.5f * mrc.length;
            offset2 = +0.5f * mrc.length;
        } else if (mrc.pivot == MeshRoundCone.Pivot.R1) {
            offset1 = 0;
            offset2 = mrc.length;
        } else {
            offset1 = -mrc.length;
            offset2 = 0;
        }

        EditorGUI.BeginChangeCheck();
        float r1 = Handles.RadiusHandle(rot, pos + rot * new Vector3(offset1, 0, 0), mrc.r1);
        if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(target, "Change Radius 1");
            mrc.r1 = r1;
            mrc.Reshape();
        }

        EditorGUI.BeginChangeCheck();
        float r2 = Handles.RadiusHandle(rot, pos + rot * new Vector3(offset2, 0, 0), mrc.r2);
        if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(target, "Change Radius 2");
            mrc.r2 = r2;
            mrc.Reshape();
        }

        EditorGUI.BeginChangeCheck();
        float length = Handles.ScaleSlider(mrc.length, pos, rot * new Vector3(0, 1, 0), rot, 1, 0.5f);
        if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(target, "Change Length");
            mrc.length = length;
            mrc.Reshape();
        }

        EditorGUI.BeginChangeCheck();
        Vector3 r1_position = Handles.PositionHandle(mrc.transform.position, Quaternion.identity); //Quaternion.identityがGlobalかLocalかで変化する
        if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(target, "Change R1Center");
            Vector3 r2_positionCalc = mrc.transform.position + mrc.length * (mrc.transform.rotation * Vector3.right);
            mrc.transform.rotation = Quaternion.FromToRotation(Vector3.right,r2_positionCalc - r1_position);
            //mrc.transform.position = r1_position + (r2_positionCalc - r1_position)*((r2_positionCalc - r1_position).sqrMagnitude - length); //大きさを固定 誤差が大きい？
            mrc.transform.position = (r1_position - r2_positionCalc).normalized * length + r2_positionCalc;
            mrc.Reshape();
        }

        EditorGUI.BeginChangeCheck();
        Vector3 r2_position = Handles.PositionHandle(mrc.transform.position + mrc.length * (mrc.transform.rotation * Vector3.right), Quaternion.identity); //Quaternion.identityがGlobalかLocalかで変化する
        if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(target, "Change R2Center");
            //length = (r2_position - mrc.transform.position).sqrMagnitude; //これはDistance()やmagnitudeより速い 変わらない？
            mrc.transform.rotation = Quaternion.FromToRotation(Vector3.right, r2_position - mrc.transform.position);
            mrc.Reshape();
        }
    }
}
