using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceLocations;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioPlayer;

    private Dictionary<string, AudioClip> keyToSfxDictionary;
    private Dictionary<string, IResourceLocation> keyToResourceLocationDictionary;

    private bool audioDisabled = false;
    
    public static SoundManager Instance { get; private set; }

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
        keyToSfxDictionary = new Dictionary<string, AudioClip>();
        keyToResourceLocationDictionary = new Dictionary<string, IResourceLocation>();
        IList<IResourceLocation> resourceLocations = AssetLoader.Instance.GetResourceLocations("AudioClips");
        if (resourceLocations is { Count: > 0 })
        {
            foreach (IResourceLocation location in resourceLocations)
            {
                keyToResourceLocationDictionary[location.PrimaryKey] = location;
            }
        }
    }
    
    public async UniTask<AudioClip> GetAudioClip(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            Debug.Log("Audio Key is Empty");
            return null;
        }
        
        if (keyToSfxDictionary.TryGetValue(key, out AudioClip clip))
            return clip;

        if (keyToResourceLocationDictionary.TryGetValue(key, out IResourceLocation location))
            clip = await AssetLoader.Instance.LoadAssetAsync<AudioClip>(location);
        else
            clip = await AssetLoader.Instance.LoadAssetAsync<AudioClip>(key);
        
        if (clip)
        {
            keyToSfxDictionary.Add(key, clip);
            return clip;
        }
        Debug.Log($"Audio Clip with key {key} not found");
        return null;
    }
    
    public async void PlayOneShot(string key, float volume = 1)
    {
        AudioClip clip = await GetAudioClip(key);
        audioPlayer.PlayOneShot(clip,volume);
    }

    public void PlayOneShot(AudioClip clip, float volume = 1)
    {
        if (clip != null || audioDisabled) return;
        audioPlayer.Stop();
        audioPlayer.PlayOneShot(clip, volume);
    }
}
