using System;
using System.Collections;
using SimpleJSON;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public InteractiveSystem iSystem;
    private Coroutine loadingCoroutine;
    public JSONNode currentConfigData;
    public static SceneType sceneType = SceneType.None;
    public static bool IsNetworkReachable => Application.internetReachability != NetworkReachability.NotReachable;

#if UNITY_EDITOR
    private readonly float interstitialTimer = 10f;
#else
    private readonly float interstitialTimer = 100f;
#endif
    private DateTime nextInterstitialTimer;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(gameObject);
    }

    public void Init()
    {
        nextInterstitialTimer = DateTime.Now.AddSeconds(interstitialTimer);
    }

    public void LoadScene(PuzzleTextureData puzzleTextureData, JSONNode configData)
    {
        if(IsSceneLoading()) return;
        loadingCoroutine = StartCoroutine(LoadSceneCoroutine(StringID.GameScene, configData, puzzleTextureData));
    }
    
    public void LoadScene(string sceneName, JSONNode node)
    {
        if(IsSceneLoading()) return;
        loadingCoroutine = StartCoroutine(LoadSceneCoroutine(sceneName, node));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName, JSONNode configData = null, PuzzleTextureData textureData = default)
    {
        iSystem = null;
        
        SoundManager.Instance.StopBGM();

        int totalPuzzleSolved = PlayerPrefs.GetInt(StringID.TotalPuzzleSolved, 0);
        
        AdManager.Instance.HideBannerAd();
        bool adWatched = false;
        if (configData != null && configData[StringID.LevelCompleted])
        {
            if(AdManager.Instance.ShowInterstitial())
                configData.Remove(StringID.LevelCompleted);
            // AdManager.Instance.ShowRewardedInterstitialAd((value) =>
            // {
            //     adWatched = true;
            // });
            // yield return new WaitUntil(() => adWatched);
        }
        else if (totalPuzzleSolved > 3 && configData.GetNextSceneType() == SceneType.LevelSelect && DateTime.Now > nextInterstitialTimer)
        {
            if(AdManager.Instance.ShowInterstitial())
                nextInterstitialTimer = nextInterstitialTimer.AddSeconds(interstitialTimer);
        }
        
        LoadingScreen.Instance.ShowLoading();
        // AsyncOperation handle = AssetLoader.Instance.LoadSceneAsync(sceneName);
        // while (handle is { isDone: false })
        // {
        //     yield return null;
        // }
        var handle = AssetLoader.Instance.LoadSceneAsync(sceneName);
        yield return handle;

        var newScene = SceneManager.GetSceneByName(sceneName);
        if (newScene.IsValid())
        {
            UIManager.Instance.SetPieceCounterDisplay(0);
            
            iSystem = FindObjectOfType<InteractiveSystem>();
            if (iSystem)
            {
                UIManager.Instance.SetGameplayOptionsUIPositions();
                iSystem.Init();
                yield return iSystem.OnSceneLoad(textureData, configData);
                AdManager.Instance.ShowBanner();
            }
            yield return UIManager.Instance.OnSceneLoad(configData);
        }
        sceneType = configData.GetNextSceneType();
        switch (sceneType)
        {
            case SceneType.None:
                break;
            case SceneType.HomeScene:
                break;
            case SceneType.LevelSelect:
                UIManager.Instance.ToggleLevelSelectPanel(true);
                UIManager.Instance.ToggleGameplayOptionsPanel(false);
                break;
            case SceneType.GameScene:
                UIManager.Instance.ToggleLevelSelectPanel(false);
                UIManager.Instance.ToggleGameplayOptionsPanel(true);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        LoadingScreen.Instance.HideLoading();
        loadingCoroutine = null;
    }

    #region Helper Methods

    public bool IsSceneLoading()
    {
        return loadingCoroutine != null;
    }

    #endregion
    
}

public enum SceneType
{
    None,
    HomeScene,
    LevelSelect,
    GameScene
}
