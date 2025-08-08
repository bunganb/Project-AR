using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class ImageSlider : MonoBehaviour
{
    public Sprite[] imageList;
    public Image displayImage;
    public Button nextButton;
    public Button previousButton;
    public Button downloadButton;

    private int currentIndex = 0;

    void Start()
    {
        // Pastikan plugin sudah diinstal dengan benar.
        // DownloadButton harus bisa diinteraksi dari awal
        downloadButton.interactable = true;

        UpdateImage();
        nextButton.onClick.AddListener(NextImage);
        previousButton.onClick.AddListener(PreviousImage);
        downloadButton.onClick.AddListener(DownloadCurrentImage);
    }

    public void NextImage()
    {
        currentIndex = (currentIndex + 1) % imageList.Length;
        UpdateImage();
    }

    public void PreviousImage()
    {
        currentIndex = (currentIndex - 1 + imageList.Length) % imageList.Length;
        UpdateImage();
    }

    private void UpdateImage()
    {
        if (imageList.Length > 0)
            displayImage.sprite = imageList[currentIndex];
    }

    public async void DownloadCurrentImage()
{
    Debug.Log("Download button clicked.");

    if (displayImage.sprite == null)
    {
        Debug.LogError("No image to download. The displayImage.sprite is null.");
#if UNITY_ANDROID
        ShowAndroidToast("Tidak ada gambar untuk disimpan!");
#endif
        return;
    }

    Debug.Log("Checking for Write permission...");
    NativeGallery.Permission permission = await NativeGallery.RequestPermissionAsync(NativeGallery.PermissionType.Write, NativeGallery.MediaType.Image);
    
    if (permission == NativeGallery.Permission.Granted)
    {
        Debug.Log("Permission granted. Saving to gallery...");
        SaveToGallery();
    }
    else
    {
        Debug.LogWarning("Permission denied. Cannot save image.");
#if UNITY_ANDROID
        ShowAndroidToast("Izin ditolak! Tidak bisa menyimpan.");
#endif
    }
}

    private void SaveToGallery()
    {
        Debug.Log("Starting SaveToGallery process.");

        if (displayImage.sprite == null)
        {
            Debug.LogError("Sprite is null in SaveToGallery. Cannot proceed.");
            return;
        }

        Texture2D texture = displayImage.sprite.texture;
        if (!texture.isReadable)
        {
            Debug.LogWarning("Texture is not readable. Creating a readable copy...");
            texture = DuplicateTexture(texture);
            if (texture == null)
            {
                Debug.LogError("Failed to create a readable texture copy.");
                return;
            }
        }

        Debug.Log("Encoding texture to PNG...");
        byte[] bytes = texture.EncodeToPNG();
        if (bytes == null || bytes.Length == 0)
        {
            Debug.LogError("Failed to encode texture to PNG. Bytes array is empty.");
            return;
        }

        string filename = $"image_{currentIndex}.png";
        string tempPath = Path.Combine(Application.temporaryCachePath, filename);
        File.WriteAllBytes(tempPath, bytes);
    
        Debug.Log($"File written to temporary path: {tempPath}");

        NativeGallery.SaveImageToGallery(tempPath, "ImageSliderGallery", filename);
        Debug.Log("NativeGallery.SaveImageToGallery called.");

#if UNITY_ANDROID
        ShowAndroidToast("Gambar berhasil disimpan ke Galeri!");
#endif
    }

    private Texture2D DuplicateTexture(Texture2D source)
    {
        RenderTexture rt = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
        Graphics.Blit(source, rt);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D readableTex = new Texture2D(source.width, source.height);
        readableTex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        readableTex.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);
        return readableTex;
    }
#if UNITY_ANDROID
    private void ShowAndroidToast(string message)
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", activity, message, toastClass.GetStatic<int>("LENGTH_SHORT"));
            toastObject.Call("show");
        }));
    }
#endif
    
}