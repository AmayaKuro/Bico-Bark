using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileMapWallBlock : MonoBehaviour
{
    private Tilemap tilemap;
    private TilemapCollider2D tilemapCollider;

    void Start()
    {
        tilemap = GetComponent<Tilemap>();
        tilemapCollider = GetComponent<TilemapCollider2D>();
    }

    public void SetVisibility(bool visible, bool instant = false)
    {
        if (instant)
        {
            tilemap.color = new Color(1, 1, 1, visible ? 1 : 0);
            tilemapCollider.enabled = visible;
        }
        else
        {
            StartCoroutine(FadeTilemap(visible));
        }
    }
    
    IEnumerator FadeTilemap(bool fadeIn)
    {
        float alpha = fadeIn ? 0f : 1f;
        float targetAlpha = fadeIn ? 1f : 0f;

        if (!fadeIn) tilemapCollider.enabled = false;

        while (Mathf.Abs(alpha - targetAlpha) > 0.01f)
        {
            alpha = Mathf.MoveTowards(alpha, targetAlpha, 2f * Time.deltaTime);
            tilemap.color = new Color(1, 1, 1, alpha);
            yield return null;
        }

        if (fadeIn) tilemapCollider.enabled = true;
    }
}