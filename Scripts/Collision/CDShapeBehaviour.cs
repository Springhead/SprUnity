using UnityEngine;
using System.Collections;
using SprCs;
using SprUnity;

public abstract class CDShapeBehaviour : SprSceneObjBehaviour {
    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // メンバ変数

    public GameObject shapeObject = null;

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // 派生クラスで実装するメソッド

    // -- 形状固有のShapePoseの取得。剛体からの相対位置姿勢による分は除く
    public virtual Posed ShapePose(GameObject shapeObject) { return new Posed(); }
    // -- SpringheadのShapeオブジェクトを構築する
    public abstract CDShapeIf CreateShape(GameObject shapeObject);

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // SprBehaviourの派生クラスで実装するメソッド

    // -- Sprオブジェクトの構築を行う
    public override ObjectIf Build() {
        if (shapeObject == null) {
            shapeObject = gameObject;
        }

        CDShapeIf shape = CreateShape(shapeObject);
        phSolid.AddShape(shape);

        GameObject solidObject = solidBehaviour.gameObject;

        // 剛体オブジェクトからの相対変換
        Posed relPoseFromSolid = solidObject.transform.ToPosed().Inv() * shapeObject.transform.ToPosed();

        phSolid.SetShapePose(phSolid.NShape() - 1, relPoseFromSolid * ShapePose(shapeObject));

        return shape;
    }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // MonoBehaviourのメソッド

    // UnityのOnValidate : SprBehaviourのものをオーバーライド
    public override void OnValidate() {
        if (GetDescStruct() == null) {
            ResetDescStruct();
        }

        if (sprObject != null) {
            CsObject d = CreateDesc();
            if (d != null) {
                var descStruct = GetDescStruct();

                sprObject.GetDesc(d);
                ApplyDesc(descStruct, d);
                sprObject.SetDesc(d);

                // SetDescとは別に別途設定する必要があるようだ
                (sprObject as CDShapeIf).SetMaterial((descStruct as CDShapeDescStruct).material);
            }
        }
    }

    private bool applicationQuit = false;
    private void OnApplicationQuit() {
        applicationQuit = true;
    }

    private void OnDestroy() {
        if (!applicationQuit) {
            phSdk.DelChildObject(sprObject);
        }
    }
    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // その他のメソッド

    public PHSolidBehaviour solidBehaviour {
        get {
            PHSolidBehaviour b = gameObject.GetComponentInParent<PHSolidBehaviour>();
            if (b == null) { throw new ObjectNotFoundException("The object should have PHSolidBehaviour", gameObject); }
            return b;
        }
    }

    public PHSolidIf phSolid {
        get {
            PHSolidIf so = solidBehaviour.sprObject as PHSolidIf;
            if (so == null) { throw new ObjectNotFoundException("The behaviour doesn't have PHSolid", gameObject); }
            return so;
        }
    }

}
