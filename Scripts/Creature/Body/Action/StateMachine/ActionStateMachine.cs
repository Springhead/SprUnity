using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif

namespace SprUnity {
    /*
    [CustomPropertyDrawer(typeof(ActionParameter))]
    public class ActionParameterPropertyDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var labelProp = property.FindPropertyRelative("label");
            var valueProp = property.FindPropertyRelative("v");
            var typeProp = property.FindPropertyRelative("type");
            Rect labelRect = position;
            EditorGUI.TextField(labelRect,labelProp.stringValue);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUIUtility.singleLineHeight;
        }
    }
    */
#if UNITY_EDITOR
    [CustomEditor(typeof(ActionStateMachine))]
    public class ActionStateMachineEditor : Editor {
        bool showParameterList;
        ActionParameter.ParameterType addType;
        public override void OnInspectorGUI() {
            EditorGUI.BeginChangeCheck();
            ActionStateMachine stateMachine = (ActionStateMachine)target;
            GUILayout.BeginVertical();
            int deleteNum = -1;
            for(int i = 0; i < stateMachine.parameters.Count(); i++) {
                /*
                ActionBoolParameter boolParam = stateMachine.parameters[i] as ActionBoolParameter;
                if(boolParam != null) {
                    GUILayout.BeginHorizontal();
                    boolParam.label = GUILayout.TextArea(boolParam.label);
                    boolParam.value = GUILayout.Toggle(boolParam.value, "");
                    if (GUILayout.Button("x")) {
                        deleteNum = i;
                    }
                    GUILayout.EndHorizontal();
                    DrawReferenceNodes(boolParam);
                    continue;
                }
                ActionIntParameter intParam = stateMachine.parameters[i] as ActionIntParameter;
                if (intParam != null) {
                    GUILayout.BeginHorizontal();
                    intParam.label = GUILayout.TextArea(intParam.label);
                    intParam.value = EditorGUILayout.IntField(intParam.value);
                    if (GUILayout.Button("x")) {
                        deleteNum = i;
                    }
                    GUILayout.EndHorizontal();
                    DrawReferenceNodes(intParam);
                    continue;
                }
                ActionFloatParameter floatParam = stateMachine.parameters[i] as ActionFloatParameter;
                if (floatParam != null) {
                    GUILayout.BeginHorizontal();
                    floatParam.label = GUILayout.TextArea(floatParam.label);
                    floatParam.value = EditorGUILayout.FloatField(floatParam.value);
                    if (GUILayout.Button("x")) {
                        deleteNum = i;
                    }
                    GUILayout.EndHorizontal();
                    DrawReferenceNodes(floatParam);
                    continue;
                }
                ActionGameObjectParameter objectParam = stateMachine.parameters[i] as ActionGameObjectParameter;
                if (objectParam != null) {
                    GUILayout.BeginHorizontal();
                    objectParam.label = GUILayout.TextArea(objectParam.label);
                    if (GUILayout.Button("x")) {
                        deleteNum = i;
                    }
                    GUILayout.EndHorizontal();
                    DrawReferenceNodes(objectParam);
                    continue;
                }
                */
                ActionParameter actionParameter = stateMachine.parameters[i];
                GUILayout.BeginHorizontal();
                actionParameter.label = GUILayout.TextArea(actionParameter.label);
                switch (actionParameter.type) {
                    case ActionParameter.ParameterType.Bool:
                        actionParameter.value = GUILayout.Toggle((bool)actionParameter.value, "");
                        break;
                    case ActionParameter.ParameterType.Int:
                        actionParameter.value = EditorGUILayout.IntField((int)actionParameter.value);
                        break;
                    case ActionParameter.ParameterType.Float:
                        actionParameter.value = EditorGUILayout.FloatField(actionParameter.value != null ? (float)actionParameter.value : 0.0f);
                        break;
                    case ActionParameter.ParameterType.GameObject:
                        /*
                        if(actionParameter.value != null) {
                            GUILayout.Label(((GameObject)actionParameter.value).name);
                        } else {
                            GUILayout.Label("Object null");
                        }
                        */
                        actionParameter.value = EditorGUILayout.ObjectField((GameObject)actionParameter.value, typeof(GameObject));
                        break;
                    default:
                        break;
                }
                if (GUILayout.Button("x")) {
                    deleteNum = i;
                }
                GUILayout.EndHorizontal();
                DrawReferenceNodes(actionParameter);
            }
            if(deleteNum >= 0) {
                stateMachine.parameters.RemoveAt(deleteNum);
            }
            GUILayout.BeginHorizontal();
            addType = (ActionParameter.ParameterType)EditorGUILayout.EnumPopup(addType);
            if(GUILayout.Button("Add Parameter")) {
                ActionParameter actionParameter = new ActionParameter();
                switch (addType) {
                    case ActionParameter.ParameterType.Bool:
                        actionParameter.label = "new Bool";
                        actionParameter.type = ActionParameter.ParameterType.Bool;
                        actionParameter.value = false;
                        break;
                    case ActionParameter.ParameterType.Int:
                        actionParameter.label = "new Int";
                        actionParameter.type = ActionParameter.ParameterType.Int;
                        actionParameter.value = 0;
                        break;
                    case ActionParameter.ParameterType.Float:
                        actionParameter.label = "new Float";
                        actionParameter.type = ActionParameter.ParameterType.Float;
                        actionParameter.value = 0.0f;
                        break;
                    case ActionParameter.ParameterType.GameObject:
                        actionParameter.label = "new GameObject";
                        actionParameter.type = ActionParameter.ParameterType.GameObject;
                        actionParameter.value = null;
                        break;
                }
                stateMachine.parameters.Add(actionParameter);

            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck()) {
                EditorUtility.SetDirty(stateMachine);
            }
            
        }

