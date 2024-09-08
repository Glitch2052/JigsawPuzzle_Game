using UnityEngine;

public class PaletteContent : MonoBehaviour
{
    private PuzzlePalette palette;
    public const float DRAG_THRESHOLD = 0.5f;
    public const int EMPTY = -999;
    private int dragPointerId = EMPTY;

    private PuzzlePiece dragObject;
    private float targetPosition;
    Vector2 dragStart;
    private float width, leftLimit, rightLimit, leftLimitSoft, rightLimitSoft, lastTouchX, currentTouchX;
    private float offset;

    private float velocity;
    private float minVelocityThreshold = 0.015f;
    private float decelerationRate = 0.95f;
    
    public void Init(PuzzlePalette palette)
    {
        this.palette = palette;
        targetPosition = transform.localPosition.x;
        leftLimit = -20f;
        rightLimit = 20f;
    }

    public void UpdatePositions()
    {
        if (Mathf.Abs(velocity) > minVelocityThreshold)
        {
            targetPosition += velocity;
            velocity *= decelerationRate;
        }
        else
        {
            velocity = 0;
        }
        
        Vector3 pos = transform.localPosition;

        pos.x = Mathf.Lerp(pos.x, targetPosition,dragPointerId == EMPTY ? 0.3f : 1f);
        pos.x = Mathf.Clamp(pos.x, leftLimit, rightLimit);

        transform.localPosition = pos;
    }
    
    public IObject OnPointerDown(Vector2 worldPos, int pointerId)
    {
        if (dragPointerId != EMPTY)
            return null;

        // dragObject = GetDragCharacter(worldPos);
        // if (dragObject is PuzzlePiece piece) dragObject = null;

        dragStart = worldPos;
        dragPointerId = pointerId;
        worldPos = palette.transform.InverseTransformPoint(worldPos);
        offset = worldPos.x - transform.localPosition.x;
        Debug.Log($"offset is {offset}");
        lastTouchX = currentTouchX = worldPos.x;

        return palette;
    }

    public IObject OnPointerDrag(Vector2 worldPos, int pointerId)
    {
        if (dragPointerId != pointerId) return null;

        if (dragObject && (worldPos - dragStart).y >= DRAG_THRESHOLD)
        {
            // palette.TransferObjectToCurrentScene(dragObject);
            // RemoveObjectFromPalette(dragObject);
            dragPointerId = EMPTY;
            return dragObject.OnPointerDown(worldPos, pointerId);
        }

        worldPos = palette.transform.InverseTransformPoint(worldPos);
        targetPosition = worldPos.x - offset;
        lastTouchX = currentTouchX;
        currentTouchX = worldPos.x;

        return palette;
    }

    public IObject OnPointerUp(Vector2 worldPos, int pointerId)
    {
        if (dragPointerId == pointerId)
        {
            // targetPosition += 5 * (currentTouchX - lastTouchX);
            velocity = 5 * (currentTouchX - lastTouchX);
            dragObject = null;
            dragPointerId = EMPTY;
            return palette;
        }

        return null;
    }
}
