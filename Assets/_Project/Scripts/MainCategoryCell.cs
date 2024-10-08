using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts
{
    public class MainCategoryCell : ICell
    {
        [SerializeField] private Button button;
        [SerializeField] private Image buttonImage;

        private PuzzleTextureData puzzleTextureData;
        
        public void UpdateCell(PuzzleTextureData data)
        {
            puzzleTextureData = data;
            buttonImage.sprite = data.sprite;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => GameManager.Instance.LoadScene(puzzleTextureData));
        }
    }
}