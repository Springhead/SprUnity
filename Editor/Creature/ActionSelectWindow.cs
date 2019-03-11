using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;

[Serializable]
public class ActionStateMachineStatus {
    public ActionStateMachine action;
    public bool isSelected = false;
    public List<ActionTransition> templeteTransition = new List<ActionTransition>();
}

public class ActionSelectWindow : EditorWindow, IHasCustomMenu {

    // インスタンス
    static ActionSelectWindow window;

    // GUI
    private Vector2 scrollPos;

    [MenuItem("Window/Action Select Window")]
    static void Open() {
        window = GetWindow<ActionSelectWindow>();
        ActionEditorWindowManager.instance.actionSelectWindow = window;
        GetActions();
    }

    public void AddItemsToMenu(GenericMenu menu) {
        menu.AddItem(new GUIContent("Reload"), false, () => {
            Open();
        });
    }

    public void OnEnable() {
        Open();
        // <!!> これ、ここか？
        for(int i = 0; i < ActionEditorWindowManager.instance.actions.Count; i++) {
            var action = ActionEditorWindowManager.instance.actions[i];
            action.isSelected = SessionState.GetBool(action.action.name, false);
            Debug.Log(action.action.name + " " + action.isSelected + " " + SessionState.GetBool(action.action.name, false));
        }
    }
	
	public void OnDisable() {
        foreach (var action in ActionEditorWindowManager.instance.actions) {
            SessionState.SetBool(action.action.name, action.isSelected);
        }
        window = null;
        ActionEditorWindowManager.instance.actionSelectWindow = null;
    }

    public void OnGUI() {
        bool textChangeComp = false;
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        GUILayout.Label("Actions");
        if (window == null) GUILayout.Label("window null");
        if (ActionEditorWindowManager.instance.actionSelectWindow == null) GUILayout.Label("Manager.actionSelectWindow null");
        foreach (var action in ActionEditorWindowManager.instance.actions) {
            GUILayout.BeginHorizontal(GUILayout.Height(20));
            EditorGUI.BeginChangeCheck();
            action.isSelected = GUILayout.Toggle(action.isSelected, "", GUILayout.Width(15));
            action.action.name = GUILayout.TextField(action.action.name);
            if (Event.current.keyCode == KeyCode.Return && Input.eatKeyPressOnTextFieldFocus) {
                textChangeComp = true;
                GUI.FocusControl("");
                Event.current.Use();
            }
            if (EditorGUI.EndChangeCheck()) {
                EditorUtility.SetDirty(action.action);
            }/*
            if (textChangeComp) {
                string mainPath = AssetDatabase.GetAssetPath(action.action);
                //EditorUtility.SetDirty(AssetDatabase.LoadMainAssetAtPath(mainPath));
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(action.action));
            }*/
            GUILayout.EndHorizontal();
        }
        foreach (var action in ActionEditorWindowManager.instance.actions) {
            Debug.Log(action.action.name + " " + action.isSelected);
        }
        GUILayout.EndScrollView();
    }

    public static void GetActions() {
        if (!ActionEditorWindowManager.instance.actionSelectWindow) return;
        // Asset全検索
        var guids = AssetDatabase.FindAssets("t:ActionStateMachine");
        // 特定フォルダ
        // var keyPosesInFolder = AssetDatabase.FindAssets("t:KeyPoseInterpolationGroup", saveFolder);

        ActionEditorWindowManager.instance.actions = new List<ActionStateMachineStatus>();

        foreach (var guid in guids) {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var action = AssetDatabase.LoadAssetAtPath<ActionStateMachine>(path);
            ActionStateMachineStatus actionStatus = new ActionStateMachineStatus();
            actionStatus.action = action;
            actionStatus.isSelected = false;
            ActionEditorWindowManager.instance.actions.Add(actionStatus);
        }
    }

    void CreateAction() {

    }

    void DeleteAction() {

    }
}
