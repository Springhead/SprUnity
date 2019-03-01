using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SprUnity;
using SprCs;
using UnityEngine;
using UnityEditor;

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
    public VerticalDraggableBox(Rect rect, string t) : base(rect, t){
        
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
    private ActionStateMachineStatus currentAction;

    //float timeAxisLength = 5.0f;

    float springDamperMax = 1.0f;

    float currentTime;
    float totalTime = 0;
    List<float> startSubmovementTime = new List<float>();
    List<float> endSubmovementTime = new List<float>();

    Vector2 graphAreaSize = new Vector2(400f, 40f);

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

    List<List<SubMovement>> submovements;

    List<VerticalDraggableBox[]> springDamperHandle;
    List<HorizontalDraggableBox[]> transitionTimeHandle;

    // Transition
    float marginLeft = 10;
    float marginTop = 20;

    // グラフ描画に使うセグメント分割数
    // 値が大きいと重いので、だれかいい方法教えてください
    int segments = 2;

    [MenuItem("Window/Action Timeline Window")]
    static void Open() {
        window = GetWindow<ActionTimelineWindow>();
        ActionEditorWindowManager.instance.timelineWindow = window;
    }

    public void OnEnable() {
        style = new GUIStyle();
        style.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
        style.alignment = TextAnchor.MiddleCenter;
        style.border = new RectOffset(12, 12, 12, 12);
        // <!!>これ、ここか？
        foreach (var action in ActionEditorWindowManager.instance.actions) {
            action.templeteTransition = new List<ActionTransition>();
            for (int i = 0; ; i++) {
                Debug.Log("tri to load:" + SessionState.GetInt(action.action.name + i, -1));
                ActionTransition transition = EditorUtility.InstanceIDToObject(SessionState.GetInt(action.action.name + i, -1)) as ActionTransition;
                SessionState.EraseInt(action.action.name + i);
                if (transition == null) break;
                else action.templeteTransition.Add(transition);
            }
        }
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
    }

    public void OnDisable() {
        foreach (var action in ActionEditorWindowManager.instance.actions) {
            for (int i = 0; i < action.templeteTransition.Count; i++) {
                SessionState.SetInt(action.action.name + i, action.templeteTransition[i].GetInstanceID());
            }
        }
        foreach(var boneStatus in boneStatusForTimelines) {
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
        var actions = ActionEditorWindowManager.instance.selectedAction;
        if (actions.Count == 1) {
            currentAction = actions[0];
            float enterTime = 0;
            startSubmovementTime = new List<float>();
            endSubmovementTime = new List<float>();
            int[] lastSub;
            lastSub = new int[boneStatusForTimelines.Count];
            Pose[] lastPose;
            lastPose = new Pose[boneStatusForTimelines.Count];
            Vector2[] lastSpringDamper;
            lastSpringDamper = new Vector2[boneStatusForTimelines.Count];
            int i = 0;
            foreach(var bone in boneStatusForTimelines) {
                lastPose[i].position = ActionEditorWindowManager.instance.body[bone.bone].ikEndEffector.transform.position;
                lastPose[i].rotation = ActionEditorWindowManager.instance.body[bone.bone].ikEndEffector.transform.rotation;
                lastSpringDamper[i] = new Vector2();
                i++;
            }
            i = 0;
            foreach (var transition in currentAction.templeteTransition) {
                // これはExitへの遷移なので別処理
                if (transition.toState == null) break;
                // Entryは除くため
                if(transition.fromState != null) enterTime += transition.time;
                var nextKeyFrame = transition.toState.keyframe;
                startSubmovementTime.Add(enterTime);
                endSubmovementTime.Add(enterTime + (transition.toState ? transition.toState.duration : 0));
                Vector2 springDamper = new Vector2((transition.toState ? transition.toState.spring : 0), (transition.toState ? transition.toState.damper : 0));
                if (nextKeyFrame) {
                    foreach (var boneKeyPose in nextKeyFrame.keyposes[0].boneKeyPoses) {
                        for (int j = 0; j < boneStatusForTimelines.Count; j++) {
                            if (boneStatusForTimelines[j].bone == boneKeyPose.boneId) {
                                submovements[j][i].p0 = lastPose[j].position;
                                submovements[j][i].q0 = lastPose[j].rotation;
                                submovements[j][i].s0 = lastSpringDamper[j];
                                submovements[j][i].t0 = enterTime;

                                submovements[j][i].p1 = lastPose[j].position = boneKeyPose.position;
                                submovements[j][i].q1 = lastPose[j].rotation = boneKeyPose.rotation;
                                submovements[j][i].s1 = springDamper; lastSpringDamper[j] = springDamper;
                                submovements[j][i].t1 = enterTime + transition.toState.duration;
                            }
                        }
                    }
                }
                i++;
            }
            totalTime = endSubmovementTime.Count == 0 ? 0 : endSubmovementTime.Max();
        } else {
            currentAction = null;
            totalTime = 0;
        }
        totalTime = Mathf.Max(5.0f, totalTime * 1.2f);
        GUILayout.BeginVertical();
        DrawSpringDamperGraph();
        DrawVelocityGraph();
        DrawTransitionGraph();
        GUILayout.EndVertical();
        DrawBone();
    }

    void DrawSpringDamperGraph(Rect area = new Rect()) {
        GUILayout.BeginArea(new Rect(0, 0, this.position.width, this.position.height / 3));
        GUILayout.Label("Spring & Damper");
        showSpring = GUILayout.Toggle(showSpring, "Spring");
        showDamper = GUILayout.Toggle(showDamper, "Damper");
        // Draw Graph Base
        int graphBottom = (int)(position.height * 0.27);
        int graphTop = (int)(position.height * 0.01);
        int graphHeight = graphBottom - graphTop;
        int graphLeft = (int)(position.width * 0.1);
        int graphRight = (int)(position.width * 0.9);
        int graphWidth = graphRight - graphLeft;
        float[] xAxis = new float[2]{ 0, totalTime };
        float[] yAxis = new float[2] { 0f, springDamperMax };
        DrawGraphBase(new Rect(graphLeft, graphTop, graphWidth, graphHeight), xAxis, yAxis, 1.0f, 0.2f, "", "");
        // Draw Time Axis
        GUI.HorizontalSlider(new Rect(graphLeft, graphBottom, graphWidth, 20), currentTime, 0, totalTime);
        if (currentAction != null) {
            // Draw Individual
            for(int i = 0; i < currentAction.templeteTransition.Count; i++) {
                var transition = currentAction.templeteTransition[i];
                if (transition.toState == null) break;
                float spring = transition.toState ? transition.toState.spring : 0;
                float damper = transition.toState ? transition.toState.damper : 0;
                GUI.Box(new Rect(graphLeft + 0.8f * position.width * (endSubmovementTime[i] / totalTime) - 5, graphBottom - (spring / springDamperMax) * graphHeight - 5, 10, 10),
                    "S");
                for (int j = 0; j < boneStatusForTimelines.Count; j++) {
                    if (submovements[j][i].t0 != submovements[j][i].t1 && boneStatusForTimelines[j].solo) {
                        Vector2 lastPos = new Vector2(graphLeft + graphWidth * (startSubmovementTime[i] / totalTime), graphBottom - (submovements[j][i].s0[0] / springDamperMax) * graphHeight);
                        Vector2 nextPos = new Vector2();
                        Vector2 sp = new Vector2();
                        Color color = boneStatusForTimelines[j].color;
                        float currentSubmovementTime;
                        for (int k = 0; k < segments; k++) {
                            currentSubmovementTime = (endSubmovementTime[i] - startSubmovementTime[i]) * ((float)(k + 1) / segments) + startSubmovementTime[i];
                            nextPos.x = graphLeft + graphWidth * (currentSubmovementTime / totalTime);
                            submovements[j][i].GetCurrentSpringDamper(currentSubmovementTime, out sp);
                            nextPos.y = graphBottom - graphHeight * ((sp[0] + submovements[j][i].s0[0]) / springDamperMax);
                            Drawing.DrawLine(lastPos, nextPos, color, 3, true);
                            lastPos = nextPos;
                        }
                    }
                }
            }
        // Draw Integrated
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
        float[] xAxis = new float[2] { 0, totalTime };
        float[] yAxis = new float[2] { 0f, 1f };
        DrawGraphBase(new Rect(graphLeft, graphTop, graphWidth, graphHeight), xAxis, yAxis, 1.0f, 0.2f, "", "");
        if (currentAction != null) {
            // Draw Individual
            for (int i = 0; i < currentAction.templeteTransition.Count; i++) {
                var transition = currentAction.templeteTransition[i];
                if (transition.toState == null) break;
                transitionTimeHandle[i][0].box.x = graphLeft + graphWidth * (startSubmovementTime[i] / totalTime);
                transitionTimeHandle[i][0].box.y = graphBottom;
                transitionTimeHandle[i][0].Draw();
                transitionTimeHandle[i][1].box.x = graphLeft + graphWidth * (endSubmovementTime[i] / totalTime);
                transitionTimeHandle[i][1].box.y = graphBottom;
                transitionTimeHandle[i][1].Draw();
                transitionTimeHandle[i][2].box.x = graphLeft + graphWidth * ((startSubmovementTime[i] + endSubmovementTime[i]) / (2 * totalTime));
                transitionTimeHandle[i][2].box.y = graphBottom;
                transitionTimeHandle[i][2].Draw();
                for(int j = 0; j < boneStatusForTimelines.Count; j++) {
                    if(submovements[j][i].t0 != submovements[j][i].t1 && boneStatusForTimelines[j].solo) {
                        Vector2 lastPos = new Vector2(graphLeft + graphWidth * (startSubmovementTime[i] / totalTime), graphBottom);
                        Vector2 nextPos = new Vector2();
                        Vector3 vel = new Vector3();
                        Color color = boneStatusForTimelines[j].color;
                        float currentSubmovementTime;
                        for(int k = 0; k < segments; k++) {
                            currentSubmovementTime = (endSubmovementTime[i] - startSubmovementTime[i]) * ((float)(k + 1) / segments) + startSubmovementTime[i];
                            nextPos.x = graphLeft + graphWidth * (currentSubmovementTime / totalTime);
                            submovements[j][i].GetCurrentVelocity(currentSubmovementTime, out vel);
                            nextPos.y = graphBottom - graphHeight * (vel.magnitude / yAxis[1]);
                            Debug.Log(currentSubmovementTime + " " +  vel.magnitude);
                            Drawing.DrawLine(lastPos, nextPos, color, 3, true);
                            lastPos = nextPos;
                        }
                    }
                }
                //GUI.Box(new Rect(graphLeft + graphWidth * (startSubmovementTime[i] / totalTime), graphBottom, 10, 10), "");
                //GUI.Box(new Rect(graphLeft + graphWidth * (endSubmovementTime[i] / totalTime), graphBottom, 10, 10), "");
                //GUI.Box(new Rect(graphLeft + graphWidth * ((startSubmovementTime[i] + endSubmovementTime[i]) / (2 * totalTime)), graphBottom, 10, 10), "");
            }
            for (int i = 0; i < currentAction.templeteTransition.Count; i++) {
                var transition = currentAction.templeteTransition[i];
                if (transition.toState == null) break;
                if (transitionTimeHandle[i][0].ProcessEvents()) {
                    float time = (transitionTimeHandle[i][0].box.x - graphLeft) * (totalTime / graphWidth);
                    if(i != 0) {
                        currentAction.templeteTransition[i].time = time - startSubmovementTime[i - 1];
                    }
                }
                if (transitionTimeHandle[i][1].ProcessEvents() && transition.toState.keyframe != null) {
                    float time = (transitionTimeHandle[i][1].box.x - graphLeft) * (totalTime / graphWidth);
                    currentAction.templeteTransition[i].toState.duration = time - startSubmovementTime[i];
                }
                //GUI.Box(new Rect(graphLeft + graphWidth * (startSubmovementTime[i] / totalTime), graphBottom, 10, 10), "");
                //GUI.Box(new Rect(graphLeft + graphWidth * (endSubmovementTime[i] / totalTime), graphBottom, 10, 10), "");
                //GUI.Box(new Rect(graphLeft + graphWidth * ((startSubmovementTime[i] + endSubmovementTime[i]) / (2 * totalTime)), graphBottom, 10, 10), "");
            }
            // Draw Integrated
        }
        GUILayout.EndArea();
    }

    void DrawTransitionGraph(Rect area = new Rect()) {
        GUILayout.BeginArea(new Rect(0, 2 * this.position.height / 3, this.position.width, this.position.height / 3));
        GUILayout.Label("Transition");
        var actions = ActionEditorWindowManager.instance.selectedAction;
        if (actions.Count == 1) {
            // ActionStatus取得
            var stream = actions[0];

            // 各種設定
            int numTransitionsInStream = stream.templeteTransition.Count;
            GUILayout.Label("num:" + numTransitionsInStream);
            int boxWidth = (int)Mathf.Max(Mathf.Min(position.width / (numTransitionsInStream + 5), maxBoxWidth), minBoxWidth);
            //int boxWidth = (int)(position.width / (numStatesInStream + 5));
            int nTransitionsFromCurrernt = stream.action.entryTransitions.Count;
            var transitionsFromCurrent = stream.action.entryTransitions;
            Vector2 boxSize = new Vector2(boxWidth, 20);
            Vector2 boxPositionBase;
            Vector2 transitionFromPos;
            //GUILayout.BeginHorizontal();

            // Entry
            boxPositionBase.x = marginLeft;
            boxPositionBase.y = 40;
            Rect entryRect = new Rect(boxPositionBase, boxSize);
            transitionFromPos = new Vector2(entryRect.xMax, (entryRect.y + entryRect.yMax) / 2);
            GUI.Box(entryRect, "Entry");

            // Others
            for(int i = 0; i < numTransitionsInStream; i++) {
                boxPositionBase.x += boxWidth * 1.5f;
                for(int j = 0; j < nTransitionsFromCurrernt; j++) {
                    string stateName = transitionsFromCurrent[j].toState == null ?
                        "Exit" :
                        transitionsFromCurrent[j].toState.name;
                    Rect boxPosition = new Rect(new Vector2(boxPositionBase.x, boxPositionBase.y + j * 30), boxSize);
                    GUI.Box(boxPosition, stateName);
                    if(transitionsFromCurrent[j] == stream.templeteTransition[i]) {
                        float boxY = (boxPosition.y + boxPosition.yMax) / 2;
                        Drawing.DrawLine(transitionFromPos, new Vector2(boxPosition.x, boxY), Color.white, 2f, true);
                        transitionFromPos = new Vector2(boxPosition.xMax, boxY);
                    }
                    Rect buttonRect = new Rect(boxPosition.x - 10, boxPosition.y, 20, 20);
                    if (GUI.Button(buttonRect, "")) {
                        if (transitionsFromCurrent[j] != stream.templeteTransition[i]) {
                            List<ActionTransition> changedTransitions = new List<ActionTransition>();
                            for (int k = 0; k < i; k++) {
                                changedTransitions.Add(stream.templeteTransition[i]);
                            }
                            changedTransitions.Add(transitionsFromCurrent[j]);
                            stream.templeteTransition = changedTransitions;
                            ReloadHandles();
                            ReloadSubmovements();
                            goto Last;
                        }
                    }
                }
                if (stream.templeteTransition[i].toState != null) {
                    transitionsFromCurrent = stream.templeteTransition[i].toState.transitions;
                    nTransitionsFromCurrernt = transitionsFromCurrent.Count;
                } else {
                    transitionsFromCurrent = new List<ActionTransition>();
                    nTransitionsFromCurrernt = 0;
                }
            }
            // Last
            //GUILayout.Space(boxWidth);
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
                    stream.templeteTransition.Add(transitionsFromCurrent[j]);
                    ReloadHandles();
                    ReloadSubmovements();
                }
            }
            Last:
            GUILayout.EndVertical();
            //GUILayout.EndHorizontal();
        }
        GUILayout.EndArea();
    }

    void DrawBone(Rect area = new Rect()) {
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
        Debug.Log("yGrid:" + yGrid);
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

    // SpringDamperやVelocityの変更用のハンドルをリロードする
    // templeteTransitionが変更されたら呼ぶこと
    void ReloadHandles() {
        springDamperHandle = new List<VerticalDraggableBox[]>();
        transitionTimeHandle = new List<HorizontalDraggableBox[]>();
        Vector2 handleSize = new Vector2(10, 10);
        Vector2 handlePos = new Vector2();
        for(int i = 0; i < currentAction.templeteTransition.Count; i++) {
            springDamperHandle.Add(new VerticalDraggableBox[2] {
                new VerticalDraggableBox(new Rect(handlePos, handleSize), "S" + i),
                new VerticalDraggableBox(new Rect(handlePos, handleSize), "D" + i),
            });
            transitionTimeHandle.Add(new HorizontalDraggableBox[3] {
                new HorizontalDraggableBox(new Rect(handlePos, handleSize), "S" + i),
                new HorizontalDraggableBox(new Rect(handlePos, handleSize), "M" + i),
                new HorizontalDraggableBox(new Rect(handlePos, handleSize), "F" + i)
            });
        }
    }

    void ReloadSubmovements() {
        // controllers * states
        submovements = new List<List<SubMovement>>();
        foreach (var bone in boneStatusForTimelines) {
            List<SubMovement> sub = new List<SubMovement>();
            foreach (var transition in currentAction.templeteTransition) {
                sub.Add(new SubMovement());
            }
            submovements.Add(sub);
        }
    }
}
