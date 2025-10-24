#if UNITY_IOS
using System;
using System.Collections;
using UnityEngine;
using Unity.Advertisement.IosSupport;
using UnityEngine.iOS;

public class IOSATTManager
{
#if !UNITY_EDITOR
    private static readonly Version CurrentVersion = new (Device.systemVersion);
    private static readonly Version Ios14  = new Version ("14.0");
#endif
    
    public static void RequestAdConsent(MonoBehaviour monoBehaviour)
    {
#if UNITY_IOS && !UNITY_EDITOR
        if (CurrentVersion >= Ios14) {
            if (ATTrackingStatusBinding.GetAuthorizationTrackingStatus () == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED) {
                Debug.Log("ATT Requested");
                ATTrackingStatusBinding.RequestAuthorizationTracking ();
                monoBehaviour.StartCoroutine (SetADConsent (true));
            }
        } 
        else 
        {
            monoBehaviour.StartCoroutine (SetADConsent (false));
        }
#endif
#if UNITY_EDITOR
        monoBehaviour.StartCoroutine(SetADConsent(false));
#endif
    }

    private static IEnumerator SetADConsent(bool isPopUp)
    {
        yield return null;
        if (isPopUp)
        {
            yield return new WaitForSeconds(0.25f);
#if UNITY_IOS && !UNITY_EDITOR
            if (ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == ATTrackingStatusBinding.AuthorizationTrackingStatus.AUTHORIZED)
            {
                // AdMobManager.AllowedTracking();
            }
            else if (ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == ATTrackingStatusBinding.AuthorizationTrackingStatus.DENIED ||
                     ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == ATTrackingStatusBinding.AuthorizationTrackingStatus.RESTRICTED)
            {
                // AdMobManager.DeniedTracking();
            }
#endif
        }
        else
        {
            // AdMobManager.AllowedTracking();
        }
    }
}
#endif