using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SprCs;
using UnityEngine;
using UnityEditor;

namespace SprUnity {

    // Source
    // https://answers.unity.com/questions/546686/editorguilayout-split-view-resizable-scroll-view.html
    // https://github.com/miguel12345/EditorGUISplitView/blob/master/Assets/EditorGUISplitView/Scripts/Editor/EditorGUISplitView.cs#L26
    public class VerticalSplitWindow : EditorWindow {
        private Vector2 scrollPos;
        bool resize = false;

        int numSplit = 3;

        void OnEnable() {

        }

        public void OnGUI() {
            GUILayout.BeginVertical();
            for (int i = 0; i < numSplit; i++) {
                scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(this.position.height / numSplit));
                PaintComponent();
                GUILayout.EndScrollView();
            }
            GUILayout.EndVertical();
        }

        private void ResizeScrolView() {

        }

        private void PaintComponent() {

        }
    }

    public class DraggableBox {
        public Rect box = new Rect(10, 10, 0, 0);
        public string text = "";
        private bool isDragged = false;
        private bool isSelected = false;
        public DraggableBox(Rect rect, string t = "") {
            box = rect;
            text = t;
        }

        public virtual void Drag(Vector2 delta) {
            box.position += delta;
        }

        public void Draw() {
            GUI.Box(box, text);
        }

        public bool ProcessEvents() {
            Event e = Event.current;
            switch (e.type) {
                case EventType.MouseDown:
                    if (e.button == 0) {
                        if (box.Contains(e.mousePosition)) {
                            isDragged = true;
                            isSelected = true;
                            GUI.changed = true;
                        } else {
                            GUI.changed = true;
                            isSelected = false;
                        }
                    }
                    if (e.button == 1) {
                        if (box.Contains(e.mousePosition)) {
                            OnContextMenu(e.mousePosition);
                        }
                    }
                    break;

                case EventType.MouseUp:
                    isDragged = false;
                    break;

                case EventType.MouseDrag:
                    if (e.button == 0 && isDragged) {
                        Drag(e.delta);
                        e.Use();
                        return true;
                    }
                    break;
            }
            return false;
        }

        void OnContextMenu(Vector2 mousePosition) {

        }
    }

    public class VerticalDraggableBox : DraggableBox {
        public VerticalDraggableBox(Rect rect, string t) : base(rect, t) {

        }
        public override void Drag(Vector2 delta) {
            box.y += delta.y;
        }
    }

    public class HorizontalDraggableBox : DraggableBox {
        public HorizontalDraggableBox(Rect rect, string t) : base(rect, t) {

        }
        public override void Drag(Vector2 delta) {
            box.x += delta.x;
        }
    }

    public class BoneStatusForTimeline {
        public HumanBodyBones bone;
        public Color color;
        public bool solo;
        public bool mute;
        public BoneStatusForTimeline(HumanBodyBones b, Color c) {
            bone = b;
            color = c;
            solo = false;
            mute = false;
        }
        public void SetSolo(bool s) {
            solo = s;
        }
        public void SetMute(bool m) {
            mute = m;
        }
    }

    public class ActionTimelineWindow : EditorWindow {

        //
        static ActionTimelineWindow window;

        // 表示用に一旦ActionStateMachineをとってくる
        private ActionStateMachine currentAction;
        private ActionStateMachineController controller;

        //float timeAxisLength = 5.0f;

        float springDamperMax = 1.0f;

        float totalTime = 0;
        float startTime = 0;
        float finishTime = 0;
        //List<float> startSubmovementTime = new List<float>();
        //List<float> endSubmovementTime = new List<float>();

        Vector2 springDamperGraphPosition;
        Vector2 velocityGraphPosition;

        static GUIStyle style;
        static int maxBoxWidth = 120;
        static int minBoxWidth = 10;

        bool showSpring = true;
        bool showDamper = true;
        bool showVelocity = true;
        bool showAngularVelocity = false;

        List<BoneStatusForTimeline> boneStatusForTimelines = new List<BoneStatusForTimeline>();

        //List<List<SubMovement>> submovements;

        VerticalDraggableBox[] springDamperHandle;
        HorizontalDraggableBox[] transitionTimeHandle;

        // Transition
        float marginLeft = 10;
        float marginTop = 20;

        // グラフ描画に使うセグメント分割数
        // 値が大きいと重いので、だれかいい方法教えてください
        int segments = 10;

        private ScriptableAction action;

        [MenuItem("Window/SprUnity Action/Action Timeline Window")]
        static void Open() {
            window = GetWindow<ActionTimelineWindow>();
            ActionEditorWindowManager.instance.timelineWindow = window;
        }

        public void OnEnable() {
            //Open();
            style = new GUIStyle();
            style.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
            style.alignment = TextAnchor.MiddleCenter;
            style.border = new RectOffset(12, 12, 12, 12);
            boneStatusForTimelines = new List<BoneStatusForTimeline>() {
            new BoneStatusForTimeline(HumanBodyBones.Head, Color.red),
            new BoneStatusForTimeline(HumanBodyBones.Hips, Color.blue),
            new BoneStatusForTimeline(HumanBodyBones.LeftHand, Color.green),
            new BoneStatusForTimeline(HumanBodyBones.RightHand, Color.yellow),
            new BoneStatusForTimeline(HumanBodyBones.LeftFoot, Color.magenta),
            new BoneStatusForTimeline(HumanBodyBones.RightFoot, Color.black)
        };
            for (int i = 0; i < boneStatusForTimelines.Count; i++) {
                boneStatusForTimelines[i].SetSolo(SessionState.GetBool(boneStatusForTimelines[i].bone.ToString() + ":solo", false));
                boneStatusForTimelines[i].SetMute(SessionState.GetBool(boneStatusForTimelines[i].bone.ToString() + ":mute", false));
            }
            showSpring = SessionState.GetBool("ActionTimeLineShowSpring", true);
            showDamper = SessionState.GetBool("ActionTimeLineShowDamper", true);
            showVelocity = SessionState.GetBool("ActionTimeLineShowVelocity", true);
            showAngularVelocity = SessionState.GetBool("ActionTimeLineShowAngularVelocity", false);

            SceneView.onSceneGUIDelegate -= OnSceneGUI;
            SceneView.onSceneGUIDelegate += OnSceneGUI;
        }

        public void OnDisable() {
            foreach (var boneStatus in boneStatusForTimelines) {
                SessionState.SetBool(boneStatus.bone.ToString() + ":solo", boneStatus.solo);
                SessionState.SetBool(boneStatus.bone.ToString() + ":mute", boneStatus.mute);
            }
            SessionState.SetBool("ActionTimeLineShowSpring", showSpring);
            SessionState.SetBool("ActionTimeLineShowDamper", showDamper);
            SessionState.SetBool("ActionTimeLineShowVelocity", showVelocity);
            SessionState.SetBool("ActionTimeLineShowAngularVelocity", showAngularVelocity);
            window = null;
            ActionEditorWindowManager.instance.timelineWindow = null;
        }

        void OnGUI() {
            if (window == null) Open();

            currentAction = ActionEditorWindowManager.instance.selectedAction;
            float controllerStartTime = 0.0f;
            float controllerFinishTime = 0.0f;
            if (currentAction) {
                controller = ActionEditorWindowManager.instance.lastSelectedActionManager?[ActionEditorWindowManager.instance.selectedAction.name];
            } else {
                controller = null;
            }
            if (controller != null) {
                controllerStartTime = controller.GetOldestStartTime();
                controllerFinishTime = controller.GetFinishTime();
            }
            startTime = controllerStartTime > controllerFinishTime ? 0.0f : controllerStartTime;
            totalTime = Mathf.Max(controllerFinishTime - controllerStartTime, 5.0f);
            finishTime = startTime + totalTime;

            GUILayout.BeginVertical();
            DrawSpringDamperGraph();
            DrawVelocityGraph();
            DrawTransitionGraph();
            GUILayout.EndVertical();
            DrawBoneTable();
        }

        void DrawSpringDamperGraph(Rect area = new Rect()) {
            GUILayout.BeginArea(new Rect(0, 0, this.position.width, this.position.height / 3));
            GUILayout.Label("Spring & Damper");
            showSpring = GUILayout.Toggle(showSpring, "Spring", GUILayout.Width(position.width * 0.1f));
            showDamper = GUILayout.Toggle(showDamper, "Damper", GUILayout.Width(position.width * 0.1f));
            // Draw Graph Base
            int graphBottom = (int)(position.height * 0.27);
            int graphTop = (int)(position.height * 0.01);
            int graphHeight = graphBottom - graphTop;
            int graphLeft = (int)(position.width * 0.1);
            int graphRight = (int)(position.width * 0.9);
            int graphWidth = graphRight - graphLeft;
            float[] xAxis = new float[2] { startTime, finishTime };
            float[] yAxis = new float[2] { 0f, springDamperMax };
            DrawGraphBase(new Rect(graphLeft, graphTop, graphWidth, graphHeight), xAxis, yAxis, 1.0f, 0.2f, "", "");

            // <!!> 1本ずつでよかった!
            if (EditorApplication.isPlaying) {
                if (currentAction != null && controller != null) {
                    for (int j = 0; j < boneStatusForTimelines.Count; j++) {
                        List<SubMovementLog> logs = null;
                        List<SubMovementLog> future = null;
                        for (int i = 0; i < controller.ActionLog.subMovementLogs.Count; i++) {
                            if (boneStatusForTimelines[j].bone.ToString().Equals(controller.ActionLog.subMovementLogs[i].bone.label)) {
                                logs = controller.ActionLog.subMovementLogs[i].logSubMovements;
                                future = controller.ActionLog.subMovementLogs[i].futureSubMovements;
                                break;
                            }
                        }
                        if (logs != null) {
                            for (int i = 0; i < logs.Count(); i++) {
                                if (logs[i].subMovement.t0 != logs[i].subMovement.t1 && boneStatusForTimelines[j].solo) {
                                    float lastPosX = graphLeft + graphWidth * ((logs[i].subMovement.t0 - startTime) / totalTime);
                                    Vector2 lastPosSpring = new Vector2(lastPosX, graphBottom - (logs[i].subMovement.s0[0] / springDamperMax) * graphHeight);
                                    Vector2 lastPosDamper = new Vector2(lastPosX, graphBottom - (logs[i].subMovement.s0[1] / springDamperMax) * graphHeight);
                                    Vector2 nextPosSpring = new Vector2();
                                    Vector2 nextPosDamper = new Vector2();
                                    Vector2 sp = new Vector2();
                                    Color color = boneStatusForTimelines[j].color;
                                    float currentSubmovementTime;
                                    for (int k = 0; k < segments; k++) {
                                        currentSubmovementTime = (logs[i].subMovement.t1 - logs[i].subMovement.t0) * ((float)(k + 1) / segments) + logs[i].subMovement.t0;
                                        float nextPosX = graphLeft + graphWidth * ((currentSubmovementTime - startTime) / totalTime);
                                        logs[i].subMovement.GetCurrentSpringDamper(currentSubmovementTime, out sp);
                                        if (showSpring) {
                                            nextPosSpring.x = nextPosX;
                                            nextPosSpring.y = graphBottom - graphHeight * ((sp[0] + logs[i].subMovement.s0[0]) / springDamperMax);
                                            Drawing.DrawLine(lastPosSpring, nextPosSpring, color, 3, true);
                                            lastPosSpring = nextPosSpring;
                                        }
                                        if (showDamper) {
                                            nextPosDamper.x = nextPosX;
                                            nextPosDamper.y = graphBottom - graphHeight * ((sp[1] + logs[i].subMovement.s0[1]) / springDamperMax);
                                            Drawing.DrawLine(lastPosDamper, nextPosDamper, color, 3, true);
                                            lastPosDamper = nextPosDamper;
                                        }
                                    }
                                }
                            }
                        } else {
                            Debug.Log("Could not find logs of Bone " + boneStatusForTimelines[j].bone.ToString());
                        }
                        if (future != null) {
                            for (int i = 0; i < future.Count(); i++) {
                                if (future[i].subMovement.t0 != future[i].subMovement.t1 && boneStatusForTimelines[j].solo) {
                                    Vector2 sp0 = new Vector2();
                                    Vector2 sp1 = new Vector2();
                                    Color color = boneStatusForTimelines[j].color;
                                    int halfSegment = segments / 2;
                                    for (int k = 0; k < halfSegment; k++) {
                                        float currentSubmovementTime0 = (future[i].subMovement.t1 - future[i].subMovement.t0) * ((float)(2 * k) / segments) + future[i].subMovement.t0;
                                        float currentSubmovementTime1 = (future[i].subMovement.t1 - future[i].subMovement.t0) * ((float)(2 * k + 1) / segments) + future[i].subMovement.t0;
                                        future[i].subMovement.GetCurrentSpringDamper(currentSubmovementTime0, out sp0);
                                        future[i].subMovement.GetCurrentSpringDamper(currentSubmovementTime1, out sp1);
                                        float p0X = graphLeft + graphWidth * ((currentSubmovementTime0 - startTime) / totalTime);
                                        float p1X = graphLeft + graphWidth * ((currentSubmovementTime1 - startTime) / totalTime);
                                        if (showSpring) {
                                            Vector2 p0Spring = new Vector2(p0X, graphBottom - graphHeight * (sp0[0] / yAxis[1]));
                                            Vector2 p1Spring = new Vector2(p1X, graphBottom - graphHeight * (sp1[0] / yAxis[1]));
                                            Drawing.DrawLine(p0Spring, p1Spring, color, 3, true);
                                        }
                                        if (showDamper) {
                                            Vector2 p0Damper = new Vector2(p0X, graphBottom - graphHeight * (sp0[1] / yAxis[1]));
                                            Vector2 p1Damper = new Vector2(p1X, graphBottom - graphHeight * (sp1[1] / yAxis[1]));
                                            Drawing.DrawLine(p0Damper, p1Damper, color, 3, true);
                                        }
                                    }
                                }
                            }
                        } else {
                            Debug.Log("Could not find Future Submovements of Bone " + boneStatusForTimelines[j].bone.ToString());
                        }
                    }
                }
            }
            GUILayout.EndArea();
        }

        void DrawVelocityGraph(Rect area = new Rect()) {
            GUILayout.BeginArea(new Rect(0, this.position.height / 3, this.position.width, this.position.height / 3));
            GUILayout.Label("Velocity");
            showVelocity = GUILayout.Toggle(showVelocity, "Velocity");
            showAngularVelocity = GUILayout.Toggle(showAngularVelocity, "AngularVelocity");
            // Draw Graph Base
            int graphBottom = (int)(position.height * 0.27);
            int graphTop = (int)(position.height * 0.01);
            int graphHeight = graphBottom - graphTop;
            int graphLeft = (int)(position.width * 0.1);
            int graphRight = (int)(position.width * 0.9);
            int graphWidth = graphRight - graphLeft;
            float[] xAxis = new float[2] { startTime, finishTime };
            float[] yAxis = new float[2] { 0f, 1f };
            DrawGraphBase(new Rect(graphLeft, graphTop, graphWidth, graphHeight), xAxis, yAxis, 1.0f, 0.2f, "", "");

            if (EditorApplication.isPlaying) {
                if (currentAction != null && controller != null) {
                    for (int j = 0; j < boneStatusForTimelines.Count; j++) {
                        List<SubMovementLog> logs = null;
                        List<SubMovementLog> future = null;
                        for (int i = 0; i < controller.ActionLog.subMovementLogs.Count; i++) {
                            if (boneStatusForTimelines[j].bone.ToString().Equals(controller.ActionLog.subMovementLogs[i].bone.label)) {
                                logs = controller.ActionLog.subMovementLogs[i].logSubMovements;
                                future = controller.ActionLog.subMovementLogs[i].futureSubMovements;
                                break;
                            }
                        }
                        if (logs != null) {
                            for (int i = 0; i < logs.Count(); i++) {
                                if (logs[i].subMovement.t0 != logs[i].subMovement.t1 && boneStatusForTimelines[j].solo) {
                                    Vector2 lastPos = new Vector2(graphLeft + graphWidth * ((logs[i].subMovement.t0 - startTime) / totalTime), graphBottom);
                                    Vector2 nextPos = new Vector2();
                                    Vector3 vel = new Vector3();
                                    Color color = boneStatusForTimelines[j].color;
                                    float currentSubmovementTime;
                                    for (int k = 0; k < segments; k++) {
                                        currentSubmovementTime = (logs[i].subMovement.t1 - logs[i].subMovement.t0) * ((float)(k + 1) / segments) + logs[i].subMovement.t0;
                                        nextPos.x = graphLeft + graphWidth * ((currentSubmovementTime - startTime) / totalTime);
                                        logs[i].subMovement.GetCurrentVelocity(currentSubmovementTime, out vel);
                                        nextPos.y = graphBottom - graphHeight * (vel.magnitude / yAxis[1]);
                                        //Debug.Log(currentSubmovementTime + " " + vel.magnitude);
                                        Drawing.DrawLine(lastPos, nextPos, color, 3, true);
                                        lastPos = nextPos;
                                    }
                                }
                            }
                        } else {
                            Debug.Log("Could not find logs of Bone " + boneStatusForTimelines[j].bone.ToString());
                        }
                        if (future != null) {
                            for (int i = 0; i < future.Count(); i++) {
                                if (future[i].subMovement.t0 != future[i].subMovement.t1 && boneStatusForTimelines[j].solo) {
                                    Vector3 vel = new Vector3();
                                    Color color = boneStatusForTimelines[j].color;
                                    int halfSegment = segments / 2;
                                    for (int k = 0; k < halfSegment; k++) {
                                        float currentSubmovementTime0 = (future[i].subMovement.t1 - future[i].subMovement.t0) * ((float)(2 * k) / segments) + future[i].subMovement.t0;
                                        future[i].subMovement.GetCurrentVelocity(currentSubmovementTime0, out vel);
                                        Vector2 p0 = new Vector2(graphLeft + graphWidth * ((currentSubmovementTime0 - startTime) / totalTime), graphBottom - graphHeight * (vel.magnitude / yAxis[1]));
                                        //Debug.Log(currentSubmovementTime + " " + vel.magnitude);
                                        float currentSubmovementTime1 = (future[i].subMovement.t1 - future[i].subMovement.t0) * ((float)(2 * k + 1) / segments) + future[i].subMovement.t0;
                                        future[i].subMovement.GetCurrentVelocity(currentSubmovementTime1, out vel);
                                        Vector2 p1 = new Vector2(graphLeft + graphWidth * ((currentSubmovementTime1 - startTime) / totalTime), graphBottom - graphHeight * (vel.magnitude / yAxis[1]));
                                        Drawing.DrawLine(p0, p1, color, 3, true);
                                    }
                                }
                            }
                        } else {
                            Debug.Log("Could not find future submovements of Bone " + boneStatusForTimelines[j].bone.ToString());
                        }
                    }
                }
            }
            GUILayout.EndArea();
        }

        void DrawTransitionGraph(Rect area = new Rect()) {
            GUILayout.BeginArea(new Rect(0, 2 * this.position.height / 3, this.position.width, this.position.height / 3));
            GUILayout.Label("Transition");
            if (currentAction) {
                if (EditorApplication.isPlaying) {
                    // 各種設定
                    if (controller == null) {
                        GUILayout.Label("Please select gameobject ActionManager attatched once");
                        goto Last;
                    }
                    if (controller != null) {
                        var specifiedTransitions = controller.specifiedTransitions;
                        int specifiedCount = controller.specifiedTransitions.Count;
                        int futureCount = controller.futureTransitions.Count;
                        int boxWidth = (int)Mathf.Max(Mathf.Min(0.66f * position.width / (specifiedCount + futureCount), maxBoxWidth), minBoxWidth);
                        //int boxWidth = (int)(position.width / (numStatesInStream + 5));
                        List<ActionTransition> transitionsFromCurrent;
                        if (controller.CurrentState) {
                            transitionsFromCurrent = controller.CurrentState.transitions;
                        } else {
                            transitionsFromCurrent = controller.StateMachine.entryTransitions;
                        }
                        int nTransitionsFromCurrernt = transitionsFromCurrent.Count;
                        Vector2 boxSize = new Vector2(boxWidth, 20);
                        Vector2 boxPositionBase;
                        Vector2 transitionFromPos;
                        //GUILayout.BeginHorizontal();
                        ActionState currentState = controller.CurrentState;

                        // Current
                        boxPositionBase.x = marginLeft;
                        boxPositionBase.y = 40;
                        Rect entryRect = new Rect(boxPositionBase, boxSize);
                        transitionFromPos = new Vector2(entryRect.xMax, (entryRect.y + entryRect.yMax) / 2);
                        GUI.Box(entryRect, currentState ? currentState.name : "Entry");

                        GUILayout.Label(controller.futureTransitions.Count.ToString());

                        // Specified
                        for (int i = 0; i < controller.specifiedTransitions.Count; i++) {
                            boxPositionBase.x += boxWidth * 1.5f;
                            for (int j = 0; j < nTransitionsFromCurrernt; j++) {
                                string stateName = transitionsFromCurrent[j].toState == null ?
                                    "Exit" :
                                    transitionsFromCurrent[j].toState.name;
                                Rect boxPosition = new Rect(new Vector2(boxPositionBase.x, boxPositionBase.y + j * 30), boxSize);
                                GUI.Box(boxPosition, stateName);
                                if (transitionsFromCurrent[j] == controller.specifiedTransitions[i]) {
                                    float boxY = (boxPosition.y + boxPosition.yMax) / 2;
                                    Drawing.DrawLine(transitionFromPos, new Vector2(boxPosition.x, boxY), Color.red, 2f, true);
                                    transitionFromPos = new Vector2(boxPosition.xMax, boxY);
                                }
                                Rect buttonRect = new Rect(boxPosition.x - 10, boxPosition.y, 20, 20);
                                if (GUI.Button(buttonRect, "")) {
                                    if (transitionsFromCurrent[j] != controller.specifiedTransitions[i]) {
                                        List<ActionTransition> changedTransitions = new List<ActionTransition>();
                                        for (int k = 0; k < i; k++) {
                                            changedTransitions.Add(controller.specifiedTransitions[k]);
                                        }
                                        changedTransitions.Add(transitionsFromCurrent[j]);
                                        controller.specifiedTransitions = changedTransitions;
                                        ReloadHandles();
                                        ReloadSubmovements();
                                        controller.PredictFutureTransition();
                                        goto Last;
                                    }
                                }
                            }
                            if (controller.specifiedTransitions[i].toState != null) {
                                transitionsFromCurrent = controller.specifiedTransitions[i].toState.transitions;
                                currentState = controller.specifiedTransitions[i].toState;
                                nTransitionsFromCurrernt = transitionsFromCurrent.Count;
                            } else {
                                transitionsFromCurrent = new List<ActionTransition>();
                                nTransitionsFromCurrernt = 0;
                                currentState = null;
                            }
                        }
                        // Future
                        for (int i = 0; i < controller.futureTransitions.Count; i++) {
                            boxPositionBase.x += boxWidth * 1.5f;
                            for (int j = 0; j < nTransitionsFromCurrernt; j++) {
                                string stateName = transitionsFromCurrent[j].toState == null ?
                                    "Exit" :
                                    transitionsFromCurrent[j].toState.name;
                                Rect boxPosition = new Rect(new Vector2(boxPositionBase.x, boxPositionBase.y + j * 30), boxSize);
                                GUI.Box(boxPosition, stateName);
                                if (transitionsFromCurrent[j] == controller.futureTransitions[i]) {
                                    float boxY = (boxPosition.y + boxPosition.yMax) / 2;
                                    Drawing.DrawLine(transitionFromPos, new Vector2(boxPosition.x, boxY), Color.white, 2f, true);
                                    transitionFromPos = new Vector2(boxPosition.xMax, boxY);
                                }
                                Rect buttonRect = new Rect(boxPosition.x - 10, boxPosition.y, 20, 20);
                                if (GUI.Button(buttonRect, "")) {
                                    List<ActionTransition> addedTransitions = new List<ActionTransition>();
                                    for (int k = 0; k < i; k++) {
                                        addedTransitions.Add(controller.futureTransitions[k]);
                                    }
                                    addedTransitions.Add(transitionsFromCurrent[j]);
                                    controller.specifiedTransitions.AddRange(addedTransitions);
                                    ReloadHandles();
                                    ReloadSubmovements();
                                    controller.PredictFutureTransition();
                                    goto Last;
                                }
                            }
                            if (controller.futureTransitions[i].toState != null) {
                                transitionsFromCurrent = controller.futureTransitions[i].toState.transitions;
                                currentState = controller.futureTransitions[i].toState;
                                nTransitionsFromCurrernt = transitionsFromCurrent.Count;
                            } else {
                                transitionsFromCurrent = new List<ActionTransition>();
                                currentState = null;
                                nTransitionsFromCurrernt = 0;
                            }
                        }
                        // Last
                        //GUILayout.Space(boxWidth);
                        /*
                        GUILayout.BeginVertical();
                        boxPositionBase.x += boxWidth * 1.5f;
                        for (int j = 0; j < nTransitionsFromCurrernt; j++) {
                            string stateName = transitionsFromCurrent[j].toState == null ?
                                    "Exit" :
                                    transitionsFromCurrent[j].toState.name;
                            Rect boxPosition = new Rect(new Vector2(boxPositionBase.x, boxPositionBase.y + j * 30), boxSize);
                            GUI.Box(boxPosition, stateName);
                            Rect buttonRect = new Rect(boxPosition.x - 10, boxPosition.y, 20, 20);
                            if (GUI.Button(buttonRect, "")) {
                                stream.specifiedTransition.Add(transitionsFromCurrent[j]);
                                ReloadHandles();
                                ReloadSubmovements();
                            }
                        }
                        */
                    }
                } else {
                    GUILayout.BeginVertical();
                    GUILayout.Label("This window only works in play mode");
                    GUILayout.EndVertical();
                }
            }
            Last:
            GUILayout.EndArea();
        }

        void DrawBoneTable(Rect area = new Rect()) {
            GUILayout.BeginArea(new Rect(this.position.width * 0.9f, 0, this.position.width * 0.1f, this.position.height));
            GUILayout.BeginVertical();
            Color defaultColor = GUI.backgroundColor;
            for (int i = 0; i < boneStatusForTimelines.Count; i++) {
                var bone = boneStatusForTimelines[i];
                GUILayout.BeginHorizontal();
                boneStatusForTimelines[i].solo = GUILayout.Toggle(bone.solo, "", GUILayout.MaxWidth(20));
                bone.mute = GUILayout.Toggle(bone.mute, "", GUILayout.MaxWidth(20));
                GUILayout.Label(bone.bone.ToString());
                GUI.backgroundColor = bone.color;
                GUILayout.Box("");
                GUI.backgroundColor = defaultColor;
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        // グラフ領域のベース部分を描画
        Rect DrawGraphBase(Rect area, float[] xAxis, float[] yAxis, float xScale, float yScale, string xLabel, string yLabel) {
            if (xAxis.Count() != 2 || yAxis.Count() != 2) return new Rect();
            if (xAxis[1] < xAxis[0] || yAxis[1] < yAxis[0]) return new Rect();

            float xRange = xAxis[1] - xAxis[0];
            float yRange = yAxis[1] - yAxis[0];

            // メンバ化するかもしれない
            Vector2 labelSize = new Vector2(20f, 20f);

            //Rect graphArea = new Rect(area.x + 10, area.y + 10, area.width - 20, area.height - 20);
            Rect graphArea = area;
            int xGrid = (int)((xAxis[1] - xAxis[0]) / xScale) + 1;
            int yGrid = (int)((yAxis[1] - yAxis[0]) / yScale) + 1;
            float xRemainder = (xAxis[1] - xAxis[0]) % xScale;
            float yRemainder = (yAxis[1] - yAxis[0]) % yScale;

            Vector2 xLabelPos;
            for (int i = 0; i < xGrid; i++) {
                var lineColor = (i == 0) ? Color.white : Color.gray;
                var lineWidth = (i == 0) ? 2f : 1f;
                var x = (graphArea.width / xRange) * xScale * i;
                Drawing.DrawLine(
                    new Vector2(graphArea.x + x, graphArea.y),
                    new Vector2(graphArea.x + x, graphArea.yMax), lineColor, lineWidth, true);
                xLabelPos = new Vector2(graphArea.x + x - 4f, graphArea.yMax + 1f);
                GUI.Label(new Rect(xLabelPos, labelSize), (xScale * i).ToString());
            }
            Drawing.DrawLine(new Vector2(graphArea.xMax, graphArea.y), new Vector2(graphArea.xMax, graphArea.yMax), Color.white, 2f, true);
            xLabelPos = new Vector2(graphArea.xMax - 4f, graphArea.yMax + 1f);
            //GUI.Label(new Rect(xLabelPos, labelSize), xAxis[1].ToString());

            Vector2 yLabelPos;
            //Debug.Log("yGrid:" + yGrid);
            for (int i = 0; i < yGrid; i++) {
                var lineColor = (i == 0) ? Color.white : Color.gray;
                var lineWidth = (i == 0) ? 2f : 1f;
                var y = (graphArea.height / yRange) * yScale * i;
                Drawing.DrawLine(
                    new Vector2(graphArea.x, graphArea.yMax - y),
                    new Vector2(graphArea.xMax, graphArea.yMax - y), lineColor, lineWidth, true);
                yLabelPos = new Vector2(graphArea.x - 20f, graphArea.yMax - y - 10f);
                GUI.Label(new Rect(yLabelPos, labelSize), (yScale * i).ToString());
            }
            Drawing.DrawLine(new Vector2(graphArea.x, graphArea.y), new Vector2(graphArea.xMax, graphArea.y), Color.white, 2f, true);

            return graphArea;
        }

        void OnSceneGUI(SceneView sceneView) {
            if (currentAction != null && controller != null) {
                if (!controller.initialized) return;
                for (int j = 0; j < boneStatusForTimelines.Count; j++) {
                    if (!boneStatusForTimelines[j].solo) continue;
                    bool added = false;
                    int count = 0;
                    Handles.color = boneStatusForTimelines[j].color;
                    List<SubMovementLog> logs = null;
                    List<SubMovementLog> future = null;
                    for (int i = 0; i < controller.ActionLog.subMovementLogs.Count; i++) {
                        if (boneStatusForTimelines[j].bone.ToString().Equals(controller.ActionLog.subMovementLogs[i].bone.label)) {
                            logs = controller.ActionLog.subMovementLogs[i].logSubMovements;
                            future = controller.ActionLog.subMovementLogs[i].futureSubMovements;
                            break;
                        }
                    }
                    if (logs == null) {
                        logs = future;
                    } else if (future != null) {
                        count = future.Count;
                        added = true;
                        logs.AddRange(future);
                    }
                    if (logs != null && logs.Count > 0) {
                        Vector3[] trajectory = new Vector3[(int)(logs.Last().subMovement.t1 * 20)];
                        Vector3 basePos = logs[0].subMovement.p0;
                        int currentLog = 0;
                        for (int i = 0; i < trajectory.Length; i++) {
                            if (logs[currentLog].subMovement.t1 < (i * 0.05)) {
                                currentLog++;
                            }
                            trajectory[i] = logs[currentLog].subMovement.p0;
                        }
                        for (int i = 0; i < logs.Count(); i++) {
                            if (logs[i].subMovement.t0 != logs[i].subMovement.t1 && boneStatusForTimelines[j].solo) {
                                int stride = (int)(logs[i].subMovement.t0 * 20);
                                int last = (int)(logs[i].subMovement.t1 * 20);
                                for (int k = stride; k < last; k++) {
                                    Vector3 pos;
                                    Quaternion ori;
                                    logs[i].subMovement.GetCurrentPose(k * 0.05f, out pos, out ori);
                                    trajectory[k] += pos;
                                }
                            }
                        }
                        for (int i = 0; i < trajectory.Length - 1; i++) {
                            Handles.DrawLine(trajectory[i], trajectory[i + 1]);
                        }
                    } else {
                        //Debug.Log("Could not find logs of Bone " + boneStatusForTimelines[j].bone.ToString());
                    }
                    if (added) {
                        int index = logs.Count - count;
                        logs.RemoveRange(index, count);
                    }
                    // <!!> future未対応!!
                }
            }
        }

        // SpringDamperやVelocityの変更用のハンドルをリロードする
        // specifiedTransitionが変更されたら呼ぶこと
        void ReloadHandles() {
            Vector2 handlePos = new Vector2();
            Vector2 handleSize = new Vector2(10, 10);
            springDamperHandle = new VerticalDraggableBox[2] {
                new VerticalDraggableBox(new Rect(handlePos, handleSize), "S"),
                new VerticalDraggableBox(new Rect(handlePos, handleSize), "D"),
            };
            transitionTimeHandle = new HorizontalDraggableBox[3] {
                new HorizontalDraggableBox(new Rect(handlePos, handleSize), "S"),
                new HorizontalDraggableBox(new Rect(handlePos, handleSize), "F"),
                new HorizontalDraggableBox(new Rect(handlePos, handleSize), "M")
            };
        }

        void ReloadSubmovements() {
            // controllers * states
            //submovements = new List<List<SubMovement>>();
            /*
            foreach (var bone in boneStatusForTimelines) {
                List<SubMovement> sub = new List<SubMovement>();
                foreach (var transition in currentAction.specifiedTransition) {
                    sub.Add(new SubMovement());
                }
                submovements.Add(sub);
            }
            */
        }

        void UpdateTrajectory() {

        }
    }

}