using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SprCs;
using SprUnity;
using UnityEngine;

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
        p1 += (p1 - p0) * 0.08f * GaussianRandom.random();
        float rotNoise = 3.0f;
        q1 = Quaternion.Euler(rotNoise * GaussianRandom.random(), rotNoise * GaussianRandom.random(), rotNoise * GaussianRandom.random()) * q1;
        // t1 += GaussianRandom.random() * 0.2f;
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

    public void GetCurrentVelocity(float t, out Vector3 v) {
        Vector3 deltaP = p1 - p0;
        float s = (t - t0) / (t1 - t0);
        float r = 30 * Mathf.Pow(s, 2) - 60 * Mathf.Pow(s, 3) + 30 * Mathf.Pow(s, 4);
        v = deltaP * r;
    }
}

// ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

public class BoneController : MonoBehaviour {
    public bool suspend = false;

    public PHIKEndEffectorBehaviour ikEndEffector;

    public bool local = false;
    public bool noise = false;

    public bool controlPosition = true;
    public bool controlRotation = true;

    public List<Bone> changeSpringDamperBones = new List<Bone>();

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

    [HideInInspector]
    public Bone bone = null;

    [HideInInspector]
    public float currTime = 0.0f;

    [HideInInspector]
    public Queue<SubMovement> posTrajectory = new Queue<SubMovement>();
    public Queue<SubMovement> rotTrajectory = new Queue<SubMovement>();

    [HideInInspector]
    public Queue<SubMovement> subTrajectory = new Queue<SubMovement>();

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

    private bool initialized = false;

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

    public void Start() {
    }

    public void Initialize() {
        posTrajectory.Clear();
        rotTrajectory.Clear();

        {
            SubMovement subMov = new SubMovement();
            subMov.p1 = subMov.p0 = transform.position;
            subMov.s1 = subMov.s0 = new Vector2(1, 1);
            subMov.t0 = 0.0f;
            subMov.t1 = 0.0001f;

            posTrajectory.Enqueue(subMov);
        }

        {
            SubMovement subMov = new SubMovement();
            subMov.q1 = subMov.q0 = transform.rotation;
            subMov.s1 = subMov.s0 = new Vector2(1, 1);
            subMov.t0 = 0.0f;
            subMov.t1 = 0.0001f;

            rotTrajectory.Enqueue(subMov);
        }

        {
            SubMovement subMov = new SubMovement();
            subMov.p1 = subMov.p0 = new Vector3();
            subMov.q1 = subMov.q0 = Quaternion.identity;
            subMov.s1 = subMov.s0 = new Vector2(1, 1);
            subMov.t0 = 0.0f;
            subMov.t1 = 0.0001f;

            subTrajectory.Enqueue(subMov);
        }

        initialized = true;
    }

    public SubMovement AddSubMovement(Pose pose, Vector2 spring, float completeTime, float duration, bool toSubTrajectory = false, bool usePos = true, bool useRot = true) {
        var posSubMov = new SubMovement();
        var rotSubMov = new SubMovement();

        // Pos
        posSubMov.p0 = posTrajectory.Last().p1;
        posSubMov.s0 = posTrajectory.Last().s1;

        posSubMov.p1 = pose.position;
        posSubMov.s1 = spring;

        if (local) {
            posSubMov.p1 = gameObject.transform.TransformDirection(posSubMov.p1);
        }

        posSubMov.t0 += currTime + completeTime - duration;
        posSubMov.t1 += currTime + completeTime;

        if (noise) {
            float posdiff = (posSubMov.p1 - posSubMov.p0).magnitude;
            if (posdiff > 0.1f) {
                posSubMov.AddNoise();
            }
        }

        // Rot
        rotSubMov.q0 = rotTrajectory.Last().q1;
        rotSubMov.s0 = rotTrajectory.Last().s1;

        rotSubMov.q1 = pose.rotation;
        rotSubMov.s1 = spring;

        rotSubMov.t0 += currTime + completeTime - duration;
        rotSubMov.t1 += currTime + completeTime;

        if (noise) {
            float rotdiff; Vector3 axis; (Quaternion.Inverse(rotSubMov.q0) * rotSubMov.q1).ToAngleAxis(out rotdiff, out axis);
            if (rotdiff > 5.0f) {
                rotSubMov.AddNoise();
            }
        }

        // Enqueue
        if (!toSubTrajectory) {
            if (usePos) { posTrajectory.Enqueue(posSubMov); }
            if (useRot) { rotTrajectory.Enqueue(rotSubMov); }

            var subMov = posSubMov;
            subMov.q0 = rotSubMov.q0;
            subMov.q1 = rotSubMov.q1;
            return subMov;
        } else {
            var subMov = posSubMov;
            subMov.q0 = rotSubMov.q0;
            subMov.q1 = rotSubMov.q1;
            subTrajectory.Enqueue(subMov);
            return subMov;
        }
    }

