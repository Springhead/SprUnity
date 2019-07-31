// c.f.) https://forum.unity.com/threads/custom-add-component-like-button.439730/

using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Threading;

namespace UnityEditorAddon {

    public class AddScriptWindow : EditorWindow {

        static AddScriptWindow _instance;
        static Styles _styles;

        Action<MonoScript> CreateScriptDelegate;
        Func<MonoScript, bool> FilerScriptDelegate;

        Vector2 _scrollPosition;
        string _className = "NewEquipmentBehaviourScript";
        bool _activeParent = true;


        string _searchString = string.Empty;

        const char UNITY_FOLDER_SEPARATOR = '/';
        string _templatePath;

        public static bool HasAssetToAdd() {
            return AddScriptWindowBackup.Instance.addAsset;
        }

        public static void Show(Action<MonoScript> onCreateScript, Func<MonoScript, bool> onFilerScript, string templatePath) {

            if (_instance == null) {
                _instance = ScriptableObject.CreateInstance<AddScriptWindow>();
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUIStyle style = new GUIStyle(GUI.skin.button);
            style.fontSize = 12;
            style.fixedWidth = 230;
            style.fixedHeight = 23;

            var rect = GUILayoutUtility.GetLastRect();
            var hasAssetToAdd = HasAssetToAdd();
            EditorGUI.BeginDisabledGroup(hasAssetToAdd);
            if (GUILayout.Button("Add Behaviour", style)) {
                rect.y += 26f;
                rect.x += rect.width;
                rect.width = style.fixedWidth;
                _instance.Init(rect, onCreateScript, onFilerScript, templatePath);
                _instance.Repaint();
            }
            EditorGUI.EndDisabledGroup();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (hasAssetToAdd) {
                Backup(onCreateScript);
            }
        }

        public static void Backup(Action<MonoScript> onCreateScript) {
            if (_instance == null) {
                _instance = ScriptableObject.CreateInstance<AddScriptWindow>();
            }
            _instance.CreateScriptDelegate = onCreateScript;
            if (AddScriptWindowBackup.Instance.addAsset) {
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(AddScriptWindowBackup.Instance.scriptPath);
                if (script.GetClass() == null) {
                    return;
                }
                _instance.CreateScriptDelegate(script);
                AddScriptWindowBackup.Instance.Reset();
            }
        }

        private void Init(Rect rect, Action<MonoScript> onCreateScript, Func<MonoScript, bool> onFilerScript, string templatePath) {
            var v2 = GUIUtility.GUIToScreenPoint(new Vector2(rect.x, rect.y));
            rect.x = v2.x;
            rect.y = v2.y;

            //CreateComponentTree();
            ShowAsDropDown(rect, new Vector2(rect.width, 320f));
            Focus();
            wantsMouseMove = true;
            CreateScriptDelegate = onCreateScript;
            FilerScriptDelegate = onFilerScript;
            _templatePath = templatePath;
        }

        void OnGUI() {
            if (_styles == null) {
                _styles = new Styles();
            }
            GUI.Label(new Rect(0.0f, 0.0f, this.position.width, this.position.height), GUIContent.none, _styles.background);

            //GUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"));
            GUILayout.Space(7);
            GUILayout.BeginHorizontal();
            //GUILayout.FlexibleSpace();
            EditorGUI.BeginDisabledGroup(!_activeParent);
            _searchString = GUILayout.TextField(_searchString, GUI.skin.FindStyle("SearchTextField"));
            var buttonStyle = _searchString == string.Empty ? GUI.skin.FindStyle("SearchCancelButtonEmpty") : GUI.skin.FindStyle("SearchCancelButton");
            if (GUILayout.Button(string.Empty, buttonStyle)) {
                // Remove focus if cleared
                _searchString = string.Empty;
                GUI.FocusControl(null);
            }
            EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();
            //GUILayout.Space(9);

            if (_activeParent) {
                _className = _searchString;
                ListGUI();
            } else {
                NewScriptGUI();
            }
        }

        void ListGUI() {
            var rect = position;
            rect.x = +1f;
            rect.y = 30f;
            rect.height -= 30f;
            rect.width -= 2f;
            GUILayout.BeginArea(rect);

            rect = GUILayoutUtility.GetRect(10f, 25f);
            GUI.Label(rect, _searchString == string.Empty ? "Behaviour" : "Search", _styles.header);
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
            var scripts = Resources.FindObjectsOfTypeAll<MonoScript>();
            var searchString = _searchString.ToLower();
            foreach (var script in scripts) {
                if (!script || script.GetClass() == null || !FilerScriptDelegate(script)) {
                    continue;
                }
                if (searchString != string.Empty && !script.name.ToLower().Contains(searchString)) {
                    continue;
                }

                var buttonRect = GUILayoutUtility.GetRect(16f, 20f, GUILayout.ExpandWidth(true));
                if (GUI.Button(buttonRect, script.name, _styles.componentButton)) {
                    CreateScriptDelegate(script);
                    //CreateScriptInstance(script);
                    Close();
                }
            }
            var rect2 = GUILayoutUtility.GetRect(16f, 20f, GUILayout.ExpandWidth(true));
            if (GUI.Button(rect2, "New Script", _styles.componentButton)) {
                _activeParent = false;
            }
            GUI.Label(new Rect((float)((double)rect2.x + (double)rect2.width - 13.0), rect2.y + 4f, 13f, 13f), "", _styles.rightArrow);

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        void NewScriptGUI() {
            var rect = position;
            rect.x = +1f;
            rect.y = 30f;
            rect.height -= 30f;
            rect.width -= 2f;
            GUILayout.BeginArea(rect);

            rect = GUILayoutUtility.GetRect(10f, 25f);
            GUI.Label(rect, "New Script", _styles.header);

            GUILayout.Label("Name", EditorStyles.label);
            EditorGUI.FocusTextInControl("NewScriptName");
            GUI.SetNextControlName("NewScriptName");
            _className = EditorGUILayout.TextField(_className);

            EditorGUILayout.Space();
            string error;
            bool flag = CanCreate(out error);
            if (!flag && _className != string.Empty) {
                GUILayout.Label(error, EditorStyles.helpBox);
            }


            EditorGUI.BeginDisabledGroup(!flag);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Create and Add")) {
                GenerateAndLoadScript();
            }
            EditorGUI.EndDisabledGroup();
            GUILayout.EndArea();
        }

        private bool CanCreate(out string error) {
            error = string.Empty;
            if (_className == string.Empty) {
                return false;
            }
            if (ClassAlreadyExists()) {
                error = "A class called \"" + _className + "\" already exists.";
            } else if (ClassNameIsInvalid()) {
                error = "The script name may only consist of a-z, A-Z, 0-9, _.";
            } else {
                return true;
            }
            return false;
        }

        private bool ClassNameIsInvalid() {
            return !CodeGenerator.IsValidLanguageIndependentIdentifier(_className);
        }

        private bool ClassAlreadyExists() {
            if (_className == string.Empty)
                return false;
            return ClassExists(_className);
        }

        private bool ClassExists(string className) {
            return AppDomain.CurrentDomain.GetAssemblies().Any((x) =>
                 x.GetFiles().Any((y) => y.Name == className));
        }

        string PathCombine(params string[] paths) {
            if (paths.Length < 2) {
                throw new ArgumentException("Argument must contain at least 2 strings to combine.");
            }

            var combinedPath = _PathCombine(paths[0], paths[1]);
            var restPaths = new string[paths.Length - 2];

            Array.Copy(paths, 2, restPaths, 0, restPaths.Length);
            foreach (var path in restPaths) combinedPath = _PathCombine(combinedPath, path);

            return combinedPath;
        }

        string _PathCombine(string head, string tail) {
            if (!head.EndsWith(UNITY_FOLDER_SEPARATOR.ToString())) {
                head = head + UNITY_FOLDER_SEPARATOR;
            }

            if (string.IsNullOrEmpty(tail)) {
                return head;
            }

            if (tail.StartsWith(UNITY_FOLDER_SEPARATOR.ToString())) {
                tail = tail.Substring(1);
            }

            return Path.Combine(head, tail);
        }

        void CopyFileFromGlobalToLocal(string absoluteSourceFilePath, string localTargetFilePath) {
            var parentDirectoryPath = Path.GetDirectoryName(localTargetFilePath);
            Directory.CreateDirectory(parentDirectoryPath);
            //File.Copy(absoluteSourceFilePath, localTargetFilePath, true);
            var text = File.ReadAllText(absoluteSourceFilePath);
            text = text.Replace("MyEquipmentBehaviour", _className);
            File.WriteAllText(localTargetFilePath, text);
        }

        void GenerateAndLoadScript() {
            var sourceFileName = _templatePath;
            var destinationPath = PathCombine("Assets", _className + ".cs");

            if (string.IsNullOrEmpty(sourceFileName)) {
                return;
            }
            var backup = AddScriptWindowBackup.Instance;
            backup.addAsset = true;
            backup.scriptPath = destinationPath;
            EditorUtility.SetDirty(backup);

            CopyFileFromGlobalToLocal(sourceFileName, destinationPath);
            AssetDatabase.ImportAsset(destinationPath);
            AssetDatabase.Refresh();
            Close();

        }

        private class Styles {
            public GUIStyle header = new GUIStyle((GUIStyle)"In BigTitle");
            public GUIStyle componentButton = new GUIStyle((GUIStyle)"PR Label");
            public GUIStyle background = (GUIStyle)"grey_border";
            public GUIStyle previewBackground = (GUIStyle)"PopupCurveSwatchBackground";
            public GUIStyle previewHeader = new GUIStyle(EditorStyles.label);
            public GUIStyle previewText = new GUIStyle(EditorStyles.wordWrappedLabel);
            public GUIStyle rightArrow = (GUIStyle)"AC RightArrow";
            public GUIStyle leftArrow = (GUIStyle)"AC LeftArrow";
            public GUIStyle groupButton;

            public Styles() {
                this.header.font = EditorStyles.boldLabel.font;
                this.componentButton.alignment = TextAnchor.MiddleLeft;
                this.componentButton.padding.left -= 15;
                this.componentButton.fixedHeight = 20f;
                this.groupButton = new GUIStyle(this.componentButton);
                this.groupButton.padding.left += 17;
                this.previewText.padding.left += 3;
                this.previewText.padding.right += 3;
                ++this.previewHeader.padding.left;
                this.previewHeader.padding.right += 3;
                this.previewHeader.padding.top += 3;
                this.previewHeader.padding.bottom += 2;
            }
        }
    }
}