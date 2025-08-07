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

    private void Awake()
    {
        quizCanvasGroup = quizPanel.GetComponent<CanvasGroup>();
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
            quizPanel.offsetMin = new Vector2(0, -600);
            quizPanel.offsetMax = new Vector2(0, -600);
            
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

        Vector2 startMin = new Vector2(0, -600);
        Vector2 startMax = new Vector2(0, -600);
        Vector2 end = Vector2.zero;

        float time = 0f;
        while (time < duration)
        {
            panel.offsetMin = Vector2.Lerp(startMin, end, time / duration);
            panel.offsetMax = Vector2.Lerp(startMax, end, time / duration);
            if (quizCanvasGroup != null)
            {
                quizCanvasGroup.alpha = Mathf.Lerp(0, 1, time / duration);
            }
            time += Time.deltaTime;
            yield return null;
        }

        panel.offsetMin = end;
        panel.offsetMax = end;
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

        Vector2 start = Vector2.zero;
        Vector2 endMin = new Vector2(0, -600);
        Vector2 endMax = new Vector2(0, -600);

        float time = 0f;
        while (time < duration)
        {
            panel.offsetMin = Vector2.Lerp(start, endMin, time / duration);
            panel.offsetMax = Vector2.Lerp(start, endMax, time / duration);
            if (quizCanvasGroup != null)
            {
                quizCanvasGroup.alpha = Mathf.Lerp(1, 0, time / duration);
            }
            time += Time.deltaTime;
            yield return null;
        }

        panel.offsetMin = endMin;
        panel.offsetMax = endMax;
        if (quizCanvasGroup != null)
        {
            quizCanvasGroup.alpha = 0;
        }
        isAnimating = false;
    }
}