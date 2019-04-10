using System.Collections;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SprUnity {

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(KeyPoseTransformer), true)]
    public class KeyPoseTransformerPropertyDrawer : PropertyDrawer {
        bool m_hideFlag = false;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            m_hideFlag = EditorGUI.Foldout(rect, m_hideFlag, label);

            if (!m_hideFlag) {
                return;
            }

            var backupIndent = EditorGUI.indentLevel;

            label = EditorGUI.BeginProperty(position, label, property);


            Type t = this.GetType();
            FieldInfo[] fields = t.GetFields();

            float y = position.y;
            for (int i = 0; i < fields.Length; i++) {
                var field = fields[i];

                y += EditorGUIUtility.singleLineHeight;
                backupIndent++;

                var propertyRect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
                DrawPropertyWindow(propertyRect, field, "");
            }
            EditorGUI.EndProperty();

            EditorGUI.indentLevel = backupIndent;
        }

        private void DrawPropertyWindow(Rect rect, FieldInfo value, string label) {
            EditorGUI.BeginChangeCheck();
            if(value.GetType() == typeof(int)) {
                //value.GetValue();
                //EditorGUI.IntField(rect, )
            }
            EditorGUI.EndChangeCheck();
        }
    }
#endif

    public static class PoseTransformLibrary {

        public static Pose Interpolation() {
            return new Pose();
        }

        public static Pose Rotate(Pose origin, Vector3 center) {
            return new Pose();
        }
    }

    public static class PositionTransformLibrary {

    }

    public static class RotationTransformLibrary {

    }

    public abstract class KeyPoseTransformer {
        public abstract KeyPose Transform(KeyPose keyPose, GameObject[] targets);
    }

    public class KeyPoseRotater : KeyPoseTransformer {
        public Vector3 center;
        public Vector3 baseDirection;
        public override KeyPose Transform(KeyPose keyPose, GameObject[] targets) {
            return null;
        }
    }
 }