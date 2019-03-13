using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SprUnity {

    public class ActionTransitionWindowEditor : ICanvasRaycastFilter {

        public ActionTransition transition;

        public ActionStateWindowEditor fromState;
        public ActionStateWindowEditor toState;

        static Texture2D arrowBar;
        static Texture2D arrowTriangle;

        Matrix4x4 matrix;

        public static void Initialize() {
            arrowBar = EditorGUIUtility.Load("AnimationEventTooltip") as Texture2D;
            arrowTriangle = EditorGUIUtility.Load("AnimationEventTooltipArrow") as Texture2D;
        }

        public void Draw() {

        }

        public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera) {
            //return Vector2.Distance(sp, transform.position) < radius;
            return false;
        }

        public Texture2D createBarTexture(Color color) {
            Texture2D texture = new Texture2D(10, 10);
            return texture;
        }

        public Texture2D createTriangleTexture(Color color) {
            Texture2D texture = new Texture2D(40, 20);
            return texture;
        }
    }

}