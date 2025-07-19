using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuNavigation : MonoBehaviour
{
    public void GoToMenuScene()
    {
        SceneManager.LoadScene("Scenes/Menu");
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
