using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SceneButton : MonoBehaviour
{
    public enum TargetScene
    {
        Menu, YoutubeVideo, VideoSource, AR, Guide, Marker, Title, InputName
    }

    public TargetScene scene;
    public TMP_InputField nameInput; 
    public bool requireName;

    public void OnButtonClick()
    {
        if (requireName)
        {
            string userName = nameInput.text.Trim();

            if (string.IsNullOrEmpty(userName))
            {
                Debug.LogWarning("Nama harus diisi!");
                return;
            }

            PlayerPrefs.SetString("UserName", userName);
            PlayerPrefs.Save();
        }

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
            case TargetScene.Marker:
                MenuNavigation.Instance.GoToMarker();
                break;
            case TargetScene.Title:
                MenuNavigation.Instance.GoToTitleScene();
                break;
            case TargetScene.InputName:
                MenuNavigation.Instance.GoToInputName();
                break;
        }
    }
}