        public void DrawReferenceNodes(ActionParameter param) {
            var backupIndent = EditorGUI.indentLevel;
            //EditorGUI.indentLevel++;
            int deleteNode = -1;
            for (int i = 0; i < param.referenceNodes.Count(); i++) {
                GUILayout.BeginHorizontal();
                param.referenceNodes[i] = (VGentNodeBase)EditorGUILayout.ObjectField(param.referenceNodes[i], typeof(VGentNodeBase));
                if (GUILayout.Button("x")) {
                    deleteNode = i;
                }
                GUILayout.EndHorizontal();
            }
            if (deleteNode >= 0) {
                param.referenceNodes.RemoveAt(deleteNode);
            }
            if(GUILayout.Button("Add Ref Node")) {
                param.referenceNodes.Add(null);
            }
            EditorGUI.indentLevel = backupIndent;
        }
    }
#endif
    [System.Serializable]
    public class ActionParameter : ISerializationCallbackReceiver{
        public string label;
        public enum ParameterType {
            Bool,
            Int,
            Float,
            GameObject,
        }
        public static Type[] types= new Type[]{
            typeof(bool),
            typeof(int),
            typeof(float),
            typeof(GameObject),
        };
        public ParameterType type;
        public object value;
        [SerializeField]
        private string valueData;
        public List<VGentNodeBase> referenceNodes = new List<VGentNodeBase>();
        public ActionParameter() {
            label = "";
        }
        public ActionParameter(string l) {
            this.label = l;
        }
        public virtual void SetInput() {
            foreach (var node in referenceNodes) {
                switch (type) {
                    case ParameterType.Bool:
                        node.SetInput<bool>((bool)value);
                        break;
                    case ParameterType.Int:
                        node.SetInput<int>((int)value);
                        break;
                    case ParameterType.Float:
                        node.SetInput<float>((float)value);
                        break;
                    case ParameterType.GameObject:
                        node.SetInput<GameObject>((GameObject)value);
                        break;
                    default:
                        break;
                }
            }
        }
        public bool SetValue<T>(T v) {
            switch (type) {
                case ParameterType.Bool:
                    if(typeof(T) == typeof(bool)) {
                        value = v;
                        return true;
                    }
                    break;
                case ParameterType.Int:
                    if(v is int) {
                        value = v;
                        return true;
                    }
                    break;
                case ParameterType.Float:
                    if(v is float) {
                        value = v;
                        return true;
                    }
                    break;
                case ParameterType.GameObject:
                    if(v is GameObject) {
                        value = v;
                        return true;
                    }
                    break;
            }
            return false;
        }

