using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    [SerializeField] private UIDocument mainMenuDoc;
    [SerializeField] private VisualTreeAsset itemElementAsset;
    [SerializeField] private PuzzleTextureDatabase puzzleTextureDatabase;

    private VisualElement rootElement;
    private ListView listView;
    private ScrollView scrollView;

    private const string MainCategoryListViewName = "MainCategoryContent";
    
    private void Awake()
    {
        Application.targetFrameRate = 60;
        rootElement = mainMenuDoc.rootVisualElement;
    }

    private void Start()
    {
        listView = rootElement.Q<ListView>(MainCategoryListViewName);
        
        listView.makeItem = () => itemElementAsset.CloneTree();
        listView.bindItem = (element, index) =>
        {
            var itemData1 = puzzleTextureDatabase.generalTextureData[(index * 2)% 8];
            var itemData2 = puzzleTextureDatabase.generalTextureData[((index * 2) + 1) % 8];
            Button button1 = element.Q<Button>("ItemButton1");
            Button button2 = element.Q<Button>("ItemButton2");
            button1.style.backgroundImage = new StyleBackground(itemData1.texture);
            button2.style.backgroundImage = new StyleBackground(itemData2.texture);
        };
        listView.itemsSource = Enumerable.Range(0, puzzleTextureDatabase.generalTextureData.Count * 200).ToList();
    }
}
