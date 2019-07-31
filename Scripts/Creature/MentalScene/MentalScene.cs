using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SprUnity;

namespace SprUnity {
    public class MentalScene : MonoBehaviour {
        public MentalGroup[] mentalGroups {
            get {
                return FindObjectsOfType<MentalGroup>();
            }
        }
        public MentalObject[] mentalObjects {
            get {
                return FindObjectsOfType<MentalObject>();
            }
        }
    }
}
