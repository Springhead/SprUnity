using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SprUnity;
using System.Linq;

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
    public string conflictName = "vroidConflict";
    public Dictionary<string, FacePart> conflicts = new Dictionary<string, FacePart>();
    protected List<exeBlendShape> exeList = new List<exeBlendShape>();
    protected List<exeBlendShape> deleteList = new List<exeBlendShape>();
    protected Body body;
    protected ActionManager actionManager;
    // Use this for initialization
    void Start() {
    }

    // ここに一般的な補完を描くべき
    void Update() {
    }
    public virtual void BlendSet(float interval, string blend, float blendv, float time) {
    }
}
