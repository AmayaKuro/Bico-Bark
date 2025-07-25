using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class SimpleExitDoor : MonoBehaviour
{
    [Header("Door Settings")]
    public GameObject doorObject; 

    [Header("Effects")]
    public GameObject appearEffect;
    public AudioClip appearSound;

    [Header("Optional Animation")]
    public bool useAnimation = false;
    public float animationDuration = 1f;

    void Start()
    {
        if (doorObject)
        {
            doorObject.SetActive(false);
        }
    }

    public void OnKeyCollected()
    {
        ShowDoor();
    }

    void ShowDoor()
    {
        if (!doorObject) return;

        if (appearEffect)
        {
            Instantiate(appearEffect, doorObject.transform.position, Quaternion.identity);
        }

        if (appearSound)
        {
            AudioSource.PlayClipAtPoint(appearSound, doorObject.transform.position);
        }

        if (useAnimation)
        {
            StartCoroutine(AnimateDoorAppear());
        }
        else
        {
            doorObject.SetActive(true);
        }
    }

    IEnumerator AnimateDoorAppear()
    {
        doorObject.SetActive(true);

        Vector3 originalScale = doorObject.transform.localScale;
        doorObject.transform.localScale = Vector3.zero;

        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;

            // Easing
            t = Mathf.SmoothStep(0f, 1f, t);

            doorObject.transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, t);
            yield return null;
        }

        doorObject.transform.localScale = originalScale;
    }
}