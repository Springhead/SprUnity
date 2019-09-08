using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SprUnity;

using UnityEngine;

public class AttentionAttr : Person.Attribute {
    public float attention = 0.0f;
    public float attentionByDistance = 0.0f;
    public float attentionByDistanceDecrease = 0.0f;

    public float lastDistance = 0.0f;

    public override void OnDrawGizmos(Person person) {
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(person.transform.position, 0.3f * 1.0f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(person.transform.position, 0.3f * attention);
    }
}

public class Attention : MonoBehaviour {

    public Body body = null;
    public LookController lookController = null;
    public MentalGroup agent;
    public MentalScene mentalScene;

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // Parameters

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // Internal Variables

    // Gaze Transition Timer (unit: second)
    private float timeFromGazeTransition = 0.0f;
    [HideInInspector]
    public float timeUntilGazeTransition = 0.0f;
    private float nextGazeTransitionTime = 0.0f;

    [HideInInspector]
    public MentalGroup currentAttentionTarget = null;

    [HideInInspector]
    public bool attentionTargetChanged = false;
    [HideInInspector]
    public float attentionChangeAngle = 0.0f;

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // MonoBehaviour Methods

    void Start() {
        if (mentalScene == null) {
            mentalScene = FindObjectOfType<MentalScene>();
        }
        if (agent == null) {
            var mentalGroup = body.GetComponent<MentalGroup>();
            if (mentalGroup.GetParts<PersonParts>() != null) {
                agent = mentalGroup;
            }
        }
    }

    void FixedUpdate() {
        attentionTargetChanged = false;

        if (currentAttentionTarget == null) {
            foreach (var mentalGroup in mentalScene.mentalGroups) {
                if (mentalGroup != agent && mentalGroup.GetParts<PersonParts>() != null) {
                    currentAttentionTarget = mentalGroup;
                    break;
                }
            }
        }

        CompAttention();
        GazeTransition();
    }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // Public APIs

    public void OverrideGazeTarget(MentalGroup person, float attention = -1, bool forceStraight = false, float overrideStare = -1) {
        if (attention >= 0.0f) {
            person.GetAttribute<AttentionAttribute>().attention = attention;
        }
        ChangeGazeTarget(person, forceStraight, overrideStare);
    }

    public void OverrideGazeTransitionTime(float time) {
        timeFromGazeTransition = 0.0f;
        timeUntilGazeTransition = time;
        nextGazeTransitionTime = time;
    }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // Private Methods

    public bool noHumanAttention = false; // <!!> sorry, for layeredgazer demo (mitake)
    void CompAttention() {
        float maxPersonAttention = 0.0f;
        foreach (var mentalGroup in mentalScene.mentalGroups) {
            if (mentalGroup != agent && mentalGroup.GetParts<PersonParts>()?.Head != null) {
                var person = mentalGroup;
                var attentionInfo = person.GetAttribute<AttentionAttribute>();
                if (!person.gameObject.activeInHierarchy) { continue; }

                // 距離による注意
                var pos = person.GetParts<PersonParts>().Head.transform.position; pos.y = 0;
                float distance = pos.magnitude;
                float min = 2.0f, max = 4.0f; // [m]
                float baseAttention = 0;
                if (distance < 3.0f) {
                    attentionInfo.attentionByDistance = (1 - (Mathf.Clamp(distance, min, max) - min) / (max - min)) * (1.0f - baseAttention) + baseAttention;
                } else {
                    attentionInfo.attentionByDistance = 0;
                }

                // 注意量
                if (!noHumanAttention) {
                    attentionInfo.attention = Mathf.Max(attentionInfo.attentionByDistance);
                } else {
                    attentionInfo.attention = 0;
                }

                if (maxPersonAttention < attentionInfo.attention) {
                    maxPersonAttention = attentionInfo.attention;
                }
            }
        }

        foreach (var mentalGroup in mentalScene.mentalGroups) {
            if (mentalGroup != agent && mentalGroup.GetParts<PersonParts>() == null) {
                var person = mentalGroup;
                var attentionInfo = person.GetAttribute<AttentionAttribute>();
                if (!person.gameObject.activeInHierarchy) { continue; }

                // 背景オブジェクトには人の注意量に応じて変化する一律の注意量を与える
                attentionInfo.attention = Mathf.Clamp(1 - maxPersonAttention, 0.0f, 1.0f);
            }
        }
    }

