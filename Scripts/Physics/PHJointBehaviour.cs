using UnityEngine;
using System.Collections;
using SprCs;
using SprUnity;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(PHJointBehaviour))]
public class PHJointBehaviourEditor : Editor {
    public void OnSceneGUI() {
        PHJointBehaviour phJointBehaviour = (PHJointBehaviour)target;

        // ----- ----- ----- ----- -----
        // Joint Pose Handle
        if (phJointBehaviour.showJointPoseHandle) {
            Tools.current = Tool.None;
            // <!!> この方式だと微小変化が常に発生してあまりUndoなどが機能しない
            Posed objectPose = phJointBehaviour.gameObject.transform.ToPosed();
            Vector3 currJointPos = (objectPose * phJointBehaviour.jointPosition.ToVec3d()).ToVector3();
            Quaternion currJointRot = phJointBehaviour.gameObject.transform.rotation * phJointBehaviour.jointOrientation;
            EditorGUI.BeginChangeCheck();
            Vector3 handlePos = Handles.PositionHandle(currJointPos, phJointBehaviour.gameObject.transform.rotation);
            Quaternion handleRot = Handles.RotationHandle(currJointRot, currJointPos);
            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(phJointBehaviour, "Joint Pos/Rot Change");
                phJointBehaviour.jointPosition = Quaternion.Inverse(phJointBehaviour.gameObject.transform.rotation) * (handlePos - phJointBehaviour.gameObject.transform.position);
                phJointBehaviour.jointOrientation = Quaternion.Inverse(phJointBehaviour.gameObject.transform.rotation) * handleRot;
            }
        }
    }
}

#endif

public abstract class PHJointBehaviour : SprSceneObjBehaviour {
    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // メンバ変数

    public GameObject socket = null;
    public GameObject plug = null;

    public GameObject jointObject = null;
    // Joint自体の位置姿勢の描画・編集有効化
    public bool showJointPoseHandle;
    // Jointの目標角度の描画・編集有効化(Jointの種類によって描画変化)
    public bool showJointTargetPositionHandle;
    public Vector3 jointPosition = new Vector3();
    public Quaternion jointOrientation =Quaternion.identity;

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

    public Posed socketPose;
    public Posed plugPose;

    // -- Sprオブジェクトの構築を行う
    public override ObjectIf Build() {
        if (!socket) { socket = gameObject.transform.parent.GetComponentInParent<PHSolidBehaviour>().gameObject; }
        if (!plug)   { plug   = gameObject.GetComponentInParent<PHSolidBehaviour>().gameObject; }

        if (socket == null) { throw new ObjectNotFoundException("Socket object did not found for Joint", gameObject); }
        if (plug == null) { throw new ObjectNotFoundException("Plug object did not found for Joint", gameObject); }

        PHSolidIf soSock = socket.GetComponent<PHSolidBehaviour>().sprObject as PHSolidIf;
        PHSolidIf soPlug = plug.GetComponent<PHSolidBehaviour>().sprObject as PHSolidIf;

        // SocketとPlugを設定。SetSocketPose/SetPlugPoseを使わずdescの段階でセット。
        if (autoSetSockPlugPose) {
            Posed jointPose = new Posed();
            if (jointObject == null) {
                jointObject = gameObject;
                jointPose = jointObject.transform.ToPosed() * new Posed(jointPosition.ToVec3d(), jointOrientation.ToQuaterniond());
            } else {
                jointPose = jointObject.transform.ToPosed();
            }
            var desc = (PHConstraintDescStruct)GetDescStruct();
            desc.poseSocket = soSock.GetPose().Inv() * jointPose;
            desc.posePlug = soPlug.GetPose().Inv() * jointPose;
        }

        PHJointIf jo = CreateJoint(soSock, soPlug);

        jo.SetName("jo:" + gameObject.name);

        // SetSocketPose / SetPlugPoseが動かなくなているかもしれない。 (2022/01)
        // 以前はこの段階でSocketとPlugを設定していたが、上でdescにセットするようにした。
        /*
        if (autoSetSockPlugPose) {
            // priority jointObject > jointPosition/Orientation > gameObject
            Posed jointPose = new Posed();
            if (jointObject == null) {
                jointObject = gameObject;
                jointPose = jointObject.transform.ToPosed() * new Posed(jointPosition.ToVec3d(), jointOrientation.ToQuaterniond());
            } else {
                jointPose = jointObject.transform.ToPosed();
            }
            jo.SetSocketPose(soSock.GetPose().Inv() * jointPose);
            jo.SetPlugPose(soPlug.GetPose().Inv() * jointPose);
        }
        */

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
