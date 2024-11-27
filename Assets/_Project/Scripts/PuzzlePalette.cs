using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using SimpleJSON;
using UnityEngine;

public class PuzzlePalette : IObject
{
    [SerializeField] private PaletteContent content;

    private float newOrthoSize;
    private float oldOrthoSize;

    public float PaletteHeight => MainCollider.size.y * LocalScale.y;
    private float zoomScaleFactor;

    public override void Init()
    {
        content.Init(this);
        //here the ref orthographic size is assumed 16 because the palette scale and size were designed with orthographic size 16
        newOrthoSize = oldOrthoSize = 16;
        ScaleAndPositionPaletteWithCamera();
    }

    public void SetUpContentData()
    {
        content.SetUpData();
    }
    
    public override void IUpdate()
    {
        ScaleAndPositionPaletteWithCamera();
        content.IUpdate();
    }

    private void ScaleAndPositionPaletteWithCamera()
    {
        newOrthoSize = iSystem.Camera.orthographicSize;
        zoomScaleFactor = newOrthoSize / oldOrthoSize;
        content.UpdateThresholdValues(zoomScaleFactor);
        LocalScale = zoomScaleFactor * LocalScale.SetZ(1);
        
        oldOrthoSize = iSystem.Camera.orthographicSize;
        
        if(!iSystem.puzzleGenerator.IsLevelCompleted)
            LocalPosition = LocalPosition.SetY(-newOrthoSize + (PaletteHeight * 0.5f) + 1.5f * LocalScale.y);
    }

    public void AddObjectToPalette(PuzzlePiece puzzlePiece)
    {
        content.AddObjectToPalette(puzzlePiece);
    }

    public override IObject OnPointerDown(Vector2 worldPos, int pointerId)
    {
        return content.OnPointerDown(worldPos, pointerId);
    }

    public override IObject OnPointerDrag(Vector2 worldPos, int pointerId)
    {
        return content.OnPointerDrag(worldPos, pointerId);
    }

    public override IObject OnPointerUp(Vector2 worldPos, int pointerId)
    {
        return content.OnPointerUp(worldPos, pointerId);
    }

    public bool AssignPuzzlePieceOnGrid()
    {
        if (content.RemoveRandomPieceFromPalette(out PuzzlePiece puzzlePiece))
        {
            //TODO : Assign Piece to Grid
            Vector2Int gridPos = puzzlePiece.gridCoordinate;
            Vector3 worldPos = iSystem.puzzleGenerator.PuzzleGrid.GetWorldPositionWithCellOffset(gridPos.x, gridPos.y);
            puzzlePiece.Position = worldPos.SetZ(-2);
            puzzlePiece.OnReleased();
            return true;
        }
        return false;
    }

    public void SortByCorners(bool value)
    {
        content.SortByCorners(value);
    }

    public Tween FadeOutPaletteOnLevelComplete()
    {
        Tween tween = transform.DOLocalMoveY(-newOrthoSize - (PaletteHeight * 0.5f) - 3f, 1f);
        tween.SetDelay(0.4f);
        tween.SetEase(Ease.OutQuad);
        return tween;
    }
}