        public void OnBeforeSerialize() {
            switch (type) {
                case ParameterType.Bool:
                    valueData = ((bool)value).ToString();
                    break;
                case ParameterType.Int:
                    valueData = ((int)value).ToString();
                    break;
                case ParameterType.Float:
                    valueData = ((float)value).ToString();
                    break;
                case ParameterType.GameObject:  // シリアライズしない
                default:
                    break;
            }
        }

        public void OnAfterDeserialize() {
            switch (type) {
                case ParameterType.Bool:
                    value = valueData == "True" ? true : false;
                    break;
                case ParameterType.Int:
                    value = int.Parse(valueData);
                    break;
                case ParameterType.Float:
                    value = float.Parse(valueData);
                    break;
                case ParameterType.GameObject:
                defalut:
                    break;
            }
        }
    }

#if UNITY_EDITOR
    [CreateAssetMenu(menuName = "Action/ Create ActionStateMachine Instance")]
#endif
    public class ActionStateMachine : ScriptableObject {

        [SerializeField]
        public List<ActionParameter> parameters;
        public ActionParameter parameter(string name) {
            foreach (var param in parameters) {
                if (param.label == name) return param;
            }
            return null;
        }

        // ----- ----- ----- ----- -----
        // 実行用

        [System.NonSerialized]
        public ActionStateMachine original = null;

        // 適用するBody
        [System.NonSerialized]
        public ActionManager manager;
        public Body Body {
            get {
                if (original != null && manager != null) {
                    return manager.body;
                }
                return null;
            }
        }
        public BlendShapeController blendController {
            get {
                if (original != null && manager != null) {
                    return manager.blendController;
                }
                return null;
            }
        }
        // instance群
        [System.NonSerialized]
        public Dictionary<ActionManager, ActionStateMachine> instances = new Dictionary<ActionManager, ActionStateMachine>();
        public ActionStateMachine GetInstance(ActionManager manager) {
            if (instances.ContainsKey(manager)) return instances[manager];
            else {
                ActionStateMachine instance = this.Clone();
                instances.Add(manager, instance);
                return instance;
            }
        }

        private ActionState currentState;
        public ActionState CurrentState { get { return currentState; } }
        private float currentDuration;
        private float currentStateTime;

        private float stateMachineTime = 0;

        private bool enabled = false;

        public List<HumanBodyBones> resource {
            get {
                List<HumanBodyBones> bones = new List<HumanBodyBones>();
                foreach(var state in states) {
                    foreach(var node in state.nodes) {
                        if (!bones.Contains(node.boneId)) bones.Add(node.boneId);
                    }
                }
                return bones;
            }
        }

        // ----- ----- ----- ----- -----
        // Editor関係

        public bool isChanged = false;
        
        [HideInInspector] public Rect entryRect = new Rect(100, 100, 100, 50);
        [HideInInspector ]public List<ActionTransition> entryTransitions = new List<ActionTransition>();
        [HideInInspector] public Rect exitRect = new Rect(100, 200, 100, 50);


        // ----- ----- ----- ----- -----
        public int nStates {
            get {
                return this.GetSubAssets().Where(value => value as ActionState != null).Count();
            }
        }

        public int nTransitions {
            get {
                return this.GetSubAssets().Where(value => value as ActionTransition != null).Count();
            }
        }

        public List<ActionState> states {
            get {
                return this.GetSubAssets().OfType<ActionState>().ToList();
            }
        }

