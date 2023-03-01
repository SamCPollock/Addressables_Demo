using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[Serializable]
public class AssetReferenceAudioClip : AssetReferenceT<AudioClip>
{
    public AssetReferenceAudioClip(string guid) : base(guid) {}
}

public class AddressablesManager_Mono : MonoBehaviour
{
    [SerializeField] private AssetReference playerArmatureAssetReference;

    [SerializeField] private AssetReferenceAudioClip musicAssetReference;

    [SerializeField] private AssetReferenceTexture2D logoAssetReference;

    [SerializeField] private CinemachineVirtualCamera cinemachineVirtualCamera;

    private GameObject playerController;

    // UI Components
    [SerializeField] private RawImage rawImageUnityLogo;
    
    // Level Loading
    private bool clearPreviousScene = false;
    private SceneInstance previousLoadedScene; 
    

    void Start()
    {
        Debug.Log("Initializing Addressables...");
        Addressables.InitializeAsync().Completed += AddressablesManager_Completed;
    }

    private void AddressablesManager_Completed(AsyncOperationHandle<IResourceLocator> obj)
    {
        Debug.Log("Initialized Addressables...");
        playerArmatureAssetReference.LoadAssetAsync<GameObject>().Completed += (playerArmatureAsset) =>
        {
            Debug.Log("Loading Player...");
            playerArmatureAssetReference.InstantiateAsync().Completed += (playerArmatureGameObject) =>
            {
                playerController = playerArmatureGameObject.Result;
                cinemachineVirtualCamera.Follow = playerController.transform.Find("PlayerCameraRoot");
                Debug.Log("Instantiated Player..."); 
            };
        };

        musicAssetReference.LoadAssetAsync<AudioClip>().Completed += (clip) =>
        {
            var audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = clip.Result;
            audioSource.playOnAwake = false;
            audioSource.loop = true;
            audioSource.Play();
            Debug.Log("Loaded Audio Assets...");

        };

        logoAssetReference.LoadAssetAsync<Texture2D>();
        Debug.Log("Loaded Assets...");

    }

    void Update()
    {
        if (logoAssetReference.Asset != null && rawImageUnityLogo.texture == null)
        {
            rawImageUnityLogo.texture = logoAssetReference.Asset as Texture2D;
            Color currentColor = rawImageUnityLogo.color;
            currentColor.a = 1.0f;
            rawImageUnityLogo.color = currentColor;
            Debug.Log("Loaded logo and associated with Canvas...");

        }
    }

    public void LoadAddressableLevel(string addressableKey)
    {
        if (clearPreviousScene)
        {
            Addressables.UnloadSceneAsync(previousLoadedScene).Completed += (asyncHandle) =>
            {
                clearPreviousScene = false;
                previousLoadedScene = new SceneInstance();
            };
        }
        else
        {
            Addressables.LoadSceneAsync(addressableKey, LoadSceneMode.Additive).Completed += (asyncHandle) =>
            {
                clearPreviousScene = true;
                previousLoadedScene = asyncHandle.Result;
            };
        }
    }
    
    /*private void OnDestroy()
    {
        playerArmatureAssetReference.ReleaseInstance(playerController);
        logoAssetReference.ReleaseAsset();
    }*/
}