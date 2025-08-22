using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ARContentHandler : MonoBehaviour
{
    [Header("UI Panel")]
    public RectTransform quizPanel;
    public TextMeshProUGUI questionText;
    public Button playButton;
    private CanvasGroup quizCanvasGroup;

    [Header("Audio")]
    public AudioSource audioSource;
    private AudioClip currentClip;
    private bool hasPlayed = false;

    [Header("Animation")]
    public float slideDuration = 0.5f;
    private bool isAnimating = false;
    
    // Store original panel height
    private float panelHeight;

    private void Awake()
    {
        quizCanvasGroup = quizPanel.GetComponent<CanvasGroup>();
        panelHeight = quizPanel.rect.height;
    }

    private void Start()
    {
        if (playButton != null)
        {
            playButton.onClick.AddListener(PlayAudio);
        }
        else
        {
            Debug.LogWarning("PlayButton belum di-assign di Inspector.");
        }

        if (quizPanel != null)
        {
            quizPanel.anchoredPosition = new Vector2(quizPanel.anchoredPosition.x, -panelHeight);
            
            if (quizCanvasGroup != null)
            {
                quizCanvasGroup.alpha = 0;
                quizCanvasGroup.interactable = false;
                quizCanvasGroup.blocksRaycasts = false;
            }
        }
    }

    public void ShowContent(string question, AudioClip clip)
    {
        if (questionText != null)
        {
            questionText.text = question;
        }

        currentClip = clip;

        if (!hasPlayed && currentClip != null)
        {
            audioSource.clip = currentClip;
            audioSource.Play();
            hasPlayed = true;
        }

        if (quizPanel != null && !isAnimating)
        {
            StopAllCoroutines();
            StartCoroutine(SlideIn(quizPanel, slideDuration));
        }
    }

    public void HideContent()
    {
        audioSource.Stop();
        hasPlayed = false;

        if (quizPanel != null && !isAnimating)
        {
            StopAllCoroutines();
            StartCoroutine(SlideOut(quizPanel, slideDuration));
        }
    }

    public void PlayAudio()
    {
        if (currentClip != null && !audioSource.isPlaying)
        {
            audioSource.clip = currentClip;
            audioSource.Play();
        }
    }

    private IEnumerator SlideIn(RectTransform panel, float duration)
    {
        isAnimating = true;
        
        if (quizCanvasGroup != null)
        {
            quizCanvasGroup.interactable = true;
            quizCanvasGroup.blocksRaycasts = true;
        }

        Vector2 startPos = new Vector2(panel.anchoredPosition.x, -panelHeight - 50);
        Vector2 endPos = new Vector2(panel.anchoredPosition.x, 0);

        float time = 0f;
        while (time < duration)
        {
            float t = time / duration;
            t = t * t * (3f - 2f * t);
            
            panel.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            
            if (quizCanvasGroup != null)
            {
                quizCanvasGroup.alpha = Mathf.Lerp(0, 1, t);
            }
            
            time += Time.deltaTime;
            yield return null;
        }

        panel.anchoredPosition = endPos;
        if (quizCanvasGroup != null)
        {
            quizCanvasGroup.alpha = 1;
        }
        isAnimating = false;
    }

    private IEnumerator SlideOut(RectTransform panel, float duration)
    {
        isAnimating = true; 
        
        if (quizCanvasGroup != null)
        {
            quizCanvasGroup.interactable = false;
            quizCanvasGroup.blocksRaycasts = false;
        }

        Vector2 startPos = new Vector2(panel.anchoredPosition.x, 0);
        Vector2 endPos = new Vector2(panel.anchoredPosition.x, -panelHeight - 50);

        float time = 0f;
        while (time < duration)
        {
            float t = time / duration;
            t = t * t * (3f - 2f * t);
            
            panel.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            
            if (quizCanvasGroup != null)
            {
                quizCanvasGroup.alpha = Mathf.Lerp(1, 0, t);
            }
            
            time += Time.deltaTime;
            yield return null;
        }

        panel.anchoredPosition = endPos;
        if (quizCanvasGroup != null)
        {
            quizCanvasGroup.alpha = 0;
        }
        isAnimating = false;
    }
}