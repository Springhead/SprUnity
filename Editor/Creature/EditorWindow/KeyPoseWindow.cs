using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using SprCs;

namespace SprUnity {

    public class KeyPoseStatus {
        public ActionTargetGraph keyPose;
        public bool isVisible;
        public bool isEditable;
        public KeyPoseStatus() {
            this.keyPose = null;
            this.isVisible = false;
            this.isEditable = false;
        }
        public KeyPoseStatus(ActionTargetGraph keyPose) {
            this.keyPose = keyPose;
            this.isVisible = false;
            this.isEditable = false;
        }
    }
    public enum BONES {
        Head = HumanBodyBones.Head,
        Neck = HumanBodyBones.Neck,

        Chest = HumanBodyBones.Chest,
        Spine = HumanBodyBones.Spine,
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

        private ActionTargetGraph latestEditableKeyPose;
        private ActionTargetGraph latestVisibleKeyPose;
        private static Dictionary<KeyPoseStatus, Rect> keyPoseDataRectDict;

        static float scrollwidth = 20;
        static float parameterheight = 150;
        static float buttonheight = 25;

        private static ActionTargetGraph recordKeyPose;
        private static StaticBoneKeyPose recordBoneKeyPose;
        private static ActionTargetGraph renameKeyPose;
        private string renaming;
        private static KeyPoseDataGroup renameKeyPoseGroup;
        private string grouprenaming;

        private Material editableMat, visibleMat;
        private Mesh leftHand;
        private Mesh rightHand;
        private Mesh head;
        private Mesh leftFoot;
        private Mesh rightFoot;

        private float handleSize = 0.05f;
        private float selectedHandleSize = 0.15f;
        private StaticBoneKeyPose selectedboneKeyPose; // マウスが上にあるKeyPoseだけハンドルを大きくする

        [MenuItem("Window/SprUnity Action/KeyPose Window")]
        static void Open() {
            window = GetWindow<KeyPoseWindow>();
            ActionEditorWindowManager.instance.keyPoseWindow = KeyPoseWindow.window;
            ReloadKeyPoseList();
            window.minSize = new Vector2(250, 300);
            window.titleContent = new GUIContent("KeyPose");
        }

        public void AddItemsToMenu(GenericMenu menu) {
            menu.AddItem(new GUIContent("Reload"), false, () => {
                ReloadKeyPoseList();
            });
        }

        public void OnEnable() {
            //ReloadKeyPoseList();
            //visibleButtonTexture = EditorGUIUtility.IconContent("ClothInspector.ViewValue").image as Texture2D;
            visibleButtonTexture = EditorGUIUtility.Load("ViewToolOrbit On") as Texture2D;
            GetEditableTexture();
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
            Event e = Event.current;
            GUISkin s = GUI.skin;
            if (myskin != null) {
                GUI.skin = myskin;
            } 

            float windowWidth = this.position.width;
            // 縦スクロールが出た場合に下に横スクロールが出るのを防ぐ
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.Height(position.height - parameterheight));

            var keyPoseStatuses = ActionEditorWindowManager.instance.keyPoseStatuses;

