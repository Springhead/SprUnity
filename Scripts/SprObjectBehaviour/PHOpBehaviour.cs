using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Reflection;
using System.Runtime.InteropServices;
using SprCs;
using SprUnity;

public  class PHOpBehaviour : SprSceneObjBehaviour {


	public PHOpObjDesc desc = null;
	public PHOpObjIf opObjIf;
    public PHOpEngineIf opEngine;

    private PHOpSpHashColliAgentIf spIf;
    private PHOpObjDesc opObjAddr;
	
    private Mesh modelMesh;
    private Vector3[] vertices;
    private Vector3[] normals ;
    private List<GameObject> particles;
	private float[] dataArray;

	private PHOpParticleDesc[] opPtcls;
    private PHOpGroupDesc[] opGrps;

    public float AttachParticleSize = 0.05f;
    public float SceneBound = 2.0f;
    public int objid;

    public bool bCollision;
	public bool bGravity;
    public bool bDrawParticles;

    public int vNum;
	public int pNum;
	public int gNum;
    public float stiffnessAlpha;

    // -- for validation check
    private int vNconst;
    private int pNconst;
    private int gNconst;
    private int objiconst;

    private int copySize;

    // -- prevent multiple key activation
    private bool firstcal;

    // -- DescStructオブジェクトを再構築する
    public override void ResetDescStruct()
    {
        desc = new PHOpObjDesc();
    }

    // -- DescStructオブジェクトを取得する
    public override CsObject GetDescStruct()
    {
        return desc;
    }

    public override void ApplyDesc(CsObject from, CsObject to) {
		//(from as PHOpObjDescStruct).ApplyTo(to as PHOpObjDesc);
	}
	
	public override CsObject CreateDesc() {
		return new PHOpObjDesc();
	}
	
	public override ObjectIf Build() {
        
        opEngine = phScene.GetOpEngine ();

		objid=opEngine.AddOpObj ();
        objiconst = objid;

        opObjIf = opEngine.GetOpObjIf (objid);

        Initial();

        return opObjIf;
	}

    public override void OnValidate()
    {
        base.OnValidate();

        if (sprObject != null)
        {
            //check validate
            if ((vNconst != vNum) || (gNconst != gNum) || (pNconst != pNum) || (objiconst != objid))
            {
                vNum = vNconst; gNconst = gNum; pNconst = pNum; objid = objiconst;
                //Assert.IsTrue(1, "Not valid edition!");
                print("Items not editable! ");

            }

            if (stiffnessAlpha > 1.0f) stiffnessAlpha = 1.0f;
            if (stiffnessAlpha < 0.0f) stiffnessAlpha = 0.0f;

            //set inspector settings
            opObjIf.SetGravity(bGravity);
            spIf.EnableCollisionDetection(bCollision);
            DrawParticles = bDrawParticles;
            opObjIf.SetBound(SceneBound);
            opObjIf.SetObjAlpha(stiffnessAlpha);

           
        }
    }

    private void Initial() {

        stiffnessAlpha = 1.0f;
        bCollision = false;
		bDrawParticles = false;
       

		particles = new List<GameObject>();
               
		//initial OpObj & collision detection
		initialObj ();
		initialCollision ();
		
		copySize = opObjIf.GetVertexNum () * 3;
		dataArray = new float[copySize];
	    opObjAddr = new PHOpObjDesc (opObjIf.GetDescAddress());

		firstcal = true;
		float dt = Time.fixedDeltaTime;
		print (dt);
		Time.fixedDeltaTime = 0.01f;
		pNum = opObjAddr.assPsNum;
		gNum = opObjAddr.assGrpNum;
		opPtcls = new PHOpParticleDesc[pNum];
		opGrps = new PHOpGroupDesc[gNum];

		//initial local op particles and groups
		for (int pi = 0; pi < pNum; pi++) {

			opPtcls[pi] = new PHOpParticleDesc (opObjIf.GetOpParticle (pi).GetDescAddress ());

		}
		for (int gi = 0; gi < pNum; gi++) {
			
			opGrps[gi] = new PHOpGroupDesc (opObjIf.GetOpGroup (gi).GetDescAddress ());
			
		}

		for(int pi = 0; pi<pNum;pi++)
		{

			GameObject sphere = new GameObject();
			sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			sphere.SetActive(false);
			sphere.name = "Ptcl_" + pi;
            //sphere.tag = "OpParticle";
			float radius = opObjAddr.objAverRadius;
			sphere.transform.localScale = new Vector3(radius,radius,radius);

			sphere.transform.SetParent(gameObject.GetComponentInParent<PHSceneBehaviour> ().transform);
			particles.Add(sphere);
		}


        vNconst = vNum;
        gNconst = gNum;
        pNconst = pNum;

		print ("obj: " + objid + " vNum: " + vNum + " pNum: " + pNum);
	}


