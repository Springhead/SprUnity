using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SprCs;
using SprUnity;

[DefaultExecutionOrder(0)]
public class FWHapticAppBehaviour : MonoBehaviour {
    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // メンバ変数

    public static FWHapticApp app = null;

    public PHSceneBehaviour phSceneBehaviour;
    public HISpidarGBehaviour hiSpidarGBehaviour;

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // Privateメンバ

    private PHHapticPointerIf pointer;
    private GameObject pointerObj;

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // MonoBehaviourのメソッド

    void Start () {
        app = new FWHapticApp();

        app.CreateSdk();
        app.GetSdk().CreateScene();
        app.SetPHScene(phSceneBehaviour.phScene);

        app.CreateTimers();

        //

        phSceneBehaviour.enableStep = false;

        //

        phSceneBehaviour.phScene.GetHapticEngine().Enable(true);
        phSceneBehaviour.phScene.GetHapticEngine().SetHapticStepMode(PHHapticEngineDesc.HapticStepMode.LOCAL_DYNAMICS);

        //

        pointer = phSceneBehaviour.phScene.CreateHapticPointer();

        var shapeDesc = new CDSphereDesc();
        shapeDesc.radius = 0.1f;
        var shape = phSceneBehaviour.phScene.GetSdk().CreateShape(CDSphereIf.GetIfInfoStatic(), shapeDesc);
        pointer.AddShape(shape);
        pointer.SetHapticRenderMode(PHHapticPointerDesc.HapticRenderMode.CONSTRAINT);
        pointer.SetLocalRange(20.0f);
        pointer.SetPosScale(100);
        pointer.EnableForce(true);

        var fwPointer = app.GetSdk().GetScene(0).CreateHapticPointer();
        fwPointer.SetHumanInterface(hiSpidarGBehaviour.hiSpidar);
        fwPointer.SetPHHapticPointer(pointer);

        // 

        pointerObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        // pointerObj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

        // 

        app.GetTimer(0).SetInterval(10);
        app.GetTimer(1).SetResolution(1);
        app.GetTimer(1).SetInterval(1);
        app.StartTimers();
    }

    void FixedUpdate() {
        app.GetSdk().GetScene(0).GetPHScene().GetHapticEngine().StepPhysicsSimulation();
        pointerObj.transform.position = pointer.GetPose().Pos().ToVector3();
    }
}
