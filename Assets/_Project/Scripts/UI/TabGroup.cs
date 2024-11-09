using System.Collections.Generic;
using UnityEngine;

public class TabGroup : MonoBehaviour
{
    public List<TabButton> tabButtons;
    public TabButton selectedTab;
    
    public Color inactiveTabImageColor;
    public Color activeTabImageColor;

    public Color inactiveTabTextColor;
    public Color activeTabTextColor;

    public List<GameObject> objectsToSwap;

    public void Subscribe(TabButton button)
    {
        tabButtons ??= new List<TabButton>();
        tabButtons.Add(button);
    }

    public void OnTabEnter(TabButton button)
    {
        
    }

    public void OnTabExit(TabButton button)
    {
        
    }

    public void OnTabSelected(TabButton button)
    {
        if (selectedTab != null)
        {
            selectedTab.DeSelect();
        }
        selectedTab = button;
        
        ResetTabs();
        button.backGround.color = activeTabImageColor;
        button.textComponent.color = activeTabTextColor;
        int index = button.transform.GetSiblingIndex();
        for (int i = 0; i < objectsToSwap.Count; i++)
        {
            objectsToSwap[i].SetActive(i == index);
        }
        
        selectedTab.Select();
    }

    public void ResetTabs()
    {
        foreach (TabButton button in tabButtons)
        {
            if(selectedTab != null && selectedTab == button) continue;
            
            button.backGround.color = inactiveTabImageColor;
            button.textComponent.color = inactiveTabTextColor;
        }
    }
}
