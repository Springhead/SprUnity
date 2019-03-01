using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using SprUnity;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(TeawaseActionBehaviour))]
public class TeawaseActionBehaviourEditor : Editor {
    public async override void OnInspectorGUI() {
        base.OnInspectorGUI();
        TeawaseActionBehaviour action = (TeawaseActionBehaviour)target;
        if (GUILayout.Button("Start")) {
            action.BeginAction();
        }
        if (GUILayout.Button("Stop")) {

        }
    }
}

#endif

public class TeawaseActionBehaviour : ScriptableAction {

    public bool editing = false;
    //[HideInInspector]
    public bool actionEnabled = false;

    public Body body;

    // KeyPose
    public KeyPoseInterpolationGroup teawaseKeyPoseUp;
    public KeyPoseInterpolationGroup teawaseKeyPoseDown;

    public bool continueLeftForward = false;
    public bool continueBothForward = false;

    public bool bothForwardLeftEnable = true;
    public bool bothForwardRightEnable = true;

    [HideInInspector]
    public float mirrorZ = -1; // AIEventHandlerからセットされる

    // Target positions
    private Vector3 leftOnlyTouchDefaultPos = new Vector3();
    private Vector3 leftTouchDefaultPos = new Vector3();
    private Vector3 rightTouchDefaultPos = new Vector3();

    private Vector3 leftTouchTargetLPF = new Vector3();
    private Vector3 rightTouchTargetLPF = new Vector3();

    [HideInInspector]
    public Vector3 leftOnlyTouchTarget = new Vector3();
    [HideInInspector]
    public Vector3 leftTouchTarget;
    [HideInInspector]
    public Vector3 rightTouchTarget;

    void Start() {

        var upKeyPoses = teawaseKeyPoseUp.keyposes[0].boneKeyPoses;
        //var downKeyPoses = teawaseKeyPoseDown.keyposes[0].boneKeyPoses;
        Pose leftHandPose = new Pose();
        Pose rightHandPose = new Pose();
        foreach (var boneKeyPose in upKeyPoses) {
            if (boneKeyPose.boneId == HumanBodyBones.LeftHand) { leftHandPose.position = boneKeyPose.position; leftHandPose.rotation = boneKeyPose.rotation; }
            if (boneKeyPose.boneId == HumanBodyBones.RightHand) { rightHandPose.position = boneKeyPose.position; rightHandPose.rotation = boneKeyPose.rotation; }
        }
        //leftOnlyTouchDefaultPos = GameObject.Find("LeftHandForward/Left").transform.position;
        leftTouchDefaultPos = leftHandPose.position;
        rightTouchDefaultPos = rightHandPose.position;
        
        leftTouchTarget = leftTouchDefaultPos;
        rightTouchTarget = rightTouchDefaultPos;

        leftTouchTargetLPF = leftTouchDefaultPos;
        rightTouchTargetLPF = rightTouchDefaultPos;
    }

    void FixedUpdate () {
        // Touch
        
        float touchTargetAlpha = 0.1f;

        if (!actionEnabled) {
            leftTouchTarget = leftTouchDefaultPos;
            leftTouchTargetLPF = leftTouchDefaultPos;
        }
        leftTouchTargetLPF = (1 - touchTargetAlpha) * leftTouchTargetLPF + touchTargetAlpha * leftTouchTarget;
        //GameObject.Find("BothHandForward/Left").transform.position = leftTouchTargetLPF + new Vector3(0, -0.1f, 0);
        //GameObject.Find("BothHandForwardQuick/Left").transform.position = leftTouchTargetLPF + new Vector3(0, -0.1f, 0);
        //GameObject.Find("BothHandForwardDown/Left").transform.position = leftTouchTargetLPF + new Vector3(0, 00f, 0);
        //GameObject.Find("BothHandForwardDownQuick/Left").transform.position = leftTouchTargetLPF + new Vector3(0, 0.0f, 0);

        if (!actionEnabled) {
            rightTouchTarget = rightTouchDefaultPos;
            rightTouchTargetLPF = rightTouchDefaultPos;
        }
        rightTouchTargetLPF = (1 - touchTargetAlpha) * rightTouchTargetLPF + touchTargetAlpha * rightTouchTarget;
        //GameObject.Find("BothHandForward/Right").transform.position = rightTouchTargetLPF + new Vector3(0, -0.1f, 0);
        //GameObject.Find("BothHandForwardQuick/Right").transform.position = rightTouchTargetLPF + new Vector3(0, -0.1f, 0);
        //GameObject.Find("BothHandForwardDown/Right").transform.position = rightTouchTargetLPF + new Vector3(0, 0.0f, 0);
        //GameObject.Find("BothHandForwardDownQuick/Right").transform.position = rightTouchTargetLPF + new Vector3(0, 0.0f, 0);

        if (!actionEnabled) {
            leftOnlyTouchTarget = leftOnlyTouchDefaultPos;
        }
        //GameObject.Find("LeftHandForward/Left").transform.position = leftOnlyTouchTarget;
        //GameObject.Find("LeftHandForwardQuick/Left").transform.position = leftOnlyTouchTarget;
        
    }
    
