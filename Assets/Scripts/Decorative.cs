using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Decorative : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private float floatSpeed;
    private float deathY = 10f;

    // New variables for smooth wave motion
    private float floatAmplitude; // How far it moves left/right
    private float floatFrequency; // How fast it oscillates
    private float startX;         // Starting X position
    private float timeOffset;     // So each object floats differently

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Random size
        float size = Random.Range(0.05f, 0.5f);
        transform.localScale = Vector3.one * size;

        // Random color
        float randomC = Random.Range(0f, 1f);
        if (randomC < 0.33f)
            spriteRenderer.color = new Color(0.83f, 0.55f, 0.92f, 0.35f);
        else if (randomC > 0.67f)
            spriteRenderer.color = new Color(0.93f, 0.6f, 0.94f, 0.35f);
        else
            spriteRenderer.color = new Color(0.94f, 0.6f, 0.8f, 0.35f);

        // Floating speed
        floatSpeed = Random.Range(0.5f, 2f);

        // Smooth horizontal motion parameters
        floatAmplitude = Random.Range(0.5f, 2f);
        floatFrequency = Random.Range(0.5f, 1.5f);
        startX = transform.position.x;
        timeOffset = Random.Range(0f, 100f); // Random phase offset
    }

    void Update()
    {
        // Smooth X oscillation using sine wave
        float x = startX + Mathf.Sin((Time.time + timeOffset) * floatFrequency) * floatAmplitude;
        float y = transform.position.y + floatSpeed * Time.deltaTime;

        transform.position = new Vector3(x, y, transform.position.z);

        if (y > deathY)
        {
            ObjectPool.Instance.ReturnToPool("decorative_circle", gameObject);
            //Destroy(gameObject);
        }
    }
}
