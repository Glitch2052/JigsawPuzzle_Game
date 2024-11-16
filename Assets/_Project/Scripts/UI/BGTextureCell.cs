using UnityEngine;
using UnityEngine.UI;

public class BGTextureCell : MonoBehaviour
{
    [SerializeField] private Toggle button;
    [SerializeField] private Image buttonImage;
    public GameObject tickImage;

    public BGTextureCell SetToggleGroup(ToggleGroup toggleGroup)
    {
        button.group = toggleGroup;
        return this;
    }

    public void UpdateCell(Sprite sprite, InteractiveSystem iSystem)
    {
        buttonImage.sprite = sprite;
        
        button.onValueChanged.RemoveAllListeners();
        button.onValueChanged.AddListener((value) =>
        {
            if (value)
            {
                iSystem.puzzleGenerator.UpdateBackgroundSprite(sprite);
            }
        });
    }

    public void ToggleClick(bool value)
    {
        button.isOn = value;
    }
}
