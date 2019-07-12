using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SprUnity {
    // Russellの円環モデル
    public class EmotionInfo : Person.Attribute{
        private float pleasant;
        private float prePleasant;
        public float Pleasant {
            get {
                return pleasant;
            }
            set {
                prePleasant = pleasant;
                pleasant = Mathf.Clamp(value, -1, 1);
            }
        } 
        public float DerivativePleasant {
            get {
                return pleasant - prePleasant;
            }
        }
        private float activated;
        private float preActivated;
        //private float activatedTime; // timeを取っておくと二回呼び出されただけでダメになる
        //private float preActivatedTime;
        private float deltaTime;
        public float Activated {
            get {
                return activated;
            }
            set {
                preActivated = activated;
                deltaTime = Time.deltaTime;
                activated = Mathf.Clamp(value, -1, 1);
            }
        }
        public float DifferenceActivated {
            get {
                return activated - preActivated;
            }
        }
        public float DerivativeActivated {
            get {
                if (deltaTime != 0) {
                    return (activated - preActivated) / deltaTime;
                }
                return 0;
            }
        }
    }
}
