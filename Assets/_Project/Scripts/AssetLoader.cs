using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public sealed class AssetLoader
{
    #region Fields and Properties
    
    private static AssetLoader instance;
    
    private readonly Dictionary<string, List<IResourceLocation>> labelToLocationDictionary;
    private readonly Dictionary<IResourceLocation, AsyncOperationHandle> locationToOperationHandleDictionary;
    //Dictionary to clear release handles on scene load
    private readonly Dictionary<IResourceLocation, AsyncOperationHandle> asyncOperationHandleDictionary;
    
    public static AssetLoader Instance
    {
        get
        {
            if (instance != null) return instance;
            Debug.LogError("Asset Loader Initialize Nai Hua Hai");
            return null;
        }
    }
    
    #endregion

    #region Initialization

    private AssetLoader()
    {
        labelToLocationDictionary = new Dictionary<string, List<IResourceLocation>>();
        locationToOperationHandleDictionary = new Dictionary<IResourceLocation, AsyncOperationHandle>();
        asyncOperationHandleDictionary = new Dictionary<IResourceLocation, AsyncOperationHandle>();
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }
    
    public static void Init()
    {
        instance = new AssetLoader();
    }

    #endregion

    #region SetUp
    
    public async UniTask LoadResourceLocations(string labelName)
    {
        IList<IResourceLocation> loadedResourceLocations = await Addressables.LoadResourceLocationsAsync(labelName);

        if (loadedResourceLocations == null || loadedResourceLocations.Count <= 0)
        {
            Debug.LogError($"No Asset ResourceLocations with Label --{labelName}-- was loaded");
            return;
        }
        if(!labelToLocationDictionary.TryAdd(labelName,loadedResourceLocations.ToList()))
            labelToLocationDictionary[labelName].AddRange(loadedResourceLocations);
    }
    
    public async UniTask LoadResourceLocations(params string[] labelNames)
    {
        await LoadResourceLocations(labelNames.ToList());
    }
    
    //Loads Resource Locations for all the given Label Names
    public async UniTask LoadResourceLocations(List<string> labelNames)
    {
        List<UniTask<IList<IResourceLocation>>> locationTasks = new List<UniTask<IList<IResourceLocation>>>();
        foreach (string label in labelNames)
        {
            locationTasks.Add(Addressables.LoadResourceLocationsAsync(label).ToUniTask());
        }
        
        IList<IResourceLocation>[] loadedResourceLocations = await UniTask.WhenAll(locationTasks);
        for (int i = 0; i < labelNames.Count; i++)
        {
            if (loadedResourceLocations[i] == null || loadedResourceLocations[i].Count <= 0)
            {
                Debug.LogError($"No Asset ResourceLocations with Label --{labelNames[i]}-- was loaded");
                continue;
            }
            if(!labelToLocationDictionary.TryAdd(labelNames[i],loadedResourceLocations[i].ToList()))
                labelToLocationDictionary[labelNames[i]].AddRange(loadedResourceLocations[i]);
        }
    }
    
    #endregion

    #region Asset Loading

    public List<IResourceLocation> GetResourceLocations(string labelName)
    {
        if (labelToLocationDictionary.TryGetValue(labelName, out var resourceLocations))
        {
            return resourceLocations;
        }
        Debug.LogError($"No Asset ResourceLocations with Label --{labelName}-- found");
        return null;
    }

    public async UniTask<T> LoadAssetAsync<T>(IResourceLocation resourceLocation, bool dontDestroyOnLoad = false) where T : Object
    {
        if (locationToOperationHandleDictionary.TryGetValue(resourceLocation, out AsyncOperationHandle handle))
        {
            return handle.Result as T;
        }
        var newHandle = Addressables.LoadAssetAsync<T>(resourceLocation);
        var result = await newHandle;
        locationToOperationHandleDictionary[resourceLocation] = newHandle;
        
        if (!dontDestroyOnLoad)
            asyncOperationHandleDictionary[resourceLocation] = newHandle;
        
        return result;
    }
    
    public async UniTask<T> LoadAssetAsync<T>(string key)
    {
        T result = await Addressables.LoadAssetAsync<T>(key);
        return result;
    }

    public AsyncOperationHandle<SceneInstance> LoadSceneAsync(string key)
    {
        return Addressables.LoadSceneAsync(key);
    }
    
    #endregion
    
    private void OnSceneUnloaded(Scene scene) {
        foreach (KeyValuePair<IResourceLocation,AsyncOperationHandle> locationToHandle in asyncOperationHandleDictionary)
        {
            try
            {
                if (locationToHandle.Value.IsValid())
                {
                    Addressables.Release(locationToHandle.Value);
                    locationToOperationHandleDictionary.Remove(locationToHandle.Key);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error releasing handle: {ex.Message}");
            }
        }
        asyncOperationHandleDictionary.Clear();
    }
}
