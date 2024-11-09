using System.Collections.Generic;
using System.Linq;
using PolyAndCode.UI;
using UnityEngine;

public class CustomPuzzleCategoryDataSource : IRecyclableScrollRectDataSource
{
    private ThemeName themeName;
    private List<PuzzleTextureData> puzzleTextureData;
    private List<Texture2D> textureList;
    private List<string> pathToTextureList;

    public CustomPuzzleCategoryDataSource(ThemeName themeName)
    {
        this.themeName = themeName;
        textureList = new List<Texture2D>();
        pathToTextureList = StorageManager.GetFilesInDirectory(StringID.CustomFolderName).ToList();
    }

    public void AddNewPath(string path,Texture2D texture)
    {
        pathToTextureList.Add(path);
        textureList.Add(texture);
    }

    public int GetItemCount()
    {
        return pathToTextureList.Count;
    }

    public void InitCell(ICell cell)
    {
        ((CustomPuzzleCategoryCell)cell).InitCell(themeName,this);
    }

    public void SetCell(ICell cell, int index)
    {
        ((CustomPuzzleCategoryCell)cell).SetCell(textureList[index], pathToTextureList[index]);
    }
}
