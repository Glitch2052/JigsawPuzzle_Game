using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using PolyAndCode.UI;
using SimpleJSON;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private PuzzleCollectionData generalCollectionData;
    [SerializeField] private List<PuzzleCollectionData> themesCollectionData;
    
    [Space(30)]
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform levelSelectPanel;
    [SerializeField] private RectTransform gamePlayOptionsPanel;
    [SerializeField] private RectTransform headerTransform;
    [SerializeField] private RectTransform bodyTransform;
    [SerializeField] private RectTransform footerTransform;
    
    [Space(30)]
    [SerializeField] private RecyclableScrollRect mainCategoryScrollRect;
    [SerializeField] private RecyclableScrollRect themeCategoryScrollRect;
    [SerializeField] private RecyclableScrollRect customCategoryScrollRect;
    [SerializeField] private RecyclableScrollRect themesOptionScrollRect;
    [SerializeField] private ScrollRect bgOptionsScrollRect;

    [Space(30)] 
    public TabButton firstCategory;
    [SerializeField] private Button themeButtonPrefab;
    [SerializeField] private Button backButton;
    [SerializeField] private RectTransform continuePanel;
    [SerializeField] private RectTransform sizeOptionsPanel;
    [SerializeField] private RawImage continuePuzzleDisplay;
    [SerializeField] private RawImage newPuzzleDisplay;
    [SerializeField] private Toggle openGalleryToggleBtn;

    [Space(30)] 
    [SerializeField] private RectTransform gameHeaderPanel;
    [SerializeField] private RectTransform timerPanel;
    [SerializeField] private TextMeshProUGUI totalTimeText;
    [SerializeField] private BGTextureCell bgTextureCellPrefab;
    [SerializeField] private Toggle changeBgToggleBtn;
    [SerializeField] private Image piecesCounter;
    public Sprite plusIconSprite;

    private PuzzleCategoryDataSource mainCategoryContentDataSource;
    private PuzzleCategoryDataSource themeCategoryContentDataSource;
    private ThemeOptionDataSource themeOptionDataSource;
    private CustomPuzzleCategoryDataSource customCategoryContentDataSource;
    private PuzzleTextureData currentTextureData;

    private Vector2 safeArea;
    private Vector2 unSafeArea;
    
    private readonly int[] sizeOptions =
    {
        36, 64, 81, 100, 144, 225
    };

    private int selectedSizeIndex = 0;
    private static bool firstTimeLoad = false;
    
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
        safeArea = new Vector2(Screen.safeArea.width, Screen.safeArea.height + Screen.safeArea.y);
        unSafeArea = new Vector2(Screen.width, Screen.height) - safeArea;
        
        Vector2 bodySizeDelta = bodyTransform.sizeDelta;
        bodySizeDelta.y = canvas.GetComponent<RectTransform>().sizeDelta.y - headerTransform.sizeDelta.y - footerTransform.sizeDelta.y - unSafeArea.y;
        bodyTransform.sizeDelta = bodySizeDelta;
        bodyTransform.anchoredPosition = bodyTransform.anchoredPosition.SetY(-unSafeArea.y * 0.5f);

        headerTransform.sizeDelta = headerTransform.sizeDelta.SetY(headerTransform.sizeDelta.y + unSafeArea.y);
        gameHeaderPanel.sizeDelta = gameHeaderPanel.sizeDelta.SetY(gameHeaderPanel.sizeDelta.y + unSafeArea.y);
        RectTransform bgScrollTransform = bgOptionsScrollRect.GetComponent<RectTransform>();
        bgScrollTransform.offsetMax = bgScrollTransform.offsetMax.SetY(-unSafeArea.y);
        
        generalCollectionData.SetUpData();
        foreach (var data in themesCollectionData)
        {
            data.SetUpData();
        }
    }
    
    public void LoadMainCategory()
    {
        mainCategoryContentDataSource ??= new PuzzleCategoryDataSource(generalCollectionData);
        mainCategoryScrollRect.Initialize(mainCategoryContentDataSource);
    }

    public void UnloadMainCategory()
    {
        mainCategoryScrollRect.ClearData();
    }

    public void LoadThemeOptions()
    {
        themesOptionScrollRect.gameObject.SetActive(true);
        themeOptionDataSource ??= new ThemeOptionDataSource(themesCollectionData);
        themesOptionScrollRect.Initialize(themeOptionDataSource);
    }

    public void UnloadThemeOptions()
    {
        themesOptionScrollRect.ClearData();
        themeCategoryScrollRect.ClearData();
        themeCategoryScrollRect.gameObject.SetActive(false);
        themesOptionScrollRect.gameObject.SetActive(false);
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

    public void LoadThemeCategory(PuzzleCollectionData puzzleCollectionData)
    {
        backButton.gameObject.SetActive(true);
        themeCategoryScrollRect.gameObject.SetActive(true);
        themeCategoryContentDataSource ??= new PuzzleCategoryDataSource();
        
        themeCategoryContentDataSource.UpdateTextureCollection(puzzleCollectionData);
        themeCategoryScrollRect.Initialize(themeCategoryContentDataSource);
    }
    
    public void UnLoadThemeCategory()
    {
        themeCategoryScrollRect.ClearData();
        themeCategoryScrollRect.gameObject.SetActive(false);
        backButton.gameObject.SetActive(false);
    }

    public async void EnableContinuePanel(PuzzleTextureData textureData)
    {
        currentTextureData = textureData;
        if (textureData is CustomPuzzleTexData customPuzzleTexData)
            continuePuzzleDisplay.texture = customPuzzleTexData.customTexture;
        else
            continuePuzzleDisplay.texture = await AssetLoader.Instance.LoadAssetAsync<Texture2D>(currentTextureData.iconTextureKey);;
        continuePanel.gameObject.SetActive(true);
    }

    public void CancelContinuePanel()
    {
        continuePanel.gameObject.SetActive(false);
    }

    public async void EnableSizeOption(PuzzleTextureData textureData)
    {
        currentTextureData = textureData;
        if (textureData is CustomPuzzleTexData customPuzzleTexData)
        {
            newPuzzleDisplay.texture = customPuzzleTexData.customTexture;
        }
        else
        {
            newPuzzleDisplay.texture = await AssetLoader.Instance.LoadAssetAsync<Texture2D>(currentTextureData.iconTextureKey);
        }
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

    private void LoadBgOptions()
    {
        var iSystem = GameManager.Instance.iSystem;
        if (iSystem != null)
        {
            var bgSprites = GameManager.Instance.iSystem.puzzleGenerator.backGroundSpriteOptions;
            bgOptionsScrollRect.content.TryGetComponent(out ToggleGroup toggleGroup);

            int selectedIndex = iSystem.puzzleGenerator.selectedBgIndex;
            for (var i = 0; i < bgSprites.Count; i++)
            {
                var bgTexCell = Instantiate(bgTextureCellPrefab, bgOptionsScrollRect.content).SetToggleGroup(toggleGroup);
                bgTexCell.UpdateCell(bgSprites[i], iSystem);
                if(selectedIndex == i)
                    bgTexCell.ToggleClick(true);
            }
        }
    }

    private void ClearBgOptions()
    {
        foreach (Transform child in bgOptionsScrollRect.content.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void OnHintButtonClicked()
    {
        var iSystem = GameManager.Instance.iSystem;
        if (iSystem != null && iSystem.palette.AssignPuzzlePieceOnGrid())
        {
            
        }
    }

    public void ToggleCornerSortingOption(bool value)
    {
        var iSystem = GameManager.Instance.iSystem;
        if (iSystem != null)
        {
            iSystem.palette.SortByCorners(value);
        }
    }

    public void StartNewGame()
    {
        DisableSizeOption();
        JSONNode configData = new JSONObject();
        configData[StringID.BoardSize] = sizeOptions[selectedSizeIndex];
        if (currentTextureData is CustomPuzzleTexData customPuzzleTexData)
        {
            configData[StringID.PuzzleSceneID] = customPuzzleTexData.jsonPath;
        }
        else
        {
            configData[StringID.PuzzleSceneID] = $"{currentTextureData.themeName}/{currentTextureData.name}.json";
        }
        configData[StringID.NewGame] = 1;
        configData.SetNextSceneType(SceneType.GameScene);
        GameManager.Instance.LoadScene(currentTextureData, configData);
    }

    public void ContinueGameFromLastSave()
    {
        CancelContinuePanel();
        JSONNode configData = new JSONObject();
        if (currentTextureData is CustomPuzzleTexData customPuzzleTexData)
        {
            configData[StringID.PuzzleSceneID] = customPuzzleTexData.jsonPath;
        }
        else
        {
            configData[StringID.PuzzleSceneID] = $"{currentTextureData.themeName}/{currentTextureData.name}.json";
        }
        configData.SetNextSceneType(SceneType.GameScene);
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

    public void ToggleGalleryOptionGameObject()
    {
        openGalleryToggleBtn.isOn = !openGalleryToggleBtn.isOn;
    }

    public void SetPieceCounterDisplay(float value)
    {
        piecesCounter.fillAmount = value;
    }

    public void IncrementPieceCounterDisplay(float value)
    {
        piecesCounter.fillAmount += value;
    }
    
    public void OnBack()
    {
        if (changeBgToggleBtn.isOn)
        {
            changeBgToggleBtn.isOn = false;
            return;
        }
        InteractiveSystem iSystem = GameManager.Instance.iSystem;
        if(iSystem)
            iSystem.OnBack();
    }

    public IEnumerator OnSceneLoad(JSONNode configData)
    {
        if (configData.GetNextSceneType() == SceneType.LevelSelect)
        {
            ToggleLevelSelectPanel(true);
            ToggleGameplayOptionsPanel(false);
            ClearBgOptions();

            if (!firstTimeLoad)
            {
                firstCategory.OnPointerClick(new PointerEventData(EventSystem.current));
                firstTimeLoad = true;
            }
        }
        if (configData.GetNextSceneType() == SceneType.GameScene)
        {
            ToggleLevelSelectPanel(false);
            ToggleGameplayOptionsPanel(true);
            LoadBgOptions();
        }
        timerPanel.gameObject.SetActive(false);
        timerPanel.anchoredPosition = timerPanel.anchoredPosition.SetY(-500f);
        yield return null;
    }

    public Tween FadeUIOnSceneComplete()
    {
        timerPanel.gameObject.SetActive(true);
        Sequence sequence = DOTween.Sequence();
        sequence.Append(gameHeaderPanel.DOAnchorPosY(gameHeaderPanel.sizeDelta.y, 1f).SetDelay(0.4f).SetEase(Ease.OutQuad));
        sequence.Append(timerPanel.DOAnchorPosY(0f,1f).SetDelay(0.4f).SetEase(Ease.OutQuad));
        return sequence;
    }

    public void UpdateTotalTimerCompletionText(double timeInSeconds)
    {
        //format time
        totalTimeText.text = $"Total Time: {FormatSecondsToHms(timeInSeconds)}";
    }
    
    private static string FormatSecondsToHms(double totalSeconds)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(totalSeconds);

        // Display only the relevant parts:
        if (timeSpan.Hours > 0)
        {
            return $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }
        if (timeSpan.Minutes > 0)
        {
            return $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }
        return timeSpan.Seconds + " seconds";
    }
}

public class Animal{
    public virtual async Task DoAction()
    {
        
    }
}

public class Dog : Animal
{
    public override async Task DoAction()
    {
        
    }
}

