using System.Collections;
using UnityEngine;

public class BlockWall : MonoBehaviour
{
    [Header("Block Settings")]
    public float fadeSpeed = 2f;
    public bool startVisible = true;

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private bool isVisible;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        isVisible = startVisible;

        if (!startVisible)
        {
            SetVisibility(false, true); // instant
        }
    }

    public void SetVisibility(bool visible, bool instant = false)
    {
        if (visible == isVisible) return;

        isVisible = visible;

        if (instant)
        {
            spriteRenderer.color = new Color(1, 1, 1, visible ? 1 : 0);
            boxCollider.enabled = visible;
        }
        else
        {
            StartCoroutine(FadeVisibility(visible));
        }
    }
    
    IEnumerator FadeVisibility(bool fadeIn)
    {
        float alpha = fadeIn ? 0f : 1f;
        float targetAlpha = fadeIn ? 1f : 0f;

        if (!fadeIn) boxCollider.enabled = false;

        while (Mathf.Abs(alpha - targetAlpha) > 0.01f)
        {
            alpha = Mathf.MoveTowards(alpha, targetAlpha, fadeSpeed * Time.deltaTime);
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
            yield return null;
        }

        if (fadeIn) boxCollider.enabled = true;
    }
}


