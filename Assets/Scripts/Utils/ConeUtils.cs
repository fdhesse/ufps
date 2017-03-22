using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;

public static class ConeUtils
{
    /*
    public Material mat;

    private GameObject _minCone, _currentCone, _maxCone;

    void Awake()
    {
        CreateAccuracyCones();
    }

    void CreateAccuracyCones()
    {
        _minCone = CreateCone("MinAccuracyCone", 90);
        //_currentCone = CreateCone("CurrentAccuracyCone", 32, 20);
        //_maxCone = CreateCone("MaxAccuracyCone", 32, 30);
    }

    private float rad = 0;

    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        rad = GUILayout.HorizontalSlider(rad, 0, 360, GUILayout.Width(500));
        if (GUI.changed)
            UpdateCone(_minCone, rad);
        GUILayout.EndHorizontal();
    }
    */

    public static GameObject CreateCone(string name, float radiusInDegree, Material mat, int precision = 32)
    {
        var go = new GameObject(name);
        var mr = go.AddComponent<MeshRenderer>();
        var mf = go.AddComponent<MeshFilter>();

        mr.shadowCastingMode = ShadowCastingMode.Off;
        mr.material = mat;

        FillConeInfos(radiusInDegree, precision, mf);

        return go;
    }

    public static void UpdateCone(this GameObject cone, float radiusInDegree, int precision = 32)
    {
        var mf = cone.GetComponent<MeshFilter>();
        FillConeInfos(radiusInDegree, precision, mf);
    }

    static void FillConeInfos(float radiusInDegree, int precision, MeshFilter mf)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        vertices.Add(Vector3.forward);

        var rad = radiusInDegree * Mathf.PI / 180f;
        var h = Mathf.Tan(rad / 2f);

        for (int i = 0; i < precision; i++)
        {
            float f1 = i / (precision - 1f) * Mathf.PI * 2;
            var a = Vector3.forward + new Vector3(Mathf.Sin(f1), Mathf.Cos(f1), 0).normalized * h;

            vertices.Add(a);
        }

        for (int i = 0; i < precision - 1; i++)
        {
            triangles.Add(0);
            triangles.Add(i + 1);
            triangles.Add(i + 2);
        }
        triangles.Add(0);
        triangles.Add(vertices.Count - 1);
        triangles.Add(0);

        var mesh = mf.mesh;

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mf.mesh = mesh;
    }
}
