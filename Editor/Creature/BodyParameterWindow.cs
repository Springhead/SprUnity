using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SprUnity {

    public class BodyParameterWindow : EditorWindow {

        // インスタンス
        static BodyParameterWindow window;

        private Vector2 scrollPos = new Vector2(0, 0);

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
            window.titleContent = new GUIContent("BodyParameter");
            ActionEditorWindowManager.instance.bodyParameterWindow = window;
            displayBodyGroup = new BoneGroupBoolPair[] {
            new BoneGroupBoolPair("Trunk", true),  // Trunk
            new BoneGroupBoolPair("LeftArm", true),  // LeftArm
            new BoneGroupBoolPair("RightArm", true),  // RightArm
            new BoneGroupBoolPair("LeftLeg", true),  // LeftLeg
            new BoneGroupBoolPair("RightLeg", true),  // RightLeg
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

        private string[] dispParam = new string[]{
        "spring",
        "damper",
        "mass",
    };
        public void OnEnable() {
            Open();
            // <!!> これ、ここか？
            for (int i = 0; i < displayBodyGroup.Length; i++) {
                displayBodyGroup[i].disp = SessionState.GetBool("BodyParamGroup" + displayBodyGroup[i].groupName, false);
            }
        }

        public void OnDisable() {
            for (int i = 0; i < displayBodyGroup.Length; i++) {
                SessionState.SetBool("BodyParamGroup" + displayBodyGroup[i].groupName, displayBodyGroup[i].disp);
            }
            window = null;
            ActionEditorWindowManager.instance.bodyParameterWindow = null;
        }

        public void OnGUI() {
            GUIStyle style = GUI.skin.GetStyle("label");
            int backFontSize = style.fontSize; // 他のウィンドウに影響しないように
            TextAnchor backAlignment = style.alignment;

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            float windowWidth = position.width;
            float groupButtonWidth = windowWidth / 5;
            for (int i = 0; i < 5; i++) {
                Rect groupButtonRect = new Rect(groupButtonWidth * i, 0, groupButtonWidth, 20);
                GUIContent buttonContent = new GUIContent(displayBodyGroup[i].groupName);
                displayBodyGroup[i].disp = GUI.Toggle(groupButtonRect, displayBodyGroup[i].disp, displayBodyGroup[i].groupName, EditorStyles.miniButtonMid);
            }
            var body = ActionEditorWindowManager.instance.body;
            if (body == null) {
                return;
            }
            windowWidth = windowWidth - 30; // 右に少しスペースを開けるため
            int nGroups = displayBodyGroup.Length;
            int num = dispParam.Length + 1; // 表示するパラメータと名前
            Rect dispParamRect = new Rect(windowWidth / num, 30, windowWidth / num, 25);
            Rect paramRect = new Rect(0, 20, windowWidth / num, 20);
            Rect groupRect = new Rect(0, 20, windowWidth / num, 40);

            style.fontSize = 15;
            style.alignment = TextAnchor.UpperCenter;
            for (int i = 0; i < dispParam.Length; i++) {
                GUI.Label(dispParamRect, new GUIContent(dispParam[i]));
                dispParamRect.x += windowWidth / num;
            }
            for (int i = 0; i < nGroups; i++) {
                if (displayBodyGroup[i].disp) {
                    int nBones = boneGroup[i].Length;
                    groupRect.y = paramRect.y + 20;
                    style.fontSize = 20;
                    style.alignment = TextAnchor.UpperRight;
                    GUI.Label(groupRect, new GUIContent(displayBodyGroup[i].groupName));
                    paramRect.y += 30;
                    style.fontSize = 10;
                    for (int j = 0; j < nBones; j++) {
                        paramRect.x = 0;
                        EditorGUILayout.BeginHorizontal();
                        paramRect.y += 20;
                        GUI.Label(paramRect, new GUIContent(boneGroup[i][j].ToString()));

                        var balljoint = body[boneGroup[i][j].ToString()].joint as PHBallJointBehaviour;
                        var phsolid = body[boneGroup[i][j].ToString()].solid;
                        if (balljoint == null) {
                            Debug.Log(boneGroup[i][j].ToString() + " error");
                        } else {
                            paramRect.x = windowWidth / num;
                            balljoint.desc.spring = EditorGUI.DoubleField(paramRect, balljoint.desc.spring);
                            paramRect.x = 2 * windowWidth / num;
                            balljoint.desc.damper = EditorGUI.DoubleField(paramRect, balljoint.desc.damper);
                            balljoint.OnValidate();
                        }
                        if (phsolid != null) {
                            paramRect.x = 3 * windowWidth / num;
                            phsolid.desc.mass = EditorGUI.DoubleField(paramRect, phsolid.desc.mass);
                            phsolid.OnValidate();
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
            GUILayoutUtility.GetRect(new GUIContent(string.Empty), GUIStyle.none, GUILayout.Width(10), GUILayout.Height(paramRect.y + 30));
            EditorGUILayout.EndScrollView();

            style.fontSize = backFontSize;
            style.alignment = backAlignment;
        }
    }

}