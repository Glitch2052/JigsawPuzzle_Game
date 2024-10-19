using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

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
    private readonly float limitLerpSpeed = 14f;

    private float velocity;
    private float minVelocityThreshold = 0.015f;
    private float decelerationRate = 0.9f;
    
    //here the values are taken as reference with cam orthographic size 16
    //at time of usage this value scale up/down based on camera zoom
    private float elevateOffset = 2.75f;
    private float dragThreshold = 2f;

    #region Recycling Content Data
    
    [SerializeField] private PuzzlePiece prototypeCell;
    private List<PuzzlePiece> cachedCells;
    private bool isRecycling;
    private bool isRearranging;
    private bool recyclingSystemInitialized;
    private int leftMostCellIndex, rightMostCellIndex;
    private int currentItemCount;
    private Vector3 zeroVector;
    private List<PuzzlePieceData> dataSource;
    private List<PuzzlePieceData> removedDataSource;
    private Bounds recyclingBounds;
    private readonly int minPoolSizeRecommended = 12;
    #endregion


    private float endOffset;
    public float Width => gap * (cachedCells.Count - 1);

    public Vector3 LocalPosition
    {
        get => transform.localPosition;
        set =>transform.localPosition = OnPositionUpdate(value);
    }
    
    private Vector3 OnPositionUpdate(Vector3 newPosition)
    {
        Vector3 currLocalPos = LocalPosition;
        return currLocalPos + OnValueChanged(newPosition - currLocalPos);
    }
    
    public void Init(PuzzlePalette palette)
    {
        this.palette = palette;
        halfCamWidth = palette.iSystem.halfCameraSize.x;
        
        gap += PuzzleGenerator.Instance.CellSize;
        endOffset = PuzzleGenerator.Instance.CellSize + 0.5f;

        recyclingSystemInitialized = false;
    }

    public void SetUpData()
    {
        // if (node == null)
        // {
        //     PuzzleGenerator.Instance.PuzzleGrid.IterateOverGridObjects((x, y, gridObject) =>
        //     {
        //         puzzlePieces.Add(gridObject.desiredPuzzlePiece);
        //         gridObject.desiredPuzzlePiece.SetParent(palette,transform);
        //         gridObject.desiredPuzzlePiece.LocalPosition = Vector3.zero;
        //     });
        // }
        // else
        // {
        //     JSONArray children = node["children"] as JSONArray;
        //     foreach (var childNodeKeyValue in children)
        //     {
        //         int index = childNodeKeyValue.Value["Index"];
        //         GridObject gridObject = PuzzleGenerator.Instance.PuzzleGrid.GetGridObject(index);
        //         if (gridObject != null)
        //         {
        //             puzzlePieces.Add(gridObject.desiredPuzzlePiece);
        //             gridObject.desiredPuzzlePiece.SetParent(palette,transform);
        //             gridObject.desiredPuzzlePiece.LocalPosition = Vector3.zero;
        //         }
        //     }
        // }
        dataSource = PuzzleGenerator.Instance.puzzlePieceDataSource;
        removedDataSource = new List<PuzzlePieceData>();
        // Utilities.ShuffleArray(dataSource, Random.Range(0, 10000));
        
        SetUpRecyclingSystem();
    }

    public void IUpdate()
    {
        if(isRearranging) return;
        
        float effectiveCamWidth = palette.iSystem.cameraSize.x / palette.LocalScale.x;
        halfCamWidth = effectiveCamWidth * 0.5f;
        rightLimit = -halfCamWidth + endOffset;
        leftLimit = Mathf.Min(rightLimit - Width + effectiveCamWidth - (endOffset * 2f), rightLimit);
        
        rightLimitSoft = Mathf.Lerp(rightLimitSoft, rightLimit, Time.deltaTime * limitLerpSpeed);
        leftLimitSoft = Mathf.Lerp(leftLimitSoft, leftLimit, Time.deltaTime * limitLerpSpeed);
        recyclingBounds.center = palette.Position;
        
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
        
        UpdatePositions();
        Debug.Log($"Current Index Count is {currentItemCount} with Data Count {dataSource.Count}");
    }

    private void UpdatePositions()
    {
        if((isRecycling && !recyclingSystemInitialized) || cachedCells.Count == 0 || isRearranging) return;

        int length = leftMostCellIndex + cachedCells.Count;
        float x = 0;
        float y = restLine;
        float z = zOrder;
        Vector3 pos = Vector3.zero;

        for (int i = leftMostCellIndex; i < length; i++)
        {
            var piece = cachedCells[i % cachedCells.Count];
            pos.Set(x, y, z);
            piece.LocalPosition = Vector3.Lerp(piece.LocalPosition, pos, Time.deltaTime * limitLerpSpeed);
            x += gap;
        }
    }

    #region Recycling System

    private void SetUpRecyclingSystem()
    {
        //New Cached Cells List
        cachedCells = new List<PuzzlePiece>();
        
        //Set Content Pos to left;
        float effectiveCamWidth = palette.iSystem.cameraSize.x / palette.LocalScale.x;
        halfCamWidth = effectiveCamWidth * 0.5f;
        transform.localPosition = LocalPosition.SetX(- halfCamWidth + endOffset);
        targetPosition = LocalPosition.x;
        
        //Set Recycling Bounds
        Vector3 boundsSize = new Vector3(palette.iSystem.cameraSize.x + 10f * palette.LocalScale.x, 1, 1);
        recyclingBounds = new Bounds(palette.Position, boundsSize);
        
        //Create Cell Pool
        CreateCellPool();
        leftMostCellIndex = 0;
        rightMostCellIndex = cachedCells.Count - 1;

        recyclingSystemInitialized = true;
    }

    private void CreateCellPool()
    {
        int poolSize = 0;
        int minPoolSize = Mathf.Min(minPoolSizeRecommended, dataSource.Count);

        Vector3 startPos = new Vector3(0, 0, zOrder);
        
        
        while (poolSize < minPoolSize && poolSize < dataSource.Count)
        {
            var piece = Instantiate(prototypeCell,transform);
            piece.SetISystem(palette.iSystem);
            piece.SetParent(palette, transform);
            piece.LocalPosition = startPos;
            
            cachedCells.Add(piece);
            
            //call cacheCell InitCellData
            //call cacheCell UpdateCellData
            cachedCells[^1].SetData();
            cachedCells[^1].UpdateData(dataSource[currentItemCount]);
            
            startPos.x += gap;
            poolSize++;
            currentItemCount++;
        }
    }
    
    private float adjustedXPosAfterRecycling;

    private Vector3 OnValueChanged(Vector3 direction)
    {
        if (isRecycling || cachedCells == null || cachedCells.Count == 0) return zeroVector;
        if (dataSource.Count <= minPoolSizeRecommended || isRearranging) return direction;
        
        if (direction.x < 0f && cachedCells[rightMostCellIndex].Position.x < recyclingBounds.max.x && currentItemCount < dataSource.Count)
        {
            //Recycle Left Items to Right
            Vector3 adjustedDirection = RecycleLeftToRight();
            adjustedXPosAfterRecycling = adjustedDirection.x;
            if(dragPointerId == EMPTY)
                targetPosition += adjustedXPosAfterRecycling;
            return adjustedDirection;
        }

        if (direction.x > 0f && cachedCells[leftMostCellIndex].Position.x > recyclingBounds.min.x  && currentItemCount > cachedCells.Count)
        {
            //Recycle Right Items to Left
            Vector3 adjustedDirection = RecycleRightToLeft();
            adjustedXPosAfterRecycling = adjustedDirection.x;
            if(dragPointerId == EMPTY)
                targetPosition += adjustedXPosAfterRecycling;
            return adjustedDirection;
        }
        
        return direction;
    }

    private Vector3 RecycleLeftToRight()
    {
        isRecycling = true;
        int n = 0;
        
        while (cachedCells[leftMostCellIndex].Position.x < recyclingBounds.min.x && currentItemCount < dataSource.Count)
        {
            Vector3 rightMostLocalPos = cachedCells[rightMostCellIndex].LocalPosition;
            rightMostLocalPos.x += gap;
            cachedCells[leftMostCellIndex].LocalPosition = rightMostLocalPos;
            
            //call cacheCell UpdateCellData
            cachedCells[leftMostCellIndex].UpdateData(dataSource[currentItemCount]);

            rightMostCellIndex = leftMostCellIndex;
            leftMostCellIndex = (leftMostCellIndex + 1) % cachedCells.Count;
            currentItemCount++;
            n++;
        }
        
        cachedCells.ForEach(p => p.LocalPosition += Vector3.left * (n * gap));
        isRecycling = false;
        return new Vector3(n * gap, 0, 0);
    }

    private Vector3 RecycleRightToLeft()
    {
        isRecycling = true;
        int n = 0;
        while (cachedCells[rightMostCellIndex].Position.x > recyclingBounds.max.x && currentItemCount > cachedCells.Count)
        {
            Vector3 leftMostLocalPos = cachedCells[leftMostCellIndex].LocalPosition;
            leftMostLocalPos.x -= gap;
            cachedCells[rightMostCellIndex].LocalPosition = leftMostLocalPos;
            
            currentItemCount--;
            
            //call cacheCell UpdateCellData
            cachedCells[rightMostCellIndex].UpdateData(dataSource[currentItemCount - cachedCells.Count]);

            leftMostCellIndex = rightMostCellIndex;
            rightMostCellIndex = (rightMostCellIndex - 1 + cachedCells.Count) % cachedCells.Count;
            
            n++;
        }
        
        cachedCells.ForEach(p => p.LocalPosition += Vector3.right * (n * gap));
        isRecycling = false;
        return new Vector3(-n * gap, 0, 0);
    }
    
    #endregion
    
    public IObject OnPointerDown(Vector2 worldPos, int pointerId)
    {
        if (dragPointerId != EMPTY)
            return null;

        dragObject = GetDraggedPiece(worldPos);

        dragStart = worldPos;
        dragPointerId = pointerId;
        worldPos = palette.transform.InverseTransformPoint(worldPos);
        offset = worldPos.x - LocalPosition.x;
        lastTouchX = currentTouchX = worldPos.x;
        velocity = 0;
        targetPosition = worldPos.x - offset;
        adjustedXPosAfterRecycling = 0;
        return palette;
    }

    public IObject OnPointerDrag(Vector2 worldPos, int pointerId)
    {
        if (dragPointerId != pointerId) return null;

        if (dragObject && (worldPos - dragStart).y >= dragThreshold)
        {
            RemoveObjectFromPalette(dragObject);
            Vector3 pos = worldPos + Vector2.up * elevateOffset;
            pos.z = dragObject.Position.z;
            dragObject.Position = pos;
            dragPointerId = EMPTY;
            return dragObject.OnPointerDown(worldPos, pointerId);
        }

        worldPos = palette.transform.InverseTransformPoint(worldPos);
        if (adjustedXPosAfterRecycling != 0)
        {
            offset -= adjustedXPosAfterRecycling;
            adjustedXPosAfterRecycling = 0;
        }
        targetPosition = worldPos.x - offset;
        lastTouchX = currentTouchX;
        currentTouchX = worldPos.x;

        return palette;
    }

    public IObject OnPointerUp(Vector2 worldPos, int pointerId)
    {
        if (dragPointerId == pointerId)
        {
            velocity += 1.75f * (currentTouchX - lastTouchX);
            dragObject = null;
            dragPointerId = EMPTY;
            return palette;
        }

        return null;
    }
    
    private void RemoveObjectFromPalette(PuzzlePiece piece)
    {
        int removedIndex = cachedCells.IndexOf(piece);
        cachedCells.RemoveAt(removedIndex);
        piece.SetParent(null);
        piece.LocalScaleLerped = Vector3.one;

        dataSource.Remove(piece.currentAssignedPieceData);
        removedDataSource.Add(piece.currentAssignedPieceData);
        
        leftMostCellIndex = Mathf.Clamp(leftMostCellIndex, 0, dataSource.Count - 1);
        rightMostCellIndex = Mathf.Clamp(rightMostCellIndex, 0, dataSource.Count - 1);
        
        if((isRecycling && !recyclingSystemInitialized) || dataSource.Count <= cachedCells.Count) return;

        isRearranging = true;
        
        var newCachePiece = Instantiate(prototypeCell,Vector3.down * 50, Quaternion.identity, transform);
        newCachePiece.SetISystem(palette.iSystem);
        newCachePiece.SetParent(palette, transform);

        currentItemCount--;
        if(currentItemCount == dataSource.Count)
            RecoverFromLeft(removedIndex,newCachePiece);
        else
            RecoverFromRight(removedIndex, newCachePiece);
        
        isRearranging = false;
    }

    private void RecoverFromRight(int removedIndex, PuzzlePiece newPiece)
    {
        int insertIndex = rightMostCellIndex;
        
        if (removedIndex > rightMostCellIndex)
            insertIndex = (rightMostCellIndex + 1) % minPoolSizeRecommended;
        
        cachedCells.Insert(insertIndex,newPiece);
        rightMostCellIndex = insertIndex;
        leftMostCellIndex = (rightMostCellIndex + 1) % cachedCells.Count;
            
        newPiece.SetData();
        newPiece.UpdateData(dataSource[currentItemCount]);
        newPiece.LocalPosition = Vector3.right * (gap * cachedCells.Count);

        currentItemCount++;
    }
    
    private void RecoverFromLeft(int removedIndex, PuzzlePiece newPiece)
    {
        int insertIndex = leftMostCellIndex;
        
        if (removedIndex < leftMostCellIndex)
            insertIndex = (leftMostCellIndex - 1 + minPoolSizeRecommended) % minPoolSizeRecommended;
        
        cachedCells.Insert(insertIndex,newPiece);
        leftMostCellIndex = insertIndex;
        rightMostCellIndex = (leftMostCellIndex - 1 + cachedCells.Count) % cachedCells.Count;
        
        newPiece.SetData();
        newPiece.UpdateData(dataSource[currentItemCount - cachedCells.Count]);
        newPiece.LocalPosition = Vector3.left;
    }

    public void AddObjectToPalette(PuzzlePiece piece)
    {
        isRearranging = true;
        
        //Destroy Old Right Most Cell
        PuzzlePiece rightMostPiece = cachedCells[rightMostCellIndex];
        cachedCells.RemoveAt(rightMostCellIndex);
        rightMostPiece.SetParent(null);
        Destroy(rightMostPiece.gameObject);
        
        //Insert Piece At Correct Cached Cell Index
        piece.SetParent(palette,transform);
        piece.LocalScaleLerped = Vector3.one;
        float xDist = piece.Position.x - transform.position.x;
        int index = Mathf.Clamp(Mathf.CeilToInt(piece.LocalPosition.x / gap), 0, cachedCells.Count);
        //here index is wrapped by (count + 1) because old cell is removed from the list above
        int cacheInsertIndex = (leftMostCellIndex + index) % (cachedCells.Count + 1);
        cachedCells.Insert(cacheInsertIndex,piece);

        //Insert Piece Data At Correct Index
        int dataSourceInsertIndex = currentItemCount - cachedCells.Count + index;
        dataSource.Insert(dataSourceInsertIndex,piece.currentAssignedPieceData);
        
        var gridObj = PuzzleGenerator.Instance.PuzzleGrid.GetGridObject(piece.currentAssignedPieceData.gridCoordinate);
        gridObj.desiredPuzzlePiece = null;

        isRearranging = false;
    }
    
    public PuzzlePiece GetDraggedPiece(Vector2 worldPoint)
    {
        foreach (PuzzlePiece piece in cachedCells)
        {
            if (piece.MainCollider.OverlapPoint(worldPoint)) return piece;
        }

        return null;
    }

    public void UpdateThresholdValues(float zoomScaleFactor)
    {
        dragThreshold *= zoomScaleFactor;
        elevateOffset *= zoomScaleFactor;
    }
    
#if UNITY_EDITOR
    public void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(recyclingBounds.min.x,-200,0),new Vector3(recyclingBounds.min.x,200,0));
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(recyclingBounds.max.x,-200,0),new Vector3(recyclingBounds.max.x,200,0));
    }
#endif
}