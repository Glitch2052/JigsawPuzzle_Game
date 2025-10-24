using DG.Tweening;
using PolyAndCode.UI;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleItemCell : MonoBehaviour, ICell
{
    [SerializeField] private Button button;
    [SerializeField] private RawImage buttonImage;
    [SerializeField] private RectTransform lockTransform;
    
    private PuzzleTextureData puzzleTextureData;
    private bool isPuzzleSolved = false;

    public void InitCell()
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(LoadPuzzleScene);
    }

    public async void SetCell(PuzzleTextureData data)
    {
        puzzleTextureData = data;
        buttonImage.texture = await AssetLoader.Instance.LoadAssetAsync<Texture2D>(data.iconResourceLocation);
        isPuzzleSolved = !data.isLocked || data.previousItem == null || PlayerPrefs.GetInt(data.previousItem.textureKey, 0) == 1;

        buttonImage.material = isPuzzleSolved ? Graphic.defaultGraphicMaterial : PuzzleCategoryDataSource.DeSaturatedMaterial;
        if (lockTransform)
            lockTransform.gameObject.SetActive(puzzleTextureData.isLocked && !isPuzzleSolved);
    }

    private void LoadPuzzleScene()
    {
        if (puzzleTextureData.isLocked && !isPuzzleSolved)
        {
            lockTransform.DOKill(true);
            lockTransform.DOPunchScale(Vector3.one * 0.25f, 0.4f);
            UIManager.Instance.PlayCompletePrevLevelPanelAnimation();
            return;
        }
        
        if (CheckForSavedScene())
        {
            //Show Continue Option
            UIManager.Instance.EnableContinuePanel(puzzleTextureData);
        }
        else
        {
            //Show Puzzle Size Option
            UIManager.Instance.EnableSizeOption(puzzleTextureData);
        }
    }

    private bool CheckForSavedScene()
    {
        string jsonPath = $"{puzzleTextureData.themeName}/{puzzleTextureData.name}.json";
        return StorageManager.IsFileExist(jsonPath);
    }
}