    public void BothForward() {
        continueBothForward = true;
        /*
        invoker.EnqueueAction(new KeyframeInfo(0.0f, 0.0f, new List<string> { "Nod/HeadUp" }));

        if (currentBasePose == "BothForwardPalmUp") {
            invoker.EnqueueAction(new KeyframeInfo(0.0f, 0.0f, new List<string> { "Rest_HandHoldingToBothHandForward" }));
            invoker.EnqueueAction(new KeyframeInfo(0.0f, 0.0f, new List<string> { "Finger/RelaxOpen" }));
        }
        
        invoker.EnqueueAction(new KeyframeInfo(0.0f, 0.0f, new List<string> { "BothHandForward/Hips" }));
        if (bothForwardLeftEnable) {
            invoker.EnqueueAction(new KeyframeInfo(0.0f, 0.0f, new List<string> { "BothHandForward/Left" }));
            invoker.EnqueueAction(new KeyframeInfo(0.2f, 0.0f, new List<string> { "Finger/Relax/Left" }));
        } else {
            invoker.EnqueueAction(new KeyframeInfo(0.0f, 0.0f, new List<string> { "Rest_HandCloseHolding/Left" }));
            invoker.EnqueueAction(new KeyframeInfo(0.2f, 0.0f, new List<string> { "Finger/Close/Left" }));
        }
        if (bothForwardRightEnable) {
            invoker.EnqueueAction(new KeyframeInfo(0.0f, 0.0f, new List<string> { "BothHandForward/Right" }));
            invoker.EnqueueAction(new KeyframeInfo(0.2f, 0.0f, new List<string> { "Finger/Relax/Right" }));
        } else {
            invoker.EnqueueAction(new KeyframeInfo(0.0f, 0.0f, new List<string> { "Rest_HandCloseHolding/Right" }));
            invoker.EnqueueAction(new KeyframeInfo(0.2f, 0.0f, new List<string> { "Finger/Close/Right" }));
        }


        invoker.EnqueueAction(new KeyframeInfo(0.25f, 0.0f, new List<string> { }, ContinueBothForward));

        currentBasePose = "BothForward";
        currentMovement = "Stop";
        */
        Debug.LogWarning("BothForward");
        var upKeyPoses = teawaseKeyPoseUp.keyposes[0].boneKeyPoses;
        //var downKeyPoses = teawaseKeyPoseDown.keyposes[0].boneKeyPoses;
        Pose hipsPose = new Pose();
        Quaternion leftHandRot = Quaternion.identity;
        Quaternion rightHandRot = Quaternion.identity;
        foreach (var boneKeyPose in upKeyPoses) {
            if (boneKeyPose.boneId == HumanBodyBones.Hips) { hipsPose.position = boneKeyPose.position; hipsPose.rotation = boneKeyPose.rotation; }
            if(boneKeyPose.boneId == HumanBodyBones.LeftHand) { leftHandRot = boneKeyPose.rotation; }
            if(boneKeyPose.boneId == HumanBodyBones.RightHand) { rightHandRot = boneKeyPose.rotation; }
        }
        body[HumanBodyBones.Hips].controller.AddSubMovement(hipsPose, new Vector2(0.5f, 0.3f), 0.8f, 0.8f);
        if (bothForwardLeftEnable) {
            body[HumanBodyBones.LeftHand].controller.AddSubMovement(new Pose(leftTouchTargetLPF, leftHandRot), new Vector2(0.5f, 0.3f), 1.5f, 1.5f);
        }
        if (bothForwardRightEnable) {
            body[HumanBodyBones.RightHand].controller.AddSubMovement(new Pose(rightTouchTargetLPF, rightHandRot), new Vector2(0.5f, 0.3f), 1.5f, 1.5f);
        }
    }

