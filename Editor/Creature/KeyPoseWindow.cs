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


        [MenuItem("Window/KeyPose Window")]
        static void Open() {
            window = GetWindow<KeyPoseWindow>();
            ActionEditorWindowManager.instance.keyPoseWindow = KeyPoseWindow.window;
            GetKeyPoses();
            window.minSize = new Vector2(250, 300);
        }

        public void AddItemsToMenu(GenericMenu menu) {
            menu.AddItem(new GUIContent("Reload"), false, () => {
                GetKeyPoses();
            });
        }

        public void OnEnable() {
            GetKeyPoses();
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
                Debug.Log("GUISkin is null");
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
                    EditorGUI.BeginChangeCheck();
                    for (int i = 0; i < keyPose.boneKeyPoses.Count; i++) {
                        GUI.changed = false;
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(keyPose.boneKeyPoses[i].boneId.ToString(), GUILayout.Width(0.27f * displayRect.width));
                        keyPose.boneKeyPoses[i].usePosition = GUILayout.Toggle(keyPose.boneKeyPoses[i].usePosition, "", GUILayout.Width(15));
                        keyPose.boneKeyPoses[i].useRotation = GUILayout.Toggle(keyPose.boneKeyPoses[i].useRotation, "", GUILayout.Width(15));
                        keyPose.boneKeyPoses[i].coordinateMode = (BoneKeyPose.CoordinateMode)EditorGUILayout.EnumPopup(keyPose.boneKeyPoses[i].coordinateMode, GUILayout.Width(0.25f * displayRect.width));
                        var tempParentBone = (HumanBodyBones)EditorGUILayout.EnumPopup(keyPose.boneKeyPoses[i].coordinateParent, GUILayout.Width(0.25f * displayRect.width));
                        if (tempParentBone != keyPose.boneKeyPoses[i].coordinateParent) {
                            keyPose.boneKeyPoses[i].ConvertBoneLocalToOtherBoneLocal(body, keyPose.boneKeyPoses[i].coordinateParent, tempParentBone);
                        }
                        if (GUI.changed) EditorUtility.SetDirty(keyPose);
                        GUILayout.EndHorizontal();
                    }
                }
            }
            EditorGUILayout.EndVertical();
            GUILayoutUtility.GetRect(new GUIContent(string.Empty), GUIStyle.none, GUILayout.Height(10));
        }
        public static void OnSceneGUI(SceneView sceneView) {
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

        public static void GetKeyPoses() {
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

        void RemoveKeyPose() {

        }

    }

}