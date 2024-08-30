using System;
using System.Collections;
using System.Collections.Generic;
using DelaunatorSharp;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class TrialPuzzlePiece : MonoBehaviour
{
    [SerializeField] private MeshFilter meshFilter;

    [Space(20), Header("Spline Data")] 
    [SerializeField, Range(0, 1)] private float t;
    [SerializeField] private SplineContainer splineContainer;

    private Mesh mesh;

    private List<Vector3> vertices;
    private void Start()
    {
        if (meshFilter == null)
            meshFilter = GetComponent<MeshFilter>();
        
        GenerateMesh();
        
    }

    private void Update()
    { 
        Vector3 pos = splineContainer.EvaluatePosition(t);
        Debug.Log($"pos is {pos}");
    }

    void GenerateMesh()
    {
        mesh = new Mesh();
        meshFilter.mesh = mesh;
        vertices = new List<Vector3>();
        
        // Add Bottom-left point
        vertices.Add(new (-1, -1));
        // vertices.Add(new (-1, 1f));
        // vertices.AddRange(GetVerticesFromSpline(new (-1, -1f),90,-1,0.025f));
        vertices.AddRange(GetVerticesFromSpline(new (-1, 1f),0,1,0.025f));
        vertices.AddRange(GetVerticesFromSpline(new (1, 1f),270,-1,0.025f));
        vertices.AddRange(GetVerticesFromSpline(new (1, -1f),180,1,0.025f));
        
        // vertices.Add(new (1, 0.75f));
        //Add Bottom-right point
        // vertices.Add(new (1, -1));
        
        // List<int> triangles = EarClipping.Triangulate(vertices);
        // UpdateMesh(vertices.ToArray(),triangles.ToArray());
    }

    void UpdateMesh(Vector3[] verts, int[] triangles)
    {
        
        mesh.Clear();
        mesh.vertices = verts;
        mesh.triangles = triangles;
    }

    List<Vector3> GetVerticesFromSpline(Vector2 startPos,float angle,int yScale, float step)
    {
        Vector3 localScale = splineContainer.transform.localScale;
        localScale.y *= yScale;
        splineContainer.transform.localScale = localScale;
        splineContainer.transform.localRotation = Quaternion.Euler(0,0,angle);
        List<Vector3> splinePoints = new List<Vector3> { startPos };

        for (float i = step; i < 1; i += step)
        {
            Vector3 p = splineContainer.EvaluatePosition(i);
            splinePoints.Add( startPos + (Vector2)p);
        }
        return splinePoints;
    }
}
