using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Management;
using UnityEngine.SceneManagement;

[System.Serializable]
public class MarkerContentData
{
    public string markerName;
    public GameObject prefab;
    public AudioClip audioClip;
    public string question;
}

public class ImageTracker : MonoBehaviour
{
    [Header("AR Components")]
    public ARTrackedImageManager trackedImages;
    public List<MarkerContentData> markerContents;
    public ARContentHandler contentHandler;
    
    [Header("Safety Settings")]
    public bool enableSafetyChecks = true;
    public float safetyCheckInterval = 0.5f;
    public bool stopXROnSceneUnload = true;
    
    [Header("Tracking Settings")]
    public float trackingThreshold = 0.5f;
    public float hideDelay = 0.2f; 
    
    [Header("Positioning Settings")]
    public bool useWorldSpacePositioning = true;
    public Vector3 positionOffset = Vector3.zero; 
    public Vector3 rotationOffset = Vector3.zero;
    public float scaleMultiplier = 0.2f;
    
    private Dictionary<string, MarkerContentData> markerDictionary;
    private Dictionary<string, GameObject> spawnedPrefabs = new Dictionary<string, GameObject>();
    private Dictionary<string, TrackingState> lastTrackingStates = new Dictionary<string, TrackingState>();
    private Dictionary<string, float> lastSeenTimes = new Dictionary<string, float>();
    private string currentActiveMarker = null;
    private bool isCleaningUp = false;
    private Coroutine safetyCheckCoroutine;
    private Coroutine trackingUpdateCoroutine;

    void Awake()
    {
        if (trackedImages == null)
            trackedImages = GetComponent<ARTrackedImageManager>();

        markerDictionary = new Dictionary<string, MarkerContentData>();
        foreach (var data in markerContents)
        {
            if (!markerDictionary.ContainsKey(data.markerName))
                markerDictionary[data.markerName] = data;
        }
        
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    void OnEnable()
    {
        isCleaningUp = false;
        
        if (IsTrackedImageManagerValid())
        {
            trackedImages.trackedImagesChanged += OnTrackedImagesChanged;
            Debug.Log("[ImageTracker] Successfully subscribed to tracked images changed event");
        }
        else
        {
            Debug.LogWarning("[ImageTracker] ARTrackedImageManager is not valid, will retry...");
            StartCoroutine(RetryInitialization());
        }
        
        if (enableSafetyChecks)
        {
            safetyCheckCoroutine = StartCoroutine(PerformSafetyChecks());
        }
        
        // Start tracking update coroutine for smooth hiding
        trackingUpdateCoroutine = StartCoroutine(UpdateTrackingStates());
    }

    void OnDisable()
    {
        PerformCleanup();
    }
    
    void OnDestroy()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        PerformCleanup();
    }
    
    void OnSceneUnloaded(Scene scene)
    {
        Debug.Log($"[ImageTracker] Scene {scene.name} unloaded, performing cleanup");
        PerformCleanup();
        
        if (stopXROnSceneUnload)
        {
            StopXRSubsystems();
        }
    }

