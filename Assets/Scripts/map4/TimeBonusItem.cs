using UnityEngine;

public class TimeBonusItem : MonoBehaviour
{
    [Header("Bonus Settings")]
    public float bonusTime = 30f;
    public bool destroyOnCollect = true;

    [Header("Visual")]
    public float rotateSpeed = 90f;
    public float bobSpeed = 2f;
    public float bobHeight = 0.3f;

    [Header("Effects")]
    public GameObject collectEffect;
    public AudioClip collectSound;

    private Vector3 startPos;
    private SwitchMazeManager mazeManager;

    [System.Obsolete]
    void Start()
    {
        startPos = transform.position;
        mazeManager = FindObjectOfType<SwitchMazeManager>();
    }

    void Update()
    {
        // Rotate
        transform.Rotate(Vector3.forward * rotateSpeed * Time.deltaTime);

        // Bob up and down
        float newY = startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CollectItem();
        }
    }

    void CollectItem()
    {
        // Add time
        if (mazeManager != null && mazeManager.useTimer)
        {
            mazeManager.AddTime(bonusTime);
            Debug.Log($"Added {bonusTime} seconds!");
        }

        // Effects
        if (collectEffect)
            Instantiate(collectEffect, transform.position, Quaternion.identity);

        if (collectSound)
            AudioSource.PlayClipAtPoint(collectSound, transform.position);

        // Destroy
        if (destroyOnCollect)
            Destroy(gameObject);
    }
}
