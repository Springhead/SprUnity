using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using SprCs;

namespace SprUnity {

    public class ActionManager : MonoBehaviour {

        public Body body = null;
        public List<ScriptableAction> actions = new List<ScriptableAction>();

        [HideInInspector]
        public ScriptableAction inAction = null;

        // ----- ----- ----- ----- -----

        private float time = 0.0f;
        private int index = 0;

        // ----- ----- ----- ----- -----

        public ScriptableAction this[string key] {
            get {
                foreach (var action in actions) { if (action.name == key) return action; }
                return null;
            }
        }

        void Start() {
        }

        void Update() {
            KeyCode[] hotKeys = { KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R, KeyCode.T, KeyCode.Y };

            for (int i = 0; i < hotKeys.Count(); i++) {
                if (Input.GetKeyDown(hotKeys[i])) {
                    if (actions.Count > i) {
                        time = 0.0f;
                        index = 0;
                        inAction = actions[i];
                        //inAction.BeginAction(body);
                    }
                }
            }
        }

        private void FixedUpdate() {
            if (body == null || body.initialized) {
                if (inAction != null && inAction.enabled) {
                    //inAction.UpdateStateMachine();
                    //inAction.UpdateAction();

                    time += Time.fixedDeltaTime;

                    if (!inAction.enabled) {
                        time = 0.0f;
                        index = 0;
                        inAction = null;
                    }
                }
            }
        }

        // ----- ----- ----- ----- -----

        public void Action(string name, GameObject[] target = null) {
            print("Action: " + name);
            foreach (var action in actions) {
                if (action.name == name) {
                    inAction = action;
                    time = 0.0f;
                    index = 0;
                    if (target.Length < 1) {
                        //inAction.BeginAction(body);
                    } else {
                        //inAction.BeginAction(body);
                    }
                }
            }
        }

    }

}