using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionStateWindowEditor {

    public ActionState state;

    public Rect rect;
    public bool isDragged;
    public bool isSelected;

    public GUIStyle currentStyle;
    public GUIStyle defaultStyle;
    public GUIStyle selectedStyle;

    public void Drag(Vector2 delta) {
        rect.position += delta;
    }

    public void Draw() {
        GUI.Box(rect, state.name, currentStyle);
    }
}
