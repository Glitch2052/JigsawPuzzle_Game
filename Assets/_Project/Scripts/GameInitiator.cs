using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using SimpleJSON;
using Unity.Advertisement.IosSupport;
using Unity.Advertisement.IosSupport.Components;
using UnityEngine;

public class GameInitiator : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private SoundManager soundManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private AdManager adManager;
    [SerializeField] private LoadingScreen loadingScreen;
    private async void Start()
    {
        Application.targetFrameRate = 60;
        
        BindObjects();
        //Show Logo
        loadingScreen.PlaySplashScreenAnimation();
        await InitializeObjects();
        
        await UniTask.WaitUntil((() => loadingScreen.SplashScreenCompleted));
        BeginGame();
    }

    private void BindObjects()
    {
        // ContextScreenView.RequestAuthorizationTrackingFromUser();
        
        loadingScreen = Instantiate(loadingScreen);
        gameManager = Instantiate(gameManager);
        uiManager = Instantiate(uiManager);
        soundManager = Instantiate(soundManager);
        adManager = Instantiate(adManager);
        AssetLoader.Init();
    }

    private async UniTask InitializeObjects()
    {
        // Wait Till Initialization Of Objects
        // like ads handler or analytics services
        List<UniTask> allTasks = new()
        {
            adManager.Init(),
            LoadAddressableLocations()
        };
        await UniTask.WhenAll(allTasks);
        
        gameManager.Init();
        uiManager.Init();
        soundManager.Init();
    }
    
    private void BeginGame()
    {
        JSONNode node = new JSONObject();
        node.SetNextSceneType(SceneType.HomeScene);
        gameManager.LoadScene(StringID.HomeScene,node);
    }

    private async UniTask LoadAddressableLocations()
    {
        List<string> labels = new List<string>();
        foreach (ThemeName themeName in Enum.GetValues(typeof(ThemeName)))
        {
            if(themeName == ThemeName.Custom) continue;
            labels.Add(themeName.ToString());
        }
        labels.Add("Scenes");
        labels.Add("AudioClips");
        labels.Add("NormalMaps");
        await AssetLoader.Instance.LoadResourceLocations(labels);
    }
}
