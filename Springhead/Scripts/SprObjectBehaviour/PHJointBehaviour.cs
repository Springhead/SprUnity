using UnityEngine;
using System.Collections;
using SprCs;
using SprUnity;

public abstract class PHJointBehaviour : SprSceneObjBehaviour {
    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // メンバ変数

    public GameObject socket = null;
    public GameObject plug = null;

    // 関節で接続された剛体同士の衝突を無効ににするかどうか
    public bool disableCollision = false;
    // 関節のPlugPoseを剛体・関節オブジェクトの初期位置に合わせて自動設定するか
    public bool autoSetSockPlugPose = true;

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // このBehaviourに対応するSpringheadオブジェクト

    public PHJointIf phJoint { get { return sprObject as PHJointIf; } }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // 派生クラスで実装するメソッド

    // -- 関節を作成する
    public abstract PHJointIf CreateJoint(PHSolidIf soSock, PHSolidIf soPlug);

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // SprBehaviourの派生クラスで実装するメソッド

    // -- Sprオブジェクトの構築を行う
    public override ObjectIf Build() {
        if (!socket) { socket = gameObject.transform.parent.GetComponentInParent<PHSolidBehaviour>().gameObject; }
        if (!plug)   { plug   = gameObject.GetComponentInParent<PHSolidBehaviour>().gameObject; }

        if (socket == null) { throw new ObjectNotFoundException("Socket object did not found for Joint", gameObject); }
        if (plug == null) { throw new ObjectNotFoundException("Plug object did not found for Joint", gameObject); }

        PHSolidIf soSock = socket.GetComponent<PHSolidBehaviour>().sprObject as PHSolidIf;
        PHSolidIf soPlug = plug.GetComponent<PHSolidBehaviour>().sprObject as PHSolidIf;

        PHJointIf jo = CreateJoint(soSock, soPlug);

        jo.SetName("jo:" + gameObject.name);

        if (autoSetSockPlugPose) {
            jo.SetSocketPose(soSock.GetPose().Inv() * gameObject.transform.ToPosed());
            jo.SetPlugPose(soPlug.GetPose().Inv() * gameObject.transform.ToPosed());
        }

        return jo;
    }

    // -- 全てのBuildが完了した後に行う処理を書く。オブジェクト同士をリンクするなど
    public override void Link() {
        if (disableCollision) {
            PHSolidIf soSock = socket.GetComponent<PHSolidBehaviour>().sprObject as PHSolidIf;
            PHSolidIf soPlug = plug.GetComponent<PHSolidBehaviour>().sprObject as PHSolidIf;
            phScene.SetContactMode(soSock, soPlug, PHSceneDesc.ContactMode.MODE_NONE);
        }
    }

}
