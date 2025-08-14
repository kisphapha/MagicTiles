using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flash : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Coroutine flashCoroutine;
    private Color originalColor;

    public float strength = 1f;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    public void PlayFlash()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor; // reset before starting
            flashCoroutine = StartCoroutine(PlayFlashEffect());
        }
    }

    private IEnumerator PlayFlashEffect()
    {
        float duration = 0.3f;
        float time = 0f;
        Color startColor = spriteRenderer.color;
        Color fadedColor = new Color(originalColor.r, originalColor.g, originalColor.b, strength);

        // Fade out
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            spriteRenderer.color = Color.Lerp(startColor, fadedColor, t);
            yield return null;
        }

        // Fade in
        time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            spriteRenderer.color = Color.Lerp(fadedColor, startColor, t);
            yield return null;
        }

        flashCoroutine = null; // mark as finished
    }
}
