using System.Collections.Generic;
using UnityEngine;

public class IOGroupedPiece : IObject
{
    private List<Vector2Int> neighbourCoordinates;
    private List<PuzzlePiece> puzzlePieces;

    public override void Init()
    {
        neighbourCoordinates = new List<Vector2Int>();
        puzzlePieces = new List<PuzzlePiece>();
    }
    
    
}
