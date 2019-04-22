using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using SprCs;

namespace SprUnity {

    public class KeyPoseStatus {
        public KeyPoseData keyPose;
        public bool isVisible;
        public bool isEditable;
        public KeyPoseStatus() {
            this.keyPose = null;
            this.isVisible = false;
            this.isEditable = false;
        }
        public KeyPoseStatus(KeyPoseData keyPose) {
            this.keyPose = keyPose;
            this.isVisible = false;
            this.isEditable = false;
        }
    }

    public class KeyPoseGroupStatus {
        public KeyPoseDataGroup keyPoseGroup;
        public List<KeyPoseStatus> keyPoseStatuses;
        public KeyPoseGroupStatus() {
            this.keyPoseGroup = null;
            this.keyPoseStatuses = new List<KeyPoseStatus>();
        }
        public KeyPoseGroupStatus(KeyPoseDataGroup keyPoseGroup) {
            this.keyPoseGroup = keyPoseGroup;
            this.keyPoseStatuses = new List<KeyPoseStatus>();
        }
    }
    public enum BONES {
        Head = HumanBodyBones.Head,
        Neck = HumanBodyBones.Neck,

        Chest = HumanBodyBones.Chest,
        Sphin = HumanBodyBones.Spine,
        Hips = HumanBodyBones.Hips,

        LeftShoulder = HumanBodyBones.LeftShoulder,
        LeftUpperArm = HumanBodyBones.LeftUpperArm,
        LeftLowerArm = HumanBodyBones.LeftLowerArm,
        LeftHand = HumanBodyBones.LeftHand,

        RightShoulder = HumanBodyBones.RightShoulder,
        RightUpperArm = HumanBodyBones.RightUpperArm,
        RightLowerArm = HumanBodyBones.RightLowerArm,
        RightHand = HumanBodyBones.RightHand,

        LeftUpperLeg = HumanBodyBones.LeftUpperLeg,
        LeftLowerLeg = HumanBodyBones.LeftLowerLeg,
        LeftFoot = HumanBodyBones.LeftFoot,

        RightUpperLeg = HumanBodyBones.RightUpperLeg,
        RightLowerLeg = HumanBodyBones.RightLowerLeg,
        RightFoot = HumanBodyBones.RightFoot,
    }
    public class KeyPoseWindow : EditorWindow, IHasCustomMenu {

        //
        static KeyPoseWindow window;

        // GUI
        static Texture2D noneButtonTexture;
        static Texture2D visibleButtonTexture;
        static Texture2D editableButtonTexture;
        static Texture2D editableLabelTexture;

        private Vector2 scrollPos;
        private Vector2 scrollPosParameterWindow;

        static private float parameterWindowHeight = 160;

        private GUISkin myskin;
        private string skinpath = "GUISkins/SprGUISkin.guiskin";
        private string editableButtonpath = "pictures/te.png";
        private string editableLabelpath = "GUISkins/labelbackEditable.png";

        private KeyPoseData latestEditableKeyPose;
        private KeyPoseData latestVisibleKeyPose;
        private static Dictionary<KeyPoseStatus, Rect> keyPoseDataRectDict;

        static float scrollwidth = 20;
        static float parameterheight = 150;
        static float buttonheight = 25;

        static KeyPoseData recordKeyPose;
        static BoneKeyPose recordBoneKeyPose;

        private Material editableMat,visibleMat;
        private Mesh leftHand;
        private Mesh rightHand;
        private Mesh head;
        private Mesh leftFoot;
        private Mesh rightFoot;

        private float handleSize = 0.05f;
        private float selectedHandleSize = 0.15f;
        private BoneKeyPose selectedboneKeyPose; // マウスが上にあるKeyPoseだけハンドルを大きくする

