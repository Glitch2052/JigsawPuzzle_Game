using System.Collections.Generic;
using PolyAndCode.UI;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private PuzzleCollectionData generalCollectionData;
    [SerializeField] private List<PuzzleCollectionData> themesCollectionData;
    
    [Space(10)]
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform levelSelectPanel;
    [SerializeField] private RectTransform gamePlayOptionsPanel;
    
    [Space(10)]
    [SerializeField] private RecyclableScrollRect mainCategoryScrollRect;
    [SerializeField] private RectTransform continuePanel;
    [SerializeField] private RectTransform sizeOptionsPanel;
    [SerializeField] private Image continuePuzzleDisplay;
    [SerializeField] private Image newPuzzleDisplay;

    private PuzzleCategoryDataSource mainCategoryContentDataSource;
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
        LoadMainCategory();
    }
    
    

    private void LoadMainCategory()
    {
        mainCategoryContentDataSource ??= new PuzzleCategoryDataSource(ThemeName.General, generalCollectionData.textureData);
        mainCategoryScrollRect.Initialize(mainCategoryContentDataSource);
    }

    public void EnableContinuePanel(PuzzleTextureData textureData)
    {
        currentTextureData = textureData;
        continuePuzzleDisplay.sprite = currentTextureData.sprite;
        continuePanel.gameObject.SetActive(true);
    }

    public void CancelContinuePanel()
    {
        continuePanel.gameObject.SetActive(false);
    }

    public void EnableSizeOption(PuzzleTextureData textureData)
    {
        currentTextureData = textureData;
        newPuzzleDisplay.sprite = currentTextureData.sprite;
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
        JSONNode configData = new JSONObject();
        configData[StringID.BoardSize] = sizeOptions[selectedSizeIndex];
        configData[StringID.PuzzleSceneID] = $"{currentTextureData.themeName}/{currentTextureData.sprite.name}.json";
        configData[StringID.NewGame] = 1;
        GameManager.Instance.LoadScene(currentTextureData, configData);
    }

    public void ContinueGameFromLastSave()
    {
        CancelContinuePanel();
        JSONNode configData = new JSONObject();
        configData[StringID.PuzzleSceneID] = $"{currentTextureData.themeName}/{currentTextureData.sprite.name}.json";
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