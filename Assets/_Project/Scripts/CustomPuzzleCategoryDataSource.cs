using System;
using System.Collections.Generic;
using System.Linq;
using PolyAndCode.UI;
using UnityEngine;

public class CustomPuzzleCategoryDataSource : IRecyclableScrollRectDataSource
{
    private readonly ThemeName themeName;
    private readonly List<CustomPuzzleTexData> puzzleTextureDataList;
    private readonly List<string> pathToTextureList;

    public CustomPuzzleCategoryDataSource(ThemeName themeName)
    {
        this.themeName = themeName;
        pathToTextureList = StorageManager.GetFilesInDirectory(StringID.CustomTextureFolder).ToList();
        puzzleTextureDataList = new List<CustomPuzzleTexData>
        {
            new CustomPuzzleTexData()
            {
                themeName = themeName,
                isTextureLoaded = false,
                isEmptyDisplaySlot = true
            }
        };
        foreach (string texturePath in pathToTextureList)
        {
            var customData = new CustomPuzzleTexData
            {
                themeName = themeName,
                isTextureLoaded = false,
                texturePath = texturePath[texturePath.IndexOf(StringID.CustomFolderName, StringComparison.Ordinal)..].Replace('\\','/'),
            };
            customData.jsonPath = customData.texturePath.Replace(StringID.Textures + "/", "").Replace(".png",".json");
            puzzleTextureDataList.Add(customData);
        }
    }

    public void AddNewPath(string path,Texture2D texture)
    {
        puzzleTextureDataList.Insert(0,new CustomPuzzleTexData
        {
            themeName = themeName,
            customTexture = texture,
            texturePath = path,
            isTextureLoaded = true
        });
        pathToTextureList.Insert(0, path);
    }

    public int GetItemCount()
    {
        return puzzleTextureDataList.Count;
    }

    public void InitCell(ICell cell)
    {
        ((CustomPuzzleItemCell)cell).InitCell(themeName,this);
    }

    public void SetCell(ICell cell, int index)
    {
        ((CustomPuzzleItemCell)cell).SetCell(puzzleTextureDataList[index]);
    }
}
