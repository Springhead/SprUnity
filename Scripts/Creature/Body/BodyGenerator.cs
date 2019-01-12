using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SprUnity {

    public class BodyGenerator {

        public bool generateFingers = false;
        public bool generateUnifiedLeg = false;

        // ----- ----- ----- ----- -----

        public BodyGenerator() {
        }

        // ----- ----- ----- ----- -----

        public GameObject Generate() {
            Bone hips = GenerateBone(null, "Hips");
            Bone spine = GenerateBone(hips, "Spine");
            // ...
        }

        // ----- ----- ----- ----- -----

        private Bone GenerateBone(Bone parent, string label) {
            var gameObject = new GameObject();
        }

    }

}