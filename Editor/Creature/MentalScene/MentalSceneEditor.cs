using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SprUnity {
    [CustomEditor(typeof(MentalScene))]
    public class MentalSceneEditor : Editor{
        private void OnSceneGUI() {
            Handles.PositionHandle(new Vector3(1,0,0), Quaternion.identity);
        }
    }
}
