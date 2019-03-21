using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerceptionScene : MonoBehaviour {

    public List<PerceptionObject> objects = new List<PerceptionObject>();

    public List<PerceptionProcessor> processors = new List<PerceptionProcessor>();

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

    private static PerceptionScene instance = null;
    public static PerceptionScene GetInstance() {
        if (instance == null) {
            instance = FindObjectOfType<PerceptionScene>();
            if (instance == null) {
                Debug.LogError("PerceptionScene Not Found");
            }
        }
        return instance;
    }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

    void Start() {
    }

    void FixedUpdate() {
    }

}
