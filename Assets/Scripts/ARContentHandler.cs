using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ARContentHandler : MonoBehaviour
{
    public GameObject infoPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public Button playButton;
    public AudioSource audioSource;
    private AudioClip currentClip;
    private bool hasPlayed = false;

    private void Start()
    {
        if (playButton != null)
        {
            playButton.onClick.AddListener(PlayAudio);
        }
        else
        {
            Debug.Log("PlayButton belum di-assign di Inspector.");
        }
    }


    public void ShowContent(string title, string description, AudioClip clip)
    {
        titleText.text = title;
        descriptionText.text = description;
        currentClip = clip;
        infoPanel.SetActive(true);

        if (!hasPlayed && currentClip != null)
        {
            audioSource.clip = currentClip;
            audioSource.Play();
            hasPlayed = true;
        }
    }
    public void HideContent()
    {
        infoPanel.SetActive(false);
        audioSource.Stop();
        hasPlayed = false; 
    }


    public void PlayAudio()
    {
        if (currentClip != null && !audioSource.isPlaying)
        {
            audioSource.clip = currentClip;
            audioSource.Play();
        }
    }
}