        static HumanBodyBones[] bones = {
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
        [MenuItem("Window/KeyPose Window")]
        static void Open() {
            window = GetWindow<KeyPoseWindow>();
            ActionEditorWindowManager.instance.keyPoseWindow = KeyPoseWindow.window;
            ReloadKeyPoseList();
            window.minSize = new Vector2(250, 300);
        }

        public void AddItemsToMenu(GenericMenu menu) {
            menu.AddItem(new GUIContent("Reload"), false, () => {
                ReloadKeyPoseList();
            });
        }

        public void OnEnable() {
            ReloadKeyPoseList();
            //visibleButtonTexture = EditorGUIUtility.IconContent("ClothInspector.ViewValue").image as Texture2D;
            visibleButtonTexture = EditorGUIUtility.Load("ViewToolOrbit On") as Texture2D;
            //if (editableButtonTexture == null) {
            //    var mono = MonoScript.FromScriptableObject(this);
            //    var scriptpath = AssetDatabase.GetAssetPath(mono);
            //    scriptpath = scriptpath.Replace("KeyPoseWindow.cs", "");
            //    var bytes = System.IO.File.ReadAllBytes(scriptpath + editableButtonpath);
            //    if (bytes != null) {
            //        editableButtonTexture = new Texture2D(1, 1);
            //        editableButtonTexture.LoadImage(System.IO.File.ReadAllBytes(scriptpath + editableButtonpath));
            //        editableButtonTexture.filterMode = FilterMode.Bilinear;
            //    } else {
            //        Debug.Log("picture null");
            //    }
            //}
            if (editableLabelTexture == null) {
                var mono = MonoScript.FromScriptableObject(this);
                var scriptpath = AssetDatabase.GetAssetPath(mono);
                scriptpath = scriptpath.Replace("KeyPoseWindow.cs", "");
                var bytes = System.IO.File.ReadAllBytes(scriptpath + editableLabelpath);
                if (bytes != null) {
                    editableLabelTexture = new Texture2D(1, 1);
                    editableLabelTexture.LoadImage(System.IO.File.ReadAllBytes(scriptpath + editableLabelpath));
                    editableLabelTexture.filterMode = FilterMode.Bilinear;
                } else {
                    Debug.Log("picture null");
                }
            }
            if (myskin == null) {
                var mono = MonoScript.FromScriptableObject(this);
                var scriptpath = AssetDatabase.GetAssetPath(mono);
                scriptpath = scriptpath.Replace("KeyPoseWindow.cs", "");
                myskin = AssetDatabase.LoadAssetAtPath<GUISkin>(scriptpath + skinpath);
            }


            var modelpath = "Assets/Libraries/SprUnity/Editor/Creature/Models/";

            editableMat = AssetDatabase.LoadAssetAtPath(modelpath + "editable.mat", typeof(Material)) as Material;
            visibleMat = AssetDatabase.LoadAssetAtPath(modelpath + "visible.mat", typeof(Material)) as Material;
            if (editableMat == null) {
                Debug.Log("mat null");
            }
            if (visibleMat == null) {
                Debug.Log("mat null");
            }

            leftHand = AssetDatabase.LoadAssetAtPath(
                modelpath + "LeftHand.fbx", typeof(Mesh)) as Mesh;
            if (leftHand == null) {
                Debug.Log("fbx null");
            }

            rightHand = AssetDatabase.LoadAssetAtPath(
                modelpath + "RightHand.fbx", typeof(Mesh)) as Mesh;
            if (rightHand == null) {
                Debug.Log("fbx null");
            }

            head = AssetDatabase.LoadAssetAtPath(
                modelpath + "Head.fbx", typeof(Mesh)) as Mesh;
            if (head == null) {
                Debug.Log("fbx null");
            }

            leftFoot = AssetDatabase.LoadAssetAtPath(
                modelpath + "LeftFoot.fbx", typeof(Mesh)) as Mesh;
            if (leftFoot == null) {
                Debug.Log("fbx null");
            }

            rightFoot = AssetDatabase.LoadAssetAtPath(
                modelpath + "RightFoot.fbx", typeof(Mesh)) as Mesh;
            if (rightFoot == null) {
                Debug.Log("fbx null");
            }

            SceneView.onSceneGUIDelegate -= OnSceneGUI;
            SceneView.onSceneGUIDelegate += OnSceneGUI;
        }

        public void OnDisable() {
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
            window = null;
            ActionEditorWindowManager.instance.keyPoseWindow = null;
        }

        void OnGUI() {
            //foreach (var sel in Selection.objects) {
            //    Debug.Log("name = " + sel.name);
            //}
            if (myskin != null) {
                GUI.skin = myskin;
            } else {
                //Debug.Log("GUISkin is null");
            }

            float windowWidth = this.position.width;
            // 縦スクロールが出た場合に下に横スクロールが出るのを防ぐ
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.Height(position.height - parameterheight));

            GUILayout.Label("KeyPoses", GUILayout.Width(windowWidth - scrollwidth));
            if (window == null) {
                Open(); // なぜかOnEnableに書くと新しくwindowが生成される
                if (window == null) {
                    GUILayout.Label("window null");
                }
            }
            if (ActionEditorWindowManager.instance.keyPoseWindow == null) {
                GUILayout.Label("Manager.keyPoseWindow null");
            }
            var body = ActionEditorWindowManager.instance.body;
            foreach (var keyPoseGroupStatus in ActionEditorWindowManager.instance.keyPoseGroupStatuses) {
                GUILayout.Label(keyPoseGroupStatus.keyPoseGroup.name, GUILayout.Width(windowWidth - scrollwidth));
                EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(windowWidth - scrollwidth));
                foreach (var keyPoseStatus in keyPoseGroupStatus.keyPoseStatuses) {
                    //Rect singleRect = GUILayoutUtility.GetRect(windowWidth, 30);
                    //GUILayout.BeginArea(singleRect);
                    GUILayout.BeginHorizontal();
                    Texture2D currentTexture = noneButtonTexture;
                    if (keyPoseStatus.isVisible) {
                        currentTexture = visibleButtonTexture;
                    } else {
                        currentTexture = noneButtonTexture;
                    }
                    if (GUILayout.Button(currentTexture, GUILayout.Width(buttonheight), GUILayout.Height(buttonheight))) {
                        if (!keyPoseStatus.isVisible) {
                            keyPoseStatus.isVisible = true;
                            latestVisibleKeyPose = keyPoseStatus.keyPose;
                            foreach (var keyPoseGroupStatus2 in ActionEditorWindowManager.instance.keyPoseGroupStatuses) {
                                foreach (var keyPoseStatus2 in keyPoseGroupStatus2.keyPoseStatuses) {
                                    if (keyPoseStatus2.isVisible && keyPoseStatus2.keyPose != latestVisibleKeyPose) {
                                        keyPoseStatus2.isVisible = false;
                                    }
                                }
                            }
                            SceneView.RepaintAll();
                        } else {
                            keyPoseStatus.isVisible = false;
                            if (latestVisibleKeyPose == keyPoseStatus.keyPose) {
                                latestVisibleKeyPose = null;
                            }
                            SceneView.RepaintAll();
                        }
                    }
                    if (keyPoseStatus.isEditable) {
                        var defaultback = GUI.skin.label.normal.background;
                        GUI.skin.label.normal.background = editableLabelTexture;
                        GUILayout.Label(keyPoseStatus.keyPose.name, GUILayout.Height(buttonheight));
                        GUI.skin.label.normal.background = defaultback;
                    } else {
                        GUILayout.Label(keyPoseStatus.keyPose.name, GUILayout.Height(buttonheight));
                    }
                    keyPoseDataRectDict[keyPoseStatus] = GUILayoutUtility.GetLastRect();
                    if (GUILayout.Button("Play", GUILayout.Width(60), GUILayout.Height(buttonheight))) {
                        if (EditorApplication.isPlaying) {
                            // @とりあえずコメントアウト
                            keyPoseStatus.keyPose.Action(body);
                        }
                    }
                    //singleKeyPose.status = (KeyPoseStatus.Status)EditorGUILayout.EnumPopup(singleKeyPose.status);
                    GUILayout.EndHorizontal();
                    //GUILayout.EndArea();
                    RightClickMenu(GUILayoutUtility.GetLastRect(), keyPoseStatus.keyPose);
                    //if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition)) {
                    //    if (Event.current.type == EventType.MouseDown) {
                    //        if (Event.current.button == 0) {
                    //            keyPoseStatus.isSelected = !keyPoseStatus.isSelected;
                    //            Repaint();
                    //        }
                    //    }
                    //}
                }
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Add KeyPoseCurrent", GUILayout.Height(buttonheight))) {
                    AddKeyPose(keyPoseGroupStatus.keyPoseGroup);
                }
                GUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndScrollView();

