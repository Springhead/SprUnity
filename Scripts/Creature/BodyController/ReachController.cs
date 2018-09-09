using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using InteraWare;

public class SubMovement {
    // Start Spr, Pos, Rot, Time
    public Vector2 s0 = new Vector2(1, 1);
    public Vector3 p0 = new Vector3();
    public Quaternion q0 = new Quaternion();
    public float t0 = 0;

    // Final Spr, Pos, Rot, Time
    public Vector2 s1 = new Vector2(1, 1);
    public Vector3 p1 = new Vector3();
    public Quaternion q1 = new Quaternion();
    public float t1 = 0;

    public void AddNoise() {
        p1 += (p1 - p0) * 0.02f * GaussianRandom.random();
        float rotNoise = 3.0f;
        q1 = Quaternion.Euler(rotNoise * GaussianRandom.random(), rotNoise * GaussianRandom.random(), rotNoise * GaussianRandom.random()) * q1;
        t1 += GaussianRandom.random() * 0.2f;
    }

    public void GetCurrentSpringDamper(float t, out Vector2 sp) {
        Vector2 deltaS = s1 - s0;

        if (t < t0) { sp = new Vector2(); return; }
        if (t1 < t) { sp = deltaS; return; }

        float s = (t - t0) / (t1 - t0);
        float r = 10 * Mathf.Pow(s, 3) - 15 * Mathf.Pow(s, 4) + 6 * Mathf.Pow(s, 5);

        sp = deltaS * r;
    }

    public void GetCurrentPose(float t, out Vector3 p, out Quaternion q) {
        Vector3 deltaP = p1 - p0;
        Quaternion deltaQ = q1 * Quaternion.Inverse(q0);

        if (t < t0) { p = new Vector3(); q = Quaternion.identity; return; }
        if (t1 < t) { p = deltaP; q = deltaQ; return; }

        float s = (t - t0) / (t1 - t0);
        float r = 10 * Mathf.Pow(s, 3) - 15 * Mathf.Pow(s, 4) + 6 * Mathf.Pow(s, 5);

        p = deltaP * r;
        q = Quaternion.Slerp(Quaternion.identity, deltaQ, r);
    }

    public float GetCurrentActiveness(float t) {
        float s = (t - t0) / (t1 - t0);
        float r = (30 * Mathf.Pow(s, 2) - 60 * Mathf.Pow(s, 3) + 30 * Mathf.Pow(s, 4)) / 1.875f;
        return r;
    }
}

// ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

public class ReachController : MonoBehaviour {
	public bool suspend = false;

    public PHIKEndEffectorBehaviour ikEndEffector;
    public Queue<SubMovement> trajectory = new Queue<SubMovement>();
	public Queue<SubMovement> subTrajectory = new Queue<SubMovement>();
    public float currTime = 0.0f;
    public bool local = false;
    public bool noise = false;

    public List<Bone> changeSpringDamperBones = new List<Bone>();

    public void Start() {
        trajectory.Clear();

		{
			SubMovement subMov = new SubMovement ();
			subMov.p1 = subMov.p0 = transform.position;
			subMov.q1 = subMov.q0 = transform.rotation;
			subMov.s1 = subMov.s0 = new Vector2 (1, 1);
			subMov.t0 = 0.0f;
			subMov.t1 = 0.0001f;

			trajectory.Enqueue (subMov);
		}

		{
			SubMovement subMov = new SubMovement ();
			subMov.p1 = subMov.p0 = new Vector3();
			subMov.q1 = subMov.q0 = Quaternion.identity;
			subMov.s1 = subMov.s0 = new Vector2 (1, 1);
			subMov.t0 = 0.0f;
			subMov.t1 = 0.0001f;

			subTrajectory.Enqueue (subMov);
		}
    }

