using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerceptionScene : MonoBehaviour {

    public List<PerceptionObject> objects = new List<PerceptionObject>();

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

    private static PerceptionScene instance = null;
    public static PerceptionScene GetInstance() {
        if (instance == null) {
            instance = FindObjectOfType<PerceptionScene>();
        }
        return instance;
    }

    public static void Add(PerceptionObject perceptionObject) {
        var perceptionScene = PerceptionScene.GetInstance();
        if (perceptionScene != null) {
            perceptionScene.objects.Add(perceptionObject);
        }
    }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

    void Start() {
    }

    void FixedUpdate() {
    }

}
