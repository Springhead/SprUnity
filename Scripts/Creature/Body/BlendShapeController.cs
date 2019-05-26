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
[CustomEditor(typeof(BlendShapeController))]
public class BlendControllerEditor : Editor {
    public override void OnInspectorGUI() {
        var bl = (BlendShapeController)target;
        base.OnInspectorGUI();
        if (bl.vrmBlendShapeProxy != null) {
            //foreach (var clip in ai.blendS.BlendShapeAvatar.Clips) {
            for (int i = 0; i < bl.vrmBlendShapeProxy.BlendShapeAvatar.Clips.Count; i++) {
                var clip = bl.vrmBlendShapeProxy.BlendShapeAvatar.Clips[i];
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
            for (int i = 0; i < bsc.blendShapeNames.Count(); i++) {
                bsc.blendShapeNames[i] = bsc.blendShapeNames[i].ToUpper();
            }
        }
        if (GUILayout.Button("Load blendS Conflict")) {
            BlendShapeConflict bsc = BlendShapeConflict.GetBlendShapeConflict(bl.conflictName);
            if (bsc != null) {
                bl.conflicts = bsc.GetDictionary();
            }
        }
        if (GUILayout.Button("Load default blendS")) {
            bl.conflicts["NEUTRAL"] = (FacePart)(-1);
            bl.conflicts["A"] = FacePart.Mouth;
            bl.conflicts["I"] = FacePart.Mouth;
            bl.conflicts["U"] = FacePart.Mouth;
            bl.conflicts["E"] = FacePart.Mouth;
            bl.conflicts["O"] = FacePart.Mouth;
            bl.conflicts["BLINK"] = FacePart.Eyes;
            bl.conflicts["BLINK_L"] = FacePart.Eyes;
            bl.conflicts["BLINK_R"] = FacePart.Eyes;
            bl.conflicts["ANGRY"] = (FacePart)(-1);
            bl.conflicts["FUN"] = (FacePart)(-1);
            bl.conflicts["JOY"] = (FacePart)(-1);
            bl.conflicts["SORROW"] = (FacePart)(-1);
            bl.conflicts["SURPRISED"] = (FacePart)(-1);
            bl.conflicts["EXTRA"] = (FacePart)(-1);
            bl.conflicts[""] = (FacePart)(-1);
        }

    }
    public void OnEnable() {
        var bl = (BlendShapeController)target;
        if (bl.conflicts == null) {
            BlendShapeConflict bsc = BlendShapeConflict.GetBlendShapeConflict(bl.conflictName);
            if (bsc != null) {
                bl.conflicts = bsc.GetDictionary();
            }
        }
    }
}
#endif

public class BlendShapeMovement {
    public string name;
    public float value;
    public float time;
    public BlendShapeMovement(string name, float value, float time = 0.3f) {
        this.name = name.ToUpper();
        this.value = value;
        this.time = time;
    }
}

public class exeBlendShape {
    public float startTime;
    public BlendShapeMovement blendShapeMovement;
    public float velocity;
    public float resetVelocity;
}

public class BlendShapeController : MonoBehaviour {
    [HideInInspector]
    public float currTime = 0.0f;
    [HideInInspector]
    public Queue<BlendShapeMovement> blendTrajectory = new Queue<BlendShapeMovement>();
    public VRMBlendShapeProxy vrmBlendShapeProxy;
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
        if (vrmBlendShapeProxy != null) {
            currTime += Time.deltaTime;
            while (blendTrajectory.Count() > 0) {
                foreach (var exe in exeList) {
                    if (conflicts.ContainsKey(exe.blendShapeMovement.name.ToUpper()) && conflicts.ContainsKey(blendTrajectory.First().name.ToUpper()) &&
                        (conflicts[exe.blendShapeMovement.name.ToUpper()] & conflicts[blendTrajectory.First().name.ToUpper()]) != 0) {
                        deleteList.Add(exe);
                    }
                }
                foreach (var deleteObj in deleteList) {
                    exeList.Remove(deleteObj);
                }
                deleteList.Clear();
                var newexe = new exeBlendShape();
                newexe.startTime = currTime;
                newexe.blendShapeMovement = blendTrajectory.Dequeue();
                newexe.velocity = (newexe.blendShapeMovement.value - vrmBlendShapeProxy.GetValue(newexe.blendShapeMovement.name.ToUpper())) / newexe.blendShapeMovement.time;
                newexe.resetVelocity = 1 / newexe.blendShapeMovement.time;
                exeList.Add(newexe);
                //Debug.Log(blendTrajectory.First().blend + oneStep);
            }
            foreach (var exe in exeList) {
                float nowblend = vrmBlendShapeProxy.GetValue(exe.blendShapeMovement.name.ToUpper());
                foreach (var oneb in vrmBlendShapeProxy.GetValues()) {
                    if (oneb.Key.ToString().ToUpper() != exe.blendShapeMovement.name.ToUpper() &&
                        (!conflicts.ContainsKey(exe.blendShapeMovement.name.ToUpper()) || ((conflicts[exe.blendShapeMovement.name.ToUpper()] & conflicts[oneb.Key.Name.ToUpper()]) != 0))) {
                        //Debug.Log("setvalue: " + oneb.Key.ToString().ToUpper() + " " + exe.bsm.blend.ToUpper() + " " +
                        //    conflicts[exe.bsm.blend.ToUpper()] + " " + conflicts[oneb.Key.Name.ToUpper()] + " " + (conflicts[exe.bsm.blend.ToUpper()] & conflicts[oneb.Key.Name.ToUpper()]));
                        vrmBlendShapeProxy.ImmediatelySetValue(oneb.Key, Mathf.Clamp01(oneb.Value - exe.resetVelocity * Time.deltaTime));
                    }
                }
                nowblend += exe.velocity * Time.deltaTime;
                if (exe.blendShapeMovement.name.ToUpper() != "") {
                    vrmBlendShapeProxy.ImmediatelySetValue(exe.blendShapeMovement.name.ToUpper(), Mathf.Clamp01(nowblend));
                }
                if (currTime - exe.startTime > exe.blendShapeMovement.time/* ||(Mathf.Abs(blendS.GetValue(blend) - blendv) < oneStep* Time.deltaTime && isAllZero)*/) {
                    if (exe.blendShapeMovement.name.ToUpper() != "") {
                        vrmBlendShapeProxy.ImmediatelySetValue(exe.blendShapeMovement.name.ToUpper(), Mathf.Clamp01(exe.blendShapeMovement.value));
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
                vrmBlendShapeProxy = body.animator.GetComponent<VRMBlendShapeProxy>();
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
        BlendShapeMovement newbsm = new BlendShapeMovement(blend.ToUpper(), blendv, time);
        blendTrajectory.Enqueue(newbsm);
    }
}
