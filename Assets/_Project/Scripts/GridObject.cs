using Unity.Jobs;

public class GridObject : IGridObject
{
    private Grid<GridObject> grid;
    public int X { get; set; }
    public int Y { get; set; }

    public PuzzlePiece puzzlePiece;

    public GridObject(Grid<GridObject> grid, int x, int y)
    {
        this.grid = grid;
        X = x;
        Y = y;
    }

    public override string ToString()
    {
        return "";
    }
}