            GUI.skin = null;
            GUILayout.Box("", GUILayout.Width(this.position.width - 10), GUILayout.Height(1));
            Rect parameterWindow = new Rect(0, position.height / 2,
                position.width, position.height - parameterWindowHeight);
            DrawParameters(parameterWindow, body);

            // LayoutとRepaintの時で同じGUIになるようにここで変更？
            foreach (var pair in keyPoseDataRectDict) {
                LeftClick(pair.Value, pair.Key);
            }
            GUI.skin = null; // 他のwindowに影響が出ないように元に戻す
        }

        public void DrawParameters(Rect displayRect, Body body) {
            // <!!> たぶん同じKeyPoseのHnadleが二つ表示される事態になっている？
            //      片方はKeyPoseのデフォルトのもの、もう片方はこちらで表示したもの
            // どう考えてもselectedは保存しとくべきか？
            GUILayout.FlexibleSpace(); //これで一番下に表示できる
            EditorGUILayout.BeginVertical();
            if (latestEditableKeyPose != null) {
                GUILayout.Label(latestEditableKeyPose.name + " Parameters");

                GUILayout.BeginHorizontal();
                GUILayout.Label("name", GUILayout.Width(0.25f * displayRect.width));
                GUILayout.Label("pos", GUILayout.Width(0.08f * displayRect.width));
                GUILayout.Label("rot", GUILayout.Width(0.08f * displayRect.width));
                GUILayout.Label("coordinate", GUILayout.Width(0.25f * displayRect.width));
                GUILayout.Label("dependent", GUILayout.Width(0.25f * displayRect.width));
                GUILayout.EndHorizontal();

                EditorGUI.BeginChangeCheck();
                for (int i = 0; i < latestEditableKeyPose.boneKeyPoses.Count; i++) {
                    GUI.changed = false;
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(latestEditableKeyPose.boneKeyPoses[i].boneId.ToString(), GUILayout.Width(0.25f * displayRect.width));
                    latestEditableKeyPose.boneKeyPoses[i].usePosition = GUILayout.Toggle(latestEditableKeyPose.boneKeyPoses[i].usePosition, "", GUILayout.Width(0.08f * displayRect.width));
                    latestEditableKeyPose.boneKeyPoses[i].useRotation = GUILayout.Toggle(latestEditableKeyPose.boneKeyPoses[i].useRotation, "", GUILayout.Width(0.08f * displayRect.width));
                    var tempcoordinateMode = (BoneKeyPose.CoordinateMode)EditorGUILayout.EnumPopup(latestEditableKeyPose.boneKeyPoses[i].coordinateMode, GUILayout.Width(0.25f * displayRect.width));
                    // <!!>BodyLocalからWorldやWorldからBodyLocalはどうすべきか
                    if (latestEditableKeyPose.boneKeyPoses[i].coordinateMode == BoneKeyPose.CoordinateMode.World && tempcoordinateMode == BoneKeyPose.CoordinateMode.BoneLocal) {
                        latestEditableKeyPose.boneKeyPoses[i].ConvertWorldToBoneLocal();
                    } else if (latestEditableKeyPose.boneKeyPoses[i].coordinateMode == BoneKeyPose.CoordinateMode.BodyLocal && tempcoordinateMode == BoneKeyPose.CoordinateMode.BoneLocal) {
                        latestEditableKeyPose.boneKeyPoses[i].ConvertBodyLocalToBoneLocal();
                    } else if (latestEditableKeyPose.boneKeyPoses[i].coordinateMode == BoneKeyPose.CoordinateMode.BodyLocal && tempcoordinateMode == BoneKeyPose.CoordinateMode.World) {
                        latestEditableKeyPose.boneKeyPoses[i].ConvertBodyLocalToWorld();
                    } else if (latestEditableKeyPose.boneKeyPoses[i].coordinateMode == BoneKeyPose.CoordinateMode.World && tempcoordinateMode == BoneKeyPose.CoordinateMode.BodyLocal) {
                        latestEditableKeyPose.boneKeyPoses[i].ConvertWorldToBodyLocal();
                    } else if (latestEditableKeyPose.boneKeyPoses[i].coordinateMode == BoneKeyPose.CoordinateMode.BoneLocal && tempcoordinateMode == BoneKeyPose.CoordinateMode.BodyLocal) {
                        latestEditableKeyPose.boneKeyPoses[i].ConvertBoneLocalToBodyLocal();
                    } else if (latestEditableKeyPose.boneKeyPoses[i].coordinateMode == BoneKeyPose.CoordinateMode.BoneLocal && tempcoordinateMode == BoneKeyPose.CoordinateMode.World) {
                        latestEditableKeyPose.boneKeyPoses[i].ConvertBoneLocalToWorld();
                    }
                    latestEditableKeyPose.boneKeyPoses[i].coordinateMode = tempcoordinateMode;
                    var tempParentBone = (BONES)EditorGUILayout.EnumPopup((BONES)latestEditableKeyPose.boneKeyPoses[i].coordinateParent, GUILayout.Width(0.25f * displayRect.width));
                    if ((HumanBodyBones)tempParentBone != latestEditableKeyPose.boneKeyPoses[i].coordinateParent) {
                        latestEditableKeyPose.boneKeyPoses[i].ConvertBoneLocalToOtherBoneLocal(latestEditableKeyPose.boneKeyPoses[i].coordinateParent, (HumanBodyBones)tempParentBone);
                    }
                    if (GUI.changed) EditorUtility.SetDirty(latestEditableKeyPose);
                    GUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndVertical();
            GUILayoutUtility.GetRect(new GUIContent(string.Empty), GUIStyle.none, GUILayout.Height(10));
        }

        void OnSceneGUI(SceneView sceneView) {
            DrawHuman(latestEditableKeyPose, latestVisibleKeyPose);

            if (latestEditableKeyPose) {
                Event e = Event.current;
                var preselected = selectedboneKeyPose;
                // マウスがドラッグ中には選択中のboneKeyPoseを変更しないように
                if (e.type != EventType.MouseDrag && e.type != EventType.Layout && e.type != EventType.Repaint) {
                    Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                    foreach (var boneKeyPose in latestEditableKeyPose.boneKeyPoses) {
                        var Point = intersectPoint(SceneView.lastActiveSceneView.camera.transform.forward,
                            boneKeyPose.position, ray.direction, ray.origin);
                        if ((Point - boneKeyPose.position).magnitude < handleSize) {
                            selectedboneKeyPose = boneKeyPose;
                        } else {
                        }
                    }
                    // 選択中のboneKeyPoseは範囲内だったら選択されたままに
                    if (preselected != null) {
                        var Point = intersectPoint(SceneView.lastActiveSceneView.camera.transform.forward,
                            preselected.position, ray.direction, ray.origin);
                        if ((Point - preselected.position).magnitude < selectedHandleSize) {
                            selectedboneKeyPose = preselected;
                        } else {
                        }
                    }
                }
                foreach (var boneKeyPose in latestEditableKeyPose.boneKeyPoses) {
                    if (boneKeyPose.usePosition) {
                        EditorGUI.BeginChangeCheck();
                        Vector3 position = new Vector3();
                        if (selectedboneKeyPose != boneKeyPose) {
                            position = AxisMove(boneKeyPose.position, boneKeyPose.localRotation, handleSize);
                        } else {
                            position = AxisMove(boneKeyPose.position, boneKeyPose.localRotation, selectedHandleSize);
                        }
                        if (EditorGUI.EndChangeCheck()) {
                            Undo.RecordObject(latestEditableKeyPose, "Change KeyPose Target Position");
                            boneKeyPose.position = position;
                            EditorUtility.SetDirty(latestEditableKeyPose);
                        }
                    }

                    if (boneKeyPose.useRotation) {
                        EditorGUI.BeginChangeCheck();
                        Quaternion rotation = new Quaternion();
                        if (selectedboneKeyPose != boneKeyPose) {
                            rotation = AxisRotate(boneKeyPose.localRotation, boneKeyPose.position, handleSize);
                        } else {
                            rotation = AxisRotate(boneKeyPose.localRotation, boneKeyPose.position, selectedHandleSize);
                        }
                        if (EditorGUI.EndChangeCheck()) {
                            Undo.RecordObject(latestEditableKeyPose, "Change KeyPose Target Rotation");
                            boneKeyPose.rotation = rotation;
                            EditorUtility.SetDirty(latestEditableKeyPose);
                        }
                    }
                }
            }
        }

        /// xyz軸のDiscハンドルを生成する
        public static Quaternion AxisRotate(Quaternion rotation, Vector3 position, float size) {
            var rotationMatrix = Matrix4x4.TRS(Vector3.zero, rotation.normalized, Vector3.one);
            if (Event.current.type == EventType.Repaint) {
                Transform sceneCamT = SceneView.lastActiveSceneView.camera.transform;
                Handles.color = new Color(1, 1, 1, 0.5f);
                Handles.CircleHandleCap(
                    10,
                    position,
                    //Quaternion.LookRotation(sceneCamT.position,position),
                    sceneCamT.rotation,
                    size,
                    EventType.Repaint
                );
            }

            Handles.color = Handles.xAxisColor;
            rotation = Handles.Disc(rotation, position, rotationMatrix.MultiplyPoint(Vector3.right), size, true, size);
            Handles.color = Handles.yAxisColor;
            rotation = Handles.Disc(rotation, position, rotationMatrix.MultiplyPoint(Vector3.up), size, true, size);
            Handles.color = Handles.zAxisColor;
            rotation = Handles.Disc(rotation, position, rotationMatrix.MultiplyPoint(Vector3.forward), size, true, size);
            Handles.color = new Color(1, 1, 1, 0.5f);
            rotation = Handles.FreeRotateHandle(rotation, position, size * 1.1f);
            return rotation;
        }
        /// xyz軸のFreeMoveハンドルを生成する
        public static Vector3 AxisMove(Vector3 position, Quaternion rotation, float sizeS) {

            var rotationMatrix = Matrix4x4.TRS(Vector3.zero, rotation.normalized, Vector3.one);
            var dirX = rotationMatrix.MultiplyPoint(Vector3.right);
            var dirY = rotationMatrix.MultiplyPoint(Vector3.up);
            var dirZ = rotationMatrix.MultiplyPoint(Vector3.forward);
            var snap = Vector3.one;
            snap.x = EditorPrefs.GetFloat("MoveSnapX", 1.0f);
            snap.y = EditorPrefs.GetFloat("MoveSnapY", 1.0f);
            snap.z = EditorPrefs.GetFloat("MoveSnapZ", 1.0f);

            // FreeMove
            var handleCapPosOffset = Vector3.zero;
            var handleCapEuler = rotation.eulerAngles;

            var handleSize = sizeS * 0.13f;

            Handles.CapFunction RectangleHandleCap2D = (id, pos, rot, size, eventType) => {
                Handles.RectangleHandleCap(id, pos + rotationMatrix.MultiplyPoint(handleCapPosOffset), rotation * Quaternion.Euler(handleCapEuler), size, eventType);
            };
            Handles.color = Handles.zAxisColor;
            handleCapPosOffset = new Vector3(1.0f, 1.0f, 0.0f) * handleSize;
            handleCapEuler = Vector3.zero;
            var movePoint = Handles.FreeMoveHandle(position, rotation, handleSize, snap, RectangleHandleCap2D);
            // XY平面上の近傍点を新しい位置とする
            if (SceneView.lastActiveSceneView.camera.orthographic) {
                position = intersectPoint(dirZ, position,
                    SceneView.lastActiveSceneView.camera.transform.forward, movePoint);
            } else {
                position = intersectPoint(dirZ, position, movePoint -
                    SceneView.lastActiveSceneView.camera.transform.position, movePoint);
            }

            Handles.color = Handles.yAxisColor;
            handleCapPosOffset = new Vector3(1.0f, 0.0f, 1.0f) * handleSize;
            handleCapEuler = new Vector3(90.0f, 0.0f, 0.0f);
            movePoint = Handles.FreeMoveHandle(position, rotation, handleSize, snap, RectangleHandleCap2D);
            // XZ平面上の近傍点を新しい位置とする
            if (SceneView.lastActiveSceneView.camera.orthographic) {
                position = intersectPoint(dirY, position,
                    SceneView.lastActiveSceneView.camera.transform.forward, movePoint);
            } else {
                position = intersectPoint(dirY, position, movePoint -
                    SceneView.lastActiveSceneView.camera.transform.position, movePoint);
            }

            Handles.color = Handles.xAxisColor;
            handleCapPosOffset = new Vector3(0.0f, 1.0f, 1.0f) * handleSize;
            handleCapEuler = new Vector3(0.0f, 90.0f, 0.0f);
            movePoint = Handles.FreeMoveHandle(position, rotation, handleSize, snap, RectangleHandleCap2D);
            // YZ平面上の近傍点を新しい位置とする
            if (SceneView.lastActiveSceneView.camera.orthographic) {
                position = intersectPoint(dirX, position,
                    SceneView.lastActiveSceneView.camera.transform.forward, movePoint);
            } else {
                position = intersectPoint(dirX, position, movePoint -
                    SceneView.lastActiveSceneView.camera.transform.position, movePoint);
            }

            Handles.color = Handles.xAxisColor;
            position = Handles.Slider(position, rotationMatrix.MultiplyPoint(Vector3.right), sizeS, Handles.ArrowHandleCap, sizeS); //X 軸
            Handles.color = Handles.yAxisColor;
            position = Handles.Slider(position, rotationMatrix.MultiplyPoint(Vector3.up), sizeS, Handles.ArrowHandleCap, sizeS); //Y 軸
            Handles.color = Handles.zAxisColor;
            position = Handles.Slider(position, rotationMatrix.MultiplyPoint(Vector3.forward), sizeS, Handles.ArrowHandleCap, sizeS); //Z 軸
                                                                                                                                      // Slider
            return position;
        }

        /* 線と平面の交点を求める
        *https://qiita.com/edo_m18/items/c8808f318f5abfa8af1e
        */
        static Vector3 intersectPoint(Vector3 n, Vector3 x, Vector3 m, Vector3 x0) {
            var h = Vector3.Dot(n, x);
            return x0 + ((h - Vector3.Dot(n, x0)) / (Vector3.Dot(n, m))) * m;
        }
    
        void DrawHuman(KeyPoseData latestEditableKeyPose, KeyPoseData latestVisibleKeyPose) {
            if (latestEditableKeyPose != null) {
                foreach (var boneKeyPose in latestEditableKeyPose.boneKeyPoses) {
                    // 調整用の手などを表示
                    editableMat.SetPass(0); // 1だと影しか見えない？ 
                    if (boneKeyPose.boneId == HumanBodyBones.LeftHand) {
                        Graphics.DrawMeshNow(leftHand, boneKeyPose.position, boneKeyPose.rotation.normalized, 0);
                    } else if (boneKeyPose.boneId == HumanBodyBones.RightHand) {
                        Graphics.DrawMeshNow(rightHand, boneKeyPose.position, boneKeyPose.rotation.normalized, 0);
                    } else if (boneKeyPose.boneId == HumanBodyBones.Head) {
                        Graphics.DrawMeshNow(head, boneKeyPose.position, boneKeyPose.rotation.normalized, 0);
                    } else if (boneKeyPose.boneId == HumanBodyBones.LeftFoot) {
                        Graphics.DrawMeshNow(leftFoot, boneKeyPose.position, boneKeyPose.rotation.normalized, 0);
                    } else if (boneKeyPose.boneId == HumanBodyBones.RightFoot) {
                        Graphics.DrawMeshNow(rightFoot, boneKeyPose.position, boneKeyPose.rotation.normalized, 0);
                    }
                }
            }
            if (latestVisibleKeyPose != null) {
                foreach (var boneKeyPose in latestVisibleKeyPose.boneKeyPoses) {
                    // 調整用の手などを表示
                    visibleMat.SetPass(0); // 1だと影しか見えない？ 
                    if (boneKeyPose.boneId == HumanBodyBones.LeftHand) {
                        Graphics.DrawMeshNow(leftHand, boneKeyPose.position, boneKeyPose.rotation.normalized, 0);
                    } else if (boneKeyPose.boneId == HumanBodyBones.RightHand) {
                        Graphics.DrawMeshNow(rightHand, boneKeyPose.position, boneKeyPose.rotation.normalized, 0);
                    } else if (boneKeyPose.boneId == HumanBodyBones.Head) {
                        Graphics.DrawMeshNow(head, boneKeyPose.position, boneKeyPose.rotation.normalized, 0);
                    } else if (boneKeyPose.boneId == HumanBodyBones.LeftFoot) {
                        Graphics.DrawMeshNow(leftFoot, boneKeyPose.position, boneKeyPose.rotation.normalized, 0);
                    } else if (boneKeyPose.boneId == HumanBodyBones.RightFoot) {
                        Graphics.DrawMeshNow(rightFoot, boneKeyPose.position, boneKeyPose.rotation.normalized, 0);
                    }
                }
            }
        }

        public static void ReloadKeyPoseList() {
            if (!ActionEditorWindowManager.instance.keyPoseWindow) return;
            // Asset全検索
            var guids = AssetDatabase.FindAssets("*").Distinct();
            // 特定フォルダ
            // var keyPosesInFolder = AssetDatabase.FindAssets("t:KeyPoseInterpolationGroup", saveFolder);

            ActionEditorWindowManager.instance.keyPoseGroupStatuses = new List<KeyPoseGroupStatus>();
            keyPoseDataRectDict = new Dictionary<KeyPoseStatus, Rect>();

            foreach (var guid in guids) {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                var keyPoseGroup = obj as KeyPoseDataGroup;
                if (keyPoseGroup != null) {
                    var keyPoseGroupStatus = new KeyPoseGroupStatus(keyPoseGroup);
                    if (keyPoseGroup.GetSubAssets() != null) {
                        foreach (var keyPose in keyPoseGroup.GetSubAssets()) {
                            // KeyPoseGroupも含まれるためnullチェック
                            if (keyPose as KeyPoseData == null) {
                                continue;
                            }
                            var keyPoseStatus = new KeyPoseStatus(keyPose as KeyPoseData);
                            keyPoseDataRectDict.Add(keyPoseStatus, new Rect());
                            keyPoseGroupStatus.keyPoseStatuses.Add(keyPoseStatus);
                        }
                        ActionEditorWindowManager.instance.keyPoseGroupStatuses.Add(keyPoseGroupStatus);
                    }
                }
            }
        }

        void AddKeyPose(KeyPoseDataGroup kpg) {
            //var keyPoseGroup = KeyPoseInterpolationGroup.CreateKeyPoseGroup();
            //keyPoseGroup.keyposes[0].InitializeByCurrentPose(ActionEditorWindowManager.instance.body);
            //ActionEditorWindowManager.instance.singleKeyPoses.Add(new KeyPoseStatus(keyPoseGroup));
            //Open();
            kpg.CreateKeyPoseInWin();
        }

        void LeftClick(Rect rect, KeyPoseStatus keyPoseStatus) {
            if (rect.Contains(Event.current.mousePosition+scrollPos) && Event.current.type == EventType.MouseDown && Event.current.button == 0) {
                keyPoseStatus.isEditable = !keyPoseStatus.isEditable;
                if (keyPoseStatus.isEditable) {
                    latestEditableKeyPose = keyPoseStatus.keyPose;
                    foreach (var keyPoseGroupStatus in ActionEditorWindowManager.instance.keyPoseGroupStatuses) {
                        foreach (var keyPoseStatus2 in keyPoseGroupStatus.keyPoseStatuses) {
                            if (keyPoseStatus2.isEditable && keyPoseStatus2.keyPose != latestEditableKeyPose) {
                                keyPoseStatus2.isEditable = false;
                            }
                        }
                    }
                } else {
                    latestEditableKeyPose = null;
                }
                Repaint();
                SceneView.RepaintAll();
            }
        }
        void RightClickMenu(Rect rect, KeyPoseData keyPoseData) {
            if (rect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown && Event.current.button == 1) {
                GenericMenu menu = new GenericMenu();
                foreach (var boneKeyPose in keyPoseData.boneKeyPoses) {
                    menu.AddItem(new GUIContent(boneKeyPose.boneId.ToString()), false,
                        () => {
                            recordBoneKeyPose = boneKeyPose;
                            recordKeyPose = keyPoseData;
                        });
                }
                menu.AddItem(new GUIContent("All"), false,
                    () => {
                        recordBoneKeyPose = null;
                        recordKeyPose = keyPoseData;
                    });
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Paste"), false,
                    () => {
                        if (recordBoneKeyPose != null) { //一部
                            foreach (var boneKeyPose in keyPoseData.boneKeyPoses) {
                                if (boneKeyPose.boneId == recordBoneKeyPose.boneId) {
                                    boneKeyPose.usePosition = recordBoneKeyPose.usePosition;
                                    boneKeyPose.useRotation = recordBoneKeyPose.usePosition;
                                    boneKeyPose.coordinateMode = recordBoneKeyPose.coordinateMode;
                                    boneKeyPose.coordinateParent = recordBoneKeyPose.coordinateParent;
                                    boneKeyPose.position = recordBoneKeyPose.position;
                                    boneKeyPose.rotation = recordBoneKeyPose.rotation;
                                    break;
                                }
                            }
                        } else if (recordKeyPose != null) { //All
                            foreach (var boneKeyPose in keyPoseData.boneKeyPoses) {
                                foreach (var record in recordKeyPose.boneKeyPoses) {
                                    if (boneKeyPose.boneId == record.boneId) {
                                        boneKeyPose.usePosition = record.usePosition;
                                        boneKeyPose.useRotation = record.usePosition;
                                        boneKeyPose.coordinateMode = record.coordinateMode;
                                        boneKeyPose.coordinateParent = record.coordinateParent;
                                        boneKeyPose.position = record.position;
                                        boneKeyPose.rotation = record.rotation;
                                    }
                                }
                            }
                        }
                        Repaint();
                        SceneView.RepaintAll();
                    });
                menu.ShowAsContext();
            }
        }

        void RemoveKeyPose() {

        }

        // KeyPoseDataを変換する
        void HipsPlus() {
            var body = GameObject.FindObjectOfType<Body>();

            foreach (var keyPoseGroupStatus in ActionEditorWindowManager.instance.keyPoseGroupStatuses) {
                foreach (var keyPoseStatus in keyPoseGroupStatus.keyPoseStatuses) {
                    foreach (var boneKeyPose in keyPoseStatus.keyPose.boneKeyPoses) {
                        Debug.Log(keyPoseStatus.keyPose.name);
                        boneKeyPose.localPosition = boneKeyPose.localPosition + body[HumanBodyBones.Hips].transform.position;
                    }
                }
            }
        }
    }

}