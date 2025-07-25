using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimplePauseZone : MonoBehaviour
{
    [Header("Timer")]
    public float pauseDuration = 10f;
    public int requiredPlayers = 2;

    [Header("Visual")]
    public Text timerText; // Optional UI
    public SpriteRenderer zoneVisual;
    public Color inactiveColor = Color.gray;
    public Color activeColor = Color.green;

    private List<GameObject> playersInZone = new List<GameObject>();
    private bool isActive = false;
    private float remainingTime;

    void Start()
    {
        if (zoneVisual)
            zoneVisual.color = inactiveColor;

        // Hide timer initially
        if (timerText)
            timerText.gameObject.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !playersInZone.Contains(other.gameObject))
        {
            playersInZone.Add(other.gameObject);
            CheckActivation();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playersInZone.Remove(other.gameObject);
        }
    }

    void CheckActivation()
    {
        if (!isActive && playersInZone.Count >= requiredPlayers)
        {
            ActivateZone();
        }
    }

    void ActivateZone()
    {
        isActive = true;
        remainingTime = pauseDuration;

        // Pause all disappearing
        DisappearingManager.PauseAll();

        // Visual
        if (zoneVisual) zoneVisual.color = activeColor;
        if (timerText) timerText.gameObject.SetActive(true);

        // Start timer
        StartCoroutine(TimerCoroutine());
    }

    IEnumerator TimerCoroutine()
    {
        while (remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;

            // Update UI
            if (timerText)
            {
                int seconds = Mathf.CeilToInt(remainingTime);
                timerText.text = seconds.ToString();

                // Flash when low time
                if (remainingTime < 3)
                {
                    timerText.color = Color.Lerp(Color.white, Color.red, Mathf.PingPong(Time.time * 4, 1));
                }
            }

            yield return null;
        }

        // Time's up!
        DeactivateZone();
    }

    void DeactivateZone()
    {
        isActive = false;

        // Resume disappearing
        DisappearingManager.ResumeAll();

        // Visual
        if (zoneVisual) zoneVisual.color = inactiveColor;
        if (timerText) timerText.gameObject.SetActive(false);

        // Disable this zone
        GetComponent<Collider2D>().enabled = false;

        // Fade out
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        float alpha = 1f;
        while (alpha > 0)
        {
            alpha -= Time.deltaTime;
            if (zoneVisual)
            {
                Color c = zoneVisual.color;
                c.a = alpha;
                zoneVisual.color = c;
            }
            yield return null;
        }
    }
}