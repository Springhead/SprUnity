using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SprUnity;

public class SampleSequenceAction : ScriptableAction {

    public KeyPoseData inLow, inHigh, outLow, outHigh;
    KeyPoseTimePair raiseHand, waveIn, waveOut;

	// Use this for initialization
	void Start () {
        HumanBodyBones[] copyBones = new HumanBodyBones[] { HumanBodyBones.LeftHand, HumanBodyBones.RightHand };
        raiseHand.keyPose.ParserSpecifiedParts(inLow, copyBones);
        waveIn.keyPose.ParserSpecifiedParts(inLow, copyBones);
        waveOut.keyPose.ParserSpecifiedParts(outLow, copyBones);
        base.Start();
    }

    public override void GenerateMovement() {
        if (!raiseHand.isUsed) {

        }
        if (!waveIn.isUsed) {

        }
        if (!waveOut.isUsed) {

        }
    }
}
