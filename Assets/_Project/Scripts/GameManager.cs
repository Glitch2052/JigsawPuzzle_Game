using System.Collections;
using SimpleJSON;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    private Coroutine loadingCoroutine;
    public JSONNode currentConfigData;
    public static SceneType SceneType = SceneType.None;

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
        AsyncOperation handle = SceneManager.LoadSceneAsync(sceneName);
        while (handle is { isDone: false })
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
            yield return UIManager.Instance.OnSceneLoad(configData);
        }
        
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