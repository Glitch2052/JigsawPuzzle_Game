using System.Collections.Generic;
using UnityEngine;

public class PaletteContent : MonoBehaviour
{
    public float zOrder;
    public float gap;
    public float restLine;
    
    private PuzzlePalette palette;
    public const float DRAG_THRESHOLD = 0.5f;
    public const int EMPTY = -999;
    private int dragPointerId = EMPTY;
    
    private PuzzlePiece dragObject;
    private float targetPosition;
    Vector2 dragStart;
    private float width, leftLimit, rightLimit, leftLimitSoft, rightLimitSoft, lastTouchX, currentTouchX;
    private float offset;
    private float halfCamWidth, halfCellSize;

    private float velocity;
    private float minVelocityThreshold = 0.015f;
    private float decelerationRate = 0.95f;

    private List<PuzzlePiece> puzzlePieces;
    
    public float Width => gap * (puzzlePieces.Count + 1);
    
    public void Init(PuzzlePalette palette)
    {
        puzzlePieces = new List<PuzzlePiece>();
        PuzzleGenerator.Instance.PuzzleGrid.IterateOverGridObjects((x, y, gridObject) =>
        {
            puzzlePieces.Add(gridObject.desiredPuzzlePiece);
            gridObject.desiredPuzzlePiece.SetParent(palette,transform);
            gridObject.desiredPuzzlePiece.LocalPosition = Vector3.zero;
        });

        gap += PuzzleGenerator.Instance.CellSize;
        this.palette = palette;
        targetPosition = transform.localPosition.x;

        halfCamWidth = palette.iSystem.cameraSize.x * 0.5f;
        halfCellSize = PuzzleGenerator.Instance.CellSize * 0.5f + 0.5f;
        leftLimit = Mathf.Min(-Width * 0.5f + halfCamWidth + halfCellSize, 0);
        rightLimit = Mathf.Max(Width * 0.5f - halfCamWidth - halfCellSize, 0);
    }

    public void IUpdate()
    {
        UpdatePositions();

        leftLimit = Mathf.Min(-Width * 0.5f + halfCamWidth + halfCellSize, 0);
        rightLimit = Mathf.Max(Width * 0.5f - halfCamWidth - halfCellSize, 0);
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

    private void UpdatePositions()
    {
        float x = -Width * 0.5f + gap;
        float y = restLine;
        float z = zOrder;
        Vector3 pos = Vector3.zero;
        
        foreach (PuzzlePiece piece in puzzlePieces)
        {
            // if (palette.IsShowing != State.Visible) y -= 1;
            pos.Set(x, y, z);
            piece.LocalPosition = Vector3.Lerp(piece.LocalPosition, pos, 0.1f);

            x += gap;
        }
    }
    
    public IObject OnPointerDown(Vector2 worldPos, int pointerId)
    {
        if (dragPointerId != EMPTY)
            return null;

        dragObject = GetDraggedPiece(worldPos);

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
            RemoveObjectFromPalette(dragObject);
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
    
    private void RemoveObjectFromPalette(PuzzlePiece piece)
    {
        puzzlePieces.Remove(piece);
    }
    
    public PuzzlePiece GetDraggedPiece(Vector2 worldPoint)
    {
        foreach (PuzzlePiece piece in puzzlePieces)
        {
            if (piece.MainCollider.OverlapPoint(worldPoint)) return piece;
        }

        return null;
    }
}
