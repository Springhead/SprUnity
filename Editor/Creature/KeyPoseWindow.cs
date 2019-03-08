using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SprUnity;
using SprCs;

public class KeyPoseStatus {
    public KeyPoseInterpolationGroup keyPose;
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
    public KeyPoseStatus(KeyPoseInterpolationGroup keyPose) {
        this.keyPose = keyPose;
        this.status = Status.None;
    }
    // 選択状態と編集状態は違うとして
    public bool isSelected;
}

public class KeyPoseWindow : EditorWindow, IHasCustomMenu {

    //
    static KeyPoseWindow window;

    // 表示用のリスト
    //List<KeyPoseStatus> singleKeyPoses = new List<KeyPoseStatus>();
    //List<KeyPoseStatus> pluralKeyPoses = new List<KeyPoseStatus>();

    static Texture2D noneButtonTexture;
    static Texture2D visibleButtonTexture;
    static Texture2D editableButtonTexture;

    private Vector2 scrollPos;
    private Vector2 scrollPosParameterWindow;

    static private float parameterWindowHeight = 160;

    [MenuItem("Window/KeyPose Window")]
    static void Open() {
        window = GetWindow<KeyPoseWindow>();
        ActionEditorWindowManager.instance.keyPoseWindow = KeyPoseWindow.window;
        GetKeyPoses();
        window.minSize = new Vector2(200, 300);
    }

    public void AddItemsToMenu(GenericMenu menu) {
        menu.AddItem(new GUIContent("Reload"), false, () => {
            Open();
        });
    }

    public void OnEnable() {
        Open();
        visibleButtonTexture = EditorGUIUtility.IconContent("ClothInspector.ViewValue").image as Texture2D;
        //visibleButtonTexture = EditorGUIUtility.Load("ViewToolOrbit") as Texture2D;
        editableButtonTexture = EditorGUIUtility.Load("ViewToolMove") as Texture2D;
        SceneView.onSceneGUIDelegate += OnSceneGUI;
    }

    public void OnDisable() {
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
        window = null;
        ActionEditorWindowManager.instance.keyPoseWindow = null;
    }

