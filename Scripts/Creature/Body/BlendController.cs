using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRM;
using SprUnity;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
[CustomEditor(typeof(BlendController))]
public class BlendControllerEditor : Editor {
    public override void OnInspectorGUI() {
        var bl = (BlendController)target;
        base.OnInspectorGUI();
        if (bl.blendS != null) {
            //foreach (var clip in ai.blendS.BlendShapeAvatar.Clips) {
            for (int i = 0; i < bl.blendS.BlendShapeAvatar.Clips.Count; i++) {
                var clip = bl.blendS.BlendShapeAvatar.Clips[i];
                GUILayout.BeginHorizontal();
                //GUILayout.Label(clip.BlendShapeName,GUILayout.Width(100));
                if (!bl.conflicts.ContainsKey(clip.BlendShapeName.ToUpper())) {
                    bl.conflicts.Add(clip.BlendShapeName.ToUpper(), 0);
                }

                bl.conflicts[clip.BlendShapeName.ToUpper()] =
                    (FacePart)EditorGUILayout.EnumFlagsField(
                        clip.BlendShapeName.ToUpper(), bl.conflicts[clip.BlendShapeName.ToUpper()]);
                GUILayout.EndHorizontal();
            }
            if (!bl.conflicts.ContainsKey("")) {
                bl.conflicts.Add("", 0);
            }
            bl.conflicts[""] = (FacePart)EditorGUILayout.EnumFlagsField("\"\"", bl.conflicts[""]);
        }

        if (GUILayout.Button("create blendS Conflict")) {
            bl.createConflict(bl.conflictName);
        }
        //if (GUILayout.Button("save blendS Conflict")) {
        //    BlendShapeConflict bsc = BlendShapeConflict.GetBlendShapeConflict(bl.conflictName);
        //    if (bsc != null) {
        //        bsc.Save(bl.conflicts);
        //    }
        //}
        if (GUILayout.Button("change upper")) {
            BlendShapeConflict bsc = BlendShapeConflict.GetBlendShapeConflict(bl.conflictName);
            for (int i = 0; i < bsc.blends.Count(); i++) {
                bsc.blends[i] = bsc.blends[i].ToUpper();
            }
        }
        if (GUILayout.Button("Load blendS Conflict")) {
            BlendShapeConflict bsc = BlendShapeConflict.GetBlendShapeConflict(bl.conflictName);
            if (bsc != null) {
                bl.conflicts = bsc.GetDictionary();
            }
        }
        if (GUILayout.Button("blendSTest")) {
            bl.blendSTest();
        }
        if (GUILayout.Button("zero")) {
            bl.blendZero();
        }
        if (GUILayout.Button("a")) {
        }
    }
    public void OnEnable() {
        var bl = (BlendController)target;
        BlendShapeConflict bsc = BlendShapeConflict.GetBlendShapeConflict(bl.conflictName);
        if (bsc != null) {
            bl.conflicts = bsc.GetDictionary();
        }
    }
}
#endif

public class BlendSMovement {
    public string blend;
    public float value;
    public float time;
    public BlendSMovement(string blend, float value, float time = 0.3f) {
        this.blend = blend;
        this.value = value;
        this.time = time;
    }
}

public class exeBlendShape {
    public float startTime;
    public BlendSMovement bsm;
    public float velocity;
    public float resetVelocity;
}
public class BlendController : MonoBehaviour {
    [HideInInspector]
    public float currTime = 0.0f;
    [HideInInspector]
    public Queue<BlendSMovement> blendTrajectory = new Queue<BlendSMovement>();
    public VRMBlendShapeProxy blendS;
    public string conflictName = "vroidConflict";
    public Dictionary<string, FacePart> conflicts = new Dictionary<string, FacePart>();
    private List<exeBlendShape> exeList = new List<exeBlendShape>();
    private List<exeBlendShape> deleteList = new List<exeBlendShape>();
    private Body body;
    private ActionManager actionManager;
    // Use this for initialization
    void Start() {
        actionManager = FindObjectOfType<ActionManager>();
        BlendShapeConflict bsc = BlendShapeConflict.GetBlendShapeConflict(conflictName);
        if (bsc != null) {
            conflicts = bsc.GetDictionary();
        }

        exeList = new List<exeBlendShape>();
    }

