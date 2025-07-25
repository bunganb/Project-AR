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
    public string title;
    public string description;
}

public class ImageTracker : MonoBehaviour
{
    public ARTrackedImageManager trackedImages;
    public List<MarkerContentData> markerContents;
    public ARContentHandler contentHandler;

    private Dictionary<string, MarkerContentData> markerDictionary;
    private Dictionary<string, GameObject> spawnedPrefabs = new Dictionary<string, GameObject>();
    private string currentActiveMarker = null;


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
            if (trackedImage.trackingState == TrackingState.Tracking)
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
            HideMarkerContent(trackedImage);
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
                instance.transform.localPosition = new Vector3(0f, 0.05f, 0f); // posisi di atas marker
                spawnedPrefabs[name] = instance;
            }

            spawnedPrefabs[name].SetActive(true);
            contentHandler.ShowContent(data.title, data.description, data.audioClip);

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
            contentHandler.HideContent();
            currentActiveMarker = null;
        }
    }

}
