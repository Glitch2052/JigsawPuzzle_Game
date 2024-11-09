using PolyAndCode.UI;
using UnityEngine;
using UnityEngine.UI;

public class CustomPuzzleCategoryCell : MonoBehaviour, ICell
{
    [SerializeField] private Button button;
    [SerializeField] private RawImage buttonImage;

    private ThemeName themeName;
    private PuzzleTextureData puzzleTextureData;

    private CustomPuzzleCategoryDataSource dataSource;

    public void InitCell(ThemeName theme, CustomPuzzleCategoryDataSource categoryDataSource)
    {
        themeName = theme;
        dataSource = categoryDataSource;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(LoadPuzzleScene);
    }

    public void SetCell(Texture2D texture, string path)
    {
        // puzzleTextureData = data;
        buttonImage.texture = texture;
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
        string jsonPath = $"{themeName}/{puzzleTextureData.texture.name}.json";
        return StorageManager.IsFileExist(jsonPath);
    }
}
