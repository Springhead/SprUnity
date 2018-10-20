using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(MeshPartController))]
public class MeshPartControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MeshPartController mesh = (MeshPartController)target;
        base.OnInspectorGUI();
        if (GUILayout.Button("FillIndexConsecutive"))
        {
            mesh.FillIndexConsecutive();
        }
    }
}
#endif

public class MeshPartController : MonoBehaviour {

    // 指定したメッシュを塊として動かす

    public string controlMeshName;

    private class DefaultVertex
    {
        public int Index { get; set; }
        public Vector3 Vertex { get; set; }
    }

    public int[] index = new int[] { };

        public Vector3 HandlerOffset;

    public SkinnedMeshRenderer skinnedMeshRenderer;

    //[HideInInspector]
    public Mesh _mesh;
    private DefaultVertex[] _defaultMeshVertices;

    [Range(-0.1f, 0.1f)]
    public float height;

    // Use this for initialization
    void Start()
    {
        if (this.skinnedMeshRenderer == null)
        {
            this.skinnedMeshRenderer = this.GetComponentInChildren<SkinnedMeshRenderer>();
        }

        // オリジナルのメッシュが入っているときはCloneを作成する
        if (!skinnedMeshRenderer.sharedMesh.name.Contains("Clone"))
        {
            this._mesh = this.skinnedMeshRenderer.sharedMesh;
            this._mesh = GameObject.Instantiate(this._mesh);
            this._mesh.MarkDynamic();
            this.skinnedMeshRenderer.sharedMesh = this._mesh;
        }
        else
        {
            this._mesh = this.skinnedMeshRenderer.sharedMesh;
        }

        var vertices = _mesh.vertices;

        int length = index.Length;
        _defaultMeshVertices = new DefaultVertex[length];

        for (int i = 0; i < length; i++)
        {
            _defaultMeshVertices[i] = new DefaultVertex();
            _defaultMeshVertices[i].Index = index[i];
            _defaultMeshVertices[i].Vertex = vertices[index[i]];
        }
        //skinnedMeshRenderer.sharedMesh.vertices = vertices;
        //skinnedMeshRenderer.sharedMesh.RecalculateNormals();
        //skinnedMeshRenderer.sharedMesh.RecalculateBounds();
    }

    // Update is called once per frame
    void Update()
    {
        
        int l = index.Length;
        var vertices = _mesh.vertices;
        Vector3 diff = new Vector3(0.0f, height, 0.0f);
        for (int i = 0; i < l; i++)
        {
            vertices[index[i]] = _defaultMeshVertices[i].Vertex + diff;
        }

        _mesh.vertices = vertices;

        //skinnedMeshRenderer.sharedMesh.RecalculateNormals();
        //skinnedMeshRenderer.sharedMesh.RecalculateBounds();
        
    }

    public void FillIndexConsecutive()
    {
        if (index.Length <= 0) return;
        int l = index.Length;
        int f = index[0];
        for(int i = 0; i < l; i++)
        {
            index[i] = f + i;
        }
    }
}
