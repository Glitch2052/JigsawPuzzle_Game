using PolyAndCode.UI;
using UnityEngine;
using UnityEngine.UI;

public class CustomPuzzleItemCell : MonoBehaviour, ICell
{
    [SerializeField] private Button button;
    [SerializeField] private RawImage buttonImage;

    private ThemeName themeName;
    private CustomPuzzleTexData puzzleTextureData;
    private CustomPuzzleCategoryDataSource dataSource;

    public void InitCell(ThemeName theme, CustomPuzzleCategoryDataSource categoryDataSource)
    {
        themeName = theme;
        dataSource = categoryDataSource;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(LoadPuzzleScene);
    }

    public void SetCell(CustomPuzzleTexData data)
    {
        puzzleTextureData = data;
        if (data.isEmptyDisplaySlot)
        {
            buttonImage.texture = UIManager.Instance.plusIconSprite.texture;
            return;
        }
        
        if(data.isTextureLoaded)
            buttonImage.texture = data.customTexture;
        else
        {
            byte[] bytes = StorageManager.ReadBytesNow(data.texturePath);
            Texture2D texToLoad = new Texture2D(0, 0);
            texToLoad.LoadImage(bytes);
            data.customTexture = texToLoad;
            data.isTextureLoaded = true;
            buttonImage.texture = data.customTexture;
        }
    }

    private void LoadPuzzleScene()
    {
        if (puzzleTextureData.isEmptyDisplaySlot)
        {
            UIManager.Instance.ToggleGalleryOptionGameObject();
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
        if (puzzleTextureData.jsonPath == string.Empty || puzzleTextureData.jsonPath == "") return false;
        return StorageManager.IsFileExist(puzzleTextureData.jsonPath);
    }
}