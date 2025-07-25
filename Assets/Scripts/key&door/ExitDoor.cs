using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class ExitDoor : MonoBehaviour
{
    [Header("Door Settings")]
    public string doorID = "ExitDoor_01";
    public bool requireAllKeys = false;
    public List<string> requiredKeyIDs = new List<string>() { "Key_01" };

    [Header("Transition")]
    public string nextSceneName;
    public int nextSceneIndex = -1;
    public float transitionDelay = 1f;

    [Header("Visual")]
    public GameObject lockedVisual;
    public GameObject unlockedVisual;
    public Animator doorAnimator;
    public string openAnimationTrigger = "Open";

    [Header("Effects")]
    public GameObject unlockEffect;
    public AudioClip unlockSound;
    public AudioClip openSound;
    public AudioClip lockedSound;

    [Header("Events")]
    public UnityEvent onDoorUnlocked;
    public UnityEvent onDoorOpened;
    public UnityEvent onDoorLocked;

    private bool isUnlocked = false;
    private bool isOpen = false;
    private KeyManager keyManager;

    [System.Obsolete]
    void Start()
    {
        keyManager = FindObjectOfType<KeyManager>();

        // Subscribe to key events
        if (keyManager)
        {
            keyManager.onKeyCollected.AddListener(CheckDoorStatus);
        }

        // Initial check
        CheckDoorStatus();
    }

    void CheckDoorStatus()
    {
        bool hasAllKeys = true;

        if (requireAllKeys && keyManager)
        {
            hasAllKeys = keyManager.HasAllKeys();
        }
        else
        {
            // Check specific keys
            foreach (string keyID in requiredKeyIDs)
            {
                if (PlayerPrefs.GetInt($"Key_{keyID}_Collected", 0) == 0)
                {
                    hasAllKeys = false;
                    break;
                }
            }
        }

        SetDoorState(hasAllKeys);
    }

    void SetDoorState(bool unlocked)
    {
        isUnlocked = unlocked;

        // Update visuals
        if (lockedVisual) lockedVisual.SetActive(!unlocked);
        if (unlockedVisual) unlockedVisual.SetActive(unlocked);

        // Play unlock effect
        if (unlocked && !isOpen)
        {
            if (unlockEffect)
                Instantiate(unlockEffect, transform.position, Quaternion.identity);

            if (unlockSound)
                AudioSource.PlayClipAtPoint(unlockSound, transform.position);

            onDoorUnlocked?.Invoke();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (isUnlocked && !isOpen)
            {
                OpenDoor();
            }
            else if (!isUnlocked)
            {
                ShowLockedFeedback();
            }
        }
    }

    void OpenDoor()
    {
        isOpen = true;

        // Animation
        if (doorAnimator)
        {
            doorAnimator.SetTrigger(openAnimationTrigger);
        }

        // Sound
        if (openSound)
        {
            AudioSource.PlayClipAtPoint(openSound, transform.position);
        }

        // Event
        onDoorOpened?.Invoke();

        // Transition
        StartCoroutine(TransitionToNextScene());
    }

    void ShowLockedFeedback()
    {
        if (lockedSound)
        {
            AudioSource.PlayClipAtPoint(lockedSound, transform.position);
        }

        // Visual shake or feedback
        StartCoroutine(ShakeDoor());

        onDoorLocked?.Invoke();

        // Show required keys UI
        if (keyManager)
        {
            keyManager.ShowRequiredKeys(requiredKeyIDs);
        }
    }

    IEnumerator ShakeDoor()
    {
        Vector3 originalPos = transform.position;
        float elapsed = 0f;
        float duration = 0.5f;
        float magnitude = 0.1f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            transform.position = new Vector3(originalPos.x + x, originalPos.y, originalPos.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPos;
    }

    IEnumerator TransitionToNextScene()
    {
        // Fade out or transition effect
        yield return new WaitForSeconds(transitionDelay);

        // Load next scene
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else if (nextSceneIndex >= 0)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
    }
}
