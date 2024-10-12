using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Serialization;

public class PaletteContent : MonoBehaviour
{
    public float zOrder;
    public float gap;
    public float restLine;
    
    private PuzzlePalette palette;
    public const int EMPTY = -999;
    private int dragPointerId = EMPTY;
    
    private PuzzlePiece dragObject;
    private float targetPosition;
    Vector2 dragStart;
    private float width, leftLimit, rightLimit, leftLimitSoft, rightLimitSoft, lastTouchX, currentTouchX;
    private float offset;
    private float halfCamWidth;
    private float halfCellSize;
    private float limitLerpSpeed = 14f;

    private float velocity;
    private float minVelocityThreshold = 0.015f;
    private float decelerationRate = 0.9f;
    
    //here the values are taken as reference with cam orthographic size 16
    //at time of usage this value scale up/down based on camera zoom
    private float elevateOffset = 2.75f;
    private float dragThreshold = 2f;

    private List<PuzzlePiece> puzzlePieces;


    private float endOffset;
    public float Width => gap * (puzzlePieces.Count - 1);

    public Vector3 LocalPosition
    {
        get => transform.localPosition;
        set =>transform.localPosition = OnPositionUpdate(value);
    }
    
    public void Init(PuzzlePalette palette)
    {
        this.palette = palette;
        puzzlePieces = new List<PuzzlePiece>();
        halfCamWidth = palette.iSystem.halfCameraSize.x;
        halfCellSize = PuzzleGenerator.Instance.CellSize * 0.5f + 0.5f;
        
        gap += PuzzleGenerator.Instance.CellSize;
        targetPosition = LocalPosition.x;
        endOffset = PuzzleGenerator.Instance.CellSize + 0.5f;

        // leftLimit = Mathf.Min(-Width * 0.5f + halfCamWidth + halfCellSize, 0);
        // rightLimit = Mathf.Max(Width * 0.5f - halfCamWidth - halfCellSize, 0);
    }

    public void SetUpData(JSONNode node)
    {
        if (node == null)
        {
            PuzzleGenerator.Instance.PuzzleGrid.IterateOverGridObjects((x, y, gridObject) =>
            {
                puzzlePieces.Add(gridObject.desiredPuzzlePiece);
                gridObject.desiredPuzzlePiece.SetParent(palette,transform);
                gridObject.desiredPuzzlePiece.LocalPosition = Vector3.zero;
            });
        }
        else
        {
            JSONArray children = node["children"] as JSONArray;
            foreach (var childNodeKeyValue in children)
            {
                int index = childNodeKeyValue.Value["Index"];
                GridObject gridObject = PuzzleGenerator.Instance.PuzzleGrid.GetGridObject(index);
                if (gridObject != null)
                {
                    puzzlePieces.Add(gridObject.desiredPuzzlePiece);
                    gridObject.desiredPuzzlePiece.SetParent(palette,transform);
                    gridObject.desiredPuzzlePiece.LocalPosition = Vector3.zero;
                }
            }
        }
        Utilities.ShuffleArray(puzzlePieces, Random.Range(0, 10000));
        
        rightLimitSoft = rightLimit = -halfCamWidth + endOffset;
        leftLimitSoft = leftLimit = Mathf.Min(rightLimit - Width + palette.iSystem.cameraSize.x, rightLimit);
    }

    public void IUpdate()
    {
        UpdatePositions();

        float effectiveCamWidth = palette.iSystem.cameraSize.x / palette.LocalScale.x;
        halfCamWidth = effectiveCamWidth * 0.5f;
        rightLimit = -halfCamWidth + endOffset;
        leftLimit = Mathf.Min(rightLimit - Width + effectiveCamWidth - (endOffset * 2f), rightLimit);

        rightLimitSoft = Mathf.Lerp(rightLimitSoft, rightLimit, Time.deltaTime * limitLerpSpeed);
        leftLimitSoft = Mathf.Lerp(leftLimitSoft, leftLimit, Time.deltaTime * limitLerpSpeed);
        
        if (Mathf.Abs(velocity) > minVelocityThreshold)
        {
            targetPosition += velocity;
            velocity *= decelerationRate;
        }
        else
        {
            velocity = 0;
        }
        
        Vector3 pos = LocalPosition;
        
        pos.x = Mathf.Lerp(pos.x, targetPosition,dragPointerId == EMPTY ? 0.3f : 1f);
        pos.x = Mathf.Clamp(pos.x, leftLimitSoft, rightLimitSoft);
        
        LocalPosition = pos;
    }

