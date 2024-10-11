using SimpleJSON;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class PuzzleGenerator : MonoBehaviour
{
    [field: SerializeField] public int XWidth {get; private set;}
    [field: SerializeField] public int YWidth {get; private set;}
    [field: SerializeField] public float CellSize {get; private set;}

    [Space(25), Header("Puzzle Data")]
    [SerializeField] private PuzzlePiece piecePrefab;
    [SerializeField] private IOGroupedPiece groupedPiecePrefab;
    public EdgeShapeSO edgeShapeSO;

    [Space(25), Header("Additional Data")] 
    [SerializeField] private SpriteRenderer border;
    [SerializeField] private Transform refImage;

    public Vector2Int GridSize => new Vector2Int(XWidth, YWidth);
    public Vector2 BoardSize => new Vector2(XWidth, YWidth) * CellSize;
    public Grid<GridObject> PuzzleGrid { get; private set; }

    private InteractiveSystem iSystem;

    [HideInInspector] public float2 puzzleBoardMinPos;
    [HideInInspector] public float2 puzzleBoardMaxPos;
    
    public static PuzzleGenerator Instance;

    private JSONNode configData;

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
        
        
        edgeShapeSO.Init();
        
        if (border)
        {
            border.gameObject.SetActive(true);
            border.size = new Vector2(XWidth, YWidth) * 2/* + new Vector2(0.27f, 0.27f)*/;
        }
        
        UpdateRefImage();
    }

    // public void GenerateGrid(int xSize, int ySize)
    // {
    //     xWidth = xSize;
    //     yWidth = ySize;
    //     GenerateGrid();
    // }

    public void GenerateGrid(PuzzleTextureData textureData, JSONNode configDataNode = null)
    {
        configData = configDataNode;
        
        //Generate Grid with X and Y Size
        Vector2 origin = new Vector2(-XWidth, -YWidth) * (0.5f * CellSize);
        Vector2 cellOffset = Vector2.one * (CellSize * 0.5f);
        puzzleBoardMinPos = origin;
        puzzleBoardMaxPos = origin + (new Vector2(XWidth, YWidth) * CellSize);
        
        edgeShapeSO.UpdatePuzzleMaterial(textureData.sprite.texture,XWidth,YWidth,CellSize);
        PuzzleGrid = new Grid<GridObject>(XWidth, YWidth, CellSize, origin, cellOffset,OnGridObjectCreated);
    }

    private GridObject OnGridObjectCreated(Grid<GridObject> grid, int x, int y)
    {
        GridObject gridObject = new GridObject(grid, x, y);
        Vector3 puzzlePos = grid.GetWorldPosition(x, y) + grid.cellOffset;
        gridObject.desiredPuzzlePiece = Instantiate(piecePrefab, puzzlePos, Quaternion.identity,iSystem.transform);
        
        //Init Puzzle Piece
        gridObject.desiredPuzzlePiece.SetISystem(iSystem);
        gridObject.desiredPuzzlePiece.SetData(CellSize,x,y);
        
        //Choose all 4 EdgeType for Puzzle Piece
        // if(configData != null)
        //     GetEdges(configData,gridObject.desiredPuzzlePiece,x,y);
        // else
        gridObject.desiredPuzzlePiece.UpdateMesh(CalculateEdges(gridObject.desiredPuzzlePiece, grid ,x, y));
        
        return gridObject;
    }

    #region Helper Methods

    private string CalculateEdges(PuzzlePiece puzzlePiece,Grid<GridObject> grid ,int x, int y)
    {
        //Here Edge Shape is
        // 0 = left, 1 = top, 2 = right, 3 = bottom
        //EdgeType is
        // 0 = flat, 1 = knob, 2 = socket
        char left = x == 0 ? '0' : GetComplimentaryEdge(grid.GetGridObject(x - 1, y).desiredPuzzlePiece.EdgeShape[2]);
        char top = (y == YWidth - 1) ? '0' : GetRandomEdge();
        char right = (x == XWidth - 1) ? '0' : GetRandomEdge();
        char bottom = y == 0 ? '0' : GetComplimentaryEdge(grid.GetGridObject(x, y - 1).desiredPuzzlePiece.EdgeShape[1]);

        // return "0000";
        return $"{left}{top}{right}{bottom}";
    }
    
    // private void GetEdges(JSONNode dataNode, PuzzlePiece puzzlePiece ,int x, int y)
    // {
    //     JSONNode node = dataNode[(x + y * xWidth).ToString()];
    //     puzzlePiece.leftEdge = (EdgeType)(int)node["Left"];
    //     puzzlePiece.topEdge = (EdgeType)(int)node["Top"];
    //     puzzlePiece.rightEdge = (EdgeType)(int)node["Right"];
    //     puzzlePiece.bottomEdge = (EdgeType)(int)node["Bottom"];
    // }

    private EdgeType GetComplimentaryEdgeType(EdgeType edgeType)
    {
        return edgeType == EdgeType.Knob ? EdgeType.Socket : EdgeType.Knob;
    }
    
    private char GetComplimentaryEdge(char edgeType)
    {
        return edgeType == '1' ? '2' : '1';
    }
    
    char GetRandomEdge()
    {
        return Random.Range(0f,1f) < 0.5f ? '1' : '2'; // Randomly returns Knob or Socket
    }
    
    EdgeType GetRandomEdgeType()
    {
        return (EdgeType)Random.Range(1, 3); // Randomly returns Knob or Socket
    }

    private void UpdateRefImage()
    {
        if (refImage)
        {
            refImage.localScale = new Vector3(XWidth * 2, YWidth * 2, 1);
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

    public IOGroupedPiece GetPuzzlePiecesGroup(Vector3 pos)
    {
        IOGroupedPiece group = Instantiate(groupedPiecePrefab, pos, Quaternion.identity, iSystem.transform);
        group.SetISystem(iSystem);
        return group;
    }

    public JSONNode ToJson(JSONNode node = null)
    {
        if (node == null)
            node = new JSONObject();

        JSONArray children = new JSONArray();

        int i = 0;
        PuzzleGrid.IterateOverGridObjects((x, y, gridObj) =>
        {
            if (gridObj.targetPuzzlePiece != null)
            {
                children[i] = gridObj.targetPuzzlePiece.ToJson();
                i++;
            }
        });
        
        node["children"] = children;

        return node;
    }

    public void FromJson(JSONNode node)
    {
        if(node == null)
            return;

        JSONArray children = node["children"] as JSONArray;
        if(children == null) return;
        foreach (var childNode in children)
        {
            int index = childNode.Value["Index"];
            GridObject gridObject = PuzzleGrid.GetGridObject(index);
            if (gridObject != null)
            {
                gridObject.AssignTargetPuzzlePiece(gridObject.desiredPuzzlePiece);
            }
        }
    }
    
    
    public JSONNode GetAllPuzzlePieceNode()
    {
        JSONNode node = new JSONObject();
        
        PuzzleGrid.IterateHorizontallyOverGridObjects((x, y, gridObj) =>
        {
            node[(x + y * XWidth).ToString()] = gridObj.desiredPuzzlePiece.EdgeTypeToJson();
        });
        return node;
    }
}