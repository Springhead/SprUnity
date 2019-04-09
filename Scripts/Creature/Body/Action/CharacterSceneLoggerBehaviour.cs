using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SprUnity;
using SprCs;

public class BoneControllerState {
    // State infos of BoneController class (for Save and Load Scene)
    public Bone bone;
    bool controlPosition;
    bool controlRotation;
    float currTime;
    Queue<SubMovement> posTrajectory = new Queue<SubMovement>();
    Queue<SubMovement> rotTrajectory = new Queue<SubMovement>();
    Queue<SubMovement> subTrajectory = new Queue<SubMovement>();
    public void Save() {
        var controller = bone.controller;
        this.controlPosition = controller.controlPosition;
        this.controlRotation = controller.controlRotation;
        this.currTime = controller.currTime;
        this.posTrajectory.Clear();
        foreach(var traj in controller.posTrajectory) {
            this.posTrajectory.Enqueue(traj.Clone());
        }
        this.rotTrajectory.Clear();
        foreach (var traj in controller.rotTrajectory) {
            this.rotTrajectory.Enqueue(traj.Clone());
        }
        this.subTrajectory.Clear();
        foreach (var traj in controller.subTrajectory) {
            this.subTrajectory.Enqueue(traj.Clone());
        }
    }
    public void Load() {
        var controller = bone.controller;
        controller.controlPosition = this.controlPosition;
        controller.controlRotation = this.controlRotation;
        controller.currTime = this.currTime;
        controller.posTrajectory.Clear();
        foreach (var traj in posTrajectory) {
            controller.posTrajectory.Enqueue(traj);
        }
        controller.rotTrajectory.Clear();
        foreach (var traj in rotTrajectory) {
            controller.rotTrajectory.Enqueue(traj);
        }
        controller.subTrajectory.Clear();
        foreach (var traj in subTrajectory) {
            controller.subTrajectory.Enqueue(traj);
        }
    }
}

public class SceneLog {
    public ObjectStatesIf savedScene;
    List<BoneControllerState> boneControllerStates;
    public SceneLog(Body body = null) {
        savedScene = ObjectStatesIf.Create();
        if (body == null) body = GameObject.FindObjectOfType<Body>();
        if (body == null) return;
        boneControllerStates = new List<BoneControllerState>();
        foreach(var bone in body.bones) {
            if(bone.controller != null) {
                BoneControllerState boneState = new BoneControllerState();
                boneState.bone = bone;
                boneControllerStates.Add(boneState);
            }
        }
    }

    public void Save(PHSceneIf phScene) {
        savedScene.SaveState(phScene);
        foreach(var boneState in boneControllerStates) {
            boneState.Save();
        }
    }

    public void Load(PHSceneIf phScene) {
        savedScene.LoadState(phScene);
        foreach(var boneState in boneControllerStates) {
            boneState.Load();
        }
    }
}

public class CharacterSceneLoggerBehaviour {

    public PHSceneBehaviour phSceneBehaviour;
    private SceneLog log;

	// Use this for initialization
	public void Start () {
        if (phSceneBehaviour == null)
            phSceneBehaviour = GameObject.FindObjectOfType<PHSceneBehaviour>();
	}
	
	public void SaveScene() {
        if (log == null) log = new SceneLog();
        log.Save(phSceneBehaviour.phScene);
    }

    public void LoadScene() {
        if (log == null) return;
        log.Load(phSceneBehaviour.phScene);
        phSceneBehaviour.phScene.GetIKEngine().ApplyExactState();
    }
}
