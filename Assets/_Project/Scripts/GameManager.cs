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
    public static SceneType SceneType = SceneType.None;

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
        
        AdManager.Instance.HideBannerAd();
        bool adWatched = false;
        if (configData != null && configData[StringID.LevelCompleted])
        {
            AdManager.Instance.ShowRewardAd((value) =>
            {
                adWatched = true;
                configData.Remove(StringID.LevelCompleted);
            });
            yield return new WaitUntil(() => adWatched);
        }
        else if (configData.GetNextSceneType() == SceneType.LevelSelect && DateTime.Now > nextInterstitialTimer)
        {
            AdManager.Instance.ShowInterstitial();
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
            iSystem = FindObjectOfType<InteractiveSystem>();
            if (iSystem)
            {
                UIManager.Instance.ToggleLevelSelectPanel(false);
                UIManager.Instance.ToggleGameplayOptionsPanel(true);
                UIManager.Instance.SetPieceCounterDisplay(0);
                iSystem.Init();
                yield return iSystem.OnSceneLoad(textureData, configData);
                AdManager.Instance.ShowBanner();
            }
            yield return UIManager.Instance.OnSceneLoad(configData);
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
    LevelSelect,
    GameScene
}