    public void FixedUpdate() {
        if (!initialized || suspend) {
            return;
        }

        // ----- ----- ----- ----- -----

        float dt = Time.fixedDeltaTime;

        while (posTrajectory.Count() >= 2 && posTrajectory.First().t1 < currTime) {
            posTrajectory.Dequeue();
        }
        while (rotTrajectory.Count() >= 2 && rotTrajectory.First().t1 < currTime) {
            rotTrajectory.Dequeue();
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
        pose.position = posTrajectory.First().p0;
        foreach (var subMov in posTrajectory) {
            Vector3 p; Quaternion q;
            subMov.GetCurrentPose(currTime, out p, out q);
            pose.position = pose.position + p;
        }
        pose.rotation = rotTrajectory.First().q0;
        foreach (var subMov in rotTrajectory) {
            Vector3 p; Quaternion q;
            subMov.GetCurrentPose(currTime, out p, out q);
            pose.rotation = q * pose.rotation;
        }

        Vector2 posSpring = posTrajectory.First().s0;
        foreach (var subMov in posTrajectory) {
            Vector2 sp;
            subMov.GetCurrentSpringDamper(currTime, out sp);
            posSpring = posSpring + sp;
        }
        Vector2 rotSpring = rotTrajectory.First().s0;
        foreach (var subMov in rotTrajectory) {
            Vector2 sp;
            subMov.GetCurrentSpringDamper(currTime, out sp);
            rotSpring = rotSpring + sp;
        }
        Vector2 spring = (posSpring + rotSpring) * 0.5f;

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

        if (currTime < posTrajectory.Last().t1 + 1.0f ||
            currTime < rotTrajectory.Last().t1 + 1.0f ||
            currTime < subTrajectory.Last().t1 + 1.0f) {
            currTime += dt;
        }

        // ----- ----- ----- ----- -----

        // Move GameObject
        if (bone != null && bone.label == "Hips" && bone.body.bodyBalancer != null) {

            if (controlPosition) {
                var targPos = pose.position + subPose.position;
                bone.body.bodyBalancer.hipsHeight = targPos.y - bone.body.bodyBalancer.initialHipsHeight;
            }

        } else {

            if (controlPosition) {
                var targPos = pose.position + subPose.position;
                if (ikEndEffector == null) {
                    gameObject.transform.position = targPos;
                } else if (ikEndEffector.phIKEndEffector.IsPositionControlEnabled()) {
                    ikEndEffector.phIKEndEffector.SetTargetPosition(targPos.ToVec3d());
                    ikEndEffector.desc.targetPosition = targPos.ToVec3d();
                }
            }

        }

        if (controlRotation) {
            var targOri = subPose.rotation * pose.rotation;
            if (ikEndEffector == null) {
                gameObject.transform.rotation = targOri;
            } else if (ikEndEffector.phIKEndEffector.IsOrientationControlEnabled()) {
                ikEndEffector.phIKEndEffector.SetTargetOrientation(targOri.ToQuaterniond());
                ikEndEffector.desc.targetOrientation = targOri.ToQuaterniond();
            }
        }

        // Update Spring & Damper Value
        foreach (var bone in changeSpringDamperBones) {
            if (bone != null) {
                bone.springRatio = spring[0];
                bone.damperRatio = spring[1];
            }
        }
    }
}