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

    private string sceneID;
    
    
    public Camera Camera { get; protected set; }
    
    public static readonly float DRAG_Z_ORDER = -145f;
    public static float gridSnapThreshold;
    public static float neighbourSnapThreshold;

    public float LeftLimit { get; set;}
    public float RightLimit { get; set;}
    public float TopLimit { get; set;}
    public float BottomLimit { get; set;}

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
    }

    private void Update()
    {
        foreach (IObject iObject in iObjects.Values)
        {
            iObject.IUpdate();
        }
    }

    public void Init()
    {
        inputSystem = new InputSystem(this);

        gridSnapThreshold = puzzleGenerator.CellSize * 0.5f;
        neighbourSnapThreshold = puzzleGenerator.CellSize * 1.5f;
        puzzleGenerator.Init(this);
    }

    public IEnumerator OnSceneLoad(PuzzleTextureData puzzleTextureData, JSONNode configData)
    {
        sceneID = configData[StringID.PuzzleSceneID];
        JSONNode node = new JSONObject();
        int squaredSize;

        if (configData[StringID.NewGame])
        {
            squaredSize = configData[StringID.BoardSize];
        }
        else
        {
            ReadOperation readOperation = new ReadOperation(sceneID, "{items}");
            yield return readOperation.Read();
            node = JSONNode.Parse(readOperation.data);
            squaredSize = node[StringID.BoardData][StringID.BoardSize];
        }
        
        int size = (int)Mathf.Sqrt(squaredSize);
        puzzleGenerator.SetGridSize(size,size);
        puzzleGenerator.UpdateBackGroundData(puzzleTextureData);
        
        puzzleGenerator.GenerateGrid(puzzleTextureData, node[StringID.BoardData]);
        
        cameraController.SetISystem(this);
        palette.SetISystem(this);
        palette.SetUpContentData();

        //From Json
        yield return puzzleGenerator.FromJson(node[StringID.BoardData]);
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
    
    public void OnBack()
    {
        SaveAll(ExportJson());
        GameManager.Instance.LoadScene(GameManager.MainMenuScene);
    }

    private JSONNode ExportJson()
    {
        JSONNode parent = new JSONObject();
        parent[StringID.BoardData] = puzzleGenerator.ToJson();
        return parent;
    }

    private void SaveAll(JSONNode data)
    {
        string json = data.ToString();
        StorageManager.Write( sceneID, json);
        
        Debug.Log($"node is {json}");
    }
}