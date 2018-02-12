using UnityEngine;
using System.Collections;
using SprCs;
using System.Threading;

[DefaultExecutionOrder(0)]
public class FWAppBehaviour : MonoBehaviour {
    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // メンバ変数

    public static FWApp app = null;
    private Thread mainloop;

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // MonoBehaviourのメソッド

    void Awake () {
        if (!enabled) { return; }

        if (app == null) {
            app = new FWApp();

            app.CreateSdk();

            FWSceneIf fwScene = app.GetSdk().CreateScene(new PHSceneDesc(), new GRSceneDesc());
            fwScene.SetRenderMode(true, true);
            fwScene.EnableRenderPHScene(true);
            fwScene.EnableRenderForce(true, true);

            GRDeviceIf device = app.GRInit();

            FWWinIf nullwin = new FWWinIf(); nullwin._this = (System.IntPtr)0;
            FWWinIf win = app.CreateWin(new FWWinDesc(), nullwin);

            mainloop = new Thread(new ThreadStart(MainLoop));
            mainloop.Start();
        }
    }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
    // その他のメソッド

    void MainLoop() {
        app.StartMainLoop();
    }
}
