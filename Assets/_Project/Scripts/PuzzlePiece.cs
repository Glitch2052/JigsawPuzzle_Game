using System.Collections.Generic;
using DG.Tweening;
using SimpleJSON;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class PuzzlePiece : IObject
{
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private MeshRenderer meshRenderer;

    [HideInInspector] public IOGroupedPiece group;
    [HideInInspector] public Vector2Int gridCoordinate;
    public Dictionary<int,Vector2Int> neighbourCoordinates;
    private Collider2D[] colliderResults;
    public ContactFilter2D contactFilter2D;

    [Space(20),Header("EdgeInfo")]
    public EdgeType leftEdge;
    public EdgeType topEdge;
    public EdgeType rightEdge;
    public EdgeType bottomEdge;

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

    // public override void Init()
    // {
    //     colliderResults = new RaycastHit2D[10];
    // }

    public void SetData(float cellSize, int x, int y)
    {
        mesh = new Mesh();
        meshFilter.mesh = mesh;

        MainCollider.size = Vector2.one * cellSize;
        gridCoordinate = new Vector2Int(x, y);
        
        //Get Neighbouring Grid Coordinates
        GetNeighbouringPieceGridCoordinates();
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

    private void GetNeighbouringPieceGridCoordinates()
    {
        neighbourCoordinates = new Dictionary<int, Vector2Int>
        {
            {0, new (gridCoordinate.x - 1, gridCoordinate.y)}, 
            {1, new (gridCoordinate.x, gridCoordinate.y + 1)},
            {2, new (gridCoordinate.x + 1, gridCoordinate.y)},
            {3, new (gridCoordinate.x, gridCoordinate.y - 1)}
        };
    }

    public override IObject OnPointerDown(Vector2 worldPos, int pointerId)
    {
        if (group)
        {
            return group.isMoving ? null : group.OnPointerDown(worldPos, pointerId);
        }
        return base.OnPointerDown(worldPos, pointerId);
    }

    public override IObject OnPointerDrag(Vector2 worldPos, int pointerId)
    {
        Debug.Log($"Pointer drag on puzzle piece having coord {gridCoordinate}");
        return base.OnPointerDrag(worldPos, pointerId);
    }

    protected override void OnSelected()
    {
        base.OnSelected();
    }

    protected override void OnReleased()
    {
        if (TryInteractWithPalette())
        {
            return;
        }

        // base.OnReleased();

        //Try Placement With Puzzle Board
        Vector2 gridPos = PuzzleGenerator.Instance.PuzzleGrid.GetWorldPositionWithCellOffset(gridCoordinate.x, gridCoordinate.y);
        if (Vector2.Distance(gridPos, Position) < InteractiveSystem.gridSnapThreshold)
        {
            //disable Input by removing Isystem
            SetISystem(null);
            
            //set grid object
            var gridObject = PuzzleGenerator.Instance.PuzzleGrid.GetGridObject(gridCoordinate.x, gridCoordinate.y);
            gridObject.AssignTargetPuzzlePiece(this);
            
            transform.DOMove(gridPos, 0.14f).SetEase(Ease.OutQuad).onComplete += OnPuzzlePiecePlacedOnBoard;
            return;
        }

        //Try Placement With Neighbouring Puzzle Piece
        foreach (KeyValuePair<int, Vector2Int> neighbourKeyValue in neighbourCoordinates)
        {
            int sideIndex = neighbourKeyValue.Key;
            Vector2Int neighbourGridPos = neighbourKeyValue.Value;

            if (!PuzzleGenerator.Instance.PuzzleGrid.GetGridObject(neighbourGridPos.x, neighbourGridPos.y,
                    out GridObject gridObject))
                continue;

            Vector2 requiredDir = Quaternion.Euler(0, 0, -90 * sideIndex) * Vector2.left;
            PuzzlePiece neighbour = gridObject.desiredPuzzlePiece;
            
            if(neighbour.parent is PuzzlePalette) continue;
            
            if (IsNeighbourWithinRange(neighbour, requiredDir))
            {
                SetISystem(null);
                neighbourCoordinates.Remove(sideIndex);
                neighbour.neighbourCoordinates.Remove(GetOppositeSideIndex(sideIndex));

                //Add It To Group
                //Case 1 : neighbour and this is not in a group
                if (neighbour.group == null && group == null)
                {
                    var newGroup = PuzzleGenerator.Instance.GetPuzzlePiecesGroup(neighbour.Position);
                    newGroup.AddPuzzlePieceToGroup(neighbour);
                    newGroup.AddPuzzlePieceToGroup(this);
                    transform.DOLocalMove(
                            (Vector2)neighbour.LocalPosition -
                            requiredDir.normalized * PuzzleGenerator.Instance.CellSize, 0.14f)
                        .SetEase(Ease.OutQuad).onComplete += OnPuzzlePiecePlacedWithNeighbour;
                }

                //Case 2 : neighbour is in a group and this is not in a group
                if (neighbour.group != null && group == null)
                {
                    var newGroup = neighbour.group;
                    newGroup.AddPuzzlePieceToGroup(this);
                    transform.DOLocalMove(
                            (Vector2)neighbour.LocalPosition -
                            requiredDir.normalized * PuzzleGenerator.Instance.CellSize, 0.14f)
                        .SetEase(Ease.OutQuad).onComplete += OnPuzzlePiecePlacedWithNeighbour;
                }

                return;
            }
        }

        float z = 0;
        IObject piece = GetBelowPuzzlePiece();
        if(piece)
            z = piece.Position.z - 0.05f;
        
        Position = Position.SetZ(z);
    }
    
    private IObject GetBelowPuzzlePiece()
    {
        colliderResults ??= new Collider2D[10];
        int hitCount = MainCollider.OverlapCollider(contactFilter2D, colliderResults);
        SortColliders(hitCount, colliderResults);
        
        Collider2D puzzleCollider = colliderResults[0];
        if (!puzzleCollider) return null;
        
        if (puzzleCollider.TryGetComponent(out PuzzlePiece puzzlePiece))
            return puzzlePiece;
        if (puzzleCollider.TryGetComponent(out IOGroupedPiece groupedPiece))
            return groupedPiece;
        return null;
    }

    private bool TryInteractWithPalette()
    {
        // colliderResults ??= new RaycastHit2D[10];
        //
        // int count = Physics2D.BoxCastNonAlloc(Position, MainCollider.size, 0, Vector2.zero,colliderResults,15f);
        // for (int i = 0; i < count; i++)
        // {
        //     if (colliderResults[i].collider.TryGetComponent(out PuzzlePalette palette))
        //     {
        //         palette.AddObjectToPalette(this);
        //         Debug.Log("Palette Found");
        //         return true;
        //     }
        // }
        if (Mathf.Abs(Position.y - iSystem.palette.Position.y) < iSystem.palette.PaletteHeight * 0.5)
        {
            iSystem.palette.AddObjectToPalette(this);
            return true;
        }
        return false;
    }

    private void OnPuzzlePiecePlacedOnBoard()
    {
        transform.parent = PuzzleGenerator.Instance.transform;
        //Play PopSound To Let Know Puzzle Piece is fitted
    }

    private void OnPuzzlePiecePlacedWithNeighbour()
    {
        SetISystem(group.iSystem);
    }

    private bool IsNeighbourWithinRange(PuzzlePiece neighbour, Vector2 dirRef)
    {
        float distBetweenPiece = Vector2.Distance(neighbour.Position, Position);
        bool isWithinRange = distBetweenPiece > InteractiveSystem.gridSnapThreshold && distBetweenPiece < InteractiveSystem.neighbourSnapThreshold;
        bool isAligned = Vector2.Angle(dirRef, neighbour.Position - Position) < 30;

        return isWithinRange && isAligned;
    }

    // 0 => returns 2
    // 1 => returns 3
    // 2 => returns 0
    // 3 => returns 1
    public static int GetOppositeSideIndex(int index)
    {
        return (index + 2) % 4;
    }

    public override JSONNode ToJson(JSONNode node = null)
    {
        node = base.ToJson(node);
        node["Index"] = gridCoordinate.x + gridCoordinate.y * PuzzleGenerator.Instance.PuzzleGrid.Width;
        
        JSONArray neighbours = new JSONArray();
        List<int> neighboursKeys = new List<int>{ 0, 1, 2, 3 };
        int count = neighboursKeys.Count;
        for (int i = 0; i < count; i++)
        {
            if (neighbourCoordinates.ContainsKey(i))
            {
                neighboursKeys.Remove(i);
            }
        }
        foreach (int remainingKey  in neighboursKeys)
        {
            neighbours.Add(remainingKey);
        }
        node["RemovedNeighbourCoord"] = neighbours;
        
        return node;
    }

    public override void FromJson(JSONNode node)
    {
        base.FromJson(node);
        
        if(node == null) return;

        JSONArray neighbours = node["RemovedNeighbourCoord"] as JSONArray;
        foreach (var childrenKeyValue in neighbours)
        {
            neighbourCoordinates.Remove(childrenKeyValue.Value);
        }
    }

    public JSONNode EdgeTypeToJson()
    {
        JSONNode edgeNode = new JSONObject();
        edgeNode["Left"] = (int)leftEdge;
        edgeNode["Top"] = (int)topEdge;
        edgeNode["Right"] = (int)rightEdge;
        edgeNode["Bottom"] = (int)bottomEdge;
        return edgeNode;
    }
}
