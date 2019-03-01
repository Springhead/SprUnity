using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class KeyPoseBone {
    static int radius = 20;
    //public Vector2 position;
    //HumanBodyBones bone;
    static public void Draw(HumanBodyBones avatarBone, Vector2 position) {
        Event e = Event.current;
        var bone = ActionEditorWindowManager.instance.body[avatarBone];
        if(bone == null || bone.controller == null) {
            GUI.Box(new Rect(position - new Vector2(radius, radius), new Vector2(2 * radius, 2 * radius)), "D");
            return;
        } else {
            string text = bone.controller.controlPosition ? "P" : "";
            text = text + (bone.controller.controlRotation ? "R" : "");
            GUI.Box(new Rect(position - new Vector2(radius, radius), new Vector2(2 * radius, 2 * radius)), text);
        }
        if (e.type == EventType.MouseDown) {
            if(e.button == 0) {
                if((e.mousePosition - position).magnitude < radius) {
                    if(e.mousePosition.y > position.y) {
                        bone.controller.controlPosition = !bone.controller.controlPosition;
                    } else {
                        bone.controller.controlRotation = !bone.controller.controlRotation;
                    }
                    e.Use();
                }
            }
        }
    }
}

public class KeyPoseBoneWindow : EditorWindow {

    //
    static KeyPoseBoneWindow window;

    struct BoneBoxPair {
        HumanBodyBones boneId;
        Vector2 pos;
    }
    private BoneBoxPair[] boneBoxPairs;

    [MenuItem("Window/KeyPoseBone Window")]
    static void Open() {
        window = GetWindow<KeyPoseBoneWindow>();
        ActionEditorWindowManager.instance.keyPoseBoneWindow = window;
    }

    public void OnEnable() {
        Open();
    }

    public void OnDisable() {
        window = null;
        ActionEditorWindowManager.instance.keyPoseBoneWindow = null;
    }

    void OnGUI() {
        HumanBodyBones[] bones = {
            HumanBodyBones.Head,
            HumanBodyBones.Neck,

            HumanBodyBones.Chest,
            HumanBodyBones.Spine,
            HumanBodyBones.Hips,

            HumanBodyBones.LeftShoulder,
            HumanBodyBones.LeftUpperArm,
            HumanBodyBones.LeftLowerArm,
            HumanBodyBones.LeftHand,

            HumanBodyBones.RightShoulder,
            HumanBodyBones.RightUpperArm,
            HumanBodyBones.RightLowerArm,
            HumanBodyBones.RightHand,

            HumanBodyBones.LeftUpperLeg,
            HumanBodyBones.LeftLowerLeg,
            HumanBodyBones.LeftFoot,

            HumanBodyBones.RightUpperLeg,
            HumanBodyBones.RightLowerLeg,
            HumanBodyBones.RightFoot,
        };

        int i = 0;
        foreach(var bone in bones) {
            KeyPoseBone.Draw(bone, new Vector2(20, 20 + i));
            i += 40;
        }
    }
}