        public List<ActionTransition> transitions {
            get {
                return this.GetSubAssets().OfType<ActionTransition>().ToList();
            }
        }

        // ----- ----- ----- ----- ----- -----
        // Create ActionStateMachine

#if UNITY_EDITOR
        [MenuItem("Action/Create ActionStateMachine")]
        static void CreateStateMachine() {
            CreateStateMachine("ActionStateMachine");
        }
#endif
#if UNITY_EDITOR
        public static void CreateStateMachine(string newName) {
            var action = CreateInstance<ActionStateMachine>();
#if UNITY_EDITOR
            AssetDatabase.CreateAsset(action, "Assets/Actions/Actions/" + newName + ".asset");
            AssetDatabase.Refresh();
# endif
        }
#endif
        // ----- ----- ----- ----- ----- -----
        // Create/Delete ActionState

        public void CreateState(Vector2 pos = default(Vector2)) {
#if UNITY_EDITOR
            var state = ScriptableObject.CreateInstance<ActionState>();
            state.name = "new state";
            state.stateMachine = this;
            state.stateNodeRect.position = pos;

            AssetDatabase.AddObjectToAsset(state, this);

            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(this));
#endif
        }

        static void DeleteState(ActionState state) {

        }

        // ----- ----- ----- ----- ----- -----
        // Clone
        public ActionStateMachine Clone() {
            ActionStateMachine clone = ScriptableObject.Instantiate<ActionStateMachine>(this);
            clone.original = this;
            return clone;
        }

        public ActionStateMachine Instantiate(ActionManager actionManager = null) {
            ActionStateMachine instance = ScriptableObject.Instantiate<ActionStateMachine>(this);
            instance.original = this;
            instance.manager = actionManager;
            return instance;
        }

        // ----- ----- ----- ----- ----- -----
        // Execution events

        // 
        public void Begin(Body body = null, GameObject targets = null) {

            if (entryTransitions.Count == 0) return;
            
            enabled = true;
            stateMachineTime = 0;
            currentStateTime = 0.0f;

            currentState = entryTransitions[0].toState;
            var logs = currentState.OnEnter(this, out currentDuration);
            Debug.Log("Begin:" + currentState.name);
            isChanged = true;
        }

        // Update StateMachine if it's enabled
        public void UpdateStateMachine() {
            if (!enabled) return;
            // Update timer
            stateMachineTime += Time.fixedDeltaTime;
            currentStateTime += Time.fixedDeltaTime;
            currentState.OnUpdate();

            int nTransition = currentState.transitions.Count;
            // 遷移の判定
            for (int i = 0; i < nTransition; i++) {
                if (currentState.transitions[i].IsTransitable(currentStateTime, currentDuration, this)) {
                    currentState.OnExit();
                    currentState = currentState.transitions[i].toState;
                    currentStateTime = 0.0f;
                    if (currentState == null) {
                        End();
                    } else {
                        var logs = currentState.OnEnter(this, out currentDuration);
                    }
                    isChanged = true;
                    break;
                }
            }
            if (nTransition == 0) {
                currentState.OnExit();
                End();
            }
        }

        // 
        public void End() {
            enabled = false;
            isChanged = true;
            ResetStateMachine();
        }

        void ResetStateMachine() {
            for(int i = 0; i < states.Count; i++) {
                states[i].IsCurrent = false;
            }
        }

        public void ApplyParameters() {
            foreach(var param in parameters) {
                param.SetInput();
            }
        }

        // ----- ----- ----- ----- -----

        void ApplyFromInstance() {
            // 

        }

        void ApplyToAllInstance() {
            // インスタンスの消去だけ
        }

        // ----- ----- ----- ----- -----

        public UnityEngine.Object[] GetSubAssets() {
#if UNITY_EDITOR
            string path = AssetDatabase.GetAssetPath(this);
            return AssetDatabase.LoadAllAssetsAtPath(path);
#else
        return null;
#endif
        }
    }

}