            GUILayout.Label("KeyPoses", GUILayout.Width(windowWidth - scrollwidth));
            if (window == null) {
                Open(); // なぜかOnEnableに書くと新しくwindowが生成される
                // 選択が消えてしまうので残っている情報からフラグを正しくする
                // latest系がstaticにできないのでReloadKeyPoseList内に書けない(staticにするとプレイすると初期化される)
                foreach (var keyPoseStatus in keyPoseStatuses) {
                        if (keyPoseStatus.keyPose == latestEditableKeyPose) {
                            keyPoseStatus.isEditable = true;
                        }
                        if (keyPoseStatus.keyPose == latestVisibleKeyPose) {
                            keyPoseStatus.isVisible = true;
                        }
                }
            }
            var body = ActionEditorWindowManager.instance.body;
                EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(windowWidth - scrollwidth));
                foreach (var keyPoseStatus in keyPoseStatuses) {
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
                                foreach (var keyPoseStatus2 in keyPoseStatuses) {
                                    if (keyPoseStatus2.isVisible && keyPoseStatus2.keyPose != latestVisibleKeyPose) {
                                        keyPoseStatus2.isVisible = false;
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
                    var defaultback = GUI.skin.label.normal.background;
                    if (keyPoseStatus.isEditable) {
                        GUI.skin.label.normal.background = GetEditableTexture();
                    }
                    if (keyPoseStatus.keyPose == renameKeyPose) {
                        renaming = GUILayout.TextField(renaming, GUILayout.Height(buttonheight));
                        if (Event.current.keyCode == KeyCode.Return) {
                            Undo.RecordObject(keyPoseStatus.keyPose, "Change KeyPose Name");
                            renameKeyPose.name = renaming;
                            renameKeyPose = null;
                            renaming = "";
                            EditorUtility.SetDirty(keyPoseStatus.keyPose);
                            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(keyPoseStatus.keyPose));
                            Repaint();
                        }
                    } else {
                        GUILayout.Label(keyPoseStatus.keyPose.name, GUILayout.Height(buttonheight));
                    }
                    GUI.skin.label.normal.background = defaultback;
                    // <!!>毎回呼ぶのか..
                    keyPoseDataRectDict[keyPoseStatus] = GUILayoutUtility.GetLastRect();
                    GUILayout.EndHorizontal();
                    RightClickMenu(GUILayoutUtility.GetLastRect(), keyPoseStatus.keyPose);
                }
                EditorGUILayout.EndVertical();

            if (GUILayout.Button("Add KeyPoseGroup", GUILayout.Height(buttonheight))) {
                AddKeyPoseGroup();
            }
            EditorGUILayout.EndScrollView();

            GUILayout.Box("", GUILayout.Width(this.position.width - 10), GUILayout.Height(1));
            Rect parameterWindow = new Rect(0, position.height / 2,
                position.width, position.height - parameterWindowHeight);

            DrawParameters(parameterWindow, body);

            // LayoutとRepaintの時で同じGUIになるようにここで変更？
            foreach (var pair in keyPoseDataRectDict) {
                LeftClick(pair.Value, pair.Key);
            }
            GUI.skin = s; // 他のwindowに影響が出ないように元に戻す
        }

