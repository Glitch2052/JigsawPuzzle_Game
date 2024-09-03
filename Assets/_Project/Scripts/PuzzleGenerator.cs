using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class PuzzleGenerator : MonoBehaviour
{
    [Header("Grid Data")]
    [SerializeField] private int xWidth;
    [SerializeField] private int yWidth;
    [SerializeField] private float cellSize = 2f;

    [Space(25), Header("Puzzle Data")]
    [SerializeField] private PuzzlePiece piecePrefab;
    [SerializeField] private IOGroupedPiece groupedPiecePrefab;
    public EdgeShapeSO edgeShapeSO;

    [Space(25), Header("Additional Data")] 
    [SerializeField] private SpriteRenderer border;
    [SerializeField] private Transform refImage;

    public float CellSize => cellSize;
    public Grid<GridObject> PuzzleGrid { get; private set; }

    private InteractiveSystem iSystem;
    private NativeList<JobHandle> meshGenerationJobHandles;

    [HideInInspector] public float2 puzzleBoardMinPos;
    [HideInInspector] public float2 puzzleBoardMaxPos;
    
    public static PuzzleGenerator Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void Init(InteractiveSystem interactiveSystem)
    {
        iSystem = interactiveSystem;
        edgeShapeSO.Init(cellSize);
        edgeShapeSO.EvaluateAllPossibleSplinesCombinationOnMainThread();

        if (border)
        {
            border.gameObject.SetActive(true);
            border.size = new Vector2(xWidth, yWidth) * 2 + new Vector2(0.27f, 0.27f);
        }
        
        UpdateRefImage();
    }

    public void GenerateGrid(int xSize, int ySize)
    {
        xWidth = xSize;
        yWidth = ySize;
        GenerateGrid();
    }

    public void GenerateGrid()
    {
        meshGenerationJobHandles = new NativeList<JobHandle>(Allocator.Temp);
        
        //Generate Grid with X and Y Size
        Vector2 origin = new Vector2(-xWidth, -yWidth) * 0.5f * cellSize;
        Vector2 cellOffset = Vector2.one * cellSize * 0.5f;
        puzzleBoardMinPos = origin;
        puzzleBoardMaxPos = origin + (new Vector2(xWidth, yWidth) * cellSize);
        
        PuzzleGrid = new Grid<GridObject>(xWidth, yWidth, cellSize, origin, cellOffset,OnGridObjectCreated);
        
        JobHandle.CompleteAll(meshGenerationJobHandles);
        meshGenerationJobHandles.Dispose();
        
        PuzzleGrid.IterateOverGridObjects((x, y, gridObj) =>
        {
            gridObj.desiredPuzzlePiece.GetMeshDataFromJob();
            gridObj.desiredPuzzlePiece.UpdateMesh();
        });
    }

    private GridObject OnGridObjectCreated(Grid<GridObject> grid, int x, int y)
    {
        GridObject gridObject = new GridObject(grid, x, y);
        Vector3 puzzlePos = grid.GetWorldPosition(x, y) + grid.cellOffset;
        gridObject.desiredPuzzlePiece = Instantiate(piecePrefab, puzzlePos, Quaternion.identity,iSystem.transform);
        
        //Init Puzzle Piece
        gridObject.desiredPuzzlePiece.SetISystem(iSystem);
        gridObject.desiredPuzzlePiece.SetData(cellSize,x,y);
        
        //Choose all 4 EdgeType for Puzzle Piece
        CalculateEdges(gridObject.desiredPuzzlePiece, grid ,x, y);
        
        //Generate Mesh
        meshGenerationJobHandles.Add(gridObject.desiredPuzzlePiece.ScheduleMeshDataGenerationJob());
        
        return gridObject;
    }

    #region Helper Methods

    private void CalculateEdges(PuzzlePiece puzzlePiece,Grid<GridObject> grid ,int x, int y)
    {
        puzzlePiece.leftEdge = x == 0 ? EdgeType.Flat : GetComplimentaryEdgeType(grid.GetGridObject(x - 1, y).desiredPuzzlePiece.rightEdge);
        puzzlePiece.rightEdge = (x == xWidth - 1) ? EdgeType.Flat : GetRandomEdgeType();
        puzzlePiece.bottomEdge = y == 0 ? EdgeType.Flat : GetComplimentaryEdgeType(grid.GetGridObject(x, y - 1).desiredPuzzlePiece.topEdge);
        puzzlePiece.topEdge = (y == yWidth - 1) ? EdgeType.Flat : GetRandomEdgeType();
    }

    private EdgeType GetComplimentaryEdgeType(EdgeType edgeType)
    {
        return edgeType == EdgeType.Knob ? EdgeType.Socket : EdgeType.Knob;
    }
    
    EdgeType GetRandomEdgeType()
    {
        return (EdgeType)Random.Range(1, 3); // Randomly returns Knob or Socket
    }

    private void UpdateRefImage()
    {
        if (refImage)
        {
            refImage.localScale = new Vector3(xWidth * 2, yWidth * 2, 1);
        }
    }
    #endregion

    public IOGroupedPiece AddPuzzlePieceToGroup(PuzzlePiece puzzlePiece, IOGroupedPiece group = null)
    {
        if (group == null)
        {
            group = Instantiate(groupedPiecePrefab, puzzlePiece.Position, Quaternion.identity, iSystem.transform);
            group.SetISystem(iSystem);
        }

        puzzlePiece.group = group;
        puzzlePiece.transform.parent = group.transform;
        return group;
    }
}