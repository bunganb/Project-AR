using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class FlexibleARTracking : MonoBehaviour
{
    [Header("Tracking Settings")]
    [Tooltip("Jika true, marker bisa dideteksi meski tracking terbatas (misalnya posisi samping).")]
    public bool acceptLimitedTracking = false;

    private ARTrackedImageManager trackedImageManager;

    void Awake()
    {
        trackedImageManager = FindObjectOfType<ARTrackedImageManager>();
    }

    void OnEnable()
    {
        if (trackedImageManager != null)
            trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        if (trackedImageManager != null)
            trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (var trackedImage in args.updated)
        {
            // Cek status tracking
            bool isTracking = trackedImage.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.Tracking;
            bool isLimited = trackedImage.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.Limited;

            if (acceptLimitedTracking && (isTracking || isLimited))
            {
                // Terima keduanya
                Debug.Log($"Marker {trackedImage.referenceImage.name} aktif (tracking/limited).");
                trackedImage.gameObject.SetActive(true);
            }
            else if (!acceptLimitedTracking && isTracking)
            {
                // Hanya tracking penuh
                Debug.Log($"Marker {trackedImage.referenceImage.name} aktif (full tracking).");
                trackedImage.gameObject.SetActive(true);
            }
            else
            {
                // Nonaktifkan kalau tidak sesuai
                trackedImage.gameObject.SetActive(false);
            }
        }
    }
}