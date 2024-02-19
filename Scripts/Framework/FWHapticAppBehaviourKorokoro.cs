using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SprCs;
using SprUnity;

[DefaultExecutionOrder(0)]
public class FWHapticAppBehaviourKorokoro : MonoBehaviour
{
    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // メンバ変数

    public static FWHapticApp app = null;

    public PHSceneBehaviour phSceneBehaviour;
    public HIKorokoroBehaviour hiKorokoroBehaviour;

    public float spring;
    public float damper;

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // Privateメンバ

    private PHHapticPointerIf pointer;
    private GameObject pointerObj;

    private Transform pointerTransform;
    private float beforespring;
    private float beforedamper;
    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // MonoBehaviourのメソッド

    void Start()
    {
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
        shapeDesc.radius = 0.05f;
        shapeDesc.material.mu = 0;
        shapeDesc.material.mu0 = 0;
        var shape = phSceneBehaviour.phScene.GetSdk().CreateShape(CDSphereIf.GetIfInfoStatic(), shapeDesc);
        pointer.AddShape(shape);
        pointer.SetHapticRenderMode(PHHapticPointerDesc.HapticRenderMode.CONSTRAINT);
        //pointer.SetLocalRange(1.0f);
        //pointer.SetPosScale(1);
        pointer.EnableForce(true);

        var fwPointer = app.GetSdk().GetScene(0).CreateHapticPointer();
        fwPointer.SetHumanInterface(hiKorokoroBehaviour.hiKorokoro);
        fwPointer.SetPHHapticPointer(pointer);

        pointer.SetReflexSpring(spring);
        pointer.SetReflexDamper(damper);
        pointer.SetFrictionSpring(0f);

        // 

        pointerObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pointerObj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

        // 

        app.GetTimer(0).SetInterval(10);
        app.GetTimer(1).SetMode(UTTimerIf.Mode.MULTIMEDIA);
        app.GetTimer(1).SetResolution(1);
        app.GetTimer(1).SetInterval(1);
        app.StartTimers();

        //

        pointerTransform = GameObject.Find("Pointer").transform;
        
        //
    }

    void FixedUpdate()
    {
        app.GetSdk().GetScene(0).GetPHScene().GetHapticEngine().StepPhysicsSimulation();
        //pointerObj.transform.position = pointer.GetPose().Pos().ToVector3();
        pointerObj.transform.position = pointerTransform.position;
        if (spring !=beforespring)
        {
            pointer.SetReflexSpring(spring);
        }
        beforespring = spring;
        if (damper != beforedamper)
        {
            pointer.SetReflexDamper(damper);
        }
        beforedamper = damper;


    }
}
