using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceLocations;
#if UNITY_EDITOR
using System.Linq;
#endif

[CreateAssetMenu(menuName = "Puzzle Data/Puzzle Collection Data",fileName = "Puzzle Collection Data")]
public class PuzzleCollectionData : ScriptableObject
{
    public ThemeName themeName;
    public List<PuzzleTextureData> textureData;

    // private Dictionary<string, IResourceLocation> keyToLocationMapDict;

    public void SetUpData()
    {
        var keyToLocationMapDict = new Dictionary<string, IResourceLocation>();
        IList<IResourceLocation> locations = AssetLoader.Instance.GetResourceLocations(themeName.ToString());
        foreach (IResourceLocation location in locations)
        {
            keyToLocationMapDict[location.PrimaryKey] = location;
        }

        foreach (PuzzleTextureData data in textureData)
        {
            data.iconResourceLocation = keyToLocationMapDict.GetValueOrDefault(data.iconTextureKey);
            data.texResourceLocation = keyToLocationMapDict.GetValueOrDefault(data.textureKey);
        }
    }

    public IResourceLocation GetIconTextureResourceLocationKey()
    {
        return textureData[0].iconResourceLocation;
    }
    // public IResourceLocation GetResourceLocation(string key)
    // {
    //     return keyToLocationMapDict.GetValueOrDefault(key);
    // }
    
#if UNITY_EDITOR
    [Space(40)] [Header("Editor Data")] 
    [SerializeField] private UnityEngine.Object spriteFolder;
    // [SerializeField] private Texture2D[] spritesList;
    
    [ContextMenu("Add Texture Data")]
    public void AddTextures()
    {
        textureData.Clear();
        
        string folderPath = AssetDatabase.GetAssetPath(spriteFolder);
        string[] assetGuids = AssetDatabase.FindAssets("t:Sprite", new[] { folderPath });

        List<Sprite> iconTextures = new List<Sprite>();
        List<Sprite> puzzleTextures = new List<Sprite>();

        foreach (string guid in assetGuids)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guid));
            if (sprite.name.Contains("Icon"))
            {
                iconTextures.Add(sprite);
            }
            else
            {
                puzzleTextures.Add(sprite);
            }
        }
        
        foreach (var puzzleSprite in puzzleTextures)
        {
            Sprite iconSprite = iconTextures.FirstOrDefault(s => s.name.Contains(puzzleSprite.name));
            textureData.Add(new PuzzleTextureData()
            {
                themeName = themeName,
                name = puzzleSprite.name,
                textureKey = GetKey(puzzleSprite),
                iconTextureKey = GetKey(iconSprite)
            });
        }
    }

    private string GetKey(Sprite sprite)
    {
        if (sprite == null) return "NONE";
        string key = AssetDatabase.GetAssetPath(sprite);
        return key.Substring(key.IndexOf(themeName.ToString(), StringComparison.Ordinal));
    }
#endif
}

[Serializable]
public class PuzzleTextureData
{
    public ThemeName themeName;
    public string name;
    public string iconTextureKey;
    public string textureKey;

    public IResourceLocation iconResourceLocation;
    public IResourceLocation texResourceLocation;
}

public class CustomPuzzleTexData : PuzzleTextureData
{
    public Texture2D customTexture;
    public string texturePath = String.Empty;
    public string jsonPath = StringID.Empty;
    public bool isTextureLoaded = false;
    public bool isEmptyDisplaySlot = false;
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