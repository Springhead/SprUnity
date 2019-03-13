using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SprUnity {

    public class KeyPoseBone {
        public static float radius = 15;
        //public Vector2 position;
        //HumanBodyBones bone;
        static public void Draw(HumanBodyBones avatarBone, Vector2 position) {
            Event e = Event.current;
            var bone = ActionEditorWindowManager.instance.body[avatarBone];
            if (bone == null || bone.controller == null) {
                GUI.Box(new Rect(position - new Vector2(radius, radius), new Vector2(2 * radius, 2 * radius)), "D");
                return;
            } else {
                if ((e.mousePosition - position).magnitude < radius) {
                    GUIStyle style = GUI.skin.GetStyle("Box");
                    GUI.backgroundColor = new Color(0, 1, 0.7f, 0.5f);
                }
                string text = bone.controller.controlPosition ? "P" : "";
                text = text + (bone.controller.controlRotation ? "R" : "");
                GUI.Box(new Rect(position - new Vector2(radius, radius), new Vector2(2 * radius, 2 * radius)), text);
            }
            if (e.type == EventType.MouseDown) {
                if (e.button == 0) {
                    if ((e.mousePosition - position).magnitude < radius) {
                        var cp = bone.controller.controlPosition;
                        var cr = bone.controller.controlRotation;
                        // PR→P→R→なし
                        if (cp && cr) {
                            bone.controller.controlRotation = false;
                        } else if (cp) {
                            bone.controller.controlRotation = true;
                            bone.controller.controlPosition = false;
                        } else if (cr) {
                            bone.controller.controlRotation = false;
                            bone.controller.controlPosition = false;
                        } else {
                            bone.controller.controlPosition = true;
                            bone.controller.controlRotation = true;
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
        private Texture2D texture = null;
        private string path = "pictures/default.png";

        [MenuItem("Window/KeyPoseBone Window")]
        static void Open() {
            window = GetWindow<KeyPoseBoneWindow>();
            ActionEditorWindowManager.instance.keyPoseBoneWindow = window;
            boneBoxPairs = new BoneBoxPair[]{
            new BoneBoxPair(HumanBodyBones.Head, new Vector2(0.5f, 0.12f)),
            new BoneBoxPair(HumanBodyBones.Neck, new Vector2(0.5f, 0.18f)),
            
            // UpperChestなし？
            new BoneBoxPair(HumanBodyBones.Chest, new Vector2(0.5f, 0.341f)),
            new BoneBoxPair(HumanBodyBones.Spine, new Vector2(0.5f, 0.4f)),
            new BoneBoxPair(HumanBodyBones.Hips, new Vector2(0.5f, 0.45f)),

            new BoneBoxPair(HumanBodyBones.LeftShoulder, new Vector2(0.567f, 0.217f)),
            new BoneBoxPair(HumanBodyBones.LeftUpperArm, new Vector2(0.633f, 0.23f)),
            new BoneBoxPair(HumanBodyBones.LeftLowerArm, new Vector2(0.74f, 0.34f)),
            new BoneBoxPair(HumanBodyBones.LeftHand, new Vector2(0.855f, 0.44f)),

            new BoneBoxPair(HumanBodyBones.RightShoulder, new Vector2(0.433f, 0.217f)),
            new BoneBoxPair(HumanBodyBones.RightUpperArm, new Vector2(0.367f, 0.23f)),
            new BoneBoxPair(HumanBodyBones.RightLowerArm, new Vector2(0.26f, 0.34f)),
            new BoneBoxPair(HumanBodyBones.RightHand, new Vector2(0.1455f, 0.44f)),

            new BoneBoxPair(HumanBodyBones.LeftUpperLeg, new Vector2(0.59f, 0.5f)),
            new BoneBoxPair(HumanBodyBones.LeftLowerLeg, new Vector2(0.565f, 0.698f)),
            new BoneBoxPair(HumanBodyBones.LeftFoot, new Vector2(0.555f, 0.937f)),

            new BoneBoxPair(HumanBodyBones.RightUpperLeg, new Vector2(0.41f, 0.5f)),
            new BoneBoxPair(HumanBodyBones.RightLowerLeg, new Vector2(0.435f, 0.698f)),
            new BoneBoxPair(HumanBodyBones.RightFoot, new Vector2(0.45f, 0.937f)),
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
            GUIStyle style = GUI.skin.GetStyle("Box");
            int backFontSize = style.fontSize; // 他のウィンドウに影響しないように
            TextAnchor backAlignment = style.alignment;
            Color backbackColor = GUI.backgroundColor;

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
            if (texture == null) {
                var mono = MonoScript.FromScriptableObject(this);
                var scriptpass = AssetDatabase.GetAssetPath(mono);
                scriptpass = scriptpass.Replace("KeyPoseBoneWindow.cs", "");
                var bytes = System.IO.File.ReadAllBytes(scriptpass + path);
                if (bytes != null) {
                    texture = new Texture2D(1, 1);
                    texture.LoadImage(System.IO.File.ReadAllBytes(scriptpass + path));
                    texture.filterMode = FilterMode.Bilinear;
                } else {
                    Debug.Log("picture null");
                }
            }
            if (texture != null) {
                EditorGUI.DrawPreviewTexture(new Rect(0, 0, width, height), texture);
            }
            style.fontSize = (int)(width + height) / 100;
            style.alignment = TextAnchor.MiddleCenter;
            KeyPoseBone.radius = (width + height) / 80;
            foreach (var bone in boneBoxPairs) {
                GUI.backgroundColor = new Color(0, 1, 0.7f, 0);
                KeyPoseBone.Draw(bone.boneId, new Vector2(width * bone.position.x, height * bone.position.y));
            }

            style.fontSize = backFontSize;
            style.alignment = backAlignment;
            GUI.backgroundColor = backbackColor;
        }
    }

}