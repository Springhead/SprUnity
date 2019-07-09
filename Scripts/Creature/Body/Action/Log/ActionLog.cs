using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

namespace SprUnity {

    public class BoneSubMovementPair {
        public Bone bone;
        public SubMovement subMovement;
        public BoneSubMovementPair(Bone bone, SubMovement sub) {
            this.bone = bone;
            this.subMovement = sub;
        }
    }

    public class SubMovementLog {
        private string source;
        public SubMovement subMovement;
        public SubMovementLog() {
            this.source = "";
            this.subMovement = null;
        }
        public SubMovementLog(string s, SubMovement sub) {
            this.source = s;
            this.subMovement = sub;
        }
        public SubMovementLog Clone() {
            SubMovementLog clone = new SubMovementLog();
            clone.source = this.source;
            clone.subMovement = this.subMovement.Clone();
            return clone;
        }
    }

    [Serializable]
    public class BoneSubMovementStream {
        public Bone bone;
        public List<SubMovementLog> logSubMovements = new List<SubMovementLog>();
        public List<SubMovementLog> futureSubMovements = new List<SubMovementLog>();
        public List<float[]> logSubmovementSources = new List<float[]>();
        public List<float[]> futureSubMovementSources = new List<float[]>();
        public List<Vector3> calculatedTrajectory = new List<Vector3>();
        public List<Vector3> loggedTrajectory = new List<Vector3>();
        public BoneSubMovementStream(Bone bone) {
            this.bone = bone;
            logSubMovements = new List<SubMovementLog>();
            futureSubMovements = new List<SubMovementLog>();

        }
        public float GetFinishTime() {
            if (futureSubMovements != null) {
                if (futureSubMovements.Count > 0) {
                    return futureSubMovements.Last().subMovement.t1;
                }
            }
            if (logSubMovements != null) {
                if (logSubMovements.Count > 0) {
                    return logSubMovements.Last().subMovement.t1;
                }
            }
            return 0.0f;
        }
        public float GetOldestStartTime() {
            if (logSubMovements != null) {
                if (logSubMovements.Count > 0) {
                    return logSubMovements[0].subMovement.t0;
                }
            }
            return Mathf.Infinity;
        }
        public void AddLog(BoneSubMovementPair subMovement, string s) {
            if (subMovement.bone == this.bone) logSubMovements.Add(new SubMovementLog(s, subMovement.subMovement));
        }
        public void AddFuture(BoneSubMovementPair subMovement, string s) {
            if (subMovement.bone == this.bone) futureSubMovements.Add(new SubMovementLog(s, subMovement.subMovement));
        }
        public void ClearLog() {
            logSubMovements.Clear();
        }
        public void ClearFuture() {
            futureSubMovements.Clear();
        }
        public BoneSubMovementStream Clone() {
            BoneSubMovementStream clone = new BoneSubMovementStream(this.bone);
            foreach (var subMovement in this.logSubMovements) {
                clone.logSubMovements.Add(subMovement.Clone());
            }
            return clone;
        }

        public void Reflesh(int logLength, float oldestTime) {
            int over = logSubMovements.Count - logLength;
            if (over > 0) {
                logSubMovements.RemoveRange(0, over);
            }
            while (logSubMovements.Count > 0) {
                if (logSubMovements[0].subMovement.t0 < oldestTime) {
                    logSubMovements.RemoveAt(0);
                } else {
                    return;
                }
            }
        }
    }

