using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SprUnity {
    public class Perception : MonoBehaviour {
        public static List<Person> persons = new List<Person>();
        public static Agent agent;
        // Use this for initialization
        void Start() {
            CreateAgent("Agent");
        }

        // Update is called once per frame
        void Update() {
        }

        void FixedUpdate() {
            foreach (var person in persons) {
                // 位置追従
                if (person.head != null && person.head != gameObject) {
                    gameObject.transform.position = person.head.transform.position;
                    gameObject.transform.rotation = person.head.transform.rotation;
                }

                // ビジュアライザの位置追従
                if (person.visualizeObject != null) {
                    person.visualizeObject.transform.localPosition = new Vector3(transform.position.x * 0.2f, transform.position.z * 0.2f - 0.3f, 2.0f);
                }
                person.UpdatePerc();
            }
        }
        // Agentの場合はBodyを経由して関節の位置などを持ってくるのでAgentはBodyInfoを持たない
        private Agent CreateAgent(string name) {
            var personObj = new GameObject(name);
            personObj.transform.parent = this.transform;
            var newAgent = personObj.AddComponent<Agent>();
            newAgent.GetAttr<AgentInfo>();
            agent = newAgent;
            return agent;
        }
        public GameObject CreatePerson<Type>(string name) where Type : Person.Attribute, new() {
            var personObj = new GameObject(name);
            personObj.transform.parent = this.transform;
            var person = personObj.AddComponent<Person>();
            person.GetAttr<BodyInfo>();
            person.GetAttr<Type>();
            persons.Add(person);
            return personObj;
        }
        public GameObject CreatePerson(string name) {
            var personObj = new GameObject(name);
            personObj.transform.parent = this.transform;
            var person = personObj.AddComponent<Person>();
            person.GetAttr<BodyInfo>();
            persons.Add(person);
            return personObj;
        }
    }
}
