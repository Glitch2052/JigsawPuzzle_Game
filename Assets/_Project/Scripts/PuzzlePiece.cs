using System.Collections.Generic;
using DG.Tweening;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class PuzzlePiece : IObject
{
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private MeshRenderer meshRenderer;

    [HideInInspector] public Vector2Int gridCoordinate;

    [Space(20),Header("EdgeInfo")]
    public EdgeType topEdge;
    public EdgeType bottomEdge;
    public EdgeType leftEdge;
    public EdgeType rightEdge;

    //Mesh Data
    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    private Vector2[] boardUVs;
    private Vector2[] meshUvs;

    //Vertices List
    private List<float3> allSplinePoints;
    
    //Native Containers For Jobs
    private NativeArray<float3> nativeVertexData;
    private NativeArray<int> tris;
    private NativeArray<float2> nativeBoardUvData;
    private NativeArray<float2> nativeMeshUvData;
    private NativeList<int> indexList;

    public void SetData(float cellSize, int x, int y)
    {
        mesh = new Mesh();
        meshFilter.mesh = mesh;
        
        MainCollider.size = Vector2.one * cellSize;
        gridCoordinate = new Vector2Int(x, y);
    }
    
    public JobHandle ScheduleMeshDataGenerationJob()
    {
        allSplinePoints = new List<float3>();
        
        // Add Left Edge
        if(leftEdge == EdgeType.Flat)
            allSplinePoints.Add(PuzzleGenerator.Instance.edgeShapeSO.BottomLeft);
        else
        {
            allSplinePoints.AddRange(leftEdge == EdgeType.Knob ? PuzzleGenerator.Instance.edgeShapeSO.GetEvaluatedVertices(EdgeName.LeftKnob) :
                PuzzleGenerator.Instance.edgeShapeSO.GetEvaluatedVertices(EdgeName.LeftSocket));
        }
        
        // Add Top Edge
        if (topEdge == EdgeType.Flat)
            allSplinePoints.Add(PuzzleGenerator.Instance.edgeShapeSO.TopLeft);
        else
        {
            allSplinePoints.AddRange(topEdge == EdgeType.Knob ? PuzzleGenerator.Instance.edgeShapeSO.GetEvaluatedVertices(EdgeName.TopKnob) :
                PuzzleGenerator.Instance.edgeShapeSO.GetEvaluatedVertices(EdgeName.TopSocket));
        }
        
        // Add Right Edge
        if (rightEdge == EdgeType.Flat)
            allSplinePoints.Add(PuzzleGenerator.Instance.edgeShapeSO.TopRight);
        else
        {
            allSplinePoints.AddRange(rightEdge == EdgeType.Knob ? PuzzleGenerator.Instance.edgeShapeSO.GetEvaluatedVertices(EdgeName.RightKnob) :
                PuzzleGenerator.Instance.edgeShapeSO.GetEvaluatedVertices(EdgeName.RightSocket));
        }
        
        // Add Bottom Edge
        if (bottomEdge == EdgeType.Flat)
            allSplinePoints.Add(PuzzleGenerator.Instance.edgeShapeSO.BottomRight);
        else
        {
            allSplinePoints.AddRange(bottomEdge == EdgeType.Knob ? PuzzleGenerator.Instance.edgeShapeSO.GetEvaluatedVertices(EdgeName.BottomKnob) :
                PuzzleGenerator.Instance.edgeShapeSO.GetEvaluatedVertices(EdgeName.BottomSocket));
        }
        
        // allSplinePoints.AddRange(PuzzleGenerator.Instance.edgeShapeSO.GetEvaluatedVertices(EdgeName.LeftSocket));
        // allSplinePoints.AddRange(PuzzleGenerator.Instance.edgeShapeSO.GetEvaluatedVertices(EdgeName.TopKnob));
        // allSplinePoints.AddRange(PuzzleGenerator.Instance.edgeShapeSO.GetEvaluatedVertices(EdgeName.RightKnob));
        // allSplinePoints.AddRange(PuzzleGenerator.Instance.edgeShapeSO.GetEvaluatedVertices(EdgeName.BottomSocket));

        //Native Vertex Container
        nativeVertexData = new NativeArray<float3>(allSplinePoints.Count, Allocator.TempJob);
        for (int i = 0; i < nativeVertexData.Length; i++)
        {
            nativeVertexData[i] = allSplinePoints[i];
        }
        
        //Native Triangles Container
        int totalTriangleIndexCount = 3 * (nativeVertexData.Length - 2);
        tris = new NativeArray<int>(totalTriangleIndexCount,Allocator.TempJob);
        indexList = new NativeList<int>(Allocator.TempJob);
        
        //Native UV Container
        nativeBoardUvData = new NativeArray<float2>(nativeVertexData.Length, Allocator.TempJob);
        nativeMeshUvData = new NativeArray<float2>(nativeVertexData.Length, Allocator.TempJob);
        
        VertexTriangulationJob triangulationJob = new VertexTriangulationJob
        {
            vertices = nativeVertexData,
            triangles = tris,
            indexList = indexList,
            
            localToWorldMatrix = transform.localToWorldMatrix,
            minBoardPosition = PuzzleGenerator.Instance.puzzleBoardMinPos,
            maxBoardPosition = PuzzleGenerator.Instance.puzzleBoardMaxPos,
            boardUvs = nativeBoardUvData,
            meshUvs = nativeMeshUvData
        };
        
        return triangulationJob.Schedule();
    }

    public void GetMeshDataFromJob()
    {
        vertices = new Vector3[nativeVertexData.Length];
        boardUVs = new Vector2[nativeBoardUvData.Length];
        meshUvs = new Vector2[nativeMeshUvData.Length];
        
        for (var i = 0; i < vertices.Length; i++)
        {
            vertices[i] = nativeVertexData[i];
            boardUVs[i] = nativeBoardUvData[i];
            meshUvs[i] = nativeMeshUvData[i];
        }

        triangles = new int[tris.Length];
        for (int i = 0; i < tris.Length; i++)
        {
            triangles[i] = tris[i];
        }
        
        //Dispose All Native Containers
        nativeVertexData.Dispose();
        nativeBoardUvData.Dispose();
        nativeMeshUvData.Dispose();
        tris.Dispose();
        indexList.Dispose();
    }
    
    public void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = boardUVs;
        mesh.uv2 = meshUvs;
    }

    public override IObject OnPointerDown(Vector2 worldPos, int pointerId)
    {
        Debug.Log($"Pointer down on Puzzle Piece having coord {gridCoordinate}");
        return base.OnPointerDown(worldPos, pointerId);
    }

    public override IObject OnPointerDrag(Vector2 worldPos, int pointerId)
    {
        Debug.Log($"Pointer drag on puzzle piece having coord {gridCoordinate}");
        return base.OnPointerDrag(worldPos, pointerId);
    }

    protected override void OnSelected()
    {
        //Set Position On Pointer Down
        Position = Position.SetZ(InteractiveSystem.DRAG_Z_ORDER);
    }

    protected override void OnReleased()
    {
        Vector2 gridPos = PuzzleGenerator.Instance.PuzzleGrid.GetWorldPositionWithCellOffset(gridCoordinate.x, gridCoordinate.y);
        if (Vector2.Distance(gridPos, Position) < InteractiveSystem.SnapThreshold)
        {
            //disable Input by removing Isystem
            SetISystem(null);
            
            //set grid object
            var gridObject = PuzzleGenerator.Instance.PuzzleGrid.GetGridObject(gridCoordinate.x, gridCoordinate.y);
            gridObject.AssignTargetPuzzlePiece(this);
            
            transform.DOMove(gridPos, 0.14f).SetEase(Ease.OutQuad).onComplete += OnPuzzlePiecePlaced;
        }
    }

    private void OnPuzzlePiecePlaced()
    {
        //Play PopSound To Let Know Puzzle Piece is fitted
    }
}
