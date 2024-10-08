using System;
using _Project.Scripts;
using UnityEngine;
using UnityEngine.UI;

public class UIManagerNew : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private ScrollRect mainCategoryScrollRect;
    [SerializeField] private RectTransform content;
    [SerializeField] private MainCategoryCell mainCategoryCell;
    [SerializeField] private PuzzleTextureDatabase puzzleTextureDatabase;

    private void Start()
    {
        foreach (var puzzleTextureData in puzzleTextureDatabase.generalTextureData)
        {
            var cell = Instantiate(mainCategoryCell, content);
            cell.UpdateCell(puzzleTextureData);
        }
    }
}
