using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using UnityEngine;

public class Person : MonoBehaviour {
    public static List<Person> persons = new List<Person>();

    // ----- ----- ----- ----- -----

    public string id = "";

    // ----- ----- ----- ----- -----
    // Basic Setting
    public bool human = true;

    // ----- ----- ----- ----- -----
    // Common Sensor Result

    // -- Body
    public GameObject head = null;
    public GameObject leftHand = null;
    public GameObject rightHand = null;

    // ----- ----- ----- ----- -----
    // Attributes
	public class Attribute { public virtual void OnDrawGizmos(Person person) { } }
    public Dictionary<Type, Attribute> attributes = new Dictionary<Type, Attribute>();
    public Type GetAttr<Type>() where Type : Attribute, new() {
        if (attributes.ContainsKey(typeof(Type))) {
            return (attributes[typeof(Type)] as Type);
        } else {
            Type newObj = new Type();
            attributes[typeof(Type)] = newObj;
            return newObj;
        }
    }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

    private TextMesh debugTextMesh = null;
    void Start () {
        id = Guid.NewGuid().ToString("N").Substring(0, 10);
        persons.Add(this);
        if (head == null) { head = gameObject; }

        // <!!>
        if (human) {
            GameObject prefab = (GameObject)Resources.Load("Prefabs/PersonDebugInfo");
            var debugObj = Instantiate(prefab, new Vector3(), Quaternion.identity);
            debugObj.transform.parent = this.transform;
            debugObj.transform.localPosition = new Vector3();
            debugTextMesh = debugObj.transform.Find("Text").gameObject.GetComponent<TextMesh>();
        }
    }

    void Update() {
        // <!!>
        if (human) {
            var netAttr = GetAttr<Network.Attribute>();
            string debugText = "";
            debugText += "id : " + id + "\r\n";
            debugText += "helmet : " + netAttr.helmet + "\r\n";
            debugText += "vip : " + netAttr.vip + "\r\n";
            debugText += "appoint : " + netAttr.appointmentStatus + "\r\n";
            debugText += "name : " + netAttr.personName + "\r\n";
            if (debugTextMesh != null) {
                debugTextMesh.text = debugText;
            }
        }
    }

    void FixedUpdate () {
		// 位置追従
		if (head != null && head != gameObject) {
			gameObject.transform.position = head.transform.position;
			gameObject.transform.rotation = head.transform.rotation;
		}
    }

    void OnDrawGizmos() {
        if (head != null) { 
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

        foreach(var kv in attributes) {
            kv.Value.OnDrawGizmos(this);
        }
    }

    void OnDestroy() {
        Debug.Log("Remove Person");
        persons.Remove(this);
    }

}
