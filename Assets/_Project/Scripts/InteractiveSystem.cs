using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(BoxCollider2D))]
public class InteractiveSystem : MonoBehaviour, IPointerDownHandler,IDragHandler,IPointerUpHandler
{
    public PuzzleGenerator puzzleGenerator;
    public PuzzlePalette palette;
    public CameraController cameraController;
    
    public SortedDictionary<int, IObject> iObjects;
    public SortedDictionary<int, IObject> touchColliders;

    public InputSystem inputSystem;
    
    public Vector2 cameraSize;
    public Vector2 halfCameraSize;
    protected Vector3 cameraPosition;
    public Camera Camera { get; protected set; }
    
    public static readonly float DRAG_Z_ORDER = -145f;
    public static float gridSnapThreshold;
    public static float neighbourSnapThreshold;

    public float LeftLimit { get; set;}
    public float RightLimit { get; set;}
    public float TopLimit { get; set;}
    public float BottomLimit { get; set;}
    
#if UNITY_EDITOR
    [Space(30),Header("Editor Only Data")]
    public Sprite puzzleSprite;
#endif

    private void Awake()
    {
        Camera = Camera.main;

        iObjects = new SortedDictionary<int, IObject>();
        touchColliders = new SortedDictionary<int, IObject>();
        
        float orthographicSize = Camera.orthographicSize;
        cameraSize.x = orthographicSize * Screen.width / Screen.height;
        cameraSize.y = orthographicSize;

        halfCameraSize = cameraSize;
        cameraSize *= 2;
        
// #if UNITY_EDITOR
//         Init(new PuzzleTextureData()
//         {
//             sprite = puzzleSprite
//         });
//         StartCoroutine(OnSceneLoad());
// #endif
    }

    private void Update()
    {
        foreach (IObject iObject in iObjects.Values)
        {
            iObject.IUpdate();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            SaveAll(ExportJson());
        }
    }

    public void Init()
    {
        inputSystem = new InputSystem(this);

        gridSnapThreshold = puzzleGenerator.CellSize * 0.5f;
        neighbourSnapThreshold = puzzleGenerator.CellSize * 1.5f;
        puzzleGenerator.Init(this);
    }

    public IEnumerator OnSceneLoad(PuzzleTextureData puzzleTextureData)
    {
        ReadOperation readOperation = new ReadOperation("Puzzle.json", "{items}");
        yield return readOperation.Read();
        JSONNode configData = JSONNode.Parse(readOperation.data);

        puzzleGenerator.GenerateGrid(puzzleTextureData, configData["PuzzlePieceData"]);
        yield return null;
        
        cameraController.SetISystem(this);
        palette.SetISystem(this);

        //From Json
        puzzleGenerator.FromJson(configData["grid"]);
        palette.SetUpContentData(configData["palette"]);

        JSONNode itemConfigData = configData["items"];
        if(itemConfigData == null) yield break;

        for (int i = 0; i < itemConfigData.Count; i++)
        {
            JSONNode childNode = itemConfigData[i];
            if (childNode["GroupedPiece"])
            {
                var groupedPiece = puzzleGenerator.GetPuzzlePiecesGroup(childNode["pos"]);
                groupedPiece.FromJson(childNode);
            }
            else
            {
                int index = childNode["Index"];
                GridObject gridObject = puzzleGenerator.PuzzleGrid.GetGridObject(index);
                if (gridObject != null)
                {
                    gridObject.desiredPuzzlePiece.FromJson(childNode);
                }
            }
        }
    }

    public void UpdateCameraSize()
    {
        float orthographicSize = Camera.orthographicSize;
        cameraSize.x = orthographicSize * Camera.aspect;
        cameraSize.y = orthographicSize;

        halfCameraSize = cameraSize;
        cameraSize *= 2;
    }
    
    public void RegisterIEntity(IObject iEntity)
    {
        int instanceID = iEntity.GetInstanceID();
        if (!iObjects.ContainsKey(instanceID))
        {
            iEntity.Init();
            iObjects.Add(instanceID, iEntity);
            iEntity.OnRegistered();
        }
    }

    public void UnRegisterIEntity(IObject iEntity)
    {
        int instanceID = iEntity.GetInstanceID();
        if (iObjects.ContainsKey(instanceID))
        {
            iObjects.Remove(instanceID);
            iEntity.OnUnRegistered();
        }
    }
    
    internal virtual void RegisterTouchCollider(Collider2D touchCollider, IObject iObject)
    {
        int instanceID = touchCollider.GetInstanceID();
        touchColliders.TryAdd(instanceID, iObject);
    }

    internal void UnRegisterTouchCollider(Collider2D touchCollider)
    {
        touchColliders.Remove(touchCollider.GetInstanceID());
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        inputSystem?.OnPointerDown(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        inputSystem?.OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        inputSystem?.OnPointerUp(eventData);
    }

    private JSONNode ExportJson()
    {
        JSONNode parent = new JSONObject();
        JSONArray children = new JSONArray();
        SerializeChildren(children);
        parent["items"] = children;
        parent["palette"] = palette.ToJson();
        parent["grid"] = puzzleGenerator.ToJson();
        parent["PuzzlePieceData"] = puzzleGenerator.GetAllPuzzlePieceNode();
        return parent;
    }

    private void SerializeChildren(JSONArray children)
    {
        foreach (IObject obj in iObjects.Values)
        {
            JSONNode node = null;
            if (((obj is PuzzlePiece piece && piece.group == null) || obj is IOGroupedPiece) && obj.parent is not PuzzlePalette)
            {
                node = obj.ToJson();
            }
            if(node != null) children.Add(node);
        }
    }

    private void SaveAll(JSONNode data)
    {
        string output = data.ToString();
        Debug.Log($"node is {output}");
        string json = data.ToString();
        StorageManager.Write( "Puzzle.json", json);
    }
}
