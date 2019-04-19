using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using SprCs;

namespace SprUnity {

    public class KeyPoseStatus {
        public KeyPoseData keyPose;
        public enum Status {
            None,
            Visible,
            Editable
        }
        public Status status;
        public KeyPoseStatus() {
            this.keyPose = null;
            this.status = Status.None;
        }
        public KeyPoseStatus(KeyPoseData keyPose) {
            this.keyPose = keyPose;
            this.status = Status.None;
        }
        // 選択状態と編集状態は違うとして
        public bool isSelected;
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

        private Vector2 scrollPos;
        private Vector2 scrollPosParameterWindow;

        static private float parameterWindowHeight = 160;

        private GUISkin myskin;
        private string skinpath = "GUISkins/SprGUISkin.guiskin";
        private string editableButtonpath = "pictures/te.png";

        static KeyPoseData latestEditableKeyPose;

        static float scrollwidth = 20;
        static float parameterheight = 150;
        static float buttonheight = 25;

        static KeyPoseData recordKeyPose;
        static BoneKeyPose recordBoneKeyPose;

        private Material mat;
        private Mesh leftHand;
        private Mesh rightHand;
        private Mesh head;
        private Mesh leftFoot;
        private Mesh rightFoot;

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
            if (editableButtonTexture == null) {
                var mono = MonoScript.FromScriptableObject(this);
                var scriptpath = AssetDatabase.GetAssetPath(mono);
                scriptpath = scriptpath.Replace("KeyPoseWindow.cs", "");
                var bytes = System.IO.File.ReadAllBytes(scriptpath + editableButtonpath);
                if (bytes != null) {
                    editableButtonTexture = new Texture2D(1, 1);
                    editableButtonTexture.LoadImage(System.IO.File.ReadAllBytes(scriptpath + editableButtonpath));
                    editableButtonTexture.filterMode = FilterMode.Bilinear;
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

            mat = AssetDatabase.LoadAssetAtPath(modelpath + "clear.mat", typeof(Material)) as Material;
            if (mat == null) {
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
            Color defaultColor = GUI.backgroundColor;
            foreach (var keyPoseGroupStatus in ActionEditorWindowManager.instance.keyPoseGroupStatuses) {
                GUILayout.Label(keyPoseGroupStatus.keyPoseGroup.name, GUILayout.Width(windowWidth - scrollwidth));
                EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(windowWidth - scrollwidth));
                foreach (var keyPoseStatus in keyPoseGroupStatus.keyPoseStatuses) {
                    //Rect singleRect = GUILayoutUtility.GetRect(windowWidth, 30);
                    //GUILayout.BeginArea(singleRect);
                    GUILayout.BeginHorizontal();
                    Texture2D currentTexture = noneButtonTexture;
                    if (keyPoseStatus.status == KeyPoseStatus.Status.None) currentTexture = noneButtonTexture;
                    if (keyPoseStatus.status == KeyPoseStatus.Status.Visible) currentTexture = visibleButtonTexture;
                    if (keyPoseStatus.status == KeyPoseStatus.Status.Editable) currentTexture = editableButtonTexture;
                    if (keyPoseStatus.isSelected) GUI.backgroundColor = Color.red;
                    if (GUILayout.Button(currentTexture, GUILayout.Width(buttonheight), GUILayout.Height(buttonheight))) {
                        if (keyPoseStatus.status == KeyPoseStatus.Status.None) {
                            keyPoseStatus.status = KeyPoseStatus.Status.Visible;
                        } else if (keyPoseStatus.status == KeyPoseStatus.Status.Visible) {
                            keyPoseStatus.status = KeyPoseStatus.Status.Editable;
                            // SceneView.onSceneGUIDelegateがエディタ上で選択中の項目が変更された時に呼び出されるため選択を変更する必要がある
                            Selection.activeObject = keyPoseStatus.keyPose;
                            latestEditableKeyPose = keyPoseStatus.keyPose;
                            // 他の編集モードのKeyPoseをVisibleにする
                            foreach (var keyPoseGroupStatus2 in ActionEditorWindowManager.instance.keyPoseGroupStatuses) {
                                foreach (var keyPoseStatus2 in keyPoseGroupStatus2.keyPoseStatuses) {
                                    if (keyPoseStatus2.status == KeyPoseStatus.Status.Editable && keyPoseStatus2.keyPose != latestEditableKeyPose) {
                                        keyPoseStatus2.status = KeyPoseStatus.Status.Visible;
                                    }
                                }
                            }
                            //Selection.objects = new Object[] { keyPoseStatus.keyPose };
                        } else if (keyPoseStatus.status == KeyPoseStatus.Status.Editable) {
                            keyPoseStatus.status = KeyPoseStatus.Status.None;
                            if (Selection.activeObject == keyPoseStatus.keyPose) {
                                Selection.activeObject = null;
                                latestEditableKeyPose = null;
                            }
                        }
                    }
                    GUI.backgroundColor = defaultColor;
                    GUILayout.Label(keyPoseStatus.keyPose.name, GUILayout.Height(buttonheight));
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

            GUI.skin = null; // 他のwindowに影響が出ないように元に戻す
        }

        public void DrawParameters(Rect displayRect, Body body) {
            // <!!> たぶん同じKeyPoseのHnadleが二つ表示される事態になっている？
            //      片方はKeyPoseのデフォルトのもの、もう片方はこちらで表示したもの
            // どう考えてもselectedは保存しとくべきか？
            List<KeyPoseData> keyPoses = new List<KeyPoseData>();
            foreach (var keyPoseGroupStatus in ActionEditorWindowManager.instance.keyPoseGroupStatuses) {
                foreach (var keyPoseStatus in keyPoseGroupStatus.keyPoseStatuses) {
                    if (keyPoseStatus.status == KeyPoseStatus.Status.Editable) keyPoses.Add(keyPoseStatus.keyPose);
                }
            }
            GUILayout.FlexibleSpace(); //これで一番下に表示できる
            EditorGUILayout.BeginVertical();
            if (keyPoses.Count != 0) {
                for (int j = 0; j < keyPoses.Count; j++) {
                    KeyPoseData keyPose = keyPoses[j];
                    GUILayout.Label(keyPose.name + " Parameters");

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("name", GUILayout.Width(0.25f * displayRect.width));
                    GUILayout.Label("pos", GUILayout.Width(0.08f * displayRect.width));
                    GUILayout.Label("rot", GUILayout.Width(0.08f * displayRect.width));
                    GUILayout.Label("coordinate", GUILayout.Width(0.25f * displayRect.width));
                    GUILayout.Label("dependent", GUILayout.Width(0.25f * displayRect.width));
                    GUILayout.EndHorizontal();

                    EditorGUI.BeginChangeCheck();
                    for (int i = 0; i < keyPose.boneKeyPoses.Count; i++) {
                        GUI.changed = false;
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(keyPose.boneKeyPoses[i].boneId.ToString(), GUILayout.Width(0.25f * displayRect.width));
                        keyPose.boneKeyPoses[i].usePosition = GUILayout.Toggle(keyPose.boneKeyPoses[i].usePosition, "", GUILayout.Width(0.08f * displayRect.width));
                        keyPose.boneKeyPoses[i].useRotation = GUILayout.Toggle(keyPose.boneKeyPoses[i].useRotation, "", GUILayout.Width(0.08f * displayRect.width));
                        keyPose.boneKeyPoses[i].coordinateMode = (BoneKeyPose.CoordinateMode)EditorGUILayout.EnumPopup(keyPose.boneKeyPoses[i].coordinateMode, GUILayout.Width(0.25f * displayRect.width));
                        var tempParentBone = (BONES)EditorGUILayout.EnumPopup((BONES)keyPose.boneKeyPoses[i].coordinateParent, GUILayout.Width(0.25f * displayRect.width));
                        if ((HumanBodyBones)tempParentBone != keyPose.boneKeyPoses[i].coordinateParent) {
                            keyPose.boneKeyPoses[i].ConvertBoneLocalToOtherBoneLocal(body, keyPose.boneKeyPoses[i].coordinateParent, (HumanBodyBones)tempParentBone);
                        }
                        if (GUI.changed) EditorUtility.SetDirty(keyPose);
                        GUILayout.EndHorizontal();
                    }
                }
            }
            EditorGUILayout.EndVertical();
            GUILayoutUtility.GetRect(new GUIContent(string.Empty), GUIStyle.none, GUILayout.Height(10));
        }

        void OnSceneGUI(SceneView sceneView) {
            DrawHuman();
            // KeyPoseDataに書いてあるのでなしでよいはず

            //var body = ActionEditorWindowManager.instance.body;
            //if (body == null) { body = GameObject.FindObjectOfType<Body>(); }
            //if (body == null) return;
            ////var target = ActionEditorWindowManager.instance.targetObject;
            //if (latestEditableKeyPose == null) {
            //    return;
            //}
            //// 他のハンドルが表示されると二重になるので他のハンドルを表示しない
            //if (Tools.current != Tool.None && Tools.current != Tool.View) {
            //    Tools.current = Tool.None;
            //}

            //var boneKeyPoseFromLocal = latestEditableKeyPose.GetBoneKeyPoses(body);
            //foreach (var boneKeyPose in boneKeyPoseFromLocal) {
            //    var parentTransform = body[boneKeyPose.coordinateParent].transform;
            //    Pose baseTransform = new Pose(parentTransform.position, parentTransform.rotation);
            //    if (boneKeyPose.usePosition) {
            //        EditorGUI.BeginChangeCheck();
            //        //Vector3 position = Handles.PositionHandle(boneKeyPose.position, Quaternion.identity);
            //        Vector3 position = AxisMove(boneKeyPose.position, boneKeyPose.rotation, 1);
            //        if (EditorGUI.EndChangeCheck()) {
            //            Undo.RecordObject(latestEditableKeyPose, "Change KeyPose Target Position");
            //            if (boneKeyPose.coordinateMode == BoneKeyPose.CoordinateMode.World) {
            //                // worldはBoneLocal->World(or BodyLocal)
            //                boneKeyPose.position = position;
            //                boneKeyPose.ConvertWorldToBoneLocal();
            //            } else if (boneKeyPose.coordinateMode == BoneKeyPose.CoordinateMode.BoneBaseLocal) {
            //                // localならWorld(or BoneLocal)->Local
            //                boneKeyPose.position = position;
            //                boneKeyPose.ConvertWorldToBoneLocal(body);
            //            } else if (boneKeyPose.coordinateMode == BoneKeyPose.CoordinateMode.BodyLocal) {
            //                boneKeyPose.position = position;
            //                boneKeyPose.ConvertWorldToBodyLocal(body);
            //            }
            //            EditorUtility.SetDirty(latestEditableKeyPose);
            //        }
            //    }

            //    if (boneKeyPose.useRotation) {
            //        EditorGUI.BeginChangeCheck();
            //        Quaternion rotation = Handles.RotationHandle(boneKeyPose.rotation, boneKeyPose.position);
            //        if (EditorGUI.EndChangeCheck()) {
            //            Undo.RecordObject(latestEditableKeyPose, "Change KeyPose Target Rotation");
            //            if (boneKeyPose.coordinateMode == BoneKeyPose.CoordinateMode.World) {
            //                // target存在する場合はBoneLocal->World(or BodyLocal)
            //                boneKeyPose.rotation = rotation;
            //                boneKeyPose.ConvertWorldToBoneLocal();
            //            } else if (boneKeyPose.coordinateMode == BoneKeyPose.CoordinateMode.BoneBaseLocal) {
            //                // target存在しないならWorld(or BoneLocal)->Local
            //                boneKeyPose.rotation = rotation;
            //                boneKeyPose.ConvertWorldToBoneLocal();
            //            } else if (boneKeyPose.coordinateMode == BoneKeyPose.CoordinateMode.BodyLocal) {
            //                // target存在しないならWorld(or BoneLocal)->Local
            //                boneKeyPose.rotation = rotation;
            //                boneKeyPose.ConvertWorldToBodyLocal(body);
            //            }
            //            EditorUtility.SetDirty(latestEditableKeyPose);
            //        }
            //    }
            //}
        }

        void DrawHuman() {
            foreach (var keyPoseGroupStatus in ActionEditorWindowManager.instance.keyPoseGroupStatuses) {
                foreach (var keyPoseStatus in keyPoseGroupStatus.keyPoseStatuses) {
                    if (keyPoseStatus.status == KeyPoseStatus.Status.Editable) {
                        foreach (var boneKeyPose in keyPoseStatus.keyPose.boneKeyPoses) {
                            // 調整用の手などを表示
                            mat.SetPass(0); // 1だと影しか見えない？ 
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
            }
        }

        public static void ReloadKeyPoseList() {
            if (!ActionEditorWindowManager.instance.keyPoseWindow) return;
            // Asset全検索
            var guids = AssetDatabase.FindAssets("*").Distinct();
            // 特定フォルダ
            // var keyPosesInFolder = AssetDatabase.FindAssets("t:KeyPoseInterpolationGroup", saveFolder);

            ActionEditorWindowManager.instance.keyPoseGroupStatuses = new List<KeyPoseGroupStatus>();

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

    }

}