using System;
using Cysharp.Threading.Tasks;
using Gley.MobileAds;
using UnityEngine;
using UnityEngine.Events;

public class AdManager : MonoBehaviour
{
    public static AdManager Instance { get; private set; }
    private bool isInitialized;
    public bool IsInitialized => isInitialized;
    private Action<bool> rewardCallback;
    private Action<bool> rewardInterstitialCallback;
    
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

    public async UniTask Init()
    {
#if UNITY_IOS
        await UniTask.Delay(1000);
        IOSATTManager.RequestAdConsent(this);
        await UniTask.Delay(500);
#else
        await UniTask.Yield();
#endif
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
    
    public bool ShowInterstitial()
    {
        if (isInitialized && API.IsInterstitialAvailable())
        {
            API.ShowInterstitial();
            return true;
        }
        Debug.LogWarning("Interstitial ad is not ready yet.");
        return false;
    }
    
    public bool ShowRewardAd(Action<bool> callback)
    {
        if (isInitialized && API.IsRewardedVideoAvailable())
        {
            rewardCallback = callback;
            API.ShowRewardedVideo(RewardCompleted);
            return true;
        }
        Debug.LogWarning("Rewarded ad is not available.");
        callback?.Invoke(false);
        return false;
    }
    
    public void ShowRewardedInterstitialAd(Action<bool> callback)
    {
        if (isInitialized && API.IsRewardedInterstitialAvailable())
        {
            rewardInterstitialCallback = callback;
            API.ShowRewardedInterstitial(RewardInterstitialCompleted);
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
    
    private void RewardInterstitialCompleted(bool completed)
    {
        rewardInterstitialCallback?.Invoke(completed);
        rewardInterstitialCallback = null;
    }
}
