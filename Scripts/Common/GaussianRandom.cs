using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SprUnity {

    public class GaussianRandom {

        public static float random(float mu = 0.0f, float sigma = 1 / 2.0f) { // デフォルトだと95%で 0±1.0 に収まる（2σ）
            float rand = 0.0f;
            while ((rand = Random.value) == 0.0f) ;
            float rand2 = Random.value;
            float normrand = Mathf.Sqrt(-2.0f * Mathf.Log(rand)) * Mathf.Cos(2.0f * Mathf.PI * rand2);
            normrand = normrand * sigma + mu;
            return normrand;
        }

    }

}
