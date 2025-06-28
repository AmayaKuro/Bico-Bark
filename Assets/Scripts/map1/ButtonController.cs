using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ButtonController : MonoBehaviour
{
    [Header("Button Type")]
    public ButtonType buttonType = ButtonType.MovePlatform;
    public enum ButtonType
    {
        MovePlatform,    // Button above - move platform
        RemoveBlock      // Button below - hide block
    }

    [Header("Target Objects")]
    public GameObject targetObject; // 
    public Transform moveTarget;    // 

    [Header("Settings")]
    public bool isToggle = false;   // Toggle or Hold
    public float moveSpeed = 2f;
    public Color pressedColor = Color.green;
    public Color normalColor = Color.red;

    [Header("Visual")]
    public SpriteRenderer buttonVisual;
    public GameObject pressEffect;
    public LineRenderer connectionLine; // Line connect button to target

    [Header("Audio")]
    public AudioClip pressSound;
    public AudioClip releaseSound;

    // States
    private bool isPressed = false;
    private int playerCount = 0;
    private Vector3 originalPosition;
    private AudioSource audioSource;

    void Start()
    {
        // Setup
        if (!buttonVisual) buttonVisual = GetComponent<SpriteRenderer>();
        audioSource = gameObject.AddComponent<AudioSource>();

        
        if (targetObject && buttonType == ButtonType.MovePlatform)
        {
            originalPosition = targetObject.transform.position;
        }

        // Visual connection line
        DrawConnectionLine();
        UpdateVisual();
    }

    void DrawConnectionLine()
    {
        if (connectionLine && targetObject)
        {
            connectionLine.positionCount = 2;
            connectionLine.SetPosition(0, transform.position);
            connectionLine.SetPosition(1, targetObject.transform.position);

            // Dash line effect
            connectionLine.material = new Material(Shader.Find("Sprites/Default"));
            connectionLine.startColor = new Color(1, 1, 1, 0.3f);
            connectionLine.endColor = new Color(1, 1, 1, 0.1f);
            connectionLine.startWidth = 0.1f;
            connectionLine.endWidth = 0.05f;
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
            if (playerCount == 0 && !isToggle) // Last player left
            {
                OnButtonReleased();
            }
        }
    }

    void OnButtonPressed()
    {
        isPressed = true;
        UpdateVisual();

        // Sound
        if (pressSound) audioSource.PlayOneShot(pressSound);

        // Effect
        if (pressEffect)
        {
            Instantiate(pressEffect, transform.position, Quaternion.identity);
        }

        // Execute action
        switch (buttonType)
        {
            case ButtonType.MovePlatform:
                StartCoroutine(MovePlatform(true));
                break;
            case ButtonType.RemoveBlock:
                RemoveBlock(true);
                break;
        }
    }

    void OnButtonReleased()
    {
        isPressed = false;
        UpdateVisual();

        // Sound
        if (releaseSound) audioSource.PlayOneShot(releaseSound);

        if (!isToggle)
        {
            switch (buttonType)
            {
                case ButtonType.MovePlatform:
                    StartCoroutine(MovePlatform(false));
                    break;
                case ButtonType.RemoveBlock:
                    RemoveBlock(false);
                    break;
            }
        }
    }

    IEnumerator MovePlatform(bool moveToTarget)
    {
        if (!targetObject) yield break;

        Vector3 destination = moveToTarget ? moveTarget.position : originalPosition;

        while (Vector3.Distance(targetObject.transform.position, destination) > 0.01f)
        {
            targetObject.transform.position = Vector3.MoveTowards(
                targetObject.transform.position,
                destination,
                moveSpeed * Time.deltaTime
            );

            // Update connection line
            if (connectionLine)
            {
                connectionLine.SetPosition(1, targetObject.transform.position);
            }

            yield return null;
        }

        targetObject.transform.position = destination;
    }

    void RemoveBlock(bool remove)
    {
        if (!targetObject) return;

        if (remove)
        {
            // Fade out animation
            StartCoroutine(FadeBlock(false));
        }
        else
        {
            // Fade in animation
            StartCoroutine(FadeBlock(true));
        }
    }
    
    IEnumerator FadeBlock(bool fadeIn)
    {
        Tilemap tilemap = targetObject.GetComponent<Tilemap>();
        TilemapCollider2D tilemapCollider = targetObject.GetComponent<TilemapCollider2D>();

        SpriteRenderer blockSprite = targetObject.GetComponent<SpriteRenderer>();
        BoxCollider2D blockCollider = targetObject.GetComponent<BoxCollider2D>();

        float alpha = fadeIn ? 0f : 1f;
        float targetAlpha = fadeIn ? 1f : 0f;
        float fadeSpeed = 2f;

        // for TILEMAP
        if (tilemap != null)
        {
            // Disable/Enable tilemap collider
            if (tilemapCollider && !fadeIn)
                tilemapCollider.enabled = false;

            while (Mathf.Abs(alpha - targetAlpha) > 0.01f)
            {
                alpha = Mathf.MoveTowards(alpha, targetAlpha, fadeSpeed * Time.deltaTime);
                tilemap.color = new Color(1, 1, 1, alpha);
                yield return null;
            }

            if (tilemapCollider && fadeIn)
                tilemapCollider.enabled = true;
        }
        // for SPRITE 
        else if (blockSprite != null)
        {
            // Disable/Enable collider
            if (blockCollider && !fadeIn)
                blockCollider.enabled = false;

            while (Mathf.Abs(alpha - targetAlpha) > 0.01f)
            {
                alpha = Mathf.MoveTowards(alpha, targetAlpha, fadeSpeed * Time.deltaTime);
                Color color = blockSprite.color;
                color.a = alpha;
                blockSprite.color = color;
                yield return null;
            }

            if (blockCollider && fadeIn)
                blockCollider.enabled = true;
        }
        else
        {
            Debug.LogWarning("Target object has neither Tilemap nor SpriteRenderer!");
        }
    }

    void UpdateVisual()
    {
        if (buttonVisual)
        {
            buttonVisual.color = isPressed ? pressedColor : normalColor;

            // Scale effect
            float scale = isPressed ? 0.9f : 1f;
            buttonVisual.transform.localScale = Vector3.one * scale;
        }
    }

    void OnDrawGizmos()
    {
        if (targetObject)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, targetObject.transform.position);

            if (buttonType == ButtonType.MovePlatform && moveTarget)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(targetObject.transform.position, moveTarget.position);
                Gizmos.DrawWireCube(moveTarget.position, Vector3.one * 0.5f);
            }
        }
    }
}