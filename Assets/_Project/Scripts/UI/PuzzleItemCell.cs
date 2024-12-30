using PolyAndCode.UI;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleItemCell : MonoBehaviour, ICell
{
    [SerializeField] private Button button;
    [SerializeField] private RawImage buttonImage;
    
    private PuzzleTextureData puzzleTextureData;

    public void InitCell()
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(LoadPuzzleScene);
    }

    public async void SetCell(PuzzleTextureData data)
    {
        puzzleTextureData = data;
        buttonImage.texture = await AssetLoader.Instance.LoadAssetAsync<Texture2D>(data.iconResourceLocation);
    }

    private void LoadPuzzleScene()
    {
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
