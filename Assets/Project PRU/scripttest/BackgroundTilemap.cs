using UnityEngine;
using UnityEngine.Tilemaps;

public class BackgroundTilemap : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TilemapRenderer renderer = GetComponent<TilemapRenderer>();
        renderer.sortingLayerName = "Background";
        renderer.sortingOrder = -10; 
    }
}
