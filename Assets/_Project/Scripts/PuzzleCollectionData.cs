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
    [SerializeField] private Sprite[] spritesList;
    
    [ContextMenu("Add Texture Data")]
    public void AddTextures()
    {
        foreach (var texture in spritesList)
        {
            textureData.Add(new PuzzleTextureData()
            {
                themeName = themeName,
                sprite = texture
            });
        }
    }
#endif
}

[Serializable]
public struct PuzzleTextureData
{
    public ThemeName themeName;
    public Sprite sprite;
}

public enum ThemeName
{
    General,
    Animals,
    Cities,
    Landmarks,
    Nature,
    Vehicles
}