    void StopXRSubsystems()
    {
        Debug.Log("[ImageTracker] Stopping XR subsystems to prevent errors...");
        
        try
        {
            var arSession = FindObjectOfType<ARSession>();
            if (arSession != null && arSession.subsystem != null)
            {
                if (arSession.subsystem.running)
                {
                    arSession.subsystem.Stop();
                    Debug.Log("[ImageTracker] Stopped AR Session subsystem");
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[ImageTracker] Exception stopping AR Session: {ex.Message}");
        }
        
        try
        {
            var xrManager = XRGeneralSettings.Instance?.Manager;
            if (xrManager != null && xrManager.isInitializationComplete)
            {
                xrManager.StopSubsystems();
                Debug.Log("[ImageTracker] Stopped XR Manager subsystems");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[ImageTracker] Exception stopping XR Manager: {ex.Message}");
        }
        
        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();
    }

    System.Collections.IEnumerator RetryInitialization()
    {
        int retryCount = 0;
        const int maxRetries = 10;
        
        while (!IsTrackedImageManagerValid() && retryCount < maxRetries)
        {
            yield return new WaitForSeconds(0.1f);
            
            if (trackedImages == null)
                trackedImages = GetComponent<ARTrackedImageManager>();
                
            retryCount++;
        }
        
        if (IsTrackedImageManagerValid())
        {
            trackedImages.trackedImagesChanged += OnTrackedImagesChanged;
            Debug.Log("[ImageTracker] Successfully initialized after retry");
        }
        else
        {
            Debug.LogError("[ImageTracker] Failed to initialize after multiple retries");
        }
    }

    void PerformCleanup()
    {
        if (isCleaningUp) return;
        isCleaningUp = true;
        
        Debug.Log("[ImageTracker] Performing cleanup...");
        
        if (safetyCheckCoroutine != null)
        {
            StopCoroutine(safetyCheckCoroutine);
            safetyCheckCoroutine = null;
        }
        
        if (trackingUpdateCoroutine != null)
        {
            StopCoroutine(trackingUpdateCoroutine);
            trackingUpdateCoroutine = null;
        }
        
        if (IsTrackedImageManagerValid())
        {
            try
            {
                trackedImages.trackedImagesChanged -= OnTrackedImagesChanged;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[ImageTracker] Exception during event unsubscription: {ex.Message}");
            }
        }

        foreach (var kvp in spawnedPrefabs)
        {
            if (kvp.Value != null)
            {
                try
                {
                    kvp.Value.SetActive(false);
                    Destroy(kvp.Value);
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[ImageTracker] Exception destroying prefab {kvp.Key}: {ex.Message}");
                }
            }
        }
        spawnedPrefabs.Clear();
        lastTrackingStates.Clear();
        lastSeenTimes.Clear();
        
        if (contentHandler != null)
        {
            try
            {
                contentHandler.HideContent();
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[ImageTracker] Exception hiding content: {ex.Message}");
            }
        }
        
        currentActiveMarker = null;
        Debug.Log("[ImageTracker] Cleanup completed");
    }

    bool IsTrackedImageManagerValid()
    {
        try
        {
            return trackedImages != null && trackedImages.gameObject != null && trackedImages.enabled;
        }
        catch (System.Exception)
        {
            return false;
        }
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        if (isCleaningUp) return;
        
        try
        {
            foreach (var trackedImage in args.added)
                SetupMarkerContent(trackedImage);

            foreach (var trackedImage in args.updated)
            {
                if (IsTrackedImageValid(trackedImage))
                {
                    string name = trackedImage.referenceImage.name;
                    lastTrackingStates[name] = trackedImage.trackingState;
                    
                    if (trackedImage.trackingState == TrackingState.Tracking)
                    {
                        lastSeenTimes[name] = Time.time;
                        UpdatePrefabTransform(trackedImage);
                    }
                }
            }

            foreach (var trackedImage in args.removed)
            {
                if (IsTrackedImageValid(trackedImage))
                {
                    string name = trackedImage.referenceImage.name;
                    lastTrackingStates[name] = TrackingState.None;
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ImageTracker] Exception in OnTrackedImagesChanged: {ex.Message}");
        }
    }

    IEnumerator UpdateTrackingStates()
    {
        while (!isCleaningUp)
        {
            yield return new WaitForSeconds(0.1f);

            string bestMarker = null;
            float bestTime = 0f;

            foreach (var kvp in lastSeenTimes)
            {
                if (lastTrackingStates.ContainsKey(kvp.Key) &&
                    lastTrackingStates[kvp.Key] == TrackingState.Tracking &&
                    kvp.Value > bestTime)
                {
                    bestMarker = kvp.Key;
                    bestTime = kvp.Value;
                }
            }

            if (bestMarker != null)
            {
                ShowMarkerContentByName(bestMarker);
            }
            else if (currentActiveMarker != null)
            {
                float lastSeen = lastSeenTimes.ContainsKey(currentActiveMarker) ? lastSeenTimes[currentActiveMarker] : 0f;
                if (Time.time - lastSeen > hideDelay)
                {
                    HideCurrentMarkerContent();
                }
            }
        }
    }

    bool IsTrackedImageValid(ARTrackedImage trackedImage)
    {
        try
        {
            return trackedImage != null && 
                   trackedImage.gameObject != null && 
                   trackedImage.referenceImage != null &&
                   trackedImage.transform != null;
        }
        catch (System.Exception)
        {
            return false;
        }
    }

    void SetupMarkerContent(ARTrackedImage trackedImage)
    {
        if (!IsTrackedImageValid(trackedImage) || isCleaningUp) return;
    
        try
        {
            string name = trackedImage.referenceImage.name;
            if (markerDictionary.TryGetValue(name, out var data))
            {
                if (!spawnedPrefabs.ContainsKey(name))
                {
                    GameObject instance;
                    
                    if (useWorldSpacePositioning)
                    {
                        Vector3 spawnPosition = trackedImage.transform.position;
                        Quaternion spawnRotation = trackedImage.transform.rotation;
                        instance = Instantiate(data.prefab, spawnPosition, spawnRotation);
                        
                        instance.transform.localScale = Vector3.one * scaleMultiplier;
                    }
                    else
                    {
                        instance = Instantiate(data.prefab, trackedImage.transform);
                        instance.transform.localPosition = positionOffset;
                        instance.transform.localRotation = Quaternion.Euler(rotationOffset);
                        instance.transform.localScale = Vector3.one * scaleMultiplier;
                    }
                    
                    instance.SetActive(trackedImage.trackingState == TrackingState.Tracking);
                    
                    spawnedPrefabs[name] = instance;
                    
                    Debug.Log($"[ImageTracker] Setup content for marker: {name} at position {instance.transform.position}");
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ImageTracker] Exception in SetupMarkerContent: {ex.Message}");
        }
    }

    void UpdatePrefabTransform(ARTrackedImage trackedImage)
    {
        if (!IsTrackedImageValid(trackedImage) || isCleaningUp) return;
    
        try
        {
            string name = trackedImage.referenceImage.name;
            if (spawnedPrefabs.TryGetValue(name, out var prefab) && prefab != null)
            {
                if (useWorldSpacePositioning)
                {
                    Vector3 targetPos = trackedImage.transform.position;
                    Quaternion targetRot = trackedImage.transform.rotation;
                    
                    if (Vector3.Distance(prefab.transform.position, targetPos) > 0.001f ||
                        Quaternion.Angle(prefab.transform.rotation, targetRot) > 0.1f)
                    {
                        prefab.transform.position = Vector3.Lerp(prefab.transform.position, targetPos, Time.deltaTime * 10f);
                        prefab.transform.rotation = Quaternion.Lerp(prefab.transform.rotation, targetRot, Time.deltaTime * 10f);
                    }
                }
                else
                {
                    Vector3 targetPos = positionOffset;
                    Quaternion targetRot = Quaternion.Euler(rotationOffset);
                    
                    if (Vector3.Distance(prefab.transform.localPosition, targetPos) > 0.001f ||
                        Quaternion.Angle(prefab.transform.localRotation, targetRot) > 0.1f)
                    {
                        prefab.transform.localPosition = Vector3.Lerp(prefab.transform.localPosition, targetPos, Time.deltaTime * 10f);
                        prefab.transform.localRotation = Quaternion.Lerp(prefab.transform.localRotation, targetRot, Time.deltaTime * 10f);
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ImageTracker] Exception in UpdatePrefabTransform: {ex.Message}");
        }
    }

    void ShowMarkerContentByName(string markerName)
    {
        if (isCleaningUp) return;

        try
        {
            if (markerDictionary.TryGetValue(markerName, out var data))
            {
                if (!spawnedPrefabs.ContainsKey(markerName))
                {
                    GameObject instance = Instantiate(
                        data.prefab, 
                        Vector3.zero, 
                        Quaternion.identity
                    );

                    instance.transform.localScale = Vector3.one * scaleMultiplier;
                    instance.SetActive(true);
                    spawnedPrefabs[markerName] = instance;
                }
                else
                {
                    spawnedPrefabs[markerName].SetActive(true);
                }

                if (currentActiveMarker != markerName && contentHandler != null)
                {
                    contentHandler.ShowContent(data.question, data.audioClip);
                    Debug.Log($"[ImageTracker] Showed UI content for marker: {markerName}");
                }

                currentActiveMarker = markerName;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ImageTracker] Exception in ShowMarkerContentByName: {ex.Message}");
        }
    }

    void HideCurrentMarkerContent()
    {
        if (currentActiveMarker == null) return;

        try
        {
            if (contentHandler != null)
            {
                contentHandler.HideContent();
                Debug.Log($"[ImageTracker] Hid UI content for marker: {currentActiveMarker}");
            }

            if (spawnedPrefabs.ContainsKey(currentActiveMarker) && spawnedPrefabs[currentActiveMarker] != null)
            {
                spawnedPrefabs[currentActiveMarker].SetActive(false);
                Debug.Log($"[ImageTracker] Hid prefab for marker: {currentActiveMarker}");
            }

            currentActiveMarker = null;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ImageTracker] Exception in HideCurrentMarkerContent: {ex.Message}");
        }
    }

    
    System.Collections.IEnumerator PerformSafetyChecks()
    {
        while (enabled && !isCleaningUp)
        {
            yield return new WaitForSeconds(safetyCheckInterval);
            
            if (!IsTrackedImageManagerValid())
            {
                Debug.LogWarning("[ImageTracker] ARTrackedImageManager became invalid, performing cleanup");
                PerformCleanup();
                yield break;
            }
            
            var keysToRemove = new List<string>();
            foreach (var kvp in spawnedPrefabs)
            {
                if (kvp.Value == null)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }
            
            foreach (var key in keysToRemove)
            {
                spawnedPrefabs.Remove(key);
                lastTrackingStates.Remove(key);
                lastSeenTimes.Remove(key);
                Debug.Log($"[ImageTracker] Removed destroyed prefab reference: {key}");
            }
        }
    }
    
    public void ResetTracker()
    {
        Debug.Log("[ImageTracker] Manual reset requested");
        PerformCleanup();
        
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(DelayedReinitialization());
        }
    }
    
    System.Collections.IEnumerator DelayedReinitialization()
    {
        yield return new WaitForSeconds(0.1f);
        
        isCleaningUp = false;
        OnEnable();
    }
}