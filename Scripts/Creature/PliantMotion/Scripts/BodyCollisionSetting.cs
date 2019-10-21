using System.Collections;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;

using SprCs;
using SprUnity;
using InteraWare;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(BodyCollisionSetting))]
public class collision_settingEditor : Editor {
    public override void OnInspectorGUI() {
        BodyCollisionSetting collision_Setting = (BodyCollisionSetting)target;
        DrawDefaultInspector();
        if (GUILayout.Button("Add Collision List")) {
            collision_Setting.AddCollionListSave();
        }
    }
}
#endif
public class BodyCollisionSetting : MonoBehaviour {
    //public bool saveOnDestroy = false;
    public bool addFirst = false;
    private PHSceneBehaviour phSceneBehaviour;
    private Body body;
    private PHSceneIf phScene;
    public string filename = "CollisionSetting.txt";
    // Use this for initialization
    void Start() {
        phSceneBehaviour = gameObject.GetComponentInParent<PHSceneBehaviour>();
        if (phSceneBehaviour == null) {
            phSceneBehaviour = FindObjectOfType<PHSceneBehaviour>();
            if (phSceneBehaviour == null) {
                throw new ObjectNotFoundException("PHSceneBehaviour was not found", gameObject);
            }
        }
        phScene = phSceneBehaviour.phScene;
        body = GetComponent<Body>();
        Load();
    }

    void OnDestroy() {
        //if (saveOnDestroy) {
        //    Save();
        //}
    }

    private void Update() {
        if (addFirst) {
            AddCollionListSave();
            addFirst = false;
        }
    }
    public void AddCollionListSave() {
        for (int i = 0; i < phScene.NContacts(); i++) {
            PHContactPointIf contact = phScene.GetContact(i);
            //Debug.Log(contact.GetPlugSolid().GetName() + " and " + contact.GetSocketSolid().GetName() + " is colliding!");
            PHSceneBehaviour.CollisionSetting newCollisionSetting = new PHSceneBehaviour.CollisionSetting();
            string solid1Name = contact.GetPlugSolid().GetName().Replace("so:", "");
            solid1Name = Regex.Replace(solid1Name, "_[0-9]*", "");
            string solid2Name = contact.GetSocketSolid().GetName().Replace("so:", "");
            solid2Name = Regex.Replace(solid2Name, "_[0-9]*", "");
            Debug.Log(solid1Name + " " + solid2Name);
            if (body[solid1Name] == null || body[solid2Name] == null) {
                continue;
            }
            newCollisionSetting.solid1 = body[solid1Name].solid;
            newCollisionSetting.solid2 = body[solid2Name].solid;
            newCollisionSetting.mode = PHSceneDesc.ContactMode.MODE_NONE;

            bool isExist = false;
            foreach (PHSceneBehaviour.CollisionSetting collisionSetting in phSceneBehaviour.collisionList) {
                if (collisionSetting.solid1 == newCollisionSetting.solid1 && collisionSetting.solid2 == newCollisionSetting.solid2 ||
                    collisionSetting.solid1 == newCollisionSetting.solid2 && collisionSetting.solid2 == newCollisionSetting.solid1) {
                    isExist = true;
                    break;
                }
            }
            if (!isExist) {
                phSceneBehaviour.collisionList.Add(newCollisionSetting);
            }
        }
        phSceneBehaviour.OnValidate();
        Save();
    }
    void Load() {
        FileInfo fileInfo = new FileInfo(Application.dataPath + "/../Settings/" + filename);
        string solid1 = "";
        GameObject solid1_gameobject = null;
        string solid2 = "";
        GameObject solid2_gameobject = null;
        List<PHSolidBehaviour> children = new List<PHSolidBehaviour>(GetComponentsInChildren<PHSolidBehaviour>());
        string mode = "";
        string line = "";
        if (fileInfo.Exists) {
            StreamReader reader = fileInfo.OpenText();

            while (reader.Peek() >= 0) {
                bool is_exist = false;
                line = reader.ReadLine();
                string[] lines = line.Split(' ');
                if (lines.Length < 3) {
                    Debug.LogError(filename + "ファイルおかしい");
                    return;
                }
                solid1 = lines[0];
                solid2 = lines[1];
                mode = lines[2];
                foreach (var child in children) {
                    if (child.name == solid1) {
                        solid1_gameobject = child.gameObject;
                    }
                }
                foreach (var child in children) {
                    if (child.name == solid2) {
                        solid2_gameobject = child.gameObject;
                    }
                }
                if (solid1_gameobject == null || solid2_gameobject == null || solid1_gameobject.GetComponent<PHSolidBehaviour>() == null || solid2_gameobject.GetComponent<PHSolidBehaviour>() == null) {
                    Debug.LogError(filename + "ファイルおかしい:" + solid1 + " " + solid2);
                    continue;
                }
                foreach (PHSceneDesc.ContactMode collisionsetting in Enum.GetValues(typeof(PHSceneDesc.ContactMode))) {
                    if (Enum.GetName(typeof(PHSceneDesc.ContactMode), collisionsetting) == mode) {
                        PHSceneBehaviour.CollisionSetting cs = new PHSceneBehaviour.CollisionSetting();
                        cs.solid1 = solid1_gameobject.GetComponent<PHSolidBehaviour>();
                        cs.solid2 = solid2_gameobject.GetComponent<PHSolidBehaviour>();
                        if (cs.solid1.name == cs.solid2.name) { //名前同じだと落ちる？
                            continue;
                        }
                        cs.mode = collisionsetting;
                        cs.targetSetMode1 = PHSceneBehaviour.CollisionSetting.CollisionTargetSettingMode.One; // こうっぽい？
                        cs.targetSetMode2 = PHSceneBehaviour.CollisionSetting.CollisionTargetSettingMode.One;
                        foreach (PHSceneBehaviour.CollisionSetting cs_one in phSceneBehaviour.collisionList) {
                            if (cs_one.solid1 == cs.solid1 && cs_one.solid2 == cs.solid2 ||
                                cs_one.solid1 == cs.solid2 && cs_one.solid2 == cs.solid1) {
                                is_exist = true;
                            }
                        }
                        if (!is_exist) {
                            phSceneBehaviour.collisionList.Add(cs);
                        }
                    }
                }
            }
            reader.Close();
        }
    }
    void Save() {
        FileInfo fileInfo = new FileInfo(Application.dataPath + "/../Settings/" + filename);
        StreamWriter writer = fileInfo.CreateText();
        foreach (PHSceneBehaviour.CollisionSetting collision in phSceneBehaviour.collisionList) {
            if (collision.solid1 != null && collision.solid2 != null) {
                bool solid1InBody = false;
                bool solid2InBody = false;
                foreach (var bone in body.bones) {
                    if (bone.solid == collision.solid1) {
                        solid1InBody = true;
                    }
                    if (bone.solid == collision.solid2) {
                        solid2InBody = true;
                    }
                }
                if (solid1InBody && solid2InBody) {
                    writer.WriteLine(collision.solid1.gameObject.name + " " + collision.solid2.gameObject.name + " " + collision.mode.ToString());
                }
            }
        }
        writer.Close();
    }
}
