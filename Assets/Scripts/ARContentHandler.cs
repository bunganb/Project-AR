using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ARContentHandler : MonoBehaviour
{
    [Header("UI Panel")]
    public RectTransform quizPanel;
    public TextMeshProUGUI questionText;
    public Button[] optionButtons;
    public Image[] optionImages;  
    public GameObject correctUI;
    public GameObject wrongUI;
    private CanvasGroup quizCanvasGroup;

    [Header("Audio")]
    public AudioSource audioSource;
    private AudioClip currentClip;
    public float audioDelay = 0.5f;
    private bool hasPlayed = false;

    [Header("Animation")]
    public float slideDuration = 0.5f;
    private bool isAnimating = false;
    private float panelHeight;

    private char correctAnswer;
    private QuizOption[] currentOptions;
    [Header("Feedback Audio")]
    public AudioSource sfxSource;
    public AudioClip correctSFX;
    public AudioClip wrongSFX;
    [Header("Audio Control")]
    public Button replayButton;

    private void Awake()
    {
        quizCanvasGroup = quizPanel.GetComponent<CanvasGroup>();
        panelHeight = quizPanel.rect.height;
        quizPanel.gameObject.SetActive(false);

        if (correctUI != null) correctUI.SetActive(false);
        if (wrongUI != null) wrongUI.SetActive(false);
        if (replayButton != null)
        {
            replayButton.onClick.RemoveAllListeners();
            replayButton.onClick.AddListener(() =>
            {
                PlayAudio(); 
            });
        }
    }

    public void PlayAudio(AudioClip clip = null)
    {
        if (clip != null)
            currentClip = clip;

        if (currentClip == null) return;

        audioSource.Stop();
        audioSource.clip = currentClip;
        audioSource.PlayDelayed(audioDelay);
        hasPlayed = true;
    }

    public void ShowContent(string question, AudioClip clip, QuizOption[] options, char correct)
    {
        questionText.text = question;
        currentClip = clip;
        correctAnswer = correct;
        currentOptions = options;
        hasPlayed = false;
        if (replayButton != null)
            replayButton.gameObject.SetActive(true);

        for (int i = 0; i < optionButtons.Length; i++)
        {
            int index = i;
            if (i < options.Length)
            {
                optionButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = options[i].text;

                if (optionImages != null && i < optionImages.Length && options[i].image != null)
                {
                    optionImages[i].sprite = options[i].image;
                    optionImages[i].enabled = true;
                }

                optionButtons[i].onClick.RemoveAllListeners();
                optionButtons[i].onClick.AddListener(() =>
                {
                    OnAnswerSelected(index);
                });

                optionButtons[i].gameObject.SetActive(true);
            }
            else
            {
                optionButtons[i].gameObject.SetActive(false);
            }
        }

        if (!isAnimating)
        {
            quizPanel.gameObject.SetActive(true);
            StartCoroutine(SlideIn(quizPanel, slideDuration));
        }
    }

    private void OnAnswerSelected(int index)
    {
        char chosen = (char)('A' + index);
        if (chosen == correctAnswer)
            ShowCorrect();
        else
            ShowWrong();
    }

    
    private void ShowCorrect()
    {
        if (correctUI != null)
        {
            correctUI.SetActive(true);
            wrongUI.SetActive(false);

            if (sfxSource != null && correctSFX != null)
            {
                sfxSource.Stop();
                sfxSource.clip = correctSFX;
                sfxSource.Play(); 
            }

            StartCoroutine(PlayFeedbackAnimation(correctUI, 1.5f));
        }
    }

    private void ShowWrong()
    {
        if (wrongUI != null)
        {
            wrongUI.SetActive(true);
            correctUI.SetActive(false);

            if (sfxSource != null && wrongSFX != null)
            {
                sfxSource.Stop(); 
                sfxSource.clip = wrongSFX;
                sfxSource.Play(); 
            }

            StartCoroutine(PlayFeedbackAnimation(wrongUI, 1.5f));
        }
    }

    private IEnumerator PlayFeedbackAnimation(GameObject feedbackUI, float duration)
    {
        RectTransform rect = feedbackUI.GetComponent<RectTransform>();
        CanvasGroup canvas = feedbackUI.GetComponent<CanvasGroup>();

        if (canvas == null)
            canvas = feedbackUI.AddComponent<CanvasGroup>();

        rect.localScale = Vector3.zero;
        canvas.alpha = 1f;

        float time = 0f;
        while (time < 0.3f)
        {
            float t = time / 0.3f;
            rect.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
            time += Time.deltaTime;
            yield return null;
        }
        rect.localScale = Vector3.one;

        yield return new WaitForSeconds(duration);

        time = 0f;
        while (time < 0.3f)
        {
            float t = time / 0.3f;
            rect.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t);
            canvas.alpha = Mathf.Lerp(1f, 0f, t);
            time += Time.deltaTime;
            yield return null;
        }

        rect.localScale = Vector3.zero;
        canvas.alpha = 0f;
        feedbackUI.SetActive(false);
    }

    public void HideContent()
    {
        audioSource.Stop();
        hasPlayed = false;

        if (!isAnimating)
        {
            StartCoroutine(SlideOut(quizPanel, slideDuration));
        }

        if (correctUI != null) correctUI.SetActive(false);
        if (wrongUI != null) wrongUI.SetActive(false);
    }


    private IEnumerator SlideIn(RectTransform panel, float duration)
    {
        isAnimating = true;
        Vector2 startPos = new Vector2(panel.anchoredPosition.x, -panelHeight - 50);
        Vector2 endPos = new Vector2(panel.anchoredPosition.x, 0);

        float time = 0f;
        while (time < duration)
        {
            float t = time / duration;
            t = t * t * (3f - 2f * t);

            panel.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

            if (quizCanvasGroup != null)
                quizCanvasGroup.alpha = Mathf.Lerp(0, 1, t);

            time += Time.deltaTime;
            yield return null;
        }

        panel.anchoredPosition = endPos;
        if (quizCanvasGroup != null)
            quizCanvasGroup.alpha = 1;

        isAnimating = false;
    }

    private IEnumerator SlideOut(RectTransform panel, float duration)
    {
        isAnimating = true;
        Vector2 startPos = new Vector2(panel.anchoredPosition.x, 0);
        Vector2 endPos = new Vector2(panel.anchoredPosition.x, -panelHeight - 50);

        float time = 0f;
        while (time < duration)
        {
            float t = time / duration;
            t = t * t * (3f - 2f * t);

            panel.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

            if (quizCanvasGroup != null)
                quizCanvasGroup.alpha = Mathf.Lerp(1, 0, t);

            time += Time.deltaTime;
            yield return null;
        }

        panel.anchoredPosition = endPos;
        if (quizCanvasGroup != null)
            quizCanvasGroup.alpha = 0;

        panel.gameObject.SetActive(false);
        isAnimating = false;
    }
}
