using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : IObject
{
    public bool additionalSpace;
    
    private int firstDragPointerId = EMPTY;
    private int secondDragPointerId = EMPTY;
    
    private float minZoom = 3f;
    private float maxZoom = 20f;
    private readonly float zoomLerpSpeed = 30f;
    private readonly float zoomSensitivity = 1.25f;

    private Vector3 targetPos;
    private Vector3 zoomMidPos;
    private Vector2 additionalSpaceSize;
    private float targetZoom;
    private float currZoomVelocity;
    private float boardWidth;
    private const float CameraZ = -150f;

    private Vector2 currTouchPos;
    private Vector2 lastTouchPos;
    private Vector3 dragDirection;
    
    public float LeftLimit { get; protected set; }
    public float RightLimit { get; protected set; }
    public float TopLimit { get; protected set; }
    public float BottomLimit { get; protected set; }

    public override void Init()
    {
        Vector2 boardSize = iSystem.puzzleGenerator.BoardSize;
        boardWidth = boardSize.x + 1f;
        float startOrthographicSize = (boardWidth / iSystem.Camera.aspect) * 0.5f;
        iSystem.Camera.orthographicSize = startOrthographicSize;

        //here min zoom is hard coded because the max player can zoom in is to see 3*3 grid
        minZoom = 6.5f;
        maxZoom = startOrthographicSize;
        
        targetZoom = iSystem.Camera.orthographicSize;
        targetPos = iSystem.Camera.transform.position.SetZ(CameraZ);
        
        additionalSpaceSize = additionalSpace ? new Vector2(10f, 5f) * maxZoom / minZoom : Vector2.zero;

        LeftLimit = Mathf.Min((-boardWidth - additionalSpaceSize.x) * 0.5f + iSystem.halfCameraSize.x,0);
        RightLimit = Mathf.Max((boardWidth + additionalSpaceSize.x) * 0.5f - iSystem.halfCameraSize.x,0);
        BottomLimit = Mathf.Min(-maxZoom - additionalSpaceSize.y + iSystem.halfCameraSize.y,0) -1f;
        TopLimit = Mathf.Max(maxZoom - iSystem.halfCameraSize.y,0) - 1f;

        iSystem.LeftLimit = (-boardWidth - additionalSpaceSize.x) * 0.5f + 0.25f;
        iSystem.RightLimit = (boardWidth + additionalSpaceSize.x) * 0.5f - 0.25f;
        iSystem.BottomLimit = (-boardSize.y - additionalSpaceSize.y * 0.25f) * 0.5f;
        iSystem.TopLimit = boardSize.y * 0.5f + 2f;
    }

    public override void IUpdate()
    {
        float currZoom = iSystem.Camera.orthographicSize;
        float newZoom = Mathf.SmoothDamp(currZoom, targetZoom, ref currZoomVelocity, 0.02f);
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
        float lerpValue = Time.deltaTime * zoomLerpSpeed;
        if (firstDragPointerId != EMPTY && secondDragPointerId == EMPTY)
            lerpValue = 1;
        
        iSystem.Camera.transform.position = Vector3.Lerp(cameraPos, targetPos, lerpValue);
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

            float deltaMagnitudeDiff = (prevTouchDeltaMag - touchDeltaMag) * zoomSensitivity;

            targetZoom = iSystem.Camera.orthographicSize + deltaMagnitudeDiff * Time.deltaTime;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
            zoomMidPos = iSystem.Camera.ScreenToWorldPoint((firstPointerData.position + secondPointerData.position) / 2);

            // ZoomTowards(midPoint, newZoom);
        }
        else if (firstDragPointerId != EMPTY && secondDragPointerId == EMPTY && firstDragPointerId == pointerId)
        {
            currTouchPos = worldPos;
            dragDirection = lastTouchPos - currTouchPos;
            dragDirection.z = 0;
            targetPos += dragDirection;
            lastTouchPos = worldPos + (Vector2)dragDirection;
        }
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
