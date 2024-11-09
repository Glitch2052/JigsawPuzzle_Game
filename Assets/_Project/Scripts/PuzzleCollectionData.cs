using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Puzzle Data/Puzzle Collection Data",fileName = "Puzzle Collection Data")]
public class PuzzleCollectionData : ScriptableObject
{
    public ThemeName themeName;
    public List<PuzzleTextureData> textureData;
    
    
#if UNITY_EDITOR
    [Space(40)]
    [Header("Editor Data")]
    [SerializeField] private Texture2D[] spritesList;
    
    [ContextMenu("Add Texture Data")]
    public void AddTextures()
    {
        foreach (var texture in spritesList)
        {
            textureData.Add(new PuzzleTextureData()
            {
                themeName = themeName,
                texture = texture
            });
        }
    }
#endif
}

[Serializable]
public class PuzzleTextureData
{
    public ThemeName themeName;
    public Texture2D texture;
}

public class CustomPuzzleTexData : PuzzleTextureData
{
    public string texturePath = String.Empty;
    public string jsonPath = StringID.Empty;
    public bool isTextureLoaded = false;
}

public enum ThemeName
{
    General = 0,
    Animals = 1,
    Cities = 2,
    Landmarks = 3,
    Nature = 4,
    Vehicles = 5,
    
    
    Custom = 1000
}