using System;

public class GridObject : IGridObject
{
    private Grid<GridObject> grid;
    public int X { get; set; }
    public int Y { get; set; }

    public string desiredEdgeShape;
    public string TargetEdgeShape { get; private set; }

    public PuzzlePiece desiredPuzzlePiece;
    public PuzzlePiece targetPuzzlePiece { get; private set; }

    public event Action OnCorrectPuzzlePieceAssigned;

    public GridObject(Grid<GridObject> grid, int x, int y)
    {
        this.grid = grid;
        X = x;
        Y = y;
    }

    public void AssignTargetPuzzlePiece(PuzzlePiece puzzlePiece)
    {
        if (desiredPuzzlePiece == puzzlePiece)
        {
            targetPuzzlePiece = puzzlePiece;
            puzzlePiece.SetISystem(null);
            UIManager.Instance.IncrementPieceCounterDisplay(1f / PuzzleGenerator.Instance.TotalPieceCount);
            SoundManager.Instance.PlayOneShot(StringID.SfxPlacingOnCorrectSpot);
            OnCorrectPuzzlePieceAssigned?.Invoke();
            OnCorrectPuzzlePieceAssigned = null;
        }
    }

    public void AssignSavedPuzzlePieceOnSceneLoad(PuzzlePiece puzzlePiece)
    {
        if (desiredPuzzlePiece == puzzlePiece)
        {
            targetPuzzlePiece = puzzlePiece;
            puzzlePiece.SetISystem(null);
            UIManager.Instance.IncrementPieceCounterDisplay(1f / PuzzleGenerator.Instance.TotalPieceCount);
            OnCorrectPuzzlePieceAssigned?.Invoke();
            OnCorrectPuzzlePieceAssigned = null;
        }
    }

    public override string ToString()
    {
        return "";
    }
}