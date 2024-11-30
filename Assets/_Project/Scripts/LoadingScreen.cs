using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private GameObject splashScreenPanel;
    [SerializeField] private Animator splashScreenAnimator;
    [SerializeField] private float splashScreenTime = 0.5f;
    [SerializeField] private GameObject loadingPanel;

    public bool SplashScreenCompleted { get; private set; }
    public static LoadingScreen Instance { get; private set; }

    
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
    
    public async void PlaySplashScreenAnimation()
    {
        splashScreenAnimator.Play("ARG_splash");
        await UniTask.WaitForSeconds(splashScreenTime);
        OnSplashScreenComplete();
    }

    private void OnSplashScreenComplete()
    {
        Destroy(splashScreenPanel.gameObject);
        SplashScreenCompleted = true;
    }

    public void ShowLoading()
    {
        loadingPanel.SetActive(true);
    }

    public void HideLoading()
    {
        loadingPanel.SetActive(false);
    }
}
