using System.Collections.Generic;
using DG.Tweening;
using SimpleJSON;
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

    public PuzzlePieceData currentAssignedPieceData;
    public string EdgeShape { get; private set; }
    

    //Mesh Data
    private Mesh mesh;
    private static readonly int GridCoord = Shader.PropertyToID("_GridCoord");

    public void SetData()
    {
        mesh = new Mesh();
        meshFilter.mesh = mesh;
    }
    
    public void UpdateData(PuzzlePieceData puzzlePieceData)
    {
        currentAssignedPieceData = puzzlePieceData;
        gridCoordinate = puzzlePieceData.gridCoordinate;
        mesh.Clear();
        
        EdgeShape = puzzlePieceData.meshData.edgeProfile;
        
        mesh.vertices = puzzlePieceData.meshData.vertices;
        mesh.triangles = puzzlePieceData.meshData.triangles; 
        mesh.uv = puzzlePieceData.meshData.uvs;

        meshRenderer.material.SetVector(GridCoord,(Vector2)gridCoordinate);

        neighbourCoordinates = puzzlePieceData.neighbourCoordinates;
    }

    public override IObject OnPointerDown(Vector2 worldPos, int pointerId)
    {
        if (group)
        {
            return group.isMoving ? null : group.OnPointerDown(worldPos, pointerId);
        }
        return base.OnPointerDown(worldPos, pointerId);
    }

    protected override void OnReleased()
    {
        if (TryInteractWithPalette())
        {
            return;
        }
        
        var gridObj = PuzzleGenerator.Instance.PuzzleGrid.GetGridObject(gridCoordinate.x, gridCoordinate.y);
        gridObj.desiredPuzzlePiece = this;
        
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
                    out GridObject gridObject) || gridObject.desiredPuzzlePiece == null)
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

        Vector3 newPos = GetPositionInsideLimit(Position.SetZ(z));
        
        MainCollider.enabled = false;
        transform.DOMove(newPos, 0.2f).onComplete += () =>
        {
            MainCollider.enabled = true;
        };
    }

    public Vector3 GetPositionInsideLimit(Vector3 pos)
    {
        pos.x = Mathf.Clamp(pos.x, iSystem.LeftLimit + Width * 0.5f, iSystem.RightLimit - Width * 0.5f);
        pos.y = Mathf.Clamp(pos.y, iSystem.BottomLimit + Height * 0.5f, iSystem.TopLimit - Height * 0.5f);
    
        return pos;
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
        node[StringID.GridIndex] = gridCoordinate.x + gridCoordinate.y * PuzzleGenerator.Instance.PuzzleGrid.Width;
        
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
        node[StringID.RemovedNeighbourCoord] = neighbours;
        
        return node;
    }

    public override void FromJson(JSONNode node)
    {
        base.FromJson(node);
        if(node == null) return;
        var pieceData = PuzzleGenerator.Instance.GetPuzzlePieceData(node[StringID.GridIndex]);
        SetData();
        UpdateData(pieceData);
        var gridObj = PuzzleGenerator.Instance.PuzzleGrid.GetGridObject(gridCoordinate.x, gridCoordinate.y);
        gridObj.desiredPuzzlePiece = this;
        
        JSONArray neighbours = node[StringID.RemovedNeighbourCoord] as JSONArray;
        foreach (var childrenKeyValue in neighbours)
        {
            neighbourCoordinates.Remove(childrenKeyValue.Value);
        }
    }
    
#if UNITY_EDITOR
    public void SetMeshData(MeshData data)
    {
        mesh = new Mesh();
        meshFilter.mesh = mesh;
        
        mesh.Clear();
        mesh.vertices = data.vertices;
        mesh.triangles = data.triangles;
        mesh.uv = data.uvs;
    }

#endif
}
