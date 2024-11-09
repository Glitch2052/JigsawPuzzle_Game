using System.Collections.Generic;
using PolyAndCode.UI;
using SimpleJSON;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private PuzzleCollectionData generalCollectionData;
    [SerializeField] private List<PuzzleCollectionData> themesCollectionData;
    
    [Space(20)]
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform levelSelectPanel;
    [SerializeField] private RectTransform gamePlayOptionsPanel;
    
    [Space(20)]
    [SerializeField] private RecyclableScrollRect mainCategoryScrollRect;
    [SerializeField] private RecyclableScrollRect themeCategoryScrollRect;
    [SerializeField] private RecyclableScrollRect customCategoryScrollRect;
    [SerializeField] private ScrollRect themesScrollRect;

    [Space(20)] 
    public TabButton firstCategory;
    [SerializeField] private Button themeButtonPrefab;
    [SerializeField] private Button backButton;
    [SerializeField] private RectTransform continuePanel;
    [SerializeField] private RectTransform sizeOptionsPanel;
    [SerializeField] private RawImage continuePuzzleDisplay;
    [SerializeField] private RawImage newPuzzleDisplay;

    private PuzzleCategoryDataSource mainCategoryContentDataSource;
    private PuzzleCategoryDataSource themeCategoryContentDataSource;
    private CustomPuzzleCategoryDataSource customCategoryContentDataSource;
    private PuzzleTextureData currentTextureData;

    private readonly int[] sizeOptions =
    {
        36, 64, 81, 100, 144, 225
    };

    private int selectedSizeIndex = 0;
    
    public static UIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(gameObject);
    }

    public void Init()
    {
        
    }
    
    public void LoadMainCategory()
    {
        mainCategoryContentDataSource ??= new PuzzleCategoryDataSource(ThemeName.General, generalCollectionData.textureData);
        mainCategoryScrollRect.Initialize(mainCategoryContentDataSource);
    }

    public void UnloadMainCategory()
    {
        mainCategoryScrollRect.ClearData();
    }

    public void LoadThemeOptions()
    {
        foreach (PuzzleCollectionData data in themesCollectionData)
        {
            Button button = Instantiate(themeButtonPrefab,themesScrollRect.content);
            TextMeshProUGUI btnText = button.GetComponentInChildren<TextMeshProUGUI>();
            btnText.text = data.themeName.ToString();
            button.onClick.AddListener(() =>
            {
                LoadThemeCategory(data);
            });
        }
    }

    public void UnloadThemeOptions()
    {
        themeCategoryScrollRect.ClearData();
        themeCategoryScrollRect.gameObject.SetActive(false);
        foreach (Transform child in themesScrollRect.content)
        {
            Destroy(child.gameObject);
        }
    }

    public void LoadCustomCategory()
    {
        customCategoryScrollRect.gameObject.SetActive(true);
        customCategoryContentDataSource ??= new CustomPuzzleCategoryDataSource(ThemeName.Custom);
        customCategoryScrollRect.Initialize(customCategoryContentDataSource);
    }

    public void UnloadCustomCategory()
    {
        customCategoryScrollRect.ClearData();
        customCategoryScrollRect.gameObject.SetActive(false);
    }

    public void AddCustomTexturePath(string path, Texture2D savedTexture)
    {
        if (path != string.Empty && path != "" && customCategoryContentDataSource != null)
        {
            customCategoryContentDataSource.AddNewPath(path, savedTexture);
        }
        customCategoryScrollRect.ReloadData();
    }

    private void LoadThemeCategory(PuzzleCollectionData puzzleCollectionData)
    {
        backButton.gameObject.SetActive(true);
        themeCategoryScrollRect.gameObject.SetActive(true);
        themeCategoryContentDataSource ??= new PuzzleCategoryDataSource(puzzleCollectionData.themeName);
        
        themeCategoryContentDataSource.UpdateTextureCollection(puzzleCollectionData.textureData);
        themeCategoryScrollRect.Initialize(themeCategoryContentDataSource);
    }
    
    public void UnLoadThemeCategory()
    {
        themeCategoryScrollRect.ClearData();
        themeCategoryScrollRect.gameObject.SetActive(false);
        backButton.gameObject.SetActive(false);
    }

    public void EnableContinuePanel(PuzzleTextureData textureData)
    {
        currentTextureData = textureData;
        continuePuzzleDisplay.texture = currentTextureData.texture;
        continuePanel.gameObject.SetActive(true);
    }

    public void CancelContinuePanel()
    {
        continuePanel.gameObject.SetActive(false);
    }

    public void EnableSizeOption(PuzzleTextureData textureData)
    {
        currentTextureData = textureData;
        newPuzzleDisplay.texture = currentTextureData.texture;
        sizeOptionsPanel.gameObject.SetActive(true);
    }

    public void EnableSizeOption()
    {
        CancelContinuePanel();
        EnableSizeOption(currentTextureData);
    }
    
    public void DisableSizeOption()
    {
        sizeOptionsPanel.gameObject.SetActive(false);
    }

    public void StartNewGame()
    {
        DisableSizeOption();
        JSONNode configData = new JSONObject();
        configData[StringID.BoardSize] = sizeOptions[selectedSizeIndex];
        configData[StringID.PuzzleSceneID] = $"{currentTextureData.themeName}/{currentTextureData.texture.name}.json";
        configData[StringID.NewGame] = 1;
        GameManager.Instance.LoadScene(currentTextureData, configData);
    }

    public void ContinueGameFromLastSave()
    {
        CancelContinuePanel();
        JSONNode configData = new JSONObject();
        configData[StringID.PuzzleSceneID] = $"{currentTextureData.themeName}/{currentTextureData.texture.name}.json";
        GameManager.Instance.LoadScene(currentTextureData, configData);
    }

    public void UpdateSelectedSizeIndex(int index)
    {
        index = Mathf.Clamp(index, 0, sizeOptions.Length);
        selectedSizeIndex = index;
    }

    public void ToggleLevelSelectPanel(bool value)
    {
        levelSelectPanel.gameObject.SetActive(value);
    }
    
    public void ToggleGameplayOptionsPanel(bool value)
    {
        gamePlayOptionsPanel.gameObject.SetActive(value);
    }

    public void ToggleReferenceImage(bool value)
    {
        PuzzleGenerator.Instance.ToggleReferenceImage(value);
    }
}