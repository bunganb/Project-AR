using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoController : MonoBehaviour
{
    [Header("Main Video")]
    public VideoPlayer videoPlayer;

    [Header("Pause/Play Button")]
    public Button playPauseButton;
    public TextMeshProUGUI playPauseButtonText;

    [Header("Replay Button")]
    public Button replayButton;

    private void Start()
    {
        if (playPauseButton != null)
            playPauseButton.onClick.AddListener(TogglePlayPause);

        if (replayButton != null)
            replayButton.onClick.AddListener(ReplayVideo);

        videoPlayer.started += OnVideoStarted;
        double duration = videoPlayer.length;

        Invoke(nameof(UpdatePlayPauseButtonText), 0.1f);
        int minutes = Mathf.FloorToInt((float)duration / 60);
        int seconds = Mathf.FloorToInt((float)duration % 60);
        Debug.Log(minutes + ":" + seconds);
    }

    private void OnVideoStarted(VideoPlayer vp)
    {
        UpdatePlayPauseButtonText();
    }

    public void TogglePlayPause()
    {
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
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
        playPauseButtonText.text = videoPlayer.isPlaying ? "Pause" : "Play";
    }
}