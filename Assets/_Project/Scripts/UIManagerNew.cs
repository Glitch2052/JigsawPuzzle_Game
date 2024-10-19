using _Project.Scripts;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIManagerNew : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private ScrollRect mainCategoryScrollRect;
    [SerializeField] private RectTransform content;
    [SerializeField] private MainCategoryCell mainCategoryCell;
    [SerializeField] private PuzzleCollectionData puzzleCollectionData;

    private void Start()
    {
        foreach (var puzzleTextureData in puzzleCollectionData.generalTextureData)
        {
            var cell = Instantiate(mainCategoryCell, content);
            cell.UpdateCell(puzzleTextureData);
        }
    }
}
