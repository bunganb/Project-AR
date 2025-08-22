using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoController : MonoBehaviour
{
    [Header("Main Video")]
    public VideoPlayer videoPlayer;
    public RawImage videoDisplay;
    private RectTransform videoDisplayRectTransform; 

    [Header("Pause/Play Button")]
    public Button playPauseButton;
    public TextMeshProUGUI playPauseButtonText;

    [Header("Replay Button")]
    public Button replayButton;

    [Header("Fullscreen Settings")]
    public Button fullscreenButton;
    private bool isFullscreen = false;

    [Header("UI to Hide")]
    public GameObject[] uiElementsToHide;

    private const float videoAspectRatio = 16f / 9f;
    private Vector2 initialSize; 

    private void Awake()
    {
        if (videoDisplay != null)
        {
            videoDisplayRectTransform = videoDisplay.GetComponent<RectTransform>();
            initialSize = videoDisplayRectTransform.sizeDelta; 
        }
    }
    private void Start()
    {
        if (videoPlayer == null)
        {
            Debug.LogError("VideoPlayer is not assigned to the VideoController script.");
            return;
        }

        if (playPauseButton != null)
        {
            playPauseButton.onClick.AddListener(TogglePlayPause);
        }

        if (replayButton != null)
        {
            replayButton.onClick.AddListener(ReplayVideo);
        }

        if (fullscreenButton != null)
        {
            fullscreenButton.onClick.AddListener(ToggleFullscreen);
        }

        isFullscreen = false; 
        ToggleFullscreen(); 
        UpdatePlayPauseButtonText();
    }

    public void ToggleFullscreen()
    {
        isFullscreen = !isFullscreen;

        if (isFullscreen)
        {
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            
            if (videoDisplayRectTransform != null)
            {
                videoDisplayRectTransform.anchorMin = Vector2.zero;
                videoDisplayRectTransform.anchorMax = Vector2.one;
                videoDisplayRectTransform.offsetMin = Vector2.zero;
                videoDisplayRectTransform.offsetMax = Vector2.zero;
                videoDisplayRectTransform.sizeDelta = Vector2.zero;
            }

            foreach (var uiElement in uiElementsToHide)
            {
                if (uiElement != null)
                {
                    uiElement.SetActive(false);
                }
            }
        }
        else
        {
            Screen.orientation = ScreenOrientation.Portrait;

            if (videoDisplayRectTransform != null)
            {
                videoDisplayRectTransform.offsetMin = Vector2.zero;
                videoDisplayRectTransform.offsetMax = Vector2.zero;
                videoDisplayRectTransform.sizeDelta = Vector2.zero;

                videoDisplayRectTransform.anchorMin = new Vector2(0f, 0.5f);
                videoDisplayRectTransform.anchorMax = new Vector2(1f, 0.5f);

                float canvasWidth = videoDisplayRectTransform.parent.GetComponent<RectTransform>().rect.width;
                float newHeight = canvasWidth / videoAspectRatio;

                videoDisplayRectTransform.offsetMin = new Vector2(0f, -newHeight / 2f);
                videoDisplayRectTransform.offsetMax = new Vector2(0f, newHeight / 2f);
            }

            foreach (var uiElement in uiElementsToHide)
            {
                if (uiElement != null)
                {
                    uiElement.SetActive(true);
                }
            }
        }
    }

    private void OnEnable()
    {
        if (isFullscreen)
        {
            Screen.orientation = ScreenOrientation.LandscapeLeft;
        }
        else
        {
            Screen.orientation = ScreenOrientation.Portrait;
        }
    }

    public void TogglePlayPause()
    {
        if (videoPlayer.isPrepared)
        {
            if (videoPlayer.isPlaying)
            {
                videoPlayer.Pause();
            }
            else
            {
                videoPlayer.Play();
            }
        }
        else
        {
            videoPlayer.Play();
        }

        UpdatePlayPauseButtonText();
    }

    public void ReplayVideo()
    {
        videoPlayer.Stop();
        videoPlayer.Play();
        UpdatePlayPauseButtonText();
    }

    private void UpdatePlayPauseButtonText()
    {
        if (playPauseButtonText != null)
        {
            playPauseButtonText.text = videoPlayer.isPlaying ? "Pause" : "Play";
        }
    }

    private void OnDisable()
    {
        if (playPauseButton != null)
        {
            playPauseButton.onClick.RemoveListener(TogglePlayPause);
        }
        
        if (replayButton != null)
        {
            replayButton.onClick.RemoveListener(ReplayVideo);
        }

        if (fullscreenButton != null)
        {
            fullscreenButton.onClick.RemoveListener(ToggleFullscreen);
        }

        Screen.orientation = ScreenOrientation.AutoRotation;
    }
}