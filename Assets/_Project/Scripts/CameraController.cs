using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : IObject
{
    public bool additionalSpace;
    
    private int firstDragPointerId = EMPTY;
    private int secondDragPointerId = EMPTY;

    private float zoomSpeed = 1f;
    private float minZoom = 3f;
    private float maxZoom = 20f;

    private Vector3 targetPos;
    private Vector3 zoomMidPos;
    private Vector2 additionalSpaceSize;
    private float targetZoom;
    private float currZoomVelocity;
    private float boardWidth;
    private float zoomLerpSpeed = 20f;
    private const float CameraZ = -150f;

    private Vector2 currTouchPos;
    private Vector2 lastTouchPos;
    private Vector3 dragDirection;
    
    public float LeftLimit { get; private set; }
    public float RightLimit { get; private set; }
    public float TopLimit { get; private set; }
    public float BottomLimit { get; private set; }

    public override void Init()
    {
        boardWidth = iSystem.puzzleGenerator.BoardSize.x + 1f;
        float startOrthographicSize = (boardWidth / iSystem.Camera.aspect) * 0.5f;
        iSystem.Camera.orthographicSize = startOrthographicSize;

        //here min zoom is hard coded because the max player can zoom in is to see 3*3 grid
        minZoom = 6.5f;
        maxZoom = startOrthographicSize;
        
        targetZoom = iSystem.Camera.orthographicSize;
        targetPos = iSystem.Camera.transform.position.SetZ(CameraZ);

        LeftLimit = Mathf.Min(-boardWidth * 0.5f + iSystem.halfCameraSize.x,0);
        RightLimit = Mathf.Max(boardWidth * 0.5f - iSystem.halfCameraSize.x,0);
        BottomLimit = Mathf.Min(-maxZoom + iSystem.halfCameraSize.y,0) -1f;
        TopLimit = Mathf.Max(maxZoom - iSystem.halfCameraSize.y,0) - 1f;

        
    }

    public override void IUpdate()
    {
        float currZoom = iSystem.Camera.orthographicSize;
        float newZoom = Mathf.SmoothDamp(currZoom, targetZoom, ref currZoomVelocity, 0.175f);
        iSystem.Camera.orthographicSize = newZoom;
        
        Vector3 cameraPos = iSystem.Camera.transform.position;
        Vector3 zoomDir = zoomMidPos - cameraPos;
        targetPos += zoomDir * (currZoom - newZoom) / currZoom;
        targetPos.z = CameraZ;
        
        
        LeftLimit = Mathf.Min((-boardWidth - additionalSpaceSize.x) * 0.5f + iSystem.halfCameraSize.x,0);
        RightLimit = Mathf.Max((boardWidth + additionalSpaceSize.x) * 0.5f - iSystem.halfCameraSize.x,0);
        BottomLimit = Mathf.Min(-maxZoom - additionalSpaceSize.y + iSystem.halfCameraSize.y,0) -1f;
        TopLimit = Mathf.Max(maxZoom - iSystem.halfCameraSize.y,0) - 1f;
        
        targetPos.x = Mathf.Clamp(targetPos.x, LeftLimit, RightLimit);
        targetPos.y = Mathf.Clamp(targetPos.y, BottomLimit, TopLimit);
        
        //Update Camera Position
        iSystem.Camera.transform.position = Vector3.Lerp(cameraPos, targetPos, Time.deltaTime * zoomLerpSpeed);
        
        additionalSpaceSize = additionalSpace ? new Vector2(10f, 5f) * maxZoom / minZoom : Vector2.zero;
    }

    public override IObject OnPointerDown(Vector2 worldPos, int pointerId)
    {
        if (firstDragPointerId != EMPTY && secondDragPointerId != EMPTY) return null;
        if (firstDragPointerId == EMPTY)
        {
            firstDragPointerId = pointerId;
            currTouchPos = lastTouchPos = worldPos;
        }
        else if (secondDragPointerId == EMPTY)
        {
            secondDragPointerId = pointerId;
        }
        return this;
    }

    public override IObject OnPointerDrag(Vector2 worldPos, int pointerId)
    {
        if (firstDragPointerId != EMPTY && secondDragPointerId != EMPTY)
        {
            PointerEventData firstPointerData = iSystem.inputSystem.GetPointerEvent(firstDragPointerId).eventData;
            PointerEventData secondPointerData = iSystem.inputSystem.GetPointerEvent(secondDragPointerId).eventData;
            
            Vector2 touchZeroPrevPos = firstPointerData.position - firstPointerData.delta;
            Vector2 touchOnePrevPos = secondPointerData.position - secondPointerData.delta;
            
            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (firstPointerData.position - secondPointerData.position).magnitude;

            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            targetZoom = iSystem.Camera.orthographicSize + deltaMagnitudeDiff * zoomSpeed * Time.deltaTime;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
            // float newZoom = Mathf.Clamp(iSystem.Camera.orthographicSize + deltaMagnitudeDiff * zoomSpeed, minZoom, maxZoom);
            zoomMidPos = iSystem.Camera.ScreenToWorldPoint((firstPointerData.position + secondPointerData.position) / 2);

            // ZoomTowards(midPoint, newZoom);
        }
        else if (firstDragPointerId != EMPTY && secondDragPointerId == EMPTY && firstDragPointerId == pointerId)
        {
            currTouchPos = worldPos;
            dragDirection = lastTouchPos - currTouchPos;
            dragDirection.z = 0;
            targetPos += dragDirection;
            lastTouchPos = currTouchPos;
        }
        InputSystem.PointerEvent pointerEvent = iSystem.inputSystem.GetPointerEvent(pointerId);
        Debug.Log($"position is {pointerEvent.eventData.position} delta is {pointerEvent.eventData.delta}");
        return this;
    }

    public override IObject OnPointerUp(Vector2 worldPos, int pointerId)
    {
        if (firstDragPointerId == pointerId)
        {
            firstDragPointerId = EMPTY;
        }
        else if (secondDragPointerId == pointerId)
        {
            secondDragPointerId = EMPTY;
            currTouchPos = lastTouchPos = iSystem.inputSystem.GetPointerEvent(firstDragPointerId).currentWorldPos;
        }
        return this;
    }
}
