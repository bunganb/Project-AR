using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

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
    public ARTrackedImageManager trackedImages;
    public List<MarkerContentData> markerContents;
    public ARContentHandler contentHandler;

    private Dictionary<string, MarkerContentData> markerDictionary;
    private Dictionary<string, GameObject> spawnedPrefabs = new Dictionary<string, GameObject>();
    private string currentActiveMarker = null;
    private Coroutine hideUICoroutine;
    public float hideUIDelay = 2f;
    void Awake()
    {
        trackedImages = GetComponent<ARTrackedImageManager>();
        markerDictionary = new Dictionary<string, MarkerContentData>();

        foreach (var data in markerContents)
        {
            markerDictionary[data.markerName] = data;
        }
    }
    

    void OnEnable() => trackedImages.trackedImagesChanged += OnTrackedImagesChanged;
    void OnDisable() => trackedImages.trackedImagesChanged -= OnTrackedImagesChanged;

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (var trackedImage in args.added)
        {
            ShowMarkerContent(trackedImage);
        }

        foreach (var trackedImage in args.updated)
        {
            Debug.Log($"[Update] {trackedImage.referenceImage.name} state: {trackedImage.trackingState}");
        
            if (trackedImage.trackingState == TrackingState.Tracking || trackedImage.trackingState == TrackingState.Limited)
            {
                ShowMarkerContent(trackedImage);
            }
            else
            {
                HideMarkerContent(trackedImage);
            }
        }

        foreach (var trackedImage in args.removed)
        {
            Debug.Log($"[Removed] {trackedImage.referenceImage.name} was removed.");
            HideMarkerContent(trackedImage);
        }
    }
    void UpdateMarkerContent(ARTrackedImage trackedImage)
    {
        string name = trackedImage.referenceImage.name;

        if (spawnedPrefabs.TryGetValue(name, out var instance))
        {
            instance.transform.position = trackedImage.transform.position;
            instance.transform.rotation = trackedImage.transform.rotation;
            instance.SetActive(true);
        }
    }

    void ShowMarkerContent(ARTrackedImage trackedImage)
    {
        string name = trackedImage.referenceImage.name;

        if (markerDictionary.TryGetValue(name, out var data))
        {
            if (currentActiveMarker != null && currentActiveMarker != name)
            {
                if (spawnedPrefabs.TryGetValue(currentActiveMarker, out var previous))
                {
                    previous.SetActive(false);
                }
            }

            if (!spawnedPrefabs.ContainsKey(name))
            {
                var instance = Instantiate(data.prefab, trackedImage.transform);
                instance.transform.localPosition = new Vector3(0f, 0.05f, 0f);
                instance.transform.localScale = Vector3.one * 0.1f;
                spawnedPrefabs[name] = instance;
            }
            if (hideUICoroutine != null)
            {
                StopCoroutine(hideUICoroutine);
                hideUICoroutine = null;
            }

            spawnedPrefabs[name].SetActive(true); 

            contentHandler.ShowContent(data.question, data.audioClip);

            currentActiveMarker = name;
        }
    }

    void HideMarkerContent(ARTrackedImage trackedImage)
    {
        string name = trackedImage.referenceImage.name;

        if (spawnedPrefabs.ContainsKey(name))
        {
            spawnedPrefabs[name].SetActive(false);
        }

        if (currentActiveMarker == name)
        {
            if (hideUICoroutine != null)
            {
                StopCoroutine(hideUICoroutine);
            }

            hideUICoroutine = StartCoroutine(DelayedHideUI(name));
        }
    }
    private IEnumerator DelayedHideUI(string markerName)
    {
        yield return new WaitForSeconds(hideUIDelay);

        if (currentActiveMarker == markerName)
        {
            contentHandler.HideContent();
            currentActiveMarker = null; 
        }
    }

}
 