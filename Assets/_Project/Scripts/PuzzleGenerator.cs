using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using SimpleJSON;
using UnityEngine;
using Random = UnityEngine.Random;

public class PuzzleGenerator : MonoBehaviour
{
    [field: SerializeField] public int XWidth {get; private set;}
    [field: SerializeField] public int YWidth {get; private set;}
    [field: SerializeField] public float CellSize {get; private set;}

    [Space(25), Header("Puzzle Data")]
    public PuzzlePiece piecePrefab;
    [SerializeField] private IOGroupedPiece groupedPiecePrefab;
    public EdgeShapeSO edgeShapeSO;

    [Space(25), Header("Additional Data")] 
    [SerializeField] private SpriteRenderer border;
    [SerializeField] private Transform refImage;
    [SerializeField] private SpriteRenderer backGround;
    public int selectedBgIndex;
    public List<Sprite> backGroundSpriteOptions;

    public int TotalPieceCount => XWidth * YWidth;
    public Vector2Int GridSize => new Vector2Int(XWidth, YWidth);
    public Vector2 BoardSize => new Vector2(XWidth, YWidth) * CellSize;
    public Grid<GridObject> PuzzleGrid { get; private set; }
    public List<PuzzlePieceData> puzzlePieceDataSource;
    private Dictionary<int, PuzzlePieceData> puzzleBoardDataDict;
    public bool IsLevelCompleted { get; private set; }

    private InteractiveSystem iSystem;
    
    public static PuzzleGenerator Instance;

    private JSONNode boardConfigData;
    private JSONNode gridConfigData;

