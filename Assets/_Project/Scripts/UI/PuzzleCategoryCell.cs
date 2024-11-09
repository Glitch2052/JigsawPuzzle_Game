using PolyAndCode.UI;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleCategoryCell : MonoBehaviour, ICell
{
    [SerializeField] private Button button;
    [SerializeField] private RawImage buttonImage;

    private ThemeName themeName;
    private PuzzleTextureData puzzleTextureData;

    public void InitCell(ThemeName theme)
    {
        themeName = theme;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(LoadPuzzleScene);
        // button.onClick.AddListener(() => GameManager.Instance.LoadScene(puzzleTextureData));
    }

    public void SetCell(PuzzleTextureData data)
    {
        puzzleTextureData = data;
        buttonImage.texture = data.texture;
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
