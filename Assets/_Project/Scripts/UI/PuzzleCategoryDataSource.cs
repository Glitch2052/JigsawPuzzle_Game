using System.Collections.Generic;
using PolyAndCode.UI;

public class PuzzleCategoryDataSource : IRecyclableScrollRectDataSource
{
    public PuzzleCollectionData PuzzleCollectionData { get; private set; }
    private List<PuzzleTextureData> puzzleTextureData;

    public PuzzleCategoryDataSource() { }
    public PuzzleCategoryDataSource(PuzzleCollectionData collectionData)
    {
        PuzzleCollectionData = collectionData;
        puzzleTextureData = collectionData.textureData;
    }

    public void UpdateTextureCollection(PuzzleCollectionData collectionData)
    {
        PuzzleCollectionData = collectionData;
        puzzleTextureData = collectionData.textureData;
    }
    
    public int GetItemCount()
    {
        return puzzleTextureData.Count;
    }

    public void InitCell(ICell cell)
    {
        ((PuzzleItemCell)cell).InitCell();
    }

    public void SetCell(ICell cell, int index)
    {
        ((PuzzleItemCell)cell).SetCell(puzzleTextureData[index]);
    }
}