    void GazeTransition() {
        // Gaze Transition Timer
        timeFromGazeTransition += Time.fixedDeltaTime;

        // ----- ----- ----- ----- -----

        if (lookController.inAction) {
            return; // 頭を動かしている間や直後は次の視線移動をしない 
        }

        // ----- ----- ----- ----- -----

        MentalGroup newAttentionTarget = null;
        Vector3 headPos = body["Head"].transform.position;
        Vector3 currDir;
        if (currentAttentionTarget != null) {
            currDir = (currentAttentionTarget.transform.position - headPos);
        } else {
            currDir = new Vector3(0, 0, 1);
        }

        // ----- ----- ----- ----- -----

        if (nextGazeTransitionTime < timeFromGazeTransition) {
            // Determine Gaze Target according to Attention Value
            List<MentalGroup> candidates = new List<MentalGroup>();
            List<float> probs = new List<float>();

            foreach (var mentalGroup in mentalScene.mentalGroups) {
                if (mentalGroup != agent && mentalGroup.GetParts<PersonParts>() != null) {
                    var person = mentalGroup;
                    var personParts = mentalGroup.GetParts<PersonParts>();
                    if (!person.gameObject.activeInHierarchy) { continue; }
                    if (person != currentAttentionTarget) {
                        // 頭がなければスキップする
                        if (personParts.Head == null) { continue; }

                        // 位置のおかしな対象はスキップする
                        if (personParts.Head.Position().z < 0.3f || personParts.Head.Position().y > 2.0f) { continue; }

                        // 注意量が小さすぎる対象はスキップする
                        if (person.GetAttribute<AttentionAttribute>().attention < 1e-5) { continue; }

                        // 現在の注視対象とのなす角を求める
                        Vector3 candDir = (personParts.Head.Position() - headPos);
                        float angleDistance = Vector3.Angle(currDir, candDir);

                        // 角度に従って遷移確率を求める（角度が小さいほど高確率で遷移する）
                        float prob = Mathf.Max(0, (1.0f * Mathf.Exp(-angleDistance / 10.0f)));

                        // 注視対象候補としてリストに追加
                        candidates.Add(person);
                        probs.Add(prob);
                    }
                }
            }

            // 正規化
            {
                float totalProb = probs.Sum();
                if (totalProb != 0) {
                    for (int i = 0; i < probs.Count; i++) { probs[i] /= totalProb; }
                } else {
                    totalProb = probs.Sum();
                    for (int i = 0; i < probs.Count; i++) { probs[i] = probs[i] / totalProb; }
                }
            }

            // TDAに基づいて遷移確率にバイアスをかける
            for (int i = 0; i < probs.Count; i++) {
                // 現在の注視対象よりTDAの大きな対象に（TDA差に比例して）遷移しやすくする
                float diffAttention = candidates[i].GetAttribute<AttentionAttribute>().attention - currentAttentionTarget.GetAttribute<AttentionAttribute>().attention;
                if (diffAttention > 0) {
                    probs[i] += diffAttention * 20;
                }
            }

            // 再度正規化
            {
                float totalProb = probs.Sum();
                if (totalProb != 0) {
                    for (int i = 0; i < probs.Count; i++) {
                        probs[i] /= totalProb;
                    }
                } else {
                    totalProb = probs.Sum();
                    for (int i = 0; i < probs.Count; i++) { probs[i] = probs[i] / totalProb; }
                }
            }

            // 選択
            float r = Random.value;
            float accumProb = 0.0f;
            for (int i = 0; i < probs.Count; i++) {
                accumProb += probs[i];
                if (r < accumProb) {
                    newAttentionTarget = candidates[i];
                    break;
                }
            }

        }

        // ----- ----- ----- ----- -----

        if (newAttentionTarget != null) {
            attentionTargetChanged = true;
            Vector3 newDir = (newAttentionTarget.transform.position - headPos);
            attentionChangeAngle = Vector3.Angle(currDir, newDir);
        }

        ChangeGazeTarget(newAttentionTarget);

    }

    void ChangeGazeTarget(MentalGroup newAttentionTarget, bool forceStraight = false, float overrideStare = -1) {
        if (newAttentionTarget != null) {
            // 目を動かす
            var attention = newAttentionTarget.GetAttribute<AttentionAttribute>().attention;
            var head = newAttentionTarget.GetParts<PersonParts>()?.Head?.gameObject;
            if (head != null) {
                lookController.target = head;
            } else {
                lookController.target = newAttentionTarget.gameObject;
            }
            lookController.speed = 0.3f;
            if (forceStraight || (newAttentionTarget.GetParts<PersonParts>() != null && lookController.straight == false)) {
                lookController.straight = true;
            } else {
                lookController.straight = false;
            }

            // 次に視線移動するまでの時間を決定する
            float x_ = LinearFunction(new Vector2(0, 100), new Vector2(1, 150), newAttentionTarget.GetAttribute<AttentionAttribute>().attention);
            float y_ = LinearFunction(new Vector2(0, 79), new Vector2(1, 32), newAttentionTarget.GetAttribute<AttentionAttribute>().attention);

            float b = y_;
            float a = -(y_ / x_);

            float x = 15, y = 0;
            for (int i = 0; i < 100; i++) { // 棄却法で指定分布に従う乱数を生成
                x = Random.value * x_;
                y = Random.value * y_;
                if (y < a * x + b) {
                    break;
                }
            }
            nextGazeTransitionTime = x / 30.0f + newAttentionTarget.GetAttribute<AttentionAttribute>().attention * 0.5f;
            timeFromGazeTransition = 0.0f;

            // 次の視線移動までの時間から直視度を決定する（チラ見は一瞬、長時間なら直視、ただし注意度が小さいときはチラ見しかしない）
            // とっても大事！
            lookController.stare = Mathf.Clamp(Mathf.Min(attention * 1.0f, nextGazeTransitionTime / 2.0f), 0.3f, 1.0f);

            if (overrideStare >= 0) {
                lookController.stare = overrideStare;
            }

            currentAttentionTarget = newAttentionTarget;
        }
    }

    float Clip(float min, float max, float value) {
        return Mathf.Min(Mathf.Max(min, value), max);
    }

    float LinearFunction(Vector2 p1, Vector2 p2, float x) {
        float a = (p2.y - p1.y) / (p2.x - p1.x);
        float b = p1.y - p1.x * a;
        return a * x + b;
    }
}

