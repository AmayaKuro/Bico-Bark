//using System.Collections;
//using TMPro;
//using UnityEngine;
//using UnityEngine.UI;

//public class SuccessSceneController : MonoBehaviour
//{
//    [Header("UI Elements")]
//    public TextMeshProUGUI congratsText;
//    public TextMeshProUGUI messageText;
//    public Button continueButton;
//    public Button quitButton;

//    [Header("Visual Effects")]
//    public ParticleSystem[] confettiEffects;
//    public GameObject[] starsObjects;
//    public Image fadePanel;

//    [Header("Audio")]
//    public AudioClip victorySound;
//    public AudioClip backgroundMusic;
//    public AudioSource audioSource;

//    [Header("Animation Settings")]
//    public float fadeInDuration = 1f;
//    public float textAnimationDelay = 0.5f;
//    public float starAnimationDelay = 0.3f;

//    void Start()
//    {
//        // Setup buttons
//        if (continueButton)
//        {
//            continueButton.onClick.AddListener(OnContinueClicked);
//            continueButton.gameObject.SetActive(false);
//        }

//        if (quitButton)
//        {
//            quitButton.onClick.AddListener(OnQuitClicked);
//            quitButton.gameObject.SetActive(false);
//        }

//        // Start success animation sequence
//        StartCoroutine(SuccessAnimationSequence());
//    }

//    IEnumerator SuccessAnimationSequence()
//    {
//        // 1. Fade in from black
//        if (fadePanel)
//        {
//            fadePanel.color = Color.black;
//            yield return FadeOut(fadePanel, fadeInDuration);
//        }

//        // 2. Play victory sound
//        if (audioSource && victorySound)
//        {
//            audioSource.PlayOneShot(victorySound);
//        }

//        // 3. Show congratulations text with animation
//        if (congratsText)
//        {
//            congratsText.gameObject.SetActive(true);
//            yield return AnimateText(congratsText, "CONGRATULATIONS!");
//        }

//        yield return new WaitForSeconds(textAnimationDelay);

//        // 4. Show message text
//        if (messageText)
//        {
//            messageText.gameObject.SetActive(true);
//            yield return AnimateText(messageText, "You have completed all levels!");
//        }

//        // 5. Animate stars one by one
//        if (starsObjects != null)
//        {
//            foreach (var star in starsObjects)
//            {
//                if (star)
//                {
//                    star.SetActive(true);
//                    AnimateStar(star);
//                    yield return new WaitForSeconds(starAnimationDelay);
//                }
//            }
//        }

//        // 6. Play confetti effects
//        if (confettiEffects != null)
//        {
//            foreach (var confetti in confettiEffects)
//            {
//                if (confetti)
//                    confetti.Play();
//            }
//        }

//        // 7. Play background music
//        if (audioSource && backgroundMusic)
//        {
//            audioSource.clip = backgroundMusic;
//            audioSource.loop = true;
//            audioSource.Play();
//        }

//        yield return new WaitForSeconds(1f);

//        // 8. Show buttons
//        if (continueButton)
//        {
//            continueButton.gameObject.SetActive(true);
//            FadeInButton(continueButton);
//        }

//        if (quitButton)
//        {
//            quitButton.gameObject.SetActive(true);
//            FadeInButton(quitButton);
//        }
//    }

//    IEnumerator AnimateText(TextMeshProUGUI textComponent, string fullText)
//    {
//        textComponent.text = "";

//        // Type writer effect
//        foreach (char letter in fullText)
//        {
//            textComponent.text += letter;
//            yield return new WaitForSeconds(0.05f);
//        }

//        // Pulse animation
//        LeanTween.scale(textComponent.gameObject, Vector3.one * 1.1f, 0.3f)
//            .setEaseOutBack()
//            .setLoopPingPong(1);
//    }

//    void AnimateStar(GameObject star)
//    {
//        // Start small and rotate
//        star.transform.localScale = Vector3.zero;

//        LeanTween.scale(star, Vector3.one, 0.5f)
//            .setEaseOutBack();

//        LeanTween.rotateZ(star, 360f, 2f)
//            .setLoopClamp();
//    }

//    void FadeInButton(Button button)
//    {
//        CanvasGroup canvasGroup = button.GetComponent<CanvasGroup>();
//        if (!canvasGroup)
//            canvasGroup = button.gameObject.AddComponent<CanvasGroup>();

//        canvasGroup.alpha = 0;

//        LeanTween.alphaCanvas(canvasGroup, 1f, 0.5f)
//            .setEaseOutQuad();

//        // Slide up animation
//        Vector3 originalPos = button.transform.position;
//        button.transform.position = originalPos - Vector3.up * 50f;

//        LeanTween.move(button.gameObject, originalPos, 0.5f)
//            .setEaseOutQuad();
//    }

//    IEnumerator FadeOut(Image image, float duration)
//    {
//        Color color = image.color;
//        float elapsed = 0;

//        while (elapsed < duration)
//        {
//            elapsed += Time.deltaTime;
//            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
//            image.color = new Color(color.r, color.g, color.b, alpha);
//            yield return null;
//        }

//        image.color = new Color(color.r, color.g, color.b, 0);
//    }
//    void OnContinueClicked()
//    {
//        // Load main menu or level selection
//        Debug.Log("Continue clicked - Load next scene");

//        // Example:
//        // SceneManager.LoadScene("MainMenu");
//        // or
//        // SceneManager.LoadScene("LevelSelection");
//    }

//    void OnQuitClicked()
//    {
//        Debug.Log("Quit clicked");

//#if UNITY_EDITOR
//        UnityEditor.EditorApplication.isPlaying = false;
//#else
//            Application.Quit();
//#endif
//    }
//}