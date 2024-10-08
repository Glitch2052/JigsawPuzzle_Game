using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Puzzle Data/Puzzle Texture Database",fileName = "puzzleTexture Database")]
public class PuzzleTextureDatabase : ScriptableObject
{
    public List<PuzzleTextureData> generalTextureData;
    
    
#if UNITY_EDITOR
    [Space(40)]
    [Header("Editor Data")]
    [SerializeField] private Texture2D[] generalTextures;
    
    [ContextMenu("Add General Textures")]
    public void AddGeneralTextures()
    {
        foreach (var texture in generalTextures)
        {
            generalTextureData.Add(new PuzzleTextureData()
            {
                themeName = ThemeName.General,
                texture = texture
            });
        }
    }
#endif
}

[Serializable]
public struct PuzzleTextureData
{
    public ThemeName themeName;
    public Texture2D texture;
}

public enum ThemeName
{
    General = 0,
}