    private int totalPiecesCountNeededForCompletion;
    private int currAssignedPieceCount;
    private Stopwatch stopwatchTimer;
    private double initialPlayedTime;

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
    }

    public void SetGridSize(int xSize, int ySize)
    {
        XWidth = xSize;
        YWidth = ySize;
        
    }

    public async void UpdateBackGroundData(PuzzleTextureData data)
    {
        if (border)
        {
            border.gameObject.SetActive(true);
            border.size = new Vector2(XWidth, YWidth) * 2;
        }
        if (refImage)
        {
            if (data is CustomPuzzleTexData customPuzzleTexData)
                refImage.GetComponent<MeshRenderer>().material.mainTexture = customPuzzleTexData.customTexture;
            else
                refImage.GetComponent<MeshRenderer>().material.mainTexture = await AssetLoader.Instance.LoadAssetAsync<Texture2D>(data.textureKey);
            refImage.localScale = new Vector3(XWidth * 2, YWidth * 2, 1);
        }
    }

    public void UpdateBackgroundSprite(Sprite sprite)
    {
        backGround.sprite = sprite;
    }

    public async UniTask GenerateGrid(PuzzleTextureData textureData, JSONNode configDataNode = null)
    {
        //Assign Json Node Loaded From File
        boardConfigData = configDataNode;
        if (boardConfigData != null)
        {
            gridConfigData = boardConfigData[StringID.GridData];
            if (boardConfigData[StringID.TotalTime]) initialPlayedTime = boardConfigData[StringID.TotalTime];
        }

        puzzlePieceDataSource = new List<PuzzlePieceData>();
        puzzleBoardDataDict = new Dictionary<int, PuzzlePieceData>();
        totalPiecesCountNeededForCompletion = XWidth * YWidth;
        currAssignedPieceCount = 0;
        
        //Generate Grid with X and Y Size
        Vector2 origin = new Vector2(-XWidth, -YWidth) * (0.5f * CellSize);
        Vector2 cellOffset = Vector2.one * (CellSize * 0.5f);
        Texture2D texture;
        if (textureData is CustomPuzzleTexData customPuzzleTexData)
            texture = customPuzzleTexData.customTexture;
        else 
            texture = await AssetLoader.Instance.LoadAssetAsync<Texture2D>(textureData.textureKey);
        edgeShapeSO.UpdatePuzzleMaterial(texture,XWidth,YWidth,CellSize);
        PuzzleGrid = new Grid<GridObject>(XWidth, YWidth, CellSize, origin, cellOffset,OnGridObjectCreated);

        //Load Puzzle Pieces From Json If Present
        if (boardConfigData != null && boardConfigData[StringID.PalettePieces] != null)
        {
            JSONNode palettePieceNode = boardConfigData[StringID.PalettePieces];
            int palettePiecesCount = palettePieceNode.Count;
            for (int i = 0; i < palettePiecesCount; i++)
            {
                puzzlePieceDataSource.Add(puzzleBoardDataDict[palettePieceNode[i]]);
            }
        }
        else
            puzzlePieceDataSource = puzzleBoardDataDict.Values.ToList();

        IsLevelCompleted = false;
        stopwatchTimer = new Stopwatch();
    }

    private GridObject OnGridObjectCreated(Grid<GridObject> grid, int x, int y)
    {
        GridObject gridObject = new GridObject(grid, x, y);
        gridObject.OnCorrectPuzzlePieceAssigned += UpdateAssignedPiecesCount;
        
        //Choose all 4 EdgeType for Puzzle Piece
        if (gridConfigData != null)
            gridObject.desiredEdgeShape = gridConfigData[Utilities.ConvertTo1DIndex(x, y, XWidth).ToString()];
        else
            gridObject.desiredEdgeShape = CalculateEdges(grid, x, y);

        Vector2Int gridCoordinate = new Vector2Int(x, y);
        var pieceData = new PuzzlePieceData
        {
            gridCoordinate = gridCoordinate,
            meshData = edgeShapeSO.GetMeshData(gridObject.desiredEdgeShape),
            neighbourCoordinates = new Dictionary<int, Vector2Int>
            {
                { 0, new(gridCoordinate.x - 1, gridCoordinate.y) },
                { 1, new(gridCoordinate.x, gridCoordinate.y + 1) },
                { 2, new(gridCoordinate.x + 1, gridCoordinate.y) },
                { 3, new(gridCoordinate.x, gridCoordinate.y - 1) }
            },
            isCornerPiece = x == 0 || y == 0 || x == XWidth - 1 || y == YWidth - 1,
        };
        // puzzlePieceDataSource.Add(pieceData);
        puzzleBoardDataDict[Utilities.ConvertTo1DIndex(x, y, XWidth)] = pieceData;
        return gridObject;
    }

    #region Helper Methods

    private string CalculateEdges(Grid<GridObject> grid ,int x, int y)
    {
        //Here Edge Shape is
        // 0 = left, 1 = top, 2 = right, 3 = bottom
        //EdgeType is
        // 0 = flat, 1 = knob, 2 = socket
        char left = x == 0 ? '0' : GetComplimentaryEdge(grid.GetGridObject(x - 1, y).desiredEdgeShape[2]);
        char top = (y == YWidth - 1) ? '0' : GetRandomEdge();
        char right = (x == XWidth - 1) ? '0' : GetRandomEdge();
        char bottom = y == 0 ? '0' : GetComplimentaryEdge(grid.GetGridObject(x, y - 1).desiredEdgeShape[1]);

        // return "0000";
        return $"{left}{top}{right}{bottom}";
    }

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

    
    #endregion

    public IOGroupedPiece GetPuzzlePiecesGroup(Vector3 pos)
    {
        IOGroupedPiece group = Instantiate(groupedPiecePrefab, pos, Quaternion.identity, iSystem.transform);
        group.SetISystem(iSystem);
        return group;
    }

    public PuzzlePieceData GetPuzzlePieceData(int index)
    {
        return puzzleBoardDataDict[index];
    }

    public JSONNode ToJson(JSONNode node = null)
    {
        if (node == null)
            node = new JSONObject();

        //Board Data Saving
        //Save All Board Pieces With key as 1D Index and Value as Edge Profile
        JSONNode boardPiecesJsonNode = new JSONObject();
        foreach (var boardData in puzzleBoardDataDict)
        {
            boardPiecesJsonNode[boardData.Key.ToString()] = boardData.Value.meshData.edgeProfile;
        }
        
        //Palette Saving
        //Save 1D index as pieces in palette
        JSONArray paletteChildren = new JSONArray();
        foreach (var pieceData in puzzlePieceDataSource)
        {
            paletteChildren.Add(Utilities.ConvertTo1DIndex(pieceData.gridCoordinate.x, pieceData.gridCoordinate.y, XWidth).ToString());
        }

        //Solved Puzzle Saving
        //Save Solved Puzzle Pieces as 1D Index
        JSONArray solvedPieces = new JSONArray();
        PuzzleGrid.IterateOverGridObjects((x, y, gridObj) =>
        {
            if (gridObj.targetPuzzlePiece != null)
            {
                solvedPieces.Add(Utilities.ConvertTo1DIndex(x, y, XWidth).ToString());
            }
        });
        
        //Unsolved Puzzle Saving
        //Save UnSolved Puzzle Pieces with 1D Index and Position
        JSONArray children = new JSONArray();
        SerializeChildren(children);
        
        node[StringID.GridData] = boardPiecesJsonNode;
        node[StringID.PalettePieces] = paletteChildren;
        node[StringID.SolvedPieces] = solvedPieces;
        node[StringID.UnSolvedPieces] = children;
        node[StringID.BoardSize] = XWidth * YWidth;
        node[StringID.BackGroundID] = backGroundSpriteOptions.IndexOf(backGround.sprite);
        node[StringID.TotalTime] = StopTimer();
        return node;
    }

    public IEnumerator FromJson(JSONNode node)
    {
        if(node == null)
            yield break;

        int solvedPiecesCount = node[StringID.SolvedPieces].Count;
        JSONNode solvedPiecesJsonNode = node[StringID.SolvedPieces];
        for (int i = 0; i < solvedPiecesCount; i++)
        {
            var data = puzzleBoardDataDict[solvedPiecesJsonNode[i]];
            var solvedPiece = Instantiate(piecePrefab, transform);
            solvedPiece.SetData();
            solvedPiece.UpdateData(data);
            solvedPiece.Position = PuzzleGrid.GetWorldPositionWithCellOffset(data.gridCoordinate.x, data.gridCoordinate.y);
            
            var gridObject = PuzzleGrid.GetGridObject(data.gridCoordinate.x, data.gridCoordinate.y);
            gridObject.desiredPuzzlePiece = solvedPiece;
            gridObject.AssignTargetPuzzlePiece(solvedPiece);
        }

        yield return null;
        
        int unSolvedPiecesCount = node[StringID.UnSolvedPieces].Count;
        JSONNode unSolvedPiecesJsonNode = node[StringID.UnSolvedPieces];
        for (int i = 0; i < unSolvedPiecesCount; i++)
        {
            JSONNode childNode = unSolvedPiecesJsonNode[i];
            if (childNode[StringID.GroupedPiece])
            {
                var groupedPiece = GetPuzzlePiecesGroup(childNode[StringID.Position]);
                groupedPiece.FromJson(childNode);
            }
            else
            {
                var unSolvedPiece = Instantiate(piecePrefab, iSystem.transform);
                unSolvedPiece.SetISystem(iSystem);
                unSolvedPiece.FromJson(childNode);
            }
        }

        selectedBgIndex = Mathf.Clamp(node[StringID.BackGroundID],0,backGroundSpriteOptions.Count);
    }
    
    private void SerializeChildren(JSONArray children)
    {
        var unSolvedPieces = iSystem.iObjects.Values;
        foreach (IObject obj in unSolvedPieces)
        {
            JSONNode node = null;
            if (((obj is PuzzlePiece piece && piece.group == null) || obj is IOGroupedPiece) && obj.parent is not PuzzlePalette)
            {
                node = obj.ToJson();
            }
            if(node != null) children.Add(node);
        }
    }

    #region Helper Methods

    private void UpdateAssignedPiecesCount()
    {
        currAssignedPieceCount++;
        CheckForLevelCompletion();
    }

    private void CheckForLevelCompletion()
    {
        if (currAssignedPieceCount < totalPiecesCountNeededForCompletion) return;
        IsLevelCompleted = true;
        iSystem.OnLevelCompleted();
    }

    public Tween FadeEdgesOnLevelComplete()
    {
        float strengthValue = 0;
        Tween tween = DOTween.To(x => { strengthValue = x; }, EdgeShapeSO.normalStrengthValue, 0f, 1.25f);
        tween.SetDelay(0.4f);
        tween.onUpdate += () => Shader.SetGlobalFloat(EdgeShapeSO.NormalStrength,strengthValue);
        return tween;
    }

    public void ToggleReferenceImage(bool value)
    {
        refImage.gameObject.SetActive(value);
    }

    public void StartTimer()
    {
        stopwatchTimer.Reset();
        stopwatchTimer.Start();
    }

    public double StopTimer()
    {
        if (!stopwatchTimer.IsRunning) return initialPlayedTime;
        
        stopwatchTimer.Stop();
        return initialPlayedTime + stopwatchTimer.Elapsed.TotalSeconds;
    }

    #endregion
}

public class PuzzlePieceData
{
    public Vector2Int gridCoordinate;
    public MeshData meshData;
    public Dictionary<int,Vector2Int> neighbourCoordinates;
    public bool isCornerPiece;
}
