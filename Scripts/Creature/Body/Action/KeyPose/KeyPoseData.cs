using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using SprCs;
using SprUnity;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VGent{

    [Serializable]
    public class ActionTarget {
        public HumanBodyBones boneId = HumanBodyBones.Hips;
        public string boneIdString = "";
        public Vector3 localPosition = new Vector3();
        public Quaternion localRotation = new Quaternion();
        public bool usePosition = true;
        public bool useRotation = true;
        public Vector3 position {
            get { return localPosition; }
            set { localPosition = value; }
        }
        public Quaternion rotation {
            get { return localRotation; }
            set { localRotation = value; }
        }

        public void Enable(bool e) {
            usePosition = useRotation = e;
        }
        public bool Enabled() {
            return usePosition || useRotation;
        }
    }

    [Serializable]
    public class StaticBoneKeyPose : ActionTarget{
        //public HumanBodyBones boneId = HumanBodyBones.Hips;
        //public string boneIdString = "";
        public enum CoordinateMode {
            World, // World
            BoneLocal, // Local coordinate (Bone GameObject)
            BodyLocal, // Local coordinate (Body GameObject)
        };
        public CoordinateMode coordinateMode = CoordinateMode.BodyLocal;
        // World Info 通常プロパティは最初の文字が大文字にするものだが分かりやすさのために小文字にする
        public Vector3 position {
            set {
                if (this.coordinateMode == CoordinateMode.BodyLocal) {
                    SetWorldToBodyLocal(value);
                } else if (this.coordinateMode == CoordinateMode.BoneLocal) {
                    SetWorldToBoneLocal(value);
                } else if (this.coordinateMode == CoordinateMode.World) {
                    localPosition = value;
                }
            }
            get {
                if (this.coordinateMode == CoordinateMode.BodyLocal) {
                    return GetPosBodyLocalToWorld();
                } else if (this.coordinateMode == CoordinateMode.BoneLocal) {
                    return GetPosBoneLocalToWorld();
                } else if (this.coordinateMode == CoordinateMode.World) {
                    return localPosition;
                }
                return new Vector3();
            }
        }

        public Quaternion rotation {
            set {
                if (this.coordinateMode == CoordinateMode.BodyLocal) {
                    SetWorldToBodyLocal(value);
                } else if (this.coordinateMode == CoordinateMode.BoneLocal) {
                    SetWorldToBoneLocal(value);
                } else if (this.coordinateMode == CoordinateMode.World) {
                    localRotation = value;
                }
            }
            get {
                if (this.coordinateMode == CoordinateMode.BodyLocal) {
                    return GetRotBodyLocalToWorld();
                } else if (this.coordinateMode == CoordinateMode.BoneLocal) {
                    return GetRotBoneLocalToWorld();
                } else if (this.coordinateMode == CoordinateMode.World) {
                    return localRotation;
                }
                return new Quaternion();
            }
        }

        // Local Info
        public Body body;
        public HumanBodyBones coordinateParent;
        //public Vector3 localPosition = new Vector3();
        public Vector3 normalizedLocalPosition = new Vector3();
        //public Quaternion localRotation = Quaternion.identity;
        // Control Flags
        //public bool usePosition = true;
        //public bool useRotation = true;
        // 
        public float lookAtRatio = 0;
        // 
        public Vector2 boneKeyPoseTiming = new Vector2(0.0f, 1.0f);
        public float startTime {
            get { return boneKeyPoseTiming.x; }
            set { boneKeyPoseTiming.x = value; }
        }
        public float endTime {
            get { return boneKeyPoseTiming.y; }
            set { boneKeyPoseTiming.y = value; }
        }

        public StaticBoneKeyPose Clone() {
            StaticBoneKeyPose k = new StaticBoneKeyPose();
            k.boneId = this.boneId;
            k.boneIdString = this.boneIdString;
            k.coordinateMode = this.coordinateMode;
            k.position = this.position;
            k.rotation = this.rotation;
            k.coordinateParent = this.coordinateParent;
            k.localPosition = this.localPosition;
            k.normalizedLocalPosition = this.normalizedLocalPosition;
            k.localRotation = this.localRotation;
            k.usePosition = this.usePosition;
            k.useRotation = this.useRotation;
            k.lookAtRatio = this.lookAtRatio;
            k.boneKeyPoseTiming = this.boneKeyPoseTiming;
            return k;
        }

        // 頭がGetやSetの関数はpositionとrotationのGetterとSetterのための関数
        public Vector3 GetPosBoneLocalToWorld() {
            if (body == null) { body = GameObject.FindObjectOfType<Body>(); }
            if (body != null) {
                Bone coordinateBaseBone = body[coordinateParent];
                if (coordinateBaseBone.ikActuator?.phIKActuator != null) {
                    //coordinateBaseBone.ikEndEffector.phIKEndEffector.GetTargetPosition;
                    //coordinateBaseBone.ikEndEffector.phIKEndEffector.GetTargetQuaternion;
                    Posed ikSolidPose = coordinateBaseBone.ikActuator.phIKActuator.GetSolidTempPose();
                    return ikSolidPose.Pos().ToVector3() + ikSolidPose.Ori().ToQuaternion() * (normalizedLocalPosition * body.height);
                } else {
                    return coordinateBaseBone.transform.position + coordinateBaseBone.transform.rotation * (normalizedLocalPosition * body.height);
                }
            }
            return new Vector3();
        }
        public Quaternion GetRotBoneLocalToWorld() {
            if (body == null) { body = GameObject.FindObjectOfType<Body>(); }
            if (body != null) {
                Bone coordinateBaseBone = body[coordinateParent];
                if (coordinateBaseBone.ikActuator?.phIKActuator != null) {
                    Posed ikSolidPose = coordinateBaseBone.ikActuator.phIKActuator.GetSolidTempPose();
                    return ikSolidPose.Ori().ToQuaternion() * localRotation;
                } else {
                    return coordinateBaseBone.transform.rotation * localRotation;
                }
            }
            return new Quaternion();
        }
        public Vector3 GetPosBodyLocalToWorld() {
            if (body == null) { body = GameObject.FindObjectOfType<Body>(); }
            if (body != null) {
                return body.transform.position + body.transform.rotation * (normalizedLocalPosition * body.height);
            }
            return new Vector3();
        }
        public Quaternion GetRotBodyLocalToWorld() {
            if (body == null) { body = GameObject.FindObjectOfType<Body>(); }
            if (body != null) {
                return body.transform.rotation * localRotation;
            }
            return new Quaternion();
        }
        public void SetWorldToBodyLocal(Vector3 value) {
            if (body == null) { body = GameObject.FindObjectOfType<Body>(); }
            if (body != null) {
                localPosition = Quaternion.Inverse(body.transform.rotation) * (value - body.transform.position);
                normalizedLocalPosition = localPosition / body.height;
            }
        }
        public void SetWorldToBodyLocal(Quaternion value) {
            if (body == null) { body = GameObject.FindObjectOfType<Body>(); }
            if (body != null) {
                localRotation = Quaternion.Inverse(body.transform.rotation) * value;
            }
        }
        public void SetWorldToBoneLocal(Vector3 value) {
            if (body == null) { body = GameObject.FindObjectOfType<Body>(); }
            if (body != null) {
                Bone coordinateBaseBone = body[coordinateParent];
                if (coordinateBaseBone.ikActuator?.phIKActuator != null) {
                    Posed ikSolidPose = coordinateBaseBone.ikActuator.phIKActuator.GetSolidTempPose();
                    localPosition = Quaternion.Inverse(ikSolidPose.Ori().ToQuaternion()) * (value - ikSolidPose.Pos().ToVector3());
                    normalizedLocalPosition = localPosition / body.height;
                } else {
                    localPosition = Quaternion.Inverse(coordinateBaseBone.transform.rotation) * (value - coordinateBaseBone.transform.position);
                    normalizedLocalPosition = localPosition / body.height;
                }
            }
        }
        public void SetWorldToBoneLocal(Quaternion value) {
            if (body == null) { body = GameObject.FindObjectOfType<Body>(); }
            if (body != null) {
                Bone coordinateBaseBone = body[coordinateParent];
                if (coordinateBaseBone.ikActuator?.phIKActuator != null) {
                    Posed ikSolidPose = coordinateBaseBone.ikActuator.phIKActuator.GetSolidTempPose();
                    localRotation = Quaternion.Inverse(ikSolidPose.Ori().ToQuaternion()) * value;
                } else {
                    localRotation = Quaternion.Inverse(coordinateBaseBone.transform.rotation) * value;
                }
            }
        }

        // 頭にConvertがついているメソッドはcoordinateParentが変更されたときの変換
        public void ConvertBoneLocalToOtherBoneLocal(HumanBodyBones from, HumanBodyBones to) {
            if (body == null) { body = GameObject.FindObjectOfType<Body>(); }
            if (body != null) {
                ConvertBoneLocalToWorld();
                coordinateParent = to;
                // coordinateParentを変えた影響をlocalPositionやlocalRotationに
                ConvertWorldToBoneLocal();
            }
        }
        public void ConvertBoneLocalToWorld() {
            if (body == null) { body = GameObject.FindObjectOfType<Body>(); }
            if (body != null) {
                Bone coordinateBaseBone = body[coordinateParent];
                if (coordinateBaseBone.ikActuator?.phIKActuator != null) {
                    Posed ikSolidPose = coordinateBaseBone.ikActuator.phIKActuator.GetSolidTempPose();
                    localPosition = ikSolidPose.Pos().ToVector3() + ikSolidPose.Ori().ToQuaternion() * (normalizedLocalPosition * body.height);
                    localRotation = ikSolidPose.Ori().ToQuaternion() * localRotation;
                } else {
                    localPosition = coordinateBaseBone.transform.position + coordinateBaseBone.transform.rotation * (normalizedLocalPosition * body.height);
                    localRotation = coordinateBaseBone.transform.rotation * localRotation;
                }
            }
        }
        // coordinateParentは正しいがlocalPositionやlocalRotationがWorldの時の変換
        public void ConvertWorldToBoneLocal() {
            if (body == null) { body = GameObject.FindObjectOfType<Body>(); }
            if (body != null) {
                Bone coordinateBaseBone = body[coordinateParent];
                if (coordinateBaseBone.ikActuator?.phIKActuator != null) {
                    Posed ikSolidPose = coordinateBaseBone.ikActuator.phIKActuator.GetSolidTempPose();
                    // Worldのpositionを求めたいので右辺はlocalPosition
                    localPosition = Quaternion.Inverse(ikSolidPose.Ori().ToQuaternion()) * (localPosition - ikSolidPose.Pos().ToVector3());
                    normalizedLocalPosition = localPosition / body.height;
                    localRotation = Quaternion.Inverse(ikSolidPose.Ori().ToQuaternion()) * localRotation;
                } else {
                    localPosition = Quaternion.Inverse(coordinateBaseBone.transform.rotation) * (localPosition - coordinateBaseBone.transform.position);
                    normalizedLocalPosition = localPosition / body.height;
                    localRotation = Quaternion.Inverse(coordinateBaseBone.transform.rotation) * localRotation;
                }
            }
        }
        public void ConvertWorldToBodyLocal() {
            if (body == null) { body = GameObject.FindObjectOfType<Body>(); }
            if (body != null) {
                localPosition = Quaternion.Inverse(body.transform.rotation) * (localPosition - body.transform.position);
                normalizedLocalPosition = localPosition / body.height;
                localRotation = Quaternion.Inverse(body.transform.rotation) * localRotation;
            }
        }
        public void ConvertBodyLocalToWorld() {
            if (body == null) { body = GameObject.FindObjectOfType<Body>(); }
            if (body != null) {
                localPosition = body.transform.position + body.transform.rotation * (normalizedLocalPosition * body.height);
                localRotation = body.transform.rotation * localRotation;
            }
        }
        public void ConvertBodyLocalToBoneLocal() {
            if (body == null) { body = GameObject.FindObjectOfType<Body>(); }
            if (body != null) {
                ConvertBodyLocalToWorld();
                ConvertWorldToBoneLocal();
            }
        }
        public void ConvertBoneLocalToBodyLocal() {
            if (body == null) { body = GameObject.FindObjectOfType<Body>(); }
            if (body != null) {
                ConvertBoneLocalToWorld();
                ConvertWorldToBodyLocal();
            }
        }
    }

    // 実行時に使用されるKeyPose
    [Serializable]
    public class KeyPose {
        public List<ActionTarget> boneKeyPoses = new List<ActionTarget>();

        public float testDuration = 1.0f;
        public float testSpring = 1.0f;
        public float testDamper = 1.0f;

        public ActionTarget this[string key] {
            get {
                foreach (var boneKeyPose in boneKeyPoses) {
                    if (boneKeyPose.boneId.ToString() == key) {
                        return boneKeyPose;
                    }
                }
                return null;
            }
        }
        // <!!> Is it better ?
        public ActionTarget this[HumanBodyBones key] {
            get { return this[key.ToString()]; }
        }

        public List<BoneSubMovementPair> Action(Body body = null, float duration = -1, float startTime = -1, float spring = -1, float damper = -1, Quaternion? rotate = null) {
            if (!rotate.HasValue) { rotate = Quaternion.identity; }

            if (duration < 0) { duration = testDuration; }
            if (startTime < 0) { startTime = 0; }
            if (spring < 0) { spring = testSpring; }
            if (damper < 0) { damper = testDamper; }

            List<BoneSubMovementPair> logs = new List<BoneSubMovementPair>();
            if (body == null) { body = GameObject.FindObjectOfType<Body>(); }
            if (body != null) {
                foreach (var boneKeyPose in boneKeyPoses) {
                    if (boneKeyPose.usePosition || boneKeyPose.useRotation) {
                        Bone bone = (boneKeyPose.boneIdString != "") ? body[boneKeyPose.boneIdString] : body[boneKeyPose.boneId];
                        var pose = new Pose(boneKeyPose.position, boneKeyPose.rotation);
                        var springDamper = new Vector2(spring, damper);
                        var sub = bone.controller.AddSubMovementWithEratta(pose, springDamper, startTime + duration, duration, usePos: boneKeyPose.usePosition, useRot: boneKeyPose.useRotation);
                        //BoneSubMovementPair log = new BoneSubMovementPair(bone, sub.Clone());
                        //logs.Add(log);
                    }
                }
            }
            return logs;
        }
    }

}