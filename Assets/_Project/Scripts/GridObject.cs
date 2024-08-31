public class GridObject : IGridObject
{
    private Grid<GridObject> grid;
    public int X { get; set; }
    public int Y { get; set; }

    public PuzzlePiece desiredPuzzlePiece;

    public PuzzlePiece targetPuzzlePiece;

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
        }
    }

    public override string ToString()
    {
        return "";
    }
}