using System;
using System.Collections;
using System.Collections.Generic;
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
    protected Vector3 cameraPosition;
    public Camera Camera { get; protected set; }
    
    public static readonly float DRAG_Z_ORDER = -45f;
    public static float gridSnapThreshold;
    public static float neighbourSnapThreshold;

    private void Awake()
    {
        Camera = Camera.main;

        iObjects = new SortedDictionary<int, IObject>();
        touchColliders = new SortedDictionary<int, IObject>();
        
        float orthographicSize = Camera.orthographicSize;
        cameraSize.x = orthographicSize * Screen.width / Screen.height;
        cameraSize.y = orthographicSize;
        cameraSize *= 2;
    }

    private void Start()
    {
        Init();
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
        puzzleGenerator.GenerateGrid();

        palette.SetISystem(this);
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
}
