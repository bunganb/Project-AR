using UnityEngine;

public class SceneButton : MonoBehaviour
{
    public enum TargetScene
    {
        Menu, YoutubeVideo, VideoSource, AR, Guide, Marker,Title
    }

    public TargetScene scene;

    public void OnButtonClick()
    {
        Debug.Log("Clicked button for: " + scene);
        if (MenuNavigation.Instance == null)
        {
            Debug.LogError("MenuNavigation instance is missing!");
            return;
        }

        switch (scene)
        {
            case TargetScene.Menu:
                MenuNavigation.Instance.GoToMenuScene();
                break;
            case TargetScene.YoutubeVideo:
                MenuNavigation.Instance.GoToVideoYt();
                break;
            case TargetScene.VideoSource:
                MenuNavigation.Instance.GoToVideoVS();
                break;
            case TargetScene.AR:
                MenuNavigation.Instance.GoToAR();
                break;
            case TargetScene.Guide:
                MenuNavigation.Instance.GoToGuide();
                break;
            case TargetScene.Marker:
                MenuNavigation.Instance.GoToMarker();
                break;
            case TargetScene.Title:
                MenuNavigation.Instance.GoToTitleScene();
                break;
        }
    }
}