using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(MeshController))]
public class MeshControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MeshController mesh = (MeshController)target;
        base.OnInspectorGUI();
        if (GUILayout.Button("LoadMesh"))
        {
            mesh.LoadMesh();
        }
        if (GUILayout.Button("LoadInitial"))
        {
            mesh.LoadInitialPosition();
        }
        if (GUILayout.Button("SavePreset"))
        {
            mesh.Save();
        }
        if (GUILayout.Button("LoadPreset"))
        {
            mesh.Load();
        }
        if (GUILayout.Button("SaveNeutral"))
        {
            mesh.SaveNeutral();
        }
        if (GUILayout.Button("LoadNeutral"))
        {
            mesh.LoadNeutral();
        }
        mesh.copyOriginal = EditorGUILayout.TextField("コピーのオリジナル", mesh.copyOriginal);
        mesh.xM = EditorGUILayout.Toggle("xMirror", mesh.xM);
        mesh.yM = EditorGUILayout.Toggle("yMirror", mesh.yM);
        mesh.zM = EditorGUILayout.Toggle("zMirror", mesh.zM);
        if (GUILayout.Button("CopyFromOtherMeshController"))
        {
            MeshController[] meshes = mesh.gameObject.GetComponents<MeshController>();
            foreach(MeshController m in meshes)
            {
                if (m.label.Equals(mesh.copyOriginal))
                {
                    mesh.CopyFromOtherMeshController(m, mesh.xM, mesh.yM, mesh.zM);
                    break;
                }
            }
        }
    }

    void OnSceneGUI()
    {
        
        MeshController mesh = (MeshController)target;
        if (mesh.mode != MeshController.EditMode.No_edit && mesh._mesh)
        {
            EditorGUI.BeginChangeCheck();
            var vertices = mesh._mesh.vertices;
            foreach(int i in mesh.indexes)
            {
                Handles.Label(vertices[i] + mesh.HandlerOffset, i.ToString());
            }
            if (mesh.editingVertices.Count > 0)
            {
                Vector3 center = vertices[mesh.editingVertices[0]];
                Vector3 vertexPos = Handles.PositionHandle(center + mesh.HandlerOffset, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    vertexPos -= (mesh.HandlerOffset + center);
                    foreach (int i in mesh.editingVertices)
                    {
                        vertices[i] += vertexPos;
                    }
                    foreach (int i in mesh.editingVerticesMirror)
                    {
                        vertices[i] += new Vector3(-vertexPos.x, vertexPos.y, vertexPos.z);
                    }

                    mesh._mesh.vertices = vertices;
                    // mesh._mesh.SetVertices(vertices.ToList()); //これのほうがいい？
                    //mesh.skinnedMeshRenderer.sharedMesh.RecalculateNormals();
                    //mesh.skinnedMeshRenderer.sharedMesh.RecalculateBounds();
                }
            }
        }
        
    }
}

#endif

public class MeshController : MonoBehaviour {

    // メッシュを編集したい

    // メンバクラスの定義

    private class DefaultVertex
    {
        public int Index { get; set; }
        public Vector3 Vertex { get; set; }
        public DefaultVertex(int i, Vector3 v) { Index = i; Vertex = v; }
    }

    [Serializable]
    public class UsingIndex
    {
        public string label;
        public List<int> index = new List<int>();
        public List<int> mirror = new List<int>();
    }

    [Serializable]
    public class VertexPos
    {
        // UsingIndex内での順番
        public int index = -1;
        public Vector3 position;
        public VertexPos(int i, Vector3 p) { index = i; position = p; }
        public void Save()
        {
            if (index == -1) return;
            //position = .localPosition;
        }
        public void Load()
        {
            if (index == -1) return;
            //if (!saved) return;
            //.transform.localPosition = position;
        }
        public Vector3 GetWeightedValue(float w)
        {
            return position * w;
        }
    }

