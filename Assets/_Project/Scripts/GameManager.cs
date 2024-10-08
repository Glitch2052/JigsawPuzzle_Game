using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private const string GameSceneName = "GameScene";
    private Coroutine loadingCoroutine;

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

    public void LoadScene(PuzzleTextureData puzzleTextureData)
    {
        if(IsSceneLoading()) return;
        
        loadingCoroutine = StartCoroutine(LoadSceneCoroutine(puzzleTextureData,GameSceneName));
    }

    public void LoadScene(string sceneName)
    {
        if(IsSceneLoading()) return;
        
        loadingCoroutine = StartCoroutine(LoadSceneCoroutine(new PuzzleTextureData(),sceneName));
    }

    private IEnumerator LoadSceneCoroutine(PuzzleTextureData data, string sceneName)
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
                interactiveSystem.Init(data);
                yield return interactiveSystem.OnSceneLoad();
            }
        }
        
        loadingCoroutine = null;
    }

    public void OnBack()
    {
        
    }

    #region Helper Methods

    public bool IsSceneLoading()
    {
        return loadingCoroutine != null;
    }

    #endregion
}
