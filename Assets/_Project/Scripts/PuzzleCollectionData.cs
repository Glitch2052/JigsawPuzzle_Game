using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Puzzle Data/Puzzle Collection Data",fileName = "Puzzle Collection Data")]
public class PuzzleCollectionData : ScriptableObject
{
    public List<PuzzleTextureData> generalTextureData;
    
    
#if UNITY_EDITOR
    [Space(40)]
    [Header("Editor Data")]
    [SerializeField] private Sprite[] generalTextures;
    
    [ContextMenu("Add General Textures")]
    public void AddGeneralTextures()
    {
        foreach (var texture in generalTextures)
        {
            generalTextureData.Add(new PuzzleTextureData()
            {
                themeName = ThemeName.General,
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
    General = 0,
}