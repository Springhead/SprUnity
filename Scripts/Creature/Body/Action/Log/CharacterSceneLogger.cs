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
    public BoneControllerState Clone() {
        BoneControllerState clone = new BoneControllerState();
        var controller = bone.controller;
        clone.controlPosition = this.controlPosition;
        clone.controlRotation = this.controlRotation;
        clone.currTime = this.currTime;
        clone.posTrajectory.Clear();
        foreach (var traj in posTrajectory) {
            clone.posTrajectory.Enqueue(traj);
        }
        clone.rotTrajectory.Clear();
        foreach (var traj in rotTrajectory) {
            clone.rotTrajectory.Enqueue(traj);
        }
        clone.subTrajectory.Clear();
        foreach (var traj in subTrajectory) {
            clone.subTrajectory.Enqueue(traj);
        }
        return clone;
    }
}

public class CharacterSceneLogger {

    public PHSceneBehaviour phSceneBehaviour;
    public ObjectStatesIf savedScene;

    List<BoneControllerState> boneControllerStates;

    public CharacterSceneLogger(Body body = null) {
        savedScene = ObjectStatesIf.Create();
        if (phSceneBehaviour == null) {
            phSceneBehaviour = GameObject.FindObjectOfType<PHSceneBehaviour>();
        }
        if (body == null) body = GameObject.FindObjectOfType<Body>();
        if (body == null) return;
        boneControllerStates = new List<BoneControllerState>();
        foreach (var bone in body.bones) {
            if (bone.controller != null) {
                BoneControllerState boneState = new BoneControllerState();
                boneState.bone = bone;
                boneControllerStates.Add(boneState);
            }
        }
    }

    public void Save() {
        if (phSceneBehaviour.phScene == null) return;
        savedScene.SaveState(phSceneBehaviour.phScene);
        foreach (var boneState in boneControllerStates) {
            boneState.Save();
        }
    }

    public void Load() {
        if (phSceneBehaviour.phScene == null) return;
        savedScene.LoadState(phSceneBehaviour.phScene);
        foreach (var boneState in boneControllerStates) {
            boneState.Load();
        }
        phSceneBehaviour.phScene.GetIKEngine().ApplyExactState();
    }

    public CharacterSceneLogger Clone() {
        CharacterSceneLogger clone = new CharacterSceneLogger();
        clone.phSceneBehaviour = this.phSceneBehaviour;
        clone.savedScene = this.savedScene;
        clone.boneControllerStates.Clear();
        foreach(var boneControllerState in boneControllerStates) {
            clone.boneControllerStates.Add(boneControllerState.Clone());
        }
        return clone;
    }
}
