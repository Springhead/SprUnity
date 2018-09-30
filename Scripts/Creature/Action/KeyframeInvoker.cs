using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using SprUnity;
using InteraWare;

[Serializable]
public class KeyframeInfo {
	public delegate void OnStartCallback();

    public Vector2 time = new Vector2(0, 0); // mean, stddev : follows gaussian random.
    public List<KeyFrame> keyframes;
	public OnStartCallback onStart;

    [HideInInspector]
    public float generatedTime = -1;
    public float GetTime() {
        if (generatedTime < 0) {
            generatedTime = Mathf.Max(0, GaussianRandom.random(time[0], time[1]));
        }
        return generatedTime;
    }

	public KeyframeInfo(float timeMu, float timeSigma, List<string> kfs, OnStartCallback onStart = null) {
        time = new Vector2(timeMu, timeSigma);

        keyframes = new List<KeyFrame>();
        foreach (var kf in kfs) {
            var obj = GameObject.Find(kf);
            if (obj != null) {
                {
                    var keyframe = obj.GetComponent<KeyFrame>();
                    if (keyframe != null) {
                        keyframes.Add(keyframe);
                    }
                }

                for (int i = 0; i < obj.transform.childCount; i++) {
                    var child = obj.transform.GetChild(i);
                    var keyframe = child.GetComponent<KeyFrame>();
                    if (keyframe != null) {
                        keyframes.Add(keyframe);
                    }
                }
            }
        }

		this.onStart = onStart;
    }
}

public class KeyframeInvoker : MonoBehaviour {

    public List<KeyframeInfo> keyframes = new List<KeyframeInfo>();

    // ----- ----- ----- ----- -----

    private float actionTimer = 0;

    // ----- ----- ----- ----- -----

    void Start() {
    }

    void FixedUpdate() {
        actionTimer += Time.fixedDeltaTime;

        List<KeyframeInfo> deleteList = new List<KeyframeInfo>();
        foreach (var info in keyframes) {
            if (info.GetTime() <= actionTimer) {
                foreach (var keyframe in info.keyframes) {
                    keyframe.Action();
                }
                deleteList.Add(info);
            }
        }

		foreach (var info in deleteList) {
			if (info.onStart != null) {
				info.onStart.Invoke ();
			}
		}
        foreach (var info in deleteList) {
            keyframes.Remove(info);
        }

        if (keyframes.Count == 0) {
            actionTimer = 0;
        }
    }

    public void EnqueueAction(KeyframeInfo info) {
        info.time[0] += actionTimer;
        keyframes.Add(info);
    }

}
