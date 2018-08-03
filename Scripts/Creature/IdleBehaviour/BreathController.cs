using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SprCs;
using InteraWare;

public class BreathController : MonoBehaviour {
    public Body body;

    private float breath = 0;

    private float period = 4.0f;

    private float timer = 0;

    // Use this for initialization
    void Start () {
    }

    void FixedUpdate () {
        timer += Time.fixedDeltaTime;
        if (timer < period * 0.6) {
            breath = 0.98f * breath + 0.02f;
        } else if (timer < period) {
            breath = 0.98f * breath;
        } else {
            timer = 0.0f;
            breath = 0;
            var reachHead = body["Head"].ikEndEffector.iktarget.GetComponent<ReachController>();
            reachHead.AddSubMovement(new Pose(new Vector3(0, 0, 0.005f), Quaternion.Euler(14 * Mathf.Deg2Rad, 0, 0)), new Vector2(1, 1), period * 0.7f, period * 0.7f, true);
            reachHead.AddSubMovement(new Pose(new Vector3(0, 0, 0.000f), Quaternion.Euler(                 0, 0, 0)), new Vector2(1, 1), period * 1.0f, period * 0.3f, true);
        }

		/*
        {
            PHIKBallActuatorIf ika = ((PHIKBallActuatorBehaviour)(body["LeftShoulder"].ikActuator)).phIKBallActuator;
            ika.SetPullbackTarget(Quaterniond.Rot(new Vec3d(0, 0, 8 * breath * Mathf.Deg2Rad)));
        }

        {
            PHIKBallActuatorIf ika = ((PHIKBallActuatorBehaviour)(body["RightShoulder"].ikActuator)).phIKBallActuator;
            ika.SetPullbackTarget(Quaterniond.Rot(new Vec3d(0, 0, 8 * -breath * Mathf.Deg2Rad)));
        }
        */

        {
            // PHIKBallActuatorIf ika = body.chest2.GetComponent<PHIKBallActuatorBehaviour>().sprObject as PHIKBallActuatorIf;
            // ika.SetPullbackTarget(Quaterniond.Rot(new Vec3d(18 * breath * Mathf.Deg2Rad, 0, 0)));
        }
    }
}
