using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TabManager : MonoBehaviour
{
    public RectTransform[] tabs;
    public Image[] tabButtonImages;
    public TextMeshProUGUI[] tabButtonTexts;
    
    public Color inactiveImageColor;
    public Color activeImageColor;

    public Color inactiveTextColor;
    public Color activeTextColor;
    
    public void SwitchToTab(int tabIndex)
    {
        for (var i = 0; i < tabs.Length; i++)
        {
            tabs[i].gameObject.SetActive(false);
            tabButtonImages[i].color = inactiveImageColor;
            tabButtonTexts[i].color = inactiveTextColor;
        }
        tabs[tabIndex].gameObject.SetActive(true);
        tabButtonImages[tabIndex].color = activeImageColor;
        tabButtonTexts[tabIndex].color = activeTextColor;
    }
}
