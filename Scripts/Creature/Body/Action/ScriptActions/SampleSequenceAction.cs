using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SprUnity;

public class SampleSequenceAction : ScriptableAction {

    public KeyPose inLow, inHigh, outLow, outHigh;
    KeyPoseTimePair raiseHand, waveIn, waveOut;

	// Use this for initialization
	void Start () {
        raiseHand.keyPose = ScriptableAction.Instantiate<KeyPose>(inLow);
        waveIn.keyPose = ScriptableAction.Instantiate<KeyPose>(inLow);
        waveOut.keyPose = ScriptableAction.Instantiate<KeyPose>(outLow);
        for (int i = 0; i < raiseHand.keyPose.boneKeyPoses.Count; i++) {
            raiseHand.keyPose.boneKeyPoses[i].Enable(false);
        }
        for (int i = 0; i < waveIn.keyPose.boneKeyPoses.Count; i++) {
            waveIn.keyPose.boneKeyPoses[i].Enable(false);
        }
        for (int i = 0; i < waveOut.keyPose.boneKeyPoses.Count; i++) {
            waveOut.keyPose.boneKeyPoses[i].Enable(false);
        }
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
