using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MultiTilemapButton : MonoBehaviour
{
    [Header("Button Mode")]
    public ButtonMode mode = ButtonMode.Hold;
    public enum ButtonMode
    {
        Hold,        
        Toggle,     
        TimedHold   
    }

    [Header("Tilemap A - Default appear")]
    public List<Tilemap> groupA = new List<Tilemap>();

    [Header("Tilemap B - Default Hide")]
    public List<Tilemap> groupB = new List<Tilemap>();

    [Header("Settings")]
    public float fadeSpeed = 2f;
    public float holdDelay = 0.5f; 
    public bool instantSwitch = false; 

    [Header("Visual")]
    public SpriteRenderer buttonSprite;
    public Color normalColor = Color.red;
    public Color pressedColor = Color.green;
    public GameObject pressEffect;

    [Header("Audio")]
    public AudioClip pressSound;
    public AudioClip releaseSound;

    [Header("Debug")]
    public bool showDebugInfo = true;

    // Private variables
    private bool isPressed = false;
    private bool isToggled = false;
    private int playerCount = 0;
    private AudioSource audioSource;
    private List<TilemapCollider2D> collidersA = new List<TilemapCollider2D>();
    private List<TilemapCollider2D> collidersB = new List<TilemapCollider2D>();
    private Coroutine currentTransition;
    private float holdTimer = 0f;

    void Start()
    {
        // Setup
        if (!buttonSprite) buttonSprite = GetComponent<SpriteRenderer>();
        audioSource = gameObject.AddComponent<AudioSource>();

        // Cache colliders
        CacheColliders();

        // Initialize visibility
        InitializeVisibility();

        // Update visual
        UpdateButtonVisual();
    }

    void CacheColliders()
    {
        // Cache group A colliders
        foreach (var tilemap in groupA)
        {
            if (tilemap != null)
            {
                var col = tilemap.GetComponent<TilemapCollider2D>();
                collidersA.Add(col);
            }
        }

        // Cache group B colliders
        foreach (var tilemap in groupB)
        {
            if (tilemap != null)
            {
                var col = tilemap.GetComponent<TilemapCollider2D>();
                collidersB.Add(col);
            }
        }
    }

    void InitializeVisibility()
    {
        // Group A starts visible
        SetGroupVisibility(groupA, collidersA, true, true);

        // Group B starts hidden
        SetGroupVisibility(groupB, collidersB, false, true);

        if (showDebugInfo)
        {
            Debug.Log($"Initialized: Group A ({groupA.Count} tilemaps) visible, Group B ({groupB.Count} tilemaps) hidden");
        }
    }

    void Update()
    {
        // Handle timed hold
        if (mode == ButtonMode.TimedHold && isPressed)
        {
            holdTimer += Time.deltaTime;

            // Visual feedback for timer
            if (buttonSprite && holdTimer < holdDelay)
            {
                float progress = holdTimer / holdDelay;
                buttonSprite.color = Color.Lerp(normalColor, pressedColor, progress);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerCount++;
            if (playerCount == 1) // First player
            {
                OnButtonPressed();
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerCount--;
            if (playerCount == 0) // Last player left
            {
                OnButtonReleased();
            }
        }
    }

    void OnButtonPressed()
    {
        isPressed = true;
        holdTimer = 0f;

        // Sound
        if (pressSound) audioSource.PlayOneShot(pressSound);

        // Effect
        if (pressEffect)
        {
            Instantiate(pressEffect, transform.position, Quaternion.identity);
        }

        // Handle based on mode
        switch (mode)
        {
            case ButtonMode.Hold:
                ExecuteSwap(true);
                break;

            case ButtonMode.Toggle:
                isToggled = !isToggled;
                ExecuteSwap(isToggled);
                break;

            case ButtonMode.TimedHold:
                StartCoroutine(TimedHoldCoroutine());
                break;
        }

        UpdateButtonVisual();
    }

    void OnButtonReleased()
    {
        isPressed = false;
        holdTimer = 0f;

        // Sound
        if (releaseSound) audioSource.PlayOneShot(releaseSound);

        // Handle based on mode
        switch (mode)
        {
            case ButtonMode.Hold:
                ExecuteSwap(false);
                break;

            case ButtonMode.Toggle:
                // Do nothing - stays in toggled state
                break;

            case ButtonMode.TimedHold:
                StopAllCoroutines();
                ExecuteSwap(false);
                break;
        }

        UpdateButtonVisual();
    }

    IEnumerator TimedHoldCoroutine()
    {
        yield return new WaitForSeconds(holdDelay);

        if (isPressed) // Still holding
        {
            ExecuteSwap(true);
        }
    }

    void ExecuteSwap(bool swapToB)
    {
        // Cancel any ongoing transition
        if (currentTransition != null)
        {
            StopCoroutine(currentTransition);
        }

        currentTransition = StartCoroutine(SwapGroups(swapToB));
    }

    IEnumerator SwapGroups(bool showGroupB)
    {
        if (showGroupB)
        {
            // Hide Group A, Show Group B
            if (instantSwitch)
            {
                SetGroupVisibility(groupA, collidersA, false, true);
                SetGroupVisibility(groupB, collidersB, true, true);
            }
            else
            {
                // Fade out A
                yield return StartCoroutine(FadeGroup(groupA, collidersA, false));
                // Fade in B
                yield return StartCoroutine(FadeGroup(groupB, collidersB, true));
            }
        }
        else
        {
            // Show Group A, Hide Group B
            if (instantSwitch)
            {
                SetGroupVisibility(groupA, collidersA, true, true);
                SetGroupVisibility(groupB, collidersB, false, true);
            }
            else
            {
                // Fade out B
                yield return StartCoroutine(FadeGroup(groupB, collidersB, false));
                // Fade in A
                yield return StartCoroutine(FadeGroup(groupA, collidersA, true));
            }
        }

        if (showDebugInfo)
        {
            Debug.Log($"Swapped to: Group {(showGroupB ? "B" : "A")}");
        }
    }

    
    IEnumerator FadeGroup(List<Tilemap> tilemaps, List<TilemapCollider2D> colliders, bool fadeIn)
    {
        float elapsed = 0f;
        float duration = 1f / fadeSpeed;
        float startAlpha = fadeIn ? 0f : 1f;
        float endAlpha = fadeIn ? 1f : 0f;

        // Disable colliders immediately when fading out
        if (!fadeIn)
        {
            for (int i = 0; i < colliders.Count; i++)
            {
                if (colliders[i] != null)
                    colliders[i].enabled = false;
            }
        }

        // Fade all tilemaps simultaneously
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, t);

            for (int i = 0; i < tilemaps.Count; i++)
            {
                if (tilemaps[i] != null)
                {
                    tilemaps[i].color = new Color(1, 1, 1, alpha);
                }
            }

            yield return null;
        }

        // Ensure final state
        for (int i = 0; i < tilemaps.Count; i++)
        {
            if (tilemaps[i] != null)
            {
                tilemaps[i].color = new Color(1, 1, 1, endAlpha);
            }
        }

        // Enable colliders when faded in
        if (fadeIn)
        {
            for (int i = 0; i < colliders.Count; i++)
            {
                if (colliders[i] != null)
                    colliders[i].enabled = true;
            }
        }
    }

    void SetGroupVisibility(List<Tilemap> tilemaps, List<TilemapCollider2D> colliders, bool visible, bool instant)
    {
        for (int i = 0; i < tilemaps.Count; i++)
        {
            if (tilemaps[i] != null)
            {
                tilemaps[i].color = new Color(1, 1, 1, visible ? 1f : 0f);

                if (i < colliders.Count && colliders[i] != null)
                {
                    colliders[i].enabled = visible;
                }
            }
        }
    }

    void UpdateButtonVisual()
    {
        if (buttonSprite)
        {
            if (mode == ButtonMode.Toggle)
            {
                buttonSprite.color = isToggled ? pressedColor : normalColor;
            }
            else
            {
                buttonSprite.color = isPressed ? pressedColor : normalColor;
            }

            // Scale effect
            float scale = (isPressed || isToggled) ? 0.9f : 1f;
            transform.localScale = Vector3.one * scale;
        }
    }

    // Public methods for external control
    public void ForceSwap(bool toGroupB)
    {
        ExecuteSwap(toGroupB);
    }

    public void ResetToDefault()
    {
        isToggled = false;
        ExecuteSwap(false);
        UpdateButtonVisual();
    }

    // Gizmos for visualization
    void OnDrawGizmos()
    {
        if (!showDebugInfo) return;

        // Draw button
        Gizmos.color = isPressed ? pressedColor : normalColor;
        Gizmos.DrawWireCube(transform.position, Vector3.one);

        // Draw connections to Group A
        Gizmos.color = Color.green;
        foreach (var tilemap in groupA)
        {
            if (tilemap != null)
            {
                Gizmos.DrawLine(transform.position, tilemap.transform.position);
                Gizmos.DrawWireCube(tilemap.transform.position, Vector3.one * 0.5f);
            }
        }

        // Draw connections to Group B
        Gizmos.color = Color.red;
        foreach (var tilemap in groupB)
        {
            if (tilemap != null)
            {
                Gizmos.DrawLine(transform.position, tilemap.transform.position);
                Gizmos.DrawWireCube(tilemap.transform.position, Vector3.one * 0.5f);
            }
        }
    }

    // Helper to add tilemaps at runtime
    public void AddTilemapToGroupA(Tilemap tilemap)
    {
        if (!groupA.Contains(tilemap))
        {
            groupA.Add(tilemap);
            var col = tilemap.GetComponent<TilemapCollider2D>();
            collidersA.Add(col);
        }
    }

    public void AddTilemapToGroupB(Tilemap tilemap)
    {
        if (!groupB.Contains(tilemap))
        {
            groupB.Add(tilemap);
            var col = tilemap.GetComponent<TilemapCollider2D>();
            collidersB.Add(col);
        }
    }
}