    [Serializable]
    public class Preset
    {
        public string label;
        public List<VertexPos> vertexPos = new List<VertexPos>();
        //[HideInInspector]
        [Range(0.0f, 100.0f)]
        public float weight;
        public bool locked = false;
    }

    // 以下、メンバ変数

    public string label;

    public int[] indexes;

    public Vector3 HandlerOffset;
    public enum EditMode
    {
        No_edit,
        Index_mode,
        GroupedIndex_mode,
        Area_mode,
    }
    [Tooltip("頂点位置編集用")]
    public List<UsingIndex> GroupedIndexes = new List<UsingIndex>();
    public EditMode mode;
    public int index = -1;

    [HideInInspector]
    public List<int> editingVertices;
    [HideInInspector]
    public List<int> editingVerticesMirror;

    public Preset neutral;
    public List<Preset> presets;
    public int presetIndex;

    public SkinnedMeshRenderer skinnedMeshRenderer;

    //[HideInInspector]
    public Mesh _mesh;
    // Meshの全点のデフォルト位置を保存
    private DefaultVertex[] _defaultMeshVertices;

    // コピー用
    [HideInInspector]
    public string copyOriginal;
    [HideInInspector]
    public bool xM;
    [HideInInspector]
    public bool yM;
    [HideInInspector]
    public bool zM;

    // Use this for initialization
    void Start() {

        if (this.skinnedMeshRenderer == null)
        {
            this.skinnedMeshRenderer = this.GetComponentInChildren<SkinnedMeshRenderer>();
        }

        // Clone mesh and save it to SkinnedMeshRenderer
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

        // Save Vertices positions

        var vertices = this._mesh.vertices;

        int length = vertices.Length;
        this._defaultMeshVertices = new DefaultVertex[length];

        for (int i = 0; i < length; i++)
        {
            _defaultMeshVertices[i] = new DefaultVertex(i, vertices[i]);
        }
    }

    // Update is called once per frame
    void Update() {
        
        // 各GameObjectのtransformをPrimitiveの加重和で更新
        float wSum = 0.0f;
        foreach (Preset p in presets)
        {
            wSum += p.weight;
        }
        if (wSum > 100.0f)
        {
            foreach (Preset p in presets)
            {
                p.weight *= (100.0f / wSum);
            }
            wSum = 100.0f;
        }
        neutral.weight = 100.0f - wSum;
        var vertices = _mesh.vertices;

        // 以下、とりあえずすべてのPresetのオブジェクト配列が同じと仮定して
        foreach (VertexPos v in neutral.vertexPos)
        {
            vertices[v.index] = v.GetWeightedValue(neutral.weight / 100.0f);
        }
        foreach (Preset p in presets)
        {
            foreach (VertexPos v in p.vertexPos)
            {
                vertices[v.index] += v.GetWeightedValue(p.weight / 100.0f);
            }
        }
        _mesh.vertices = vertices;
    }

    public void OnValidate()
    {
        ReloadEditingVertices();
    }

    public void LoadMesh()
    {
        if (this.skinnedMeshRenderer == null)
        {
            this.skinnedMeshRenderer = this.GetComponentInChildren<SkinnedMeshRenderer>();
        }

        // Clone mesh and save it to SkinnedMeshRenderer
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

        // Save Vertices positions

        var vertices = this._mesh.vertices;

        int length = vertices.Length;
        this._defaultMeshVertices = new DefaultVertex[length];

        for (int i = 0; i < length; i++)
        {
            _defaultMeshVertices[i] = new DefaultVertex(i, vertices[i]);
        }
    }

    public void LoadInitialPosition()
    {
        var vertices = this.skinnedMeshRenderer.sharedMesh.vertices;
        int length = vertices.Length;

        for (int i = 0; i < length; i++)
        {
            vertices[i] = _defaultMeshVertices[i].Vertex;
        }
        this.skinnedMeshRenderer.sharedMesh.vertices = vertices;
        //this.skinnedMeshRenderer.sharedMesh.RecalculateNormals();
        //this.skinnedMeshRenderer.sharedMesh.RecalculateBounds();
    }

