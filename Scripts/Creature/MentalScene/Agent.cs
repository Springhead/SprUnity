using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SprUnity {

    public class AgentInfo : Person.Attribute {
        public Body body;
        public override void StartPerc(Person person) {
            body = GameObject.FindObjectOfType<Body>();
        }
    }
    public class Agent : Person {
        void OnDrawGizmos() {
        }
    }
}