	private void initialObj()
	{

		bGravity = false;
		modelMesh = GetComponent<MeshFilter> ().mesh;
		vertices = modelMesh.vertices;
		normals = modelMesh.normals;
		
		PHSdkIf phSdk = phScene.GetSdk ();
		GRMeshDesc grMesh = new GRMeshDesc ();

        vNum = modelMesh.vertices.Length;

		//vector<int> a;
		for (int vi=0; vi<modelMesh.vertices.Length; vi++) {
			Vec3f v = new Vec3f ();
			v.x = vertices [vi].x;
			v.y = vertices [vi].y;
			v.z = -vertices [vi].z;
			
			opObjIf.AddVertextoLocalBuffer (v);
		}
       

		opObjIf.InitialObjUsingLocalBuffer (AttachParticleSize);
		opObjIf.SetBound(SceneBound);
        opObjIf.SetObjAlpha(stiffnessAlpha);
		opObjIf.SetGravity(false);
		

	}
    private void initialCollision()
	{
		//Collision
		
		spIf = phScene.GetOpColliAgent();
		
		SprCs.CDBounds bounds;
		bounds = new SprCs.CDBounds();
		
		
		float boundcube = opObjIf.GetBoundLength();
		bounds.min.x = -boundcube;
		bounds.min.y = -boundcube;
		bounds.min.z = -boundcube;
		bounds.max.x = boundcube;
		bounds.max.y = boundcube;
		bounds.max.z = boundcube;
		
		spIf.Initial(0.5f, bounds);
		
		spIf.EnableCollisionDetection(false);
		
		

	}

    // Update is called once per frame
    void FixedUpdate () {
		
		opEngine.StepWithBlend ();
        KeyConfigs();

        //Marshal Copy for vertices
        try
		{
			Marshal.Copy(opObjAddr.objTargetVtsArr._this, dataArray, 0, copySize);
			
		}
		finally
		{
			
		}


		for (int vi=0; vi<vNum; vi++) {
			vertices[vi].x = dataArray[vi*3];
			vertices [vi].y = dataArray[vi*3 + 1];
			vertices [vi].z = -dataArray[vi*3 + 2];
	
		}

		modelMesh.vertices = vertices;

		modelMesh.RecalculateNormals ();

		DrawDebugs ();

	}

    private void DrawDebugs()
	{
        if (bDrawParticles)
        {
            for (int pi = 0; pi < pNum; pi++)
            {
                PHOpParticleDesc dp = opPtcls[pi];
                particles[pi].transform.position = new Vector3(dp.pCurrCtr.x, dp.pCurrCtr.y, -dp.pCurrCtr.z);
            }
        }
		
		
	}

    private bool DrawParticles
    {
        get { return bDrawParticles; }
        set
        {
            bDrawParticles = value;
            for (int pi = 0; pi < pNum; pi++)
            {

                particles[pi].SetActive(value);

                PHOpParticleDesc dp = opPtcls[pi];
                particles[pi].transform.position = new Vector3(dp.pCurrCtr.x, dp.pCurrCtr.y, -dp.pCurrCtr.z);


            }
        }

    }

    private void KeyConfigs()
	{
		if (!Input.anyKey)
			return;

		if(firstcal) 
		{
			firstcal = !firstcal;
		}else {
			firstcal = !firstcal;
				return;
		}
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            print("Pop first particle up");
            PHOpParticleIf pIf = (PHOpParticleIf)opObjIf.GetOpParticle(objid);

            arraywrapper_PHOpParticleDesc dp = (arraywrapper_PHOpParticleDesc)pIf.GetParticleDesc();
            dp[0].pCurrCtr.x += dp[0].pCurrCtr.x;
            dp[0].pCurrCtr.y += 1.0F;
            dp[0].pCurrCtr.x += dp[0].pCurrCtr.z;

        }
        else if (Input.GetKeyDown("c"))
        {
            bCollision = !bCollision;
            print("bCollision is " + bCollision);
            spIf.EnableCollisionDetection(bCollision);
        }
        else if (Input.GetKeyDown("g"))
        {
            bGravity = !bGravity;
            print("bGravity is " + bGravity);
            opObjIf.SetGravity(bGravity);
        }
        else if (Input.GetKeyDown("p"))
        {
            DrawParticles = !DrawParticles;
            print("DrawParticles is " + DrawParticles);
        }
	}
    
}
