using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;

public class Person : MonoBehaviour {
    public static List<Person> persons = new List<Person>();

    // ----- ----- ----- ----- -----
    // Basic Setting
    public bool human = true;

    public bool ignoredByAttention = false; // このフラグが有効な場合はAttentionの対象としては無視される

    public bool autoEliminate = false; // このPersonは自動的に消滅する
    public float autoEliminateTime = -1; // [秒]

	public DateTime createdTime;

    // ----- ----- ----- ----- -----
    // Common Sensor Result

    // -- Body
    public GameObject head = null;
    public GameObject leftHand = null;
    public GameObject rightHand = null;

    // -- Facial Expression
    // <TBD>

    // -- Gaze
    public Vector3 gaze = new Vector3(0, 0, -1); // in 頭ローカル座標系

    // -- Voice
    public float volume = 0.0f;
    public float pitch = 0.0f;

    // ----- ----- ----- ----- -----
    // Individual Sensor Information
    public class SensorInfo { public virtual void OnDrawGizmos(Person person) { } }
    public Dictionary<Type, SensorInfo> sensorInfo = new Dictionary<Type, SensorInfo>();
    public Type GetSensorInfo<Type> () where Type : SensorInfo {
        return (sensorInfo.ContainsKey(typeof(Type))) ? (sensorInfo[typeof(Type)] as Type) : null;
    }
    public Type AddSensorInfo<Type>() where Type : SensorInfo, new() {
        if (sensorInfo.ContainsKey(typeof(Type))) {
            return (sensorInfo[typeof(Type)] as Type);
        } else {
            Type newObj = new Type();
            sensorInfo[typeof(Type)] = newObj;
            return newObj;
        }
    }

    // ----- ----- ----- ----- -----
    // Perception (processed information from common sensor result)
    public class Perception { public virtual void OnDrawGizmos(Person person) { } }
    public Dictionary<Type, Perception> perception = new Dictionary<Type, Perception>();
    public Type GetPerception<Type>() where Type : Perception {
        return (perception.ContainsKey(typeof(Type))) ? (perception[typeof(Type)] as Type) : null;
    }
    public Type AddPerception<Type>() where Type : Perception, new() {
        if (perception.ContainsKey(typeof(Type))) {
            return (perception[typeof(Type)] as Type);
        } else {
            Type newObj = new Type();
            perception[typeof(Type)] = newObj;
            return newObj;
        }
    }

    // ----- ----- ----- ----- -----
    // State Value for Character AI
	public class StateValue { public virtual void OnDrawGizmos(Person person) { } }
    public Dictionary<Type, StateValue> stateValue = new Dictionary<Type, StateValue>();
    public Type GetStateValue<Type>() where Type : StateValue {
        return (stateValue.ContainsKey(typeof(Type))) ? (stateValue[typeof(Type)] as Type) : null;
    }
    public Type AddStateValue<Type>() where Type : StateValue, new() {
        if (stateValue.ContainsKey(typeof(Type))) {
            return (stateValue[typeof(Type)] as Type);
        } else {
            Type newObj = new Type();
            stateValue[typeof(Type)] = newObj;
            return newObj;
        }
    }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

    void Start () {
        persons.Add(this);
		createdTime = DateTime.Now;

        if (head == null) { head = gameObject; }
	}
	
	void FixedUpdate () {
		// 位置追従
		if (head != null && head != gameObject) {
			gameObject.transform.position = head.transform.position;
			gameObject.transform.rotation = head.transform.rotation;
		}

		if (head == null && !autoEliminate) {
			autoEliminate = true;
			autoEliminateTime = 0.5f;
		}

        // 自動消滅処理
        if (autoEliminate) {
            if (autoEliminateTime > 0) {
                autoEliminateTime -= Time.fixedDeltaTime;
            } else {
				Destroy (this.gameObject);
            }
        }
    }

    void OnDrawGizmos() {
        if (head != null) { 
            Gizmos.color = Color.green;
            Gizmos.DrawLine(head.transform.position, head.transform.position + gaze);

            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(head.transform.position, 0.1f);
            var footPos = head.transform.position; footPos.y = 0;
            Gizmos.DrawLine(head.transform.position, footPos);
        }
		if (leftHand != null) {
			Gizmos.color = Color.gray;
			Gizmos.DrawWireSphere(leftHand.transform.position, 0.1f);
		}
		if (rightHand != null) {
			Gizmos.color = Color.gray;
			Gizmos.DrawWireSphere(rightHand.transform.position, 0.1f);
		}

        foreach(var kv in sensorInfo) {
            kv.Value.OnDrawGizmos(this);
        }
        foreach (var kv in perception) {
            kv.Value.OnDrawGizmos(this);
        }
		foreach (var kv in stateValue) {
			kv.Value.OnDrawGizmos(this);
		}
    }

    void OnDestroy() {
        persons.Remove(this);
    }

}