    public class ActionLog {
        // 開始時のSceneLog
        public CharacterSceneLogger sceneLog;
        // 発行されたSubMovementとかその軌道とか
        public List<BoneSubMovementStream> subMovementLogs;
        public BoneSubMovementStream this[HumanBodyBones boneId] {
            get {
                foreach (var sub in subMovementLogs) {
                    if (sub.bone.label == boneId.ToString()) return sub;
                }
                return null;
            }
        }
        public ActionLog(Body body = null) {
            sceneLog = new CharacterSceneLogger(body);
            subMovementLogs = new List<BoneSubMovementStream>();
        }
        public float GetFinishTime() {
            float t = 0.0f;
            foreach (var log in subMovementLogs) {
                t = Mathf.Max(t, log.GetFinishTime());
            }
            return t;
        }
        public float GetOldestStartTime() {
            float t = Mathf.Infinity;
            foreach (var log in subMovementLogs) {
                t = Mathf.Min(t, log.GetOldestStartTime());
            }
            return t;
        }
        public void AddLog(BoneSubMovementPair boneSubMovement, string s) {
            foreach (var subMovementLog in subMovementLogs) {
                if (boneSubMovement.bone == subMovementLog.bone) {
                    subMovementLog.AddLog(boneSubMovement, s);
                    return;
                }
            }
            // なければ追加
            BoneSubMovementStream newLogs = new BoneSubMovementStream(boneSubMovement.bone);
            newLogs.AddLog(boneSubMovement, s);
            subMovementLogs.Add(newLogs);
        }
        public void AddFuture(BoneSubMovementPair boneSubMovement, string s) {
            foreach (var subMovementLog in subMovementLogs) {
                if (boneSubMovement.bone == subMovementLog.bone) {
                    subMovementLog.AddFuture(boneSubMovement, s);
                    return;
                }
            }
            // なければ追加
            BoneSubMovementStream newLogs = new BoneSubMovementStream(boneSubMovement.bone);
            newLogs.AddFuture(boneSubMovement, s);
            subMovementLogs.Add(newLogs);
        }
        public void AddFuture(BoneKeyPose boneKeyPose, string s, float startTime, float duration, float spring, float damper, Body body) {
            var bone = body[boneKeyPose.boneId];
            SubMovement boneSubMovement = new SubMovement();
            foreach (var subMovementLog in subMovementLogs) {
                if (subMovementLog.bone.label == boneKeyPose.boneId.ToString()) {
                    SubMovement last = subMovementLog.futureSubMovements.Count > 0 ? subMovementLog.futureSubMovements.Last().subMovement : (subMovementLog.logSubMovements.Count > 0 ? subMovementLog.logSubMovements.Last().subMovement : null);
                    if (last != null) {
                        boneSubMovement.p0 = last.p1;
                        boneSubMovement.q0 = last.q1;
                        boneSubMovement.s0 = last.s1;
                    } else {
                        boneSubMovement.p0 = bone.transform.position;
                        boneSubMovement.q0 = bone.transform.rotation;
                        boneSubMovement.s0 = new Vector2(bone.springRatio, bone.damperRatio);
                    }
                    boneSubMovement.t0 = startTime;

                    boneSubMovement.t1 = startTime + duration;
                    boneSubMovement.p1 = boneKeyPose.position;
                    boneSubMovement.q1 = boneKeyPose.rotation;
                    boneSubMovement.s1 = new Vector2(spring, damper);

                    AddFuture(new BoneSubMovementPair(body[boneKeyPose.boneId], boneSubMovement), s);
                    return;
                }
            }
            boneSubMovement.t0 = startTime;
            boneSubMovement.p0 = bone.transform.position;
            boneSubMovement.q0 = bone.transform.rotation;
            boneSubMovement.s0 = new Vector2(bone.springRatio, bone.damperRatio);

            boneSubMovement.t1 = startTime + duration;
            boneSubMovement.p1 = boneKeyPose.position;
            boneSubMovement.q1 = boneKeyPose.rotation;
            boneSubMovement.s1 = new Vector2(spring, damper);

            AddFuture(new BoneSubMovementPair(body[boneKeyPose.boneId], boneSubMovement), s);
            return;
        }
        public void ClearLog() {
            foreach (var subMovementLog in subMovementLogs) {
                subMovementLog.ClearLog();
            }
        }
        public void ClearFuture() {
            foreach (var subMovementLog in subMovementLogs) {
                subMovementLog.ClearFuture();
            }
        }
        public void ClearAll() {
            ClearLog();
            ClearFuture();
        }
        public ActionLog Clone() {
            ActionLog clone = new ActionLog();
            clone.sceneLog = this.sceneLog.Clone();
            clone.subMovementLogs = new List<BoneSubMovementStream>();
            foreach (var subMovementLog in this.subMovementLogs) {
                clone.subMovementLogs.Add(subMovementLog.Clone());
            }
            return clone;
        }
    }
}
