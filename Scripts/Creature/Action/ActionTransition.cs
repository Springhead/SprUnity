using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CreateAssetMenu(menuName = "Action/ Create ActionTransition Instance")]
public class ActionTransition : ScriptableObject {

    public ActionStateMachine stateMachine;

    [SerializeField]
    public ActionState toState;
    public ActionState fromState;

    [System.Serializable]
    public class Condition {
        // この値を保持しているTransitionインスタンスは取得できる？
        public ActionTransition parent;
        public enum ConditionType{
            time,
            flag,
        };
        public ConditionType type;
        public float transitTime;
        string flagName;
        public Condition() {
        }
        public bool IsSatisfied() {
            // 外部からフラグをオンオフできるようにする？
            if(type == ConditionType.time) return parent.fromState.timeFromEnter >= transitTime ? true : false;
            if(type == ConditionType.flag) {
                // 親アセットのStateMachineにflagNameをキーとする変数を探す
                if (parent.stateMachine) {
                    return parent.stateMachine.flags[flagName];
                }
            }
            return false;
        }
    }
    [SerializeField]
    List<Condition> conditions;

    public void Transit() {

    }

    public bool IsTransitable() {
        foreach(var condition in conditions) {
            if (!condition.IsSatisfied()) {
                return false;
            }
        }
        return true;
    }
}

#endif