    private void UpdatePositions()
    {
        float x = 0;
        float y = restLine;
        float z = zOrder;
        Vector3 pos = Vector3.zero;
        
        foreach (PuzzlePiece piece in puzzlePieces)
        {
            // if (palette.IsShowing != State.Visible) y -= 1;
            pos.Set(x, y, z);
            piece.LocalPosition = Vector3.Lerp(piece.LocalPosition, pos, Time.deltaTime * limitLerpSpeed);

            x += gap;
        }
    }

    private Vector3 OnPositionUpdate(Vector3 newPosition)
    { 
        return LocalPosition + OnValueChanged(newPosition - LocalPosition);
    }

    private Vector3 OnValueChanged(Vector3 direction)
    {
        Debug.Log($"moved in the direction {direction}");
        return direction;
    }
    
    public IObject OnPointerDown(Vector2 worldPos, int pointerId)
    {
        if (dragPointerId != EMPTY)
            return null;

        dragObject = GetDraggedPiece(worldPos);

        dragStart = worldPos;
        dragPointerId = pointerId;
        worldPos = palette.transform.InverseTransformPoint(worldPos);
        offset = worldPos.x - LocalPosition.x;
        Debug.Log($"offset is {offset}");
        lastTouchX = currentTouchX = worldPos.x;
        velocity = 0;
        targetPosition = worldPos.x - offset;
        
        return palette;
    }

    public IObject OnPointerDrag(Vector2 worldPos, int pointerId)
    {
        if (dragPointerId != pointerId) return null;

        if (dragObject && (worldPos - dragStart).y >= dragThreshold)
        {
            // palette.TransferObjectToCurrentScene(dragObject);
            RemoveObjectFromPalette(dragObject);
            Vector3 pos = worldPos + Vector2.up * elevateOffset;
            pos.z = dragObject.Position.z;
            dragObject.Position = pos;
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
            velocity += 1.75f * (currentTouchX - lastTouchX);
            dragObject = null;
            dragPointerId = EMPTY;
            return palette;
        }

        return null;
    }
    
    private void RemoveObjectFromPalette(PuzzlePiece piece)
    {
        puzzlePieces.Remove(piece);
        piece.SetParent(null);
        piece.LocalScaleLerped = Vector3.one;
    }

    public void AddObjectToPalette(PuzzlePiece piece)
    {
        piece.SetParent(palette,transform);
        piece.LocalScaleLerped = Vector3.one;

        float xDistance = piece.LocalPosition.x;
        int index = Mathf.Clamp(Mathf.CeilToInt(xDistance/gap), 0, puzzlePieces.Count - 1);
        puzzlePieces.Insert(index, piece);
    }
    
    public PuzzlePiece GetDraggedPiece(Vector2 worldPoint)
    {
        foreach (PuzzlePiece piece in puzzlePieces)
        {
            if (piece.MainCollider.OverlapPoint(worldPoint)) return piece;
        }

        return null;
    }

    public JSONNode ToJson(JSONNode node = null)
    {
        if (node == null)
            node = new JSONObject();
        
        JSONArray children = new JSONArray();
        int i = 0;
        foreach (PuzzlePiece piece in puzzlePieces)
        {
            children[i] = piece.ToJson();
            i++;
        }
        
        node["children"] = children;
        return node;
    }

    public void UpdateThresholdValues(float zoomScaleFactor)
    {
        dragThreshold *= zoomScaleFactor;
        elevateOffset *= zoomScaleFactor;
    }
}