	public SubMovement AddSubMovement(Pose pose, Vector2 spring, float completeTime, float duration, bool toSubTrajectory = false) {
        var subMov = new SubMovement();

        subMov.p0 = trajectory.Last().p1;
        subMov.q0 = trajectory.Last().q1;
        subMov.s0 = trajectory.Last().s1;

        subMov.p1 = pose.position;
        subMov.q1 = pose.rotation;
        subMov.s1 = spring;

        if (local) {
            subMov.p1 = gameObject.transform.TransformDirection(subMov.p1);
        }

        subMov.t0 += currTime + completeTime - duration;
        subMov.t1 += currTime + completeTime;

        if (noise) {
            float posdiff = (subMov.p1 - subMov.p0).magnitude;
            float rotdiff; Vector3 axis; (Quaternion.Inverse(subMov.q0) * subMov.q1).ToAngleAxis(out rotdiff, out axis);
            if (posdiff > 0.1f || rotdiff > 5.0f) {
                subMov.AddNoise();
            }
        }

		if (!toSubTrajectory) {
        	trajectory.Enqueue(subMov);
		} else {
			subTrajectory.Enqueue(subMov);
		}
        return subMov;
    }
	public SubMovement AddSubMovement(PosRot pose, Vector2 spring, float completeTime, float duration, bool toSubTrajectory = false) {
		return AddSubMovement(new Pose(pose.position, pose.rotation), spring, completeTime, duration, toSubTrajectory);
    }

    public void FixedUpdate() {
		if (suspend) {
			return;
		}

		// ----- ----- ----- ----- -----

        float dt = Time.fixedDeltaTime;

        while (trajectory.Count() >= 2 && trajectory.First().t1 < currTime) {
            trajectory.Dequeue();
        }
		while (subTrajectory.Count() >= 2 && subTrajectory.First().t1 < currTime) {
			subTrajectory.Dequeue();
		}

        // ----- ----- ----- ----- -----

        // <!!> IKEndEffectorをDisableするとpullbackが効いて手が浮いてきてしまうのでこのままでは使えない。
        /*
        if (ikEndEffector != null) {
            if (ikEndEffector.phIKEndEffector.IsEnabled()) {
                if (trajectory.Count() <= 1 && trajectory.Last().t1 < currTime) {
					Debug.Log (name + " : Disable IK");
                    ikEndEffector.phIKEndEffector.Enable(false);
                }
            } else {
                if (trajectory.Count() > 1 || (trajectory.Count() == 1 && currTime < trajectory.Last().t1)) {
					Debug.Log (name + " : Enable IK");
                    ikEndEffector.phIKEndEffector.Enable(true);
                }
            }
        }
        */

        // ----- ----- ----- ----- -----

        Pose pose = new Pose();
        pose.position = trajectory.First().p0;
        pose.rotation = trajectory.First().q0;
        foreach (var subMov in trajectory) {
            Vector3 p; Quaternion q;
            subMov.GetCurrentPose(currTime, out p, out q);
            pose.position = pose.position + p;
            pose.rotation = q * pose.rotation;
        }

        Vector2 spring = trajectory.First().s0;
        foreach (var subMov in trajectory) {
            Vector2 sp;
            subMov.GetCurrentSpringDamper(currTime, out sp);
            spring = spring + sp;
        }

        // if (currTime < trajectory.Last().t1 + 1.0f) {
        //     currTime += dt;
        // }

		// ----- ----- ----- ----- -----

		Pose subPose = new Pose();
		subPose.position = subTrajectory.First().p0;
		subPose.rotation = subTrajectory.First().q0;
		foreach (var subMov in subTrajectory) {
			Vector3 p; Quaternion q;
			subMov.GetCurrentPose(currTime, out p, out q);
			subPose.position = subPose.position + p;
			subPose.rotation = q * subPose.rotation;
		}
			
		// ----- ----- ----- ----- -----

		if (currTime < trajectory.Last().t1 + 1.0f || currTime < subTrajectory.Last().t1 + 1.0f) {
			currTime += dt;
		}

        // ----- ----- ----- ----- -----

        // Move GameObject
        if (ikEndEffector == null || ikEndEffector.phIKEndEffector.IsPositionControlEnabled()) {
			gameObject.transform.position = pose.position + subPose.position;
        }
        if (ikEndEffector == null || ikEndEffector.phIKEndEffector.IsOrientationControlEnabled()) {
			gameObject.transform.rotation = subPose.rotation * pose.rotation;
        }
            
        // Update Spring & Damper Value
        foreach (var bone in changeSpringDamperBones) {
            bone.SetSpringDamperInRatio(spring);
        }
    }
}