    void Update() {
        if (blendS != null) {
            currTime += Time.deltaTime;
            if (blendTrajectory.Count() > 0) {
                var nextblendOK = true;
                foreach (var exe in exeList) {
                    if ((conflicts[exe.bsm.blend] & conflicts[blendTrajectory.First().blend]) != 0) {
                        nextblendOK = false;
                    }
                }
                if (nextblendOK) {
                    var newexe = new exeBlendShape();
                    newexe.startTime = currTime;
                    newexe.bsm = blendTrajectory.Dequeue();
                    newexe.velocity = Mathf.Abs(newexe.bsm.value - blendS.GetValue(newexe.bsm.blend)) / newexe.bsm.time;
                    newexe.resetVelocity = 1 / newexe.bsm.time;
                    exeList.Add(newexe);
                    //Debug.Log(blendTrajectory.First().blend + oneStep);
                }
            }
            foreach (var exe in exeList) {
                float nowblend = blendS.GetValue(exe.bsm.blend);
                foreach (var oneb in blendS.GetValues()) {
                    if (oneb.Key.ToString() != exe.bsm.blend &&
                        ((conflicts[exe.bsm.blend.ToUpper()] & conflicts[oneb.Key.Name.ToUpper()]) != 0)) {
                        blendS.ImmediatelySetValue(oneb.Key, Mathf.Clamp01(oneb.Value - exe.resetVelocity * Time.deltaTime));
                    }
                }
                if (nowblend >= exe.bsm.value) {
                    nowblend -= exe.velocity * Time.deltaTime;
                } else {
                    nowblend += exe.velocity * Time.deltaTime;
                }
                if (exe.bsm.blend != "") {
                    blendS.ImmediatelySetValue(exe.bsm.blend, Mathf.Clamp01(nowblend));
                }
                if (currTime - exe.startTime > exe.bsm.time/* ||(Mathf.Abs(blendS.GetValue(blend) - blendv) < oneStep* Time.deltaTime && isAllZero)*/) {
                    if (exe.bsm.blend != "") {
                        blendS.ImmediatelySetValue(exe.bsm.blend, Mathf.Clamp01(exe.bsm.value));
                    }
                    deleteList.Add(exe);
                }
            }
            foreach (var deleteObj in deleteList) {
                exeList.Remove(deleteObj);
            }
            deleteList.Clear();

        }
    }
    public void createConflict(string name) {
        BlendShapeConflict.GetBlendShapeConflict(name);
    }
    public void blendSTest() {
        //actionManager.Action("blendSTest");
        BlendSMovement newbsm = new BlendSMovement("A", 1f, 0.3f);
        BlendSMovement newbsm2 = new BlendSMovement("BLINK", 1f, 0.3f);
        BlendSMovement newbsm3 = new BlendSMovement("BLINK_L", 1f, 0.1f);
        BlendSMovement newbsm4 = new BlendSMovement("BLINK_R", 1f, 0.2f);
        BlendSMovement newbsm5 = new BlendSMovement("JOY", 1f, 0.2f);
        blendTrajectory.Enqueue(newbsm);
        blendTrajectory.Enqueue(newbsm2);
        blendTrajectory.Enqueue(newbsm3);
        blendTrajectory.Enqueue(newbsm4);
        blendTrajectory.Enqueue(newbsm5);
    }
    public void blendZero() {
        BlendSMovement newbsm = new BlendSMovement("", 1f, 0.3f);
        blendTrajectory.Enqueue(newbsm);
    }
}