    void OnGUI() {
        GUILayout.BeginArea(new Rect(0, 0, position.width, position.height - parameterWindowHeight));
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        GUILayout.Label("KeyPoses");
        if (window == null) GUILayout.Label("window null");
        if (ActionEditorWindowManager.instance.keyPoseWindow == null) GUILayout.Label("Manager.keyPoseWindow null");
        var body = ActionEditorWindowManager.instance.body;
        float windowWidth = this.position.width;
        Color defaultColor = GUI.backgroundColor;
        int nSingle = ActionEditorWindowManager.instance.singleKeyPoses.Count;
        for(int i = 0; i < nSingle; i++) {
            var singleKeyPose = ActionEditorWindowManager.instance.singleKeyPoses[i];
            //Rect singleRect = GUILayoutUtility.GetRect(windowWidth, 30);
            //GUILayout.BeginArea(singleRect);
            GUILayout.BeginHorizontal();
            Texture2D currentTexture = noneButtonTexture;
            if (singleKeyPose.status == KeyPoseStatus.Status.None) currentTexture = noneButtonTexture;
            if (singleKeyPose.status == KeyPoseStatus.Status.Visible) currentTexture = visibleButtonTexture;
            if (singleKeyPose.status == KeyPoseStatus.Status.Editable) currentTexture = editableButtonTexture;
            if (singleKeyPose.isSelected) GUI.backgroundColor = Color.red;
            if(GUILayout.Button(currentTexture, GUILayout.Width(20), GUILayout.Height(20))) {
                if (singleKeyPose.status == KeyPoseStatus.Status.None) singleKeyPose.status = KeyPoseStatus.Status.Visible;
                else if (singleKeyPose.status == KeyPoseStatus.Status.Visible) { singleKeyPose.status = KeyPoseStatus.Status.Editable; Selection.activeObject = singleKeyPose.keyPose.keyposes[0]; }
                else if (singleKeyPose.status == KeyPoseStatus.Status.Editable) singleKeyPose.status = KeyPoseStatus.Status.None;
            }
            GUI.backgroundColor = defaultColor;
            GUILayout.Label(singleKeyPose.keyPose.name);
            singleKeyPose.keyPose.hotKey = (KeyCode)EditorGUILayout.EnumPopup(singleKeyPose.keyPose.hotKey, GUILayout.Width(60));
            if (GUILayout.Button("Play", GUILayout.Width(60))) {
                if (EditorApplication.isPlaying) {
                    singleKeyPose.keyPose.Action(body);
                }
            }
            //singleKeyPose.status = (KeyPoseStatus.Status)EditorGUILayout.EnumPopup(singleKeyPose.status);
            GUILayout.EndHorizontal();
            //GUILayout.EndArea();
            if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition)) {
                if(Event.current.type == EventType.MouseDown) {
                    if(Event.current.button == 0) {
                        singleKeyPose.isSelected = !singleKeyPose.isSelected;
                        Repaint();
                    }
                }
            }
            ActionEditorWindowManager.instance.singleKeyPoses[i] = singleKeyPose;
        }
        if(GUILayout.Button("Add KeyPoseCurrent")) {
            AddKeyPose();
        }
        if (GUILayout.Button("Integrate KeyPoseCurrent")) {
            IntegrateKeyPoses();
        }
        if (ActionEditorWindowManager.instance.pluralKeyPoses.Count > 0) {
            GUILayout.Space(10);
            GUILayout.Label("Interpolation");
            int nPlural = ActionEditorWindowManager.instance.pluralKeyPoses.Count;
            for (int i = 0; i < nPlural; i++) {
                var pluralKeyPose = ActionEditorWindowManager.instance.pluralKeyPoses[i];
                GUILayout.BeginHorizontal();
                Texture2D currentTexture = noneButtonTexture;
                if (pluralKeyPose.status == KeyPoseStatus.Status.None) currentTexture = noneButtonTexture;
                if (pluralKeyPose.status == KeyPoseStatus.Status.Visible) currentTexture = visibleButtonTexture;
                if (pluralKeyPose.status == KeyPoseStatus.Status.Editable) currentTexture = editableButtonTexture;
                if (pluralKeyPose.isSelected) GUI.backgroundColor = Color.red;
                if (GUILayout.Button(currentTexture, GUILayout.Width(20), GUILayout.Height(20))) {
                    if (pluralKeyPose.status == KeyPoseStatus.Status.None) pluralKeyPose.status = KeyPoseStatus.Status.Visible;
                    else if (pluralKeyPose.status == KeyPoseStatus.Status.Visible) pluralKeyPose.status = KeyPoseStatus.Status.Editable;
                    else if (pluralKeyPose.status == KeyPoseStatus.Status.Editable) pluralKeyPose.status = KeyPoseStatus.Status.None;
                }
                GUI.backgroundColor = defaultColor;
                GUILayout.Label(pluralKeyPose.keyPose.name);
                pluralKeyPose.keyPose.hotKey = (KeyCode)EditorGUILayout.EnumPopup(pluralKeyPose.keyPose.hotKey, GUILayout.Width(60));
                //singleKeyPose.status = (KeyPoseStatus.Status)EditorGUILayout.EnumPopup(singleKeyPose.status);
                GUILayout.EndHorizontal();
                if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition)) {
                    if (Event.current.type == EventType.MouseDown) {
                        if (Event.current.button == 0) {
                            pluralKeyPose.isSelected = !pluralKeyPose.isSelected;
                        }
                    }
                }
                ActionEditorWindowManager.instance.pluralKeyPoses[i] = pluralKeyPose;
            }
        }

        if (GUILayout.Button("Separate KeyPoseCurrent")) {
            SeparateKeyPose();
        }
        GUILayout.EndScrollView();
        GUILayout.EndArea();

        //Rect parameterWindow = GUILayoutUtility.GetRect(position.width, 120);
        Rect parameterWindow = new Rect(0, position.height - parameterWindowHeight, position.width, parameterWindowHeight);
        DrawParameters(parameterWindow, body);
    }

    public static void OnSceneGUI(SceneView sceneView) {
        var body = ActionEditorWindowManager.instance.body;
        if(body == null) { body = GameObject.FindObjectOfType<Body>(); }
        //var target = ActionEditorWindowManager.instance.targetObject;
        foreach (var keyPoseGroup in ActionEditorWindowManager.instance.singleKeyPoses) {
            if (keyPoseGroup.status == KeyPoseStatus.Status.Editable) {
                KeyPose keyPose = keyPoseGroup.keyPose.keyposes[0];
                var boneKeyPoseFromLocal = keyPose.GetBoneKeyPoses(body);
                foreach (var boneKeyPose in boneKeyPoseFromLocal) {
                    var parentTransform = body[boneKeyPose.coordinateParent].transform;
                    Pose baseTransform = new Pose(parentTransform.position, parentTransform.rotation);
                    if (boneKeyPose.usePosition) {
                        EditorGUI.BeginChangeCheck();
                        Vector3 position = Handles.PositionHandle(boneKeyPose.position, Quaternion.identity);
                        if (EditorGUI.EndChangeCheck()) {
                            Undo.RecordObject(keyPose, "Change KeyPose Target Position");
                            if (boneKeyPose.coordinateMode == BoneKeyPose.CoordinateMode.World) {
                                // worldはBoneLocal->World(or BodyLocal)
                                boneKeyPose.position = position;
                                boneKeyPose.ConvertWorldToBoneLocal();
                            } else if(boneKeyPose.coordinateMode == BoneKeyPose.CoordinateMode.BoneBaseLocal) {
                                // localならWorld(or BoneLocal)->Local
                                boneKeyPose.position = position;
                                boneKeyPose.ConvertWorldToBoneLocal(body);
                            } else if(boneKeyPose.coordinateMode == BoneKeyPose.CoordinateMode.BodyLocal) {
                                boneKeyPose.position = position;
                                boneKeyPose.ConvertWorldToBodyLocal(body);
                            }
                            EditorUtility.SetDirty(keyPose);
                        }
                    }

                    if (boneKeyPose.useRotation) {
                        EditorGUI.BeginChangeCheck();
                        Quaternion rotation = Handles.RotationHandle(boneKeyPose.rotation, boneKeyPose.position);
                        if (EditorGUI.EndChangeCheck()) {
                            Undo.RecordObject(keyPose, "Change KeyPose Target Rotation");
                            if (boneKeyPose.coordinateMode == BoneKeyPose.CoordinateMode.World) {
                                // target存在する場合はBoneLocal->World(or BodyLocal)
                                boneKeyPose.rotation = rotation;
                                boneKeyPose.ConvertWorldToBoneLocal();
                            } else if(boneKeyPose.coordinateMode == BoneKeyPose.CoordinateMode.BoneBaseLocal) {
                                // target存在しないならWorld(or BoneLocal)->Local
                                boneKeyPose.rotation = rotation;
                                boneKeyPose.ConvertWorldToBoneLocal();
                            } else if (boneKeyPose.coordinateMode == BoneKeyPose.CoordinateMode.BodyLocal) {
                                // target存在しないならWorld(or BoneLocal)->Local
                                boneKeyPose.rotation = rotation;
                                boneKeyPose.ConvertWorldToBodyLocal(body);
                            }
                            EditorUtility.SetDirty(keyPose);
                        }
                    }
                }
            }
        }
    }

    public void DrawParameters(Rect displayRect, Body body) {
        // <!!> たぶん同じKeyPoseのHnadleが二つ表示される事態になっている？
        //      片方はKeyPoseのデフォルトのもの、もう片方はこちらで表示したもの
        // どう考えてもselectedは保存しとくべきか？
        List<KeyPose> keyPoses = new List<KeyPose>();
        foreach(var singleKeyPose in ActionEditorWindowManager.instance.singleKeyPoses) {
            if (singleKeyPose.isSelected) keyPoses.Add(singleKeyPose.keyPose.keyposes[0]);
        }
        foreach (var pluralKeyPose in ActionEditorWindowManager.instance.pluralKeyPoses) {
            if (pluralKeyPose.isSelected) {
                for (int i = 0; i < pluralKeyPose.keyPose.keyposes.Count; i++) {
                    keyPoses.Add(pluralKeyPose.keyPose.keyposes[i]);
                }
            }
        }

        GUILayout.BeginArea(displayRect);
        scrollPosParameterWindow = GUILayout.BeginScrollView(scrollPosParameterWindow);
        if (keyPoses.Count != 0) {
            for (int j = 0; j < keyPoses.Count; j++) {
                KeyPose keyPose = keyPoses[j];
                GUILayout.Label(keyPose.name + " Parameters");
                EditorGUI.BeginChangeCheck();
                for (int i = 0; i < keyPose.boneKeyPoses.Count; i++) {
                    GUI.changed = false;
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(keyPose.boneKeyPoses[i].boneId.ToString(), GUILayout.Width(0.33f * displayRect.width));
                    keyPose.boneKeyPoses[i].usePosition = GUILayout.Toggle(keyPose.boneKeyPoses[i].usePosition, "", GUILayout.Width(15));
                    keyPose.boneKeyPoses[i].useRotation = GUILayout.Toggle(keyPose.boneKeyPoses[i].useRotation, "", GUILayout.Width(15));
                    keyPose.boneKeyPoses[i].coordinateMode = (BoneKeyPose.CoordinateMode)EditorGUILayout.EnumPopup(keyPose.boneKeyPoses[i].coordinateMode, GUILayout.Width(0.33f * displayRect.width));
                    var tempParentBone = (HumanBodyBones)EditorGUILayout.EnumPopup(keyPose.boneKeyPoses[i].coordinateParent, GUILayout.Width(0.33f * displayRect.width));
                    if (tempParentBone != keyPose.boneKeyPoses[i].coordinateParent) {
                        keyPose.boneKeyPoses[i].ConvertBoneLocalToOtherBoneLocal(body, keyPose.boneKeyPoses[i].coordinateParent, tempParentBone);
                    }
                    if (GUI.changed) EditorUtility.SetDirty(keyPose);
                    GUILayout.EndHorizontal();
                }
            }
        }
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    public static void GetKeyPoses() {
        if (!ActionEditorWindowManager.instance.keyPoseWindow) return;
        // Asset全検索
        var guids = AssetDatabase.FindAssets("t:KeyPoseInterpolationGroup");
        // 特定フォルダ
        // var keyPosesInFolder = AssetDatabase.FindAssets("t:KeyPoseInterpolationGroup", saveFolder);

        ActionEditorWindowManager.instance.singleKeyPoses = new List<KeyPoseStatus>();
        ActionEditorWindowManager.instance.pluralKeyPoses = new List<KeyPoseStatus>();

        foreach (var guid in guids) {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var keyPoseGroup = AssetDatabase.LoadAssetAtPath<KeyPoseInterpolationGroup>(path);
            var keyPoseStatus = new KeyPoseStatus(keyPoseGroup);
            if(keyPoseGroup.keyposes.Count == 1) {
                ActionEditorWindowManager.instance.singleKeyPoses.Add(keyPoseStatus);
            } else {
                ActionEditorWindowManager.instance.pluralKeyPoses.Add(keyPoseStatus);
            }
        }
    }

    void AddKeyPose() {
        var keyPoseGroup = KeyPoseInterpolationGroup.CreateKeyPoseGroup();
        keyPoseGroup.keyposes[0].InitializeByCurrentPose(ActionEditorWindowManager.instance.body);
        ActionEditorWindowManager.instance.singleKeyPoses.Add(new KeyPoseStatus(keyPoseGroup));
        Open();
    }

    void IntegrateKeyPoses() {
        List<KeyPoseInterpolationGroup> baseKeyPoses = new List<KeyPoseInterpolationGroup>();
        var singles = ActionEditorWindowManager.instance.singleKeyPoses;
        foreach(var keyPose in singles) {
            // とりあえずStatusを使うがisSelectedにする
            if(keyPose.isSelected) {
                baseKeyPoses.Add(keyPose.keyPose);
            }
        }
        var integratedKeyPose = KeyPoseInterpolationGroup.IntegrateKeyPoseGroup(baseKeyPoses);
        if (integratedKeyPose == null) return;
        KeyPoseStatus newKeyPoseStatus = new KeyPoseStatus(integratedKeyPose);
        ActionEditorWindowManager.instance.pluralKeyPoses.Add(newKeyPoseStatus);
        Open();
    }

    void RemoveKeyPose() {

    }

    public void SeparateKeyPose() {
        KeyPoseStatus baseKeyPoseStatus = new KeyPoseStatus();
        KeyPoseInterpolationGroup baseKeyPose = new KeyPoseInterpolationGroup();
        var plurals = ActionEditorWindowManager.instance.pluralKeyPoses;
        int count = 0;
        foreach(var plural in plurals) {
            if (plural.isSelected) {
                baseKeyPoseStatus = plural;
                baseKeyPose = plural.keyPose;
                count++;
            }
        }
        if (count != 1) {
            Debug.LogWarning("SeparateKeyPose: two or more keyposes are selected");
            return;
        }
        List<KeyPoseInterpolationGroup> newKeyPoses = baseKeyPose.SeparateKeyPoseGroup();
        foreach(var newKeyPose in newKeyPoses) {
            ActionEditorWindowManager.instance.singleKeyPoses.Add(new KeyPoseStatus(newKeyPose));
        }
        ActionEditorWindowManager.instance.pluralKeyPoses.Remove(baseKeyPoseStatus);
        Open();
    }
}
