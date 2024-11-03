using System.Collections;
using System.Collections.Generic;
using PolyAndCode.UI;
using UnityEngine;

public class PuzzleCategoryDataSource : IRecyclableScrollRectDataSource
{
    private ThemeName themeName;
    private List<PuzzleTextureData> puzzleTextureData;

    public PuzzleCategoryDataSource(ThemeName themeName, List<PuzzleTextureData> textureData)
    {
        this.themeName = themeName;
        puzzleTextureData = textureData;
    }
    
    public int GetItemCount()
    {
        return puzzleTextureData.Count;
    }

    public void InitCell(ICell cell)
    {
        ((PuzzleCategoryCell)cell).InitCell(themeName);
    }

    public void SetCell(ICell cell, int index)
    {
        ((PuzzleCategoryCell)cell).SetCell(puzzleTextureData[index]);
    }
}
