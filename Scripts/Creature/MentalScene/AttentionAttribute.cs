using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SprUnity;
namespace VGent {
    public class AttentionAttribute : MentalAttribute {
        public float attention = 0.0f;
        public float attentionByDistance = 0.0f;
        public float attentionByDistanceDecrease = 0.0f;

        public float lastDistance = 0.0f;

        public void OnDrawGizmos() {
            var personParts = mentalGroup.GetParts<PersonParts>();
            if (personParts?.Head != null) {
                Gizmos.color = Color.gray;
                Gizmos.DrawWireSphere(personParts.Head.Position(), 0.3f * 1.0f);
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(personParts.Head.Position(), 0.3f * attention);
            }
        }
    }
}
