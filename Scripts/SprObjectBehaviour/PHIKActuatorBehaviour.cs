using UnityEngine;
using System.Collections;
using SprCs;
using System;

/*
  
    ＜SolidとJointとIKActuatorの取り得る関係の可能性一覧＞
      
    - solidObj1 <PHSolid>
        - jointObj1 <PHJoint, PHIKAct>    
    - solidObj2 <PHSolid>
        - jointObj2 <PHJoint, PHIKAct> sock:solidObj1 plug:solidObj2

    ***

    - solidObj1 <PHSolid, PHJoint, PHIKAct> 
    - solidObj2 <PHSolid, PHJoint, PHIKAct> sock:solidObj1 plug:solidObj2

    ***

    - solidObj1 <PHSolid, PHJoint, PHIKAct>
        - solidObj2 <PHSolid, PHJoint, PHIKAct> sock:solidObj1 plug:solidObj2

    ***

    - solidObj1 <PHSolid>
        - jointObj1 <PHJoint, PHIKAct>
        - solidObj2 <PHSolid>
            - jointObj2 <PHJoint, PHIKAct>

    ***

    - solidObj1 <PHSolid, PHJoint, PHIKAct>
        - solidObj2 <PHSolid>
            - jointObj2 <PHJoint, PHIKAct>

*/

public abstract class PHIKActuatorBehaviour : SprSceneObjBehaviour {
    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // このBehaviourに対応するSpringheadオブジェクト

    public PHIKActuatorIf phIKActuator { get { return sprObject as PHIKActuatorIf; } }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // SprBehaviourの派生クラスで実装するメソッド

    // -- 全てのBuildが完了した後に行う処理を書く。オブジェクト同士をリンクするなど
    public override void Link() {
        // まず、このIKActuatorに付随する関節を探す。同じGameObjectに付いているはず
        PHJointBehaviour jo = gameObject.GetComponent<PHJointBehaviour>();
        
        if (jo != null && jo.sprObject != null && sprObject != null) {
            // 次に、関節の親関節を探す。関節のソケット剛体を探し、それを基準に探す
            // （親関節　＝　この関節のソケット剛体をプラグ剛体として持つ関節）
            // 親関節がないかどんどん上に行って探す
            while (true) {
                PHJointBehaviour joParent = null;
                var jos = jo.socket.GetComponentsInChildren<PHJointBehaviour>(); //InChildrenで探す必要はないはず
                foreach (var j in jos) {
                    if (j.plug == jo.socket) { joParent = j; break; }
                }

                if (joParent != null && jo != joParent) {
                    // 親関節に付随するIKActuatorを親Actuatorとして登録する
                    PHIKActuatorBehaviour act = joParent.GetComponent<PHIKActuatorBehaviour>();
                    if (act != null && act.sprObject != null && sprObject != act.sprObject) {
                        act.sprObject.AddChildObject(sprObject);
                        break;
                    }
                    if(act == null) {
                        jo = joParent;
                    }
                } else {
                    break;
                }
            }
        }
    }
}
