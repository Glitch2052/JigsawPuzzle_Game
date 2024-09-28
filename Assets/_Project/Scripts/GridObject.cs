public class GridObject : IGridObject
{
    private Grid<GridObject> grid;
    public int X { get; set; }
    public int Y { get; set; }

    public PuzzlePiece desiredPuzzlePiece;
    public PuzzlePiece targetPuzzlePiece { get; private set; }

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
        }
    }

    public override string ToString()
    {
        return "";
    }
}