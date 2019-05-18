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
        if (GUILayout.Button("save blendS Conflict")) {
            BlendShapeConflict bsc = BlendShapeConflict.GetBlendShapeConflict(bl.conflictName);
            if (bsc != null) {
                bsc.Save(bl.conflicts);
            }
        }
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
    }
    public void OnEnable() {
        var bl = (BlendController)target;
        if (bl.conflicts == null) {
            BlendShapeConflict bsc = BlendShapeConflict.GetBlendShapeConflict(bl.conflictName);
            if (bsc != null) {
                bl.conflicts = bsc.GetDictionary();
            }
        }
    }
}
#endif

public class BlendSMovement {
    public string blend;
    public float value;
    public float time;
    public BlendSMovement(string blend, float value, float time = 0.3f) {
        this.blend = blend.ToUpper();
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
        body = FindObjectOfType<Body>();
        if (bsc != null) {
            conflicts = bsc.GetDictionary();
        }
        exeList = new List<exeBlendShape>();
    }

    void Update() {
        if (blendS != null) {
            currTime += Time.deltaTime;
            while (blendTrajectory.Count() > 0) {
                foreach (var exe in exeList) {
                    if (conflicts.ContainsKey(exe.bsm.blend.ToUpper()) && conflicts.ContainsKey(blendTrajectory.First().blend.ToUpper()) &&
                        (conflicts[exe.bsm.blend.ToUpper()] & conflicts[blendTrajectory.First().blend.ToUpper()]) != 0) {
                        deleteList.Add(exe);
                    }
                }
                foreach (var deleteObj in deleteList) {
                    exeList.Remove(deleteObj);
                }
                deleteList.Clear();
                var newexe = new exeBlendShape();
                newexe.startTime = currTime;
                newexe.bsm = blendTrajectory.Dequeue();
                newexe.velocity = (newexe.bsm.value - blendS.GetValue(newexe.bsm.blend.ToUpper())) / newexe.bsm.time;
                newexe.resetVelocity = 1 / newexe.bsm.time;
                exeList.Add(newexe);
                //Debug.Log(blendTrajectory.First().blend + oneStep);
            }
            foreach (var exe in exeList) {
                float nowblend = blendS.GetValue(exe.bsm.blend.ToUpper());
                foreach (var oneb in blendS.GetValues()) {
                    if (oneb.Key.ToString().ToUpper() != exe.bsm.blend.ToUpper() &&
                        (!conflicts.ContainsKey(exe.bsm.blend.ToUpper()) || ((conflicts[exe.bsm.blend.ToUpper()] & conflicts[oneb.Key.Name.ToUpper()]) != 0))) {
                        //Debug.Log("setvalue: " + oneb.Key.ToString().ToUpper() + " " + exe.bsm.blend.ToUpper() + " " +
                        //    conflicts[exe.bsm.blend.ToUpper()] + " " + conflicts[oneb.Key.Name.ToUpper()] + " " + (conflicts[exe.bsm.blend.ToUpper()] & conflicts[oneb.Key.Name.ToUpper()]));
                        blendS.ImmediatelySetValue(oneb.Key, Mathf.Clamp01(oneb.Value - exe.resetVelocity * Time.deltaTime));
                    }
                }
                nowblend += exe.velocity * Time.deltaTime;
                if (exe.bsm.blend.ToUpper() != "") {
                    blendS.ImmediatelySetValue(exe.bsm.blend.ToUpper(), Mathf.Clamp01(nowblend));
                }
                if (currTime - exe.startTime > exe.bsm.time/* ||(Mathf.Abs(blendS.GetValue(blend) - blendv) < oneStep* Time.deltaTime && isAllZero)*/) {
                    if (exe.bsm.blend.ToUpper() != "") {
                        blendS.ImmediatelySetValue(exe.bsm.blend.ToUpper(), Mathf.Clamp01(exe.bsm.value));
                    }
                    deleteList.Add(exe);
                }
            }
            foreach (var deleteObj in deleteList) {
                exeList.Remove(deleteObj);
            }
            deleteList.Clear();
        } else {
            if (body.initialized) {
                blendS = body.animator.GetComponent<VRMBlendShapeProxy>();
            }
        }

    }
    public void createConflict(string name) {
        BlendShapeConflict.GetBlendShapeConflict(name);
    }
    public void BlendSet(float interval, string blend, float blendv, float time) {
        StartCoroutine(coBlendSet(interval, blend.ToUpper(), blendv, time));
    }
    private IEnumerator coBlendSet(float interval, string blend, float blendv, float time) {
        yield return new WaitForSeconds(interval);
        BlendSMovement newbsm = new BlendSMovement(blend.ToUpper(), blendv, time);
        blendTrajectory.Enqueue(newbsm);
    }
}
