using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SprUnity;

public class PersonParts : MentalParts {
    public MentalObject Head;
    public MentalObject Neck;

    public MentalObject Chest;
    public MentalObject Spine;
    public MentalObject Hips;

    public MentalObject LeftShoulder;
    public MentalObject LeftUpperArm;
    public MentalObject LeftLowerArm;
    public MentalObject LeftHand;

    public MentalObject RightShoulder;
    public MentalObject RightUpperArm;
    public MentalObject RightLowerArm;
    public MentalObject RightHand;

    public MentalObject LeftUpperLeg;
    public MentalObject LeftLowerLeg;
    public MentalObject LeftFoot;

    public MentalObject RightUpperLeg;
    public MentalObject RightLowerLeg;
    public MentalObject RightFoot;

    public MentalObject LeftToes;
    public MentalObject RightToes;
    public MentalObject LeftEye;
    public MentalObject RightEye;
    public MentalObject Jaw;

    public MentalObject LeftThumbProximal;
    public MentalObject LeftThumbIntermediate;
    public MentalObject LeftThumbDistal;
    public MentalObject LeftIndexProximal;
    public MentalObject LeftIndexIntermediate;
    public MentalObject LeftIndexDistal;
    public MentalObject LeftMiddleProximal;
    public MentalObject LeftMiddleIntermediate;
    public MentalObject LeftMiddleDistal;
    public MentalObject LeftRingProximal;
    public MentalObject LeftRingIntermediate;
    public MentalObject LeftRingDistal;
    public MentalObject LeftLittleProximal;
    public MentalObject LeftLittleIntermediate;
    public MentalObject LeftLittleDistal;

    public MentalObject RightThumbProximal;
    public MentalObject RightThumbIntermediate;
    public MentalObject RightThumbDistal;
    public MentalObject RightIndexProximal;
    public MentalObject RightIndexIntermediate;
    public MentalObject RightIndexDistal;
    public MentalObject RightMiddleProximal;
    public MentalObject RightMiddleIntermediate;
    public MentalObject RightMiddleDistal;
    public MentalObject RightRingProximal;
    public MentalObject RightRingIntermediate;
    public MentalObject RightRingDistal;
    public MentalObject RightLittleProximal;
    public MentalObject RightLittleIntermediate;
    public MentalObject RightLittleDistal;

    public MentalObject this[string key] {
        get {
            foreach (var field in this.GetType().GetFields()) {
                if (field.FieldType == typeof(MentalObject)) {
                    if (key == field.Name) {
                        return (MentalObject)field.GetValue(this);
                    }
                }
            }
            return null;
        }
        set {
            foreach (var field in this.GetType().GetFields()) {
                if (field.FieldType == typeof(MentalObject)) {
                    if (key == field.Name) {
                        field.SetValue(this, value);
                    }
                }
            }
        }
    }

    public MentalObject this[HumanBodyBones key] {
        get {
            return this[key.ToString()];
        }
        set {
            this[key.ToString()] = value;
        }
    }

    void OnDrawGizmos() {
        if (Head != null) {
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(Head.transform.position, 0.1f);
            var footPos = Head.transform.position; footPos.y = 0;
            Gizmos.DrawLine(Head.transform.position, footPos);
        }
        if (LeftHand != null) {
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(LeftHand.transform.position, 0.1f);
        }
        if (RightHand != null) {
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(RightHand.transform.position, 0.1f);
        }
    }
}
