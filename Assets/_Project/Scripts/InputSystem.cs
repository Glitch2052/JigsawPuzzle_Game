using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum TouchStatus
{
    Down,
    Dragged,
    Up
}

public class InputSystem
{
    public class PointerEvent
    {
        public PointerEventData eventData;
        public IObject iObject;

        public TouchStatus status;
        public Vector2 startScreenPos, currentScreenPos;
        public Vector2 startWorldPos, currentWorldPos;
        public float dragDistance;
        public int startFrame, lastDragFrame;
    }

    private readonly InteractiveSystem iSystem;
    private readonly SortedDictionary<int, IObject> listeners;
    private PointerEvent[] pointerEvents;

    public int NoOfTouches
    {
        get
        {
            int c = 0;
            foreach (PointerEvent item in pointerEvents)
            {
                if (item != null)
                    if (!item.status.Equals(TouchStatus.Up))
                        c++;
            }

            return c;
        }
    }

    public PointerEvent LastPointerEvent { get; private set; }

    private PointerEventData currentEventData;

    Camera camera;
    protected RaycastHit2D[] raycastHits;

    public InputSystem(InteractiveSystem iSystem)
    {
        this.iSystem = iSystem;
        listeners = iSystem.touchColliders;

        camera = iSystem.Camera;
        raycastHits = new RaycastHit2D[10];

        pointerEvents = new PointerEvent[10];
    }

    public IObject OnPointerDown(PointerEventData eventData)
    {
        currentEventData = eventData;
        IObject handlingListener = null;
        Vector3 worldPos = camera.ScreenToWorldPoint(eventData.position);
        foreach (IObject listener in OverlappingListeners(worldPos))
        {
            if (listener && (handlingListener = listener.OnPointerDown(worldPos, eventData.pointerId)))
            {
                break;
            }
        }

        CachePointerEventData(eventData, TouchStatus.Down, worldPos, handlingListener);

        currentEventData = null;
        return handlingListener;
    }

    public IObject OnDrag(PointerEventData eventData)
    {
        currentEventData = eventData;
        Vector3 worldPos = camera.ScreenToWorldPoint(eventData.position);
        IObject handlingListener = GetPointerEventListener(eventData.pointerId);

        if (handlingListener) handlingListener = handlingListener.OnPointerDrag(worldPos, eventData.pointerId);

        if (!handlingListener)
        {
            foreach (IObject listener in OverlappingListeners(worldPos))
            {
                if (listener && (handlingListener = listener.OnPointerDrag(worldPos, eventData.pointerId)))
                {
                    break;
                }
            }
        }

        CachePointerEventData(eventData, TouchStatus.Dragged, worldPos, handlingListener);

        currentEventData = null;
        return handlingListener;
    }

    public IObject OnPointerUp(PointerEventData eventData)
    {
        currentEventData = eventData;
        Vector3 worldPos = camera.ScreenToWorldPoint(eventData.position);
        IObject handlingListener = GetPointerEventListener(eventData.pointerId);

        if (handlingListener) handlingListener = handlingListener.OnPointerUp(worldPos, eventData.pointerId);

        if (!handlingListener)
        {
            foreach (IObject listener in OverlappingListeners(worldPos))
            {
                if (listener && (handlingListener = listener.OnPointerUp(worldPos, eventData.pointerId)))
                {
                    break;
                }
            }
        }

        CachePointerEventData(eventData, TouchStatus.Up, worldPos);

        currentEventData = null;
        return handlingListener;
    }

    void OnTap(PointerEvent data)
    {
        Vector2 worldPos = data.startWorldPos;
        foreach (IObject listener in OverlappingListeners(worldPos))
        {
            if (listener && listener.OnTap(worldPos, data.eventData.pointerId))
            {
                break;
            }
        }
    }

    protected virtual IEnumerable<IObject> OverlappingListeners(Vector2 worldPos)
    {
        int results = Physics2D.RaycastNonAlloc(worldPos, Vector2.zero, raycastHits, 500f, LayerMask.GetMask("Touchable"));
        SortRayCastHits(raycastHits);

        for (int i = 0; i < results; i++)
        {
            if(listeners.TryGetValue(raycastHits[i].collider.GetInstanceID(), out IObject listener))
                yield return listener;
        }

        // No listener handled this event.
        // Pass To CameraController
        yield return iSystem.cameraController;
    }

    void SortRayCastHits(RaycastHit2D[] hits)
    {
        for (int i = 0; i < hits.Length; i++)
        {
            for (int j = 0; j < hits.Length; j++)
            {
                if (hits[i].distance > hits[j].distance)
                {
                    // Swap
                    (hits[i], hits[j]) = (hits[j], hits[i]);
                }
            }
        }
    }

    private void CachePointerEventData(PointerEventData data, TouchStatus status, Vector2 worldPos, IObject listener = null)
    {
        int index = data.pointerId;

        index = Mathf.Clamp(index, 0, pointerEvents.Length - 1);

        if (status == TouchStatus.Down)
            pointerEvents[index] = new PointerEvent();

        PointerEvent pEvent = pointerEvents[index];
        pEvent ??= new PointerEvent();

        pEvent.eventData = data;
        pEvent.iObject = listener;
        pEvent.status = status;

        float dt = 0;
        if (status != TouchStatus.Down)
        {
            dt = (Time.frameCount - pEvent.startFrame) * Time.deltaTime;
            pEvent.dragDistance += pEvent.eventData.delta.sqrMagnitude;
        }

        switch (status)
        {
            case TouchStatus.Down:
                pEvent.startScreenPos = pEvent.currentScreenPos = data.position;
                pEvent.startWorldPos = pEvent.currentWorldPos = worldPos;
                pEvent.startFrame = pEvent.lastDragFrame = Time.frameCount;
                break;
            case TouchStatus.Dragged:
                pEvent.currentScreenPos = data.position;
                pEvent.currentWorldPos = worldPos;
                pEvent.lastDragFrame = Time.frameCount;
                break;
            case TouchStatus.Up:
                if (dt < 0.25f && pEvent.dragDistance < 10)
                {
                    // It is a tap gesture
                    OnTap(pEvent);
                }

                break;
        }

        LastPointerEvent = pointerEvents[index];
    }

    public PointerEvent GetPointerEvent(int pointerId)
    {
        int index = Mathf.Clamp(pointerId, 0, pointerEvents.Length);
        return pointerEvents[index];
    }

    public Vector2 GetCurrentWorldPosition(int pointerId)
    {
        return camera.ScreenToWorldPoint(GetPointerEvent(pointerId).eventData.position);
    }

    public PointerEventData GetPointerEventData(int pointerId)
    {
        PointerEventData data = GetPointerEvent(pointerId).eventData;
        if (data != null && data.pointerId == pointerId)
            return data;
        else
            return null;
    }

    public IObject GetPointerEventListener(int pointerId)
    {
        return GetPointerEvent(pointerId)?.iObject;
    }

    public PointerEventData GetCurrentPointerEvent()
    {
        return currentEventData;
    }

    public void OnApplicationPause()
    {
        ReleaseAllPointers();
    }

    public void ReleaseAllPointers()
    {
        for (int i = 0; i < pointerEvents.Length; i++)
        {
            if (pointerEvents[i] != null && pointerEvents[i].iObject != null)
            {
                // Touch is still active
                OnPointerUp(pointerEvents[i].eventData);
            }
        }
    }
}