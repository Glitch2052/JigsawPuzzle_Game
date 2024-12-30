using PolyAndCode.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ThemeCategoryCell : MonoBehaviour, ICell
{
    [SerializeField] private Button button;
    [SerializeField] private RawImage buttonImage;
    [SerializeField] private TextMeshProUGUI themeText;

    private PuzzleCollectionData currCollectionData;
    
    public void InitCell()
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(LoadThemeCategoryPuzzleItems);
    }

    public async void SetCell(PuzzleCollectionData collectionData)
    {
        currCollectionData = collectionData;
        buttonImage.texture =
            await AssetLoader.Instance.LoadAssetAsync<Texture2D>(collectionData.GetIconTextureResourceLocationKey());
        themeText.text = collectionData.themeName.ToString();
    }

    private void LoadThemeCategoryPuzzleItems()
    {
        UIManager.Instance.LoadThemeCategory(currCollectionData);
    }
}