        public void DrawParameters(Rect displayRect, Body body) {
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
                /*
                for (int i = 0; i < latestEditableKeyPose.boneKeyPoses.Count(); i++) {
                    GUI.changed = false;
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(latestEditableKeyPose.boneKeyPoses[i].boneId.ToString(), GUILayout.Width(0.25f * displayRect.width));
                    latestEditableKeyPose.boneKeyPoses[i].usePosition = GUILayout.Toggle(latestEditableKeyPose.boneKeyPoses[i].usePosition, "", GUILayout.Width(0.08f * displayRect.width));
                    latestEditableKeyPose.boneKeyPoses[i].useRotation = GUILayout.Toggle(latestEditableKeyPose.boneKeyPoses[i].useRotation, "", GUILayout.Width(0.08f * displayRect.width));
                    var tempcoordinateMode = (StaticBoneKeyPose.CoordinateMode)EditorGUILayout.EnumPopup(latestEditableKeyPose.boneKeyPoses[i].coordinateMode, GUILayout.Width(0.25f * displayRect.width));
                    var precoodinateMode = latestEditableKeyPose.boneKeyPoses[i].coordinateMode;
                    latestEditableKeyPose.boneKeyPoses[i].coordinateMode = tempcoordinateMode;
                    // <!!>BodyLocalからWorldやWorldからBodyLocalはどうすべきか
                    if (precoodinateMode == StaticBoneKeyPose.CoordinateMode.World && tempcoordinateMode == StaticBoneKeyPose.CoordinateMode.BoneLocal) {
                        latestEditableKeyPose.boneKeyPoses[i].ConvertWorldToBoneLocal();
                    } else if (precoodinateMode == StaticBoneKeyPose.CoordinateMode.BodyLocal && tempcoordinateMode == StaticBoneKeyPose.CoordinateMode.BoneLocal) {
                        latestEditableKeyPose.boneKeyPoses[i].ConvertBodyLocalToBoneLocal();
                    } else if (precoodinateMode == StaticBoneKeyPose.CoordinateMode.BodyLocal && tempcoordinateMode == StaticBoneKeyPose.CoordinateMode.World) {
                        latestEditableKeyPose.boneKeyPoses[i].ConvertBodyLocalToWorld();
                    } else if (precoodinateMode == StaticBoneKeyPose.CoordinateMode.World && tempcoordinateMode == StaticBoneKeyPose.CoordinateMode.BodyLocal) {
                        latestEditableKeyPose.boneKeyPoses[i].ConvertWorldToBodyLocal();
                    } else if (precoodinateMode == StaticBoneKeyPose.CoordinateMode.BoneLocal && tempcoordinateMode == StaticBoneKeyPose.CoordinateMode.BodyLocal) {
                        latestEditableKeyPose.boneKeyPoses[i].ConvertBoneLocalToBodyLocal();
                    } else if (precoodinateMode == StaticBoneKeyPose.CoordinateMode.BoneLocal && tempcoordinateMode == StaticBoneKeyPose.CoordinateMode.World) {
                        latestEditableKeyPose.boneKeyPoses[i].ConvertBoneLocalToWorld();
                    }
                    var tempParentBone = (BONES)EditorGUILayout.EnumPopup((BONES)latestEditableKeyPose.boneKeyPoses[i].coordinateParent, GUILayout.Width(0.25f * displayRect.width));
                    if (latestEditableKeyPose.boneKeyPoses[i].coordinateMode == StaticBoneKeyPose.CoordinateMode.BoneLocal &&
                        (HumanBodyBones)tempParentBone != latestEditableKeyPose.boneKeyPoses[i].coordinateParent) {
                        latestEditableKeyPose.boneKeyPoses[i].ConvertBoneLocalToOtherBoneLocal(latestEditableKeyPose.boneKeyPoses[i].coordinateParent, (HumanBodyBones)tempParentBone);
                    }
                    if (GUI.changed) EditorUtility.SetDirty(latestEditableKeyPose);
                    GUILayout.EndHorizontal();
                }
                */
            }
            EditorGUILayout.EndVertical();
            GUILayoutUtility.GetRect(new GUIContent(string.Empty), GUIStyle.none, GUILayout.Height(10));
        }

        void OnSceneGUI(SceneView sceneView) {
            DrawHuman(latestEditableKeyPose, latestVisibleKeyPose);
            DrawHandles();            
        }

        void DrawHuman(ActionTargetGraph latestEditableKeyPose, ActionTargetGraph latestVisibleKeyPose) {
            /*
            if (latestEditableKeyPose != null) {
                foreach (var boneKeyPose in latestEditableKeyPose.boneKeyPoses) {
                    if (!boneKeyPose.usePosition && !boneKeyPose.useRotation) {
                        continue;
                    }
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
            if (latestVisibleKeyPose != null && latestEditableKeyPose != latestVisibleKeyPose) {
                foreach (var boneKeyPose in latestVisibleKeyPose.boneKeyPoses) {
                    if (!boneKeyPose.usePosition && !boneKeyPose.useRotation) {
                        continue;
                    }
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
            */
        }

        void DrawHandles() {
            /*
            if (latestEditableKeyPose) {
                Event e = Event.current;
                var preselected = selectedboneKeyPose;
                // マウスがドラッグ中には選択中のboneKeyPoseを変更しないように
                if (e.type != EventType.MouseDrag && e.type != EventType.Layout && e.type != EventType.Repaint) {
                    Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                    foreach (var boneKeyPose in latestEditableKeyPose.boneKeyPoses) {
                        var Point = SceneViewHandles.intersectPoint(SceneView.lastActiveSceneView.camera.transform.forward,
                            boneKeyPose.position, ray.direction, ray.origin);
                        if ((Point - boneKeyPose.position).magnitude < handleSize) {
                            selectedboneKeyPose = boneKeyPose;
                        }
                    }
                    // 選択中のboneKeyPoseは範囲内だったら選択されたままに
                    if (preselected != null) {
                        var Point = SceneViewHandles.intersectPoint(SceneView.lastActiveSceneView.camera.transform.forward,
                            preselected.position, ray.direction, ray.origin);
                        if ((Point - preselected.position).magnitude < selectedHandleSize) {
                            selectedboneKeyPose = preselected;
                        } else {
                            selectedboneKeyPose = null;
                        }
                    }
                }
                foreach (var boneKeyPose in latestEditableKeyPose.boneKeyPoses) {
                    if (boneKeyPose.usePosition) {
                        EditorGUI.BeginChangeCheck();
                        Vector3 position = new Vector3();
                        if (selectedboneKeyPose != boneKeyPose) {
                            position = SceneViewHandles.AxisMove(boneKeyPose.position, boneKeyPose.localRotation, handleSize);
                        } else {
                            position = SceneViewHandles.AxisMove(boneKeyPose.position, boneKeyPose.localRotation, selectedHandleSize);
                        }
                        if (EditorGUI.EndChangeCheck()) {
                            Undo.RecordObject(latestEditableKeyPose, "Change KeyPose Target Position");
                            boneKeyPose.position = position;
                            EditorUtility.SetDirty(latestEditableKeyPose);
                        }
                    }

                    if (boneKeyPose.useRotation) {
                        EditorGUI.BeginChangeCheck();
                        Quaternion localRotation = new Quaternion();
                        if (selectedboneKeyPose != boneKeyPose) {
                            localRotation = SceneViewHandles.AxisRotate(boneKeyPose.localRotation, boneKeyPose.position, handleSize);
                        } else {
                            localRotation = SceneViewHandles.AxisRotate(boneKeyPose.localRotation, boneKeyPose.position, selectedHandleSize);
                        }
                        if (EditorGUI.EndChangeCheck()) {
                            Undo.RecordObject(latestEditableKeyPose, "Change KeyPose Target Rotation");
                            boneKeyPose.localRotation = localRotation;
                            EditorUtility.SetDirty(latestEditableKeyPose);
                        }
                    }
                }
            }
            */
        }

        public static void ReloadKeyPoseList() {
            if (!ActionEditorWindowManager.instance.keyPoseWindow) return;
            // Asset全検索
            var guids = AssetDatabase.FindAssets("*").Distinct();
            // 特定フォルダ
            // var keyPosesInFolder = AssetDatabase.FindAssets("t:KeyPoseInterpolationGroup", saveFolder);

            ActionEditorWindowManager.instance.keyPoseStatuses = new List<KeyPoseStatus>();
            keyPoseDataRectDict = new Dictionary<KeyPoseStatus, Rect>();

            foreach (var guid in guids) {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                var keyPose = obj as ActionTargetGraph;
                if (keyPose != null) {
                    var keyPoseStatus = new KeyPoseStatus(keyPose);
                    keyPoseDataRectDict.Add(keyPoseStatus, new Rect());
                    ActionEditorWindowManager.instance.keyPoseStatuses.Add(keyPoseStatus);
                }
            }
        }

        void AddKeyPoseGroup() {
            KeyPoseDataGroup.CreateKeyPoseDataGroupAsset();
        }
        void AddKeyPose(KeyPoseDataGroup kpg) {
            //Undo.RecordObject(kpg, "Add KeyPose to " + kpg.name); <!!> Undo出来ない
            kpg.CreateKeyPoseInWin();
        }

        void LeftClick(Rect rect, KeyPoseStatus keyPoseStatus) {
            if (rect.Contains(Event.current.mousePosition + scrollPos) &&
                Event.current.mousePosition.y < position.height - parameterWindowHeight &&
                Event.current.type == EventType.MouseDown &&
                Event.current.button == 0) {
                /*
                keyPoseStatus.isEditable = !keyPoseStatus.isEditable;
                if (keyPoseStatus.isEditable) {
                    latestEditableKeyPose = keyPoseStatus.keyPose;
                    foreach (var keyPoseGroupStatus in ActionEditorWindowManager.instance.keyPoseGroupStatuses) {
                        foreach (var keyPoseStatus2 in keyPoseGroupStatus.keyPoseStatuses) {
                            if (keyPoseStatus2.isEditable && keyPoseStatus2.keyPose != latestEditableKeyPose) {
                                keyPoseStatus2.isEditable = false;
                                selectedboneKeyPose = null;
                            }
                        }
                    }
                } else {
                    latestEditableKeyPose = null;
                }
                */
                Repaint();
                SceneView.RepaintAll();
            }
        }
        void RightClickMenu(Rect rect, ActionTargetGraph KeyPoseNodeGraph) {
            if (rect.Contains(Event.current.mousePosition) &&
                Event.current.type == EventType.MouseDown &&
                Event.current.button == 1) {
                GenericMenu menu = new GenericMenu();
                /*
                foreach (var boneKeyPose in ActionTargetGraph.boneKeyPoses) {
                    menu.AddItem(new GUIContent(boneKeyPose.boneId.ToString()), false,
                        () => {
                            recordBoneKeyPose = boneKeyPose;
                            recordKeyPose = ActionTargetGraph;
                        });
                }
                menu.AddItem(new GUIContent("All"), false,
                    () => {
                        recordBoneKeyPose = null;
                        recordKeyPose = ActionTargetGraph;
                    });
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Paste"), false,
                    () => {
                        if (recordBoneKeyPose != null) { //一部
                            foreach (var boneKeyPose in ActionTargetGraph.boneKeyPoses) {
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
                            foreach (var boneKeyPose in ActionTargetGraph.boneKeyPoses) {
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
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Rename"), false,
                    () => {
                        renameKeyPose = ActionTargetGraph;
                        renaming = ActionTargetGraph.name;
                        Repaint();
                    });
                menu.AddItem(new GUIContent("Delete"), false,
                    () => {
                        RemoveKeyPose(ActionTargetGraph);
                        Repaint();
                        SceneView.RepaintAll();
                    });
                    */
                menu.ShowAsContext();
            }
        }

        void RightClickGroupMenu(Rect rect, KeyPoseDataGroup keyPoseDataGroup) {
            GenericMenu menu = new GenericMenu();
            if (rect.Contains(Event.current.mousePosition) &&
                Event.current.type == EventType.MouseDown &&
                Event.current.button == 1) {
                menu.AddItem(new GUIContent("Rename"), false,
                    () => {
                        renameKeyPoseGroup = keyPoseDataGroup;
                        grouprenaming = keyPoseDataGroup.name;
                        Repaint();
                    });
                menu.ShowAsContext();
            }
        }
        void RemoveKeyPose(ActionTargetGraph KeyPoseNodeGraph) {
            if (KeyPoseNodeGraph == null) {
                Debug.LogWarning("No sub asset.");
                return;
            }

            if (AssetDatabase.IsSubAsset(KeyPoseNodeGraph)) {
                string path = AssetDatabase.GetAssetPath(KeyPoseNodeGraph);
                DestroyImmediate(KeyPoseNodeGraph, true);
                AssetDatabase.ImportAsset(path);
            }
        }

        Texture2D GetEditableTexture() {
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
            return editableLabelTexture;
        }
    }
}