using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SprCs;
using InteraWare;

public class JitterController : MonoBehaviour {
    public Body body;

    private float period = 0.0f;
    private float timer = 0;
    private Vector2 lastYuragi = new Vector2();

    // Use this for initialization
    void Start () {
    }

    // Update is called once per frame
    void FixedUpdate () {
        timer += Time.fixedDeltaTime;
        if (period < timer) {
            timer = 0.0f;

            float variance = 0.01f;
            Vector2 yuragi = new Vector2(GaussianRandom(0, variance), GaussianRandom(0, variance));
            float distance = (yuragi - lastYuragi).magnitude;

            period = Mathf.Max(3.0f, GaussianRandom(distance / variance * 5.0f, 2.0f));

            body["Base"].GetComponent<ReachController>().AddSubMovement(new Pose(new Vector3(yuragi.x, 0, yuragi.y), Quaternion.identity), new Vector2(1, 1), period * 2.0f, period * 2.0f);

            lastYuragi = yuragi;
        }
    }

    // ----- ----- ----- ----- -----

    float GaussianRandom(float mu = 0.0f, float sigma = 0.5f) {
        float rand = 0.0f;
        while ((rand = Random.value) == 0.0f) ;
        float rand2 = Random.value;
        float normrand = Mathf.Sqrt(-2.0f * Mathf.Log(rand)) * Mathf.Cos(2.0f * Mathf.PI * rand2);
        normrand = normrand * sigma + mu;
        return normrand;
    }
}