    string leftUpDown = "Up", rightUpDown = "Up";
    public void ContinueBothForward() {
        Debug.Log("ContinueBothForward");
        /*
        if (continueBothForward) {
            if (bothForwardLeftEnable) {
                if (leftTouchTargetLPF.y < 0.1f) {
                    invoker.EnqueueAction(new KeyframeInfo(0.0f, 0.0f, new List<string> { "BothHandForwardDownQuick/Left" }));
                    leftUpDown = "Down";
                } else if (leftTouchTargetLPF.y > 0.15f) {
                    invoker.EnqueueAction(new KeyframeInfo(0.0f, 0.0f, new List<string> { "BothHandForwardQuick/Left" }));
                    leftUpDown = "Up";
                } else {
                    if (leftUpDown == "Up") {
                        invoker.EnqueueAction(new KeyframeInfo(0.0f, 0.0f, new List<string> { "BothHandForwardQuick/Left" }));
                    } else {
                        invoker.EnqueueAction(new KeyframeInfo(0.0f, 0.0f, new List<string> { "BothHandForwardDownQuick/Left" }));
                    }
                }
            } else {
                invoker.EnqueueAction(new KeyframeInfo(0.0f, 0.0f, new List<string> { "Rest_HandCloseHolding/Left" }));
            }

            if (bothForwardRightEnable) {
                if (rightTouchTargetLPF.y < 0.1f) {
                    invoker.EnqueueAction(new KeyframeInfo(0.0f, 0.0f, new List<string> { "BothHandForwardDownQuick/Right" }));
                    rightUpDown = "Down";
                } else if (rightTouchTargetLPF.y > 0.15f) {
                    invoker.EnqueueAction(new KeyframeInfo(0.0f, 0.0f, new List<string> { "BothHandForwardQuick/Right" }));
                    rightUpDown = "Up";
                } else {
                    if (rightUpDown == "Up") {
                        invoker.EnqueueAction(new KeyframeInfo(0.0f, 0.0f, new List<string> { "BothHandForwardQuick/Right" }));
                    } else {
                        invoker.EnqueueAction(new KeyframeInfo(0.0f, 0.0f, new List<string> { "BothHandForwardDownQuick/Right" }));
                    }
                }
            } else {
                invoker.EnqueueAction(new KeyframeInfo(0.0f, 0.0f, new List<string> { "Rest_HandCloseHolding/Right" }));
            }

            if (bothForwardLeftEnable) {
                if (leftTouchTargetLPF.z > mirrorZ - 0.01f) {
                    invoker.EnqueueAction(new KeyframeInfo(0.0f, 0.0f, new List<string> { "Finger/OpenWide/Left" }));
                } else {
                    invoker.EnqueueAction(new KeyframeInfo(0.0f, 0.0f, new List<string> { "Finger/RelaxOpen/Left" }));
                }
            } else {
                invoker.EnqueueAction(new KeyframeInfo(0.0f, 0.0f, new List<string> { "Finger/Close/Left" }));
            }

            if (bothForwardRightEnable) {
                if (rightTouchTargetLPF.z > mirrorZ - 0.01f) {
                    invoker.EnqueueAction(new KeyframeInfo(0.0f, 0.0f, new List<string> { "Finger/OpenWide/Right" }));
                } else {
                    invoker.EnqueueAction(new KeyframeInfo(0.0f, 0.0f, new List<string> { "Finger/RelaxOpen/Right" }));
                }
            } else {
                invoker.EnqueueAction(new KeyframeInfo(0.0f, 0.0f, new List<string> { "Finger/Close/Right" }));
            }

            invoker.EnqueueAction(new KeyframeInfo(0.0f, 0.0f, new List<string> { "BothHandForward/Hips" }));

            invoker.EnqueueAction(new KeyframeInfo(0.25f, 0.0f, new List<string> { }, ContinueBothForward));
        }
        */
        var upKeyPoses = teawaseKeyPoseUp.keyposes[0].boneKeyPoses;
        //var downKeyPoses = teawaseKeyPoseDown.keyposes[0].boneKeyPoses;
        Pose hipsPose = new Pose();
        Quaternion leftHandRot = Quaternion.identity;
        Quaternion rightHandRot = Quaternion.identity;
        foreach (var boneKeyPose in upKeyPoses) {
            if (boneKeyPose.boneId == HumanBodyBones.Hips) { hipsPose.position = boneKeyPose.position; hipsPose.rotation = boneKeyPose.rotation; }
            if (boneKeyPose.boneId == HumanBodyBones.LeftHand) { leftHandRot = boneKeyPose.rotation; }
            if (boneKeyPose.boneId == HumanBodyBones.RightHand) { rightHandRot = boneKeyPose.rotation; }
        }
        body[HumanBodyBones.Hips].controller.AddSubMovement(hipsPose, new Vector2(0.5f, 0.3f), 0.8f, 0.8f);
        if (bothForwardLeftEnable) {
            body[HumanBodyBones.LeftHand].controller.AddSubMovement(new Pose(leftTouchTargetLPF, leftHandRot), new Vector2(1.0f, 1.0f), 0.5f, 0.5f);
        }
        if (bothForwardRightEnable) {
            body[HumanBodyBones.RightHand].controller.AddSubMovement(new Pose(rightTouchTargetLPF, rightHandRot), new Vector2(1.0f, 1.0f), 0.5f, 0.5f);
        }
    }
    /*
    public void BeginAction() {
    }
    /*/
    public async Task BeginAction() {
        body = GameObject.FindObjectOfType<Body>();
        BothForward();
        
        while (continueBothForward) { 
            await Task.Delay(250);
            ContinueBothForward();
        }
        actionEnabled = false;
    }
    
    public void UpdateAction() {

    }

    public void EndAction() {
        continueBothForward = false;
    }
}
