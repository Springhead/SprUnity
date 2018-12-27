using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

using UnityEngine;

public class MotionSaver : MonoBehaviour {
    public List<GameObject> objects = new List<GameObject>();

    private StreamWriter writer = null;

    // Use this for initialization
    void OnEnable () {
        string filename = DateTime.Now.ToString("yyyy_MMdd_hhmmss") + ".txt";
        FileInfo fileinfo = new FileInfo(Application.dataPath + "/../" + filename);
        writer = fileinfo.CreateText();

        foreach (var obj in objects) {
            writer.Write(obj.name + ", ");
        }
        writer.WriteLine();
    }

    void OnDisable() {
        if (writer != null) {
            writer.Close();
            writer = null;
        }
    }

    // Update is called once per frame
    void FixedUpdate () {
		if (enabled) {
            foreach (var obj in objects) {
                Vector3 pos = obj.transform.position;
                Quaternion rot = obj.transform.rotation;
                writer.Write(pos.x + "," + pos.y + "," + pos.z + ",");
                writer.Write(rot.x + "," + rot.y + "," + rot.z + "," + rot.w + ",");
            }
            writer.WriteLine();
        }
	}
}
