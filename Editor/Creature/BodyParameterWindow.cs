using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BodyParameterWindow : EditorWindow {

    // インスタンス
    static BodyParameterWindow window;

    private Vector2 scrollPos;

    public class BoneGroupBoolPair {
        public string groupName;
        public bool disp;
        public BoneGroupBoolPair(string s, bool d) {
            groupName = s;
            disp = d;
        }
    }
    public static BoneGroupBoolPair[] displayBodyGroup;
    public static HumanBodyBones[][] boneGroup;

    [MenuItem("Window/Body Parameter Window")]
    static void Open() {
        window = GetWindow<BodyParameterWindow>();
        ActionEditorWindowManager.instance.bodyParameterWindow = window;
        displayBodyGroup = new BoneGroupBoolPair[] {
            new BoneGroupBoolPair("Trunk", false),  // Trunk
            new BoneGroupBoolPair("LeftArm", false),  // LeftArm
            new BoneGroupBoolPair("RightArm", false),  // RightArm
            new BoneGroupBoolPair("LeftLeg", false),  // LeftLeg
            new BoneGroupBoolPair("RightLeg", false),  // RightLeg
        };
        boneGroup = new HumanBodyBones[][] {
        new HumanBodyBones[]{
            HumanBodyBones.Head,
            HumanBodyBones.Neck,
            HumanBodyBones.Chest,
            HumanBodyBones.Spine,
            HumanBodyBones.Hips
        },
        new HumanBodyBones[]{
            HumanBodyBones.LeftShoulder,
            HumanBodyBones.LeftUpperArm,
            HumanBodyBones.LeftLowerArm,
            HumanBodyBones.LeftHand
        },
        new HumanBodyBones[]{
            HumanBodyBones.RightShoulder,
            HumanBodyBones.RightUpperArm,
            HumanBodyBones.RightLowerArm,
            HumanBodyBones.RightHand
        },
        new HumanBodyBones[]{
            HumanBodyBones.LeftUpperLeg,
            HumanBodyBones.LeftLowerLeg,
            HumanBodyBones.LeftFoot
        },
        new HumanBodyBones[]{
            HumanBodyBones.RightUpperLeg,
            HumanBodyBones.RightLowerLeg,
            HumanBodyBones.RightFoot
        }
        };
    }

    public void OnEnable() {
        Open();
        // <!!> これ、ここか？
        for (int i = 0; i < displayBodyGroup.Length; i++) {
            displayBodyGroup[i].disp = SessionState.GetBool("BodyParamGroup" + displayBodyGroup[i].groupName, false);
        }
    }

    public void OnDisable() {
        for (int i = 0; i <  displayBodyGroup.Length; i++) {
            SessionState.SetBool("BodyParamGroup" + displayBodyGroup[i].groupName, displayBodyGroup[i].disp);
        }
        window = null;
        ActionEditorWindowManager.instance.bodyParameterWindow = null;
    }

    public void OnGUI() {
        float windowWidth = position.width;
        float groupButtonWidth = windowWidth / 5;
        for(int i = 0; i < 5; i++) {
            Rect groupButtonRect = new Rect(groupButtonWidth * i, 0, groupButtonWidth, 20);
            GUIContent buttonContent = new GUIContent(displayBodyGroup[i].groupName);
            displayBodyGroup[i].disp = GUI.Toggle(groupButtonRect, displayBodyGroup[i].disp, displayBodyGroup[i].groupName, EditorStyles.miniButtonMid);
        }
        var body = ActionEditorWindowManager.instance.body;
        if (body == null) return;
        int nGroups = displayBodyGroup.Length;
        for(int i = 0; i < nGroups; i++) {
            if (displayBodyGroup[i].disp) {
                int nBones = boneGroup[i].Length;
                for(int j = 0; j < nBones; j++) {
                    //body[boneGroup[i][j]].
                }
            }
        }
    }
}
