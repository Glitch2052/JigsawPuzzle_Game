using System;
using Gley.MobileAds;
using UnityEngine;

public class AdManager : MonoBehaviour
{
    public static AdManager Instance { get; private set; }
    private bool isInitialized;
    public bool IsInitialized => isInitialized;
    private Action<bool> rewardCallback;
    
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
        API.Initialize((() => isInitialized = true));
    }
    
    public void ShowBanner()
    {
        if (isInitialized)
        {
            API.ShowBanner(BannerPosition.Bottom, BannerType.Adaptive);
        }
        else
        {
            Debug.LogWarning("Banner ad is not ready.");
        }
    }

    public void HideBannerAd()
    {
        API.HideBanner();
    }
    
    public void ShowInterstitial()
    {
        if (isInitialized && API.IsInterstitialAvailable())
        {
            API.ShowInterstitial();
        }
        else
        {
            Debug.LogWarning("Interstitial ad is not ready yet.");
        }
    }
    
    public void ShowRewardAd(Action<bool> callback)
    {
        if (isInitialized && API.IsRewardedVideoAvailable())
        {
            rewardCallback = callback;
            API.ShowRewardedVideo(RewardCompleted);
        }
        else
        {
            Debug.LogWarning("Rewarded ad is not available.");
            callback?.Invoke(false);
        }
    }

    private void RewardCompleted(bool completed)
    {
        rewardCallback?.Invoke(completed);
        rewardCallback = null;
    }
}
