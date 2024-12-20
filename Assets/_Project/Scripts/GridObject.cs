﻿using System;

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
            OnCorrectPuzzlePieceAssigned?.Invoke();
            OnCorrectPuzzlePieceAssigned = null;
        }
    }

    public override string ToString()
    {
        return "";
    }
}