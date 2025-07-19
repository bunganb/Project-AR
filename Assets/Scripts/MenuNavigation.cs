using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuNavigation : MonoBehaviour
{
    public static MenuNavigation Instance;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void GoToMenuScene()
    {
        SceneManager.LoadScene("Scenes/Menu");
    }
    public void GoToTitleScene()
    {
        SceneManager.LoadScene("Scenes/TitleScene");
    }
    public void GoToVideoYt()
    {
        SceneManager.LoadScene("Scenes/VideoPlayerScene(Youtube)");
    }
    public void GoToVideoVS()
    {
        SceneManager.LoadScene("Scenes/VideoPlayerScene(VidSource)");
    }
    public void GoToAR()
    {
        SceneManager.LoadScene("Scenes/ARScene");
    }
    public void GoToGuide()
    {
        SceneManager.LoadScene("Scenes/GuideScene");
    }
    public void GoToMarker()
    {
        SceneManager.LoadScene("Scenes/Gallery Marker");
    }
}
