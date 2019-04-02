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

    }
    public void Load() {

    }
}

public class SceneLog {
    public ObjectStatesIf savedScene;
    List<BoneControllerState> boneControllerStates;
}

public class CharacterSceneLoggerBehaviour : MonoBehaviour {

    SceneLog savedScene;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