    public void SetBlendShapeWeight(int index, float value)
    {
        this.presets[index].weight = value;
    }

    public void Save()
    {
        if (presetIndex > this.presets.Count) return;
        if (presets[presetIndex].locked) return;
        //int l = (int)Mathf.Min(presets[presetIndex].vertexPos.Count, indexes.Count());
        var vertices = _mesh.vertices;
        presets[presetIndex].vertexPos.Clear();
        for(int i = 0; i < indexes.Count(); i++)
        {
            presets[presetIndex].vertexPos.Add(new VertexPos(indexes[i], vertices[indexes[i]]));
        }
    }

    public void Load()
    {
        if (presetIndex > this.presets.Count) return;
        int l = (int)Mathf.Min(presets[presetIndex].vertexPos.Count, indexes.Count());
        var vertices = _mesh.vertices;
        for (int i = 0; i < l; i++)
        {
            vertices[presets[presetIndex].vertexPos[i].index] = presets[presetIndex].vertexPos[i].position;
        }
        _mesh.vertices = vertices;
    }

    public void SaveNeutral()
    {
        if (neutral.locked) return;
        //int l = (int)Mathf.Min(presets[presetIndex].vertexPos.Count, indexes.Count());
        var vertices = _mesh.vertices;
        neutral.vertexPos.Clear();
        for (int i = 0; i < indexes.Count(); i++)
        {
            neutral.vertexPos.Add(new VertexPos(indexes[i], vertices[indexes[i]]));
        }
    }

    public void LoadNeutral()
    {
        int l = (int)Mathf.Min(neutral.vertexPos.Count, indexes.Count());
        var vertices = _mesh.vertices;
        for (int i = 0; i < l; i++)
        {
            vertices[neutral.vertexPos[i].index] = neutral.vertexPos[i].position;
        }
        _mesh.vertices = vertices;
    }

    public void ReloadEditingVertices()
    {
        if (_mesh)
        {
            editingVertices.Clear();
            editingVerticesMirror.Clear();
            switch (mode)
            {
                case EditMode.Index_mode:
                    if (index < _mesh.vertices.Count() && index > -1)
                        editingVertices.Add(index);
                    break;
                case EditMode.GroupedIndex_mode:
                    if (index < GroupedIndexes.Count() && index > -1)
                    {
                        foreach (int i in GroupedIndexes[index].index)
                        {
                            editingVertices.Add(i);
                        }
                        foreach (int i in GroupedIndexes[index].mirror)
                        {
                            editingVerticesMirror.Add(i);
                        }
                    }
                    break;
                case EditMode.Area_mode:
                    break;
                case EditMode.No_edit:
                    break;
            }
        }
    }

    public void CopyFromOtherMeshController(MeshController m, bool xMirror = false, bool yMirror = false, bool zMirror = false)
    {
        if (indexes.Count() != m.indexes.Count()) return;
        int indexesLength = indexes.Length;
        // neutralのコピー
        neutral = new Preset();
        neutral.label = m.neutral.label;
        for(int i = 0; i < indexesLength; i++)
        {
            Vector3 v = m.neutral.vertexPos[i].position;
            neutral.vertexPos.Add(new VertexPos(indexes[i], new Vector3(xMirror ? -v.x : v.x, yMirror ? -v.y : v.y, zMirror ? -v.z : v.z)));
        }
        neutral.locked = m.neutral.locked;
        // presetsのコピー
        presets.Clear();
        foreach(Preset p in m.presets)
        {
            Preset copiedPreset = new Preset();
            copiedPreset.label = p.label;
            for (int i = 0; i < indexesLength; i++)
            {
                Vector3 v = p.vertexPos[i].position;
                copiedPreset.vertexPos.Add(new VertexPos(indexes[i], new Vector3(xMirror ? -v.x : v.x, yMirror ? -v.y : v.y, zMirror ? -v.z : v.z)));
            }
            copiedPreset.locked = p.locked;
            presets.Add(copiedPreset);
        }
    }
}