using System.Collections;
using UnityEngine;

public class MazeDoor : MonoBehaviour
{
    [Header("Door Settings")]
    public string doorID = "D1";
    public bool startOpen = false;
    public float animationSpeed = 2f;

    [Header("Visual")]
    public SpriteRenderer doorVisual;
    public GameObject openEffect;
    public GameObject closeEffect;

    [Header("Audio")]
    public AudioClip openSound;
    public AudioClip closeSound;

    // Private
    private bool isOpen;
    private BoxCollider2D doorCollider;
    private AudioSource audioSource;
    private Coroutine animationCoroutine;

    void Start()
    {
        isOpen = startOpen;
        doorCollider = GetComponent<BoxCollider2D>();
        audioSource = gameObject.AddComponent<AudioSource>();

        if (!doorVisual) doorVisual = GetComponent<SpriteRenderer>();

        // Set initial state
        UpdateDoorState(true);
    }

    public void ToggleDoorState()
    {
        SetDoorOpen(!isOpen);
    }

    public void SetDoorOpen(bool open)
    {
        if (isOpen == open) return;

        isOpen = open;
        UpdateDoorState(false);
    }

    void UpdateDoorState(bool instant)
    {
        // Stop previous animation
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        if (instant)
        {
            // Instant update
            doorCollider.enabled = !isOpen;
            if (doorVisual)
            {
                doorVisual.color = new Color(1, 1, 1, isOpen ? 0.2f : 1f);
            }
        }
        else
        {
            // Animated update
            animationCoroutine = StartCoroutine(AnimateDoor());
        }

        Debug.Log($"Door {doorID}: {(isOpen ? "OPEN" : "CLOSED")}");
    }

    IEnumerator AnimateDoor()
    {
        // Sound
        if (isOpen && openSound) audioSource.PlayOneShot(openSound);
        if (!isOpen && closeSound) audioSource.PlayOneShot(closeSound);

        // Effect
        if (isOpen && openEffect) Instantiate(openEffect, transform.position, Quaternion.identity);
        if (!isOpen && closeEffect) Instantiate(closeEffect, transform.position, Quaternion.identity);

        // Animation
        float startAlpha = doorVisual.color.a;
        float targetAlpha = isOpen ? 0.2f : 1f;
        float elapsed = 0f;

        while (elapsed < 1f / animationSpeed)
        {
            elapsed += Time.deltaTime;
            float t = elapsed * animationSpeed;

            if (doorVisual)
            {
                float alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                doorVisual.color = new Color(1, 1, 1, alpha);
            }

            yield return null;
        }

        // Final state
        doorCollider.enabled = !isOpen;
        if (doorVisual)
        {
            doorVisual.color = new Color(1, 1, 1, targetAlpha);
        }
    }

    public bool IsOpen()
    {
        return isOpen;
    }

    // Show door ID in editor
    void OnDrawGizmos()
    {
        Gizmos.color = isOpen ? Color.green : Color.red;
        Gizmos.DrawWireCube(transform.position, GetComponent<BoxCollider2D>()?.size ?? Vector3.one);

#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position, doorID);
#endif
    }
}
