using UnityEngine;
using UnityEngine.UI;

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
        UpdateImage();

        nextButton.onClick.AddListener(NextImage);
        previousButton.onClick.AddListener(PreviousImage);
        downloadButton.onClick.AddListener(DownloadCurrentImage);
    }

    public void NextImage()
    {
        currentIndex++;
        if (currentIndex >= imageList.Length)
            currentIndex = 0;

        UpdateImage();
    }

    public void PreviousImage()
    {
        currentIndex--;
        if (currentIndex < 0)
            currentIndex = imageList.Length - 1;


        UpdateImage();
    }

    private void UpdateImage()
    {
        if (imageList.Length > 0)
        {
            displayImage.sprite = imageList[currentIndex];
            
        }
    }
    public void DownloadCurrentImage()
    {
        if (displayImage.sprite == null) return;

        Texture2D texture = displayImage.sprite.texture;

        if (!texture.isReadable)
        {
            return;
        }
        Texture2D copy = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
        copy.SetPixels(texture.GetPixels());
        copy.Apply();

        byte[] bytes = copy.EncodeToPNG();
        string filePath = Application.persistentDataPath + $"/image_{currentIndex}.png";
        System.IO.File.WriteAllBytes(filePath, bytes);

        #if UNITY_EDITOR
                UnityEditor.EditorUtility.RevealInFinder(filePath);
        #elif UNITY_STANDALONE_WIN
            System.Diagnostics.Process.Start("explorer.exe", "/select," + filePath.Replace("/", "\\"));
        #endif
    }


}