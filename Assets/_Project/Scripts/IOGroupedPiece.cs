using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class IOGroupedPiece : IObject
{
    private List<PuzzlePiece> puzzlePieces;

    public bool isMoving { get; private set; }

    public override void Init()
    {
        puzzlePieces = new List<PuzzlePiece>();
    }

    public void AddPuzzlePieceToGroup(PuzzlePiece puzzlePiece)
    {
        puzzlePieces.Add(puzzlePiece);
        puzzlePiece.transform.parent = transform;
        puzzlePiece.group = this;
    }
    
    public void AddPuzzlePieceToGroup(List <PuzzlePiece> puzzlePiece)
    {
        puzzlePieces.AddRange(puzzlePiece);
    }

    protected override void OnReleased()
    {
        base.OnReleased();
        
        //Try Placement With Puzzle Board
        var puzzlePiece = puzzlePieces[0];
        Vector2 gridPos = PuzzleGenerator.Instance.PuzzleGrid.GetWorldPositionWithCellOffset(puzzlePiece.gridCoordinate.x, puzzlePiece.gridCoordinate.y);
        if (Vector2.Distance(gridPos, Position) < InteractiveSystem.gridSnapThreshold)
        {
            //disable Input by removing Isystem
            //set grid objects of each puzzle piece
            SetISystem(null);
            foreach (var piece in puzzlePieces)
            {
                piece.SetISystem(null);
                var gridObject = PuzzleGenerator.Instance.PuzzleGrid.GetGridObject(puzzlePiece.gridCoordinate.x, puzzlePiece.gridCoordinate.y);
                gridObject.AssignTargetPuzzlePiece(puzzlePiece);
            }
            transform.DOMove(gridPos, 0.14f).SetEase(Ease.OutQuad).onComplete += OnPuzzlePiecePlacedOnBoard;
            return;
        }
        
        //Check if Any of the piece in this group is close to other pieces
        foreach (PuzzlePiece piece in puzzlePieces)
        {
            foreach (KeyValuePair<int,Vector2Int> neighbourKeyValue in piece.neighbourCoordinates)
            {
                int sideIndex = neighbourKeyValue.Key;
                Vector2Int neighbourGridPos = neighbourKeyValue.Value;
            
                if (!PuzzleGenerator.Instance.PuzzleGrid.GetGridObject(neighbourGridPos.x, neighbourGridPos.y, out GridObject gridObject))
                    continue;
                
                Vector2 requiredDir = Quaternion.Euler(0, 0, -90 * sideIndex) * Vector2.left;
                PuzzlePiece neighbour = gridObject.desiredPuzzlePiece;

                if (IsNeighbourWithinRange(piece ,neighbour, requiredDir))
                {
                    piece.neighbourCoordinates.Remove(sideIndex);
                    neighbour.neighbourCoordinates.Remove(PuzzlePiece.GetOppositeSideIndex(sideIndex));
                    
                    //case 3 : neighbour is not in a group and this is in a group
                    if (neighbour.group == null)
                    {
                        neighbour.SetISystem(null);
                        isMoving = true;
                        Vector3 movePos = neighbour.Position -
                            ((Vector3)requiredDir.normalized * PuzzleGenerator.Instance.CellSize) + (Position - piece.Position);
                        transform.DOMove(movePos, 0.14f).SetEase(Ease.OutQuad).onComplete += () =>
                        {
                            AddPuzzlePieceToGroup(neighbour);
                            neighbour.SetISystem(iSystem);
                            isMoving = false;
                        };
                    }
                    //case 4 : both are in different group
                    else
                    {
                        neighbour.group.isMoving = true;
                        isMoving = true;
                        Vector3 movePos = neighbour.Position -
                            ((Vector3)requiredDir.normalized * PuzzleGenerator.Instance.CellSize) + (Position - piece.Position);
                        transform.DOMove(movePos, 0.14f).SetEase(Ease.OutQuad).onComplete += () =>
                        {
                            foreach (PuzzlePiece tempPieces in puzzlePieces)
                            {
                                neighbour.group.AddPuzzlePieceToGroup(tempPieces);
                            }

                            neighbour.group.isMoving = false;
                            isMoving = false;
                            Destroy(gameObject);
                        };
                    }
                    break;
                }
            }
        }
    }
    
    private void OnPuzzlePiecePlacedOnBoard()
    {
        foreach (PuzzlePiece piece in puzzlePieces)
        {
            piece.transform.parent = PuzzleGenerator.Instance.transform;
        }
        
        //Play PopSound To Let Know Puzzle Piece is fitted
        
        
        Destroy(gameObject);
    }
    
    private bool IsNeighbourWithinRange(PuzzlePiece piece, PuzzlePiece neighbour, Vector2 dirRef)
    {
        float distBetweenPiece = Vector2.Distance(piece.Position,  neighbour.Position);
        bool isWithinRange = distBetweenPiece > InteractiveSystem.gridSnapThreshold && distBetweenPiece < InteractiveSystem.neighbourSnapThreshold;
        bool isAligned = Vector2.Angle(dirRef, neighbour.Position - piece.Position) < 30;

        return isWithinRange && isAligned;
    }
}
