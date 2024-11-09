using System;
using System.Collections;
using SimpleJSON;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public static readonly string MainMenuScene = "MainMenu";
    private static readonly string GameSceneName = "GameScene";
    private Coroutine loadingCoroutine;
    public JSONNode currentConfigData;

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

        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        UIManager.Instance.Init();
    }

    public void LoadScene(PuzzleTextureData puzzleTextureData, JSONNode configData)
    {
        if(IsSceneLoading()) return;
        loadingCoroutine = StartCoroutine(LoadSceneCoroutine(GameSceneName, puzzleTextureData, configData));
    }
    
    public void LoadScene(string sceneName)
    {
        if(IsSceneLoading()) return;
        loadingCoroutine = StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName, PuzzleTextureData textureData = default, JSONNode configData = null)
    {
        AsyncOperation handle = SceneManager.LoadSceneAsync(sceneName);
        while (!handle.isDone)
        {
            yield return null;
        }

        var newScene = SceneManager.GetSceneByName(sceneName);
        if (newScene.IsValid())
        {
            InteractiveSystem interactiveSystem = FindObjectOfType<InteractiveSystem>();
            if (interactiveSystem)
            {
                UIManager.Instance.ToggleLevelSelectPanel(false);
                UIManager.Instance.ToggleGameplayOptionsPanel(true);
                interactiveSystem.Init();
                yield return interactiveSystem.OnSceneLoad(textureData, configData);
            }
            else if (UIManager.Instance != null)
            {
                UIManager.Instance.ToggleLevelSelectPanel(true);
                UIManager.Instance.ToggleGameplayOptionsPanel(false);
                UIManager.Instance.Init();
            }
        }
        
        loadingCoroutine = null;
    }

    public void OnBack()
    {
        InteractiveSystem interactiveSystem = FindObjectOfType<InteractiveSystem>();
        if(interactiveSystem)
            interactiveSystem.OnBack();
    }

    #region Helper Methods

    public bool IsSceneLoading()
    {
        return loadingCoroutine != null;
    }

    #endregion
}
