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

    public class BoneBoxPair {
        public HumanBodyBones boneId;
        public Vector2 position;
        public BoneBoxPair(HumanBodyBones bone, Vector2 pos) {
            boneId = bone;
            position = pos;
        }
    }
    private static BoneBoxPair[] boneBoxPairs;

    [MenuItem("Window/KeyPoseBone Window")]
    static void Open() {
        window = GetWindow<KeyPoseBoneWindow>();
        ActionEditorWindowManager.instance.keyPoseBoneWindow = window;
        boneBoxPairs = new BoneBoxPair[]{
            new BoneBoxPair(HumanBodyBones.Head, new Vector2(0.5f, 0.1f)),
            new BoneBoxPair(HumanBodyBones.Neck, new Vector2(0.5f, 0.2f)),

            new BoneBoxPair(HumanBodyBones.Chest, new Vector2(0.5f, 0.3f)),
            new BoneBoxPair(HumanBodyBones.Spine, new Vector2(0.5f, 0.4f)),
            new BoneBoxPair(HumanBodyBones.Hips, new Vector2(0.5f, 0.5f)),

            new BoneBoxPair(HumanBodyBones.LeftShoulder, new Vector2(0.6f, 0.28f)),
            new BoneBoxPair(HumanBodyBones.LeftUpperArm, new Vector2(0.7f, 0.35f)),
            new BoneBoxPair(HumanBodyBones.LeftLowerArm, new Vector2(0.7f, 0.45f)),
            new BoneBoxPair(HumanBodyBones.LeftHand, new Vector2(0.7f, 0.55f)),

            new BoneBoxPair(HumanBodyBones.RightShoulder, new Vector2(0.4f, 0.28f)),
            new BoneBoxPair(HumanBodyBones.RightUpperArm, new Vector2(0.3f, 0.35f)),
            new BoneBoxPair(HumanBodyBones.RightLowerArm, new Vector2(0.3f, 0.45f)),
            new BoneBoxPair(HumanBodyBones.RightHand, new Vector2(0.3f, 0.55f)),

            new BoneBoxPair(HumanBodyBones.LeftUpperLeg, new Vector2(0.6f, 0.65f)),
            new BoneBoxPair(HumanBodyBones.LeftLowerLeg, new Vector2(0.6f, 0.8f)),
            new BoneBoxPair(HumanBodyBones.LeftFoot, new Vector2(0.65f, 0.85f)),

            new BoneBoxPair(HumanBodyBones.RightUpperLeg, new Vector2(0.4f, 0.65f)),
            new BoneBoxPair(HumanBodyBones.RightLowerLeg, new Vector2(0.4f, 0.8f)),
            new BoneBoxPair(HumanBodyBones.RightFoot, new Vector2(0.35f, 0.85f)),
        };
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

        float height = position.height;
        float width = position.width;
        foreach(var bone in boneBoxPairs) {
            KeyPoseBone.Draw(bone.boneId, new Vector2(width * bone.position.x, height * bone.position.y));
        }
    }
}
