using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ActionStateWindowEditor {

    public ActionState state;

    public Rect rect;
    public bool isDragged;
    public bool isSelected;

    public static GUIStyle defaultStyle;
    public static GUIStyle selectedStyle;

    public static void Initialize() {
        defaultStyle = new GUIStyle();
        defaultStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
        defaultStyle.alignment = TextAnchor.MiddleCenter;
        defaultStyle.border = new RectOffset(12, 12, 12, 12);

        selectedStyle = new GUIStyle();
        selectedStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1 on.png") as Texture2D;
        selectedStyle.alignment = TextAnchor.MiddleCenter;
        selectedStyle.border = new RectOffset(12, 12, 12, 12);
    }

    public void Drag(Vector2 delta) {
        rect.position += delta;
    }

    public void Draw() {
        GUI.Box(rect, state.name, defaultStyle);
    }
}
