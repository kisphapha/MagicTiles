using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecorativeNote : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private float floatSpeed;
    private float deathY = 10f;

    // Smooth floating motion
    private float floatAmplitude;
    private float floatFrequency;
    private float startX;
    private float timeOffset;

    // Rotation
    private float rotationSpeed;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        var color = spriteRenderer.color;
        spriteRenderer.color = new Color(color.r, color.g, color.b,0.2f);

        // Random size
        float size = Random.Range(0.1f, 0.2f);
        transform.localScale = Vector3.one * size;

        // Floating speed
        floatSpeed = Random.Range(0.5f, 2f);

        // Smooth X sway
        floatAmplitude = Random.Range(0.5f, 2f);
        floatFrequency = Random.Range(0.5f, 1.5f);
        startX = transform.position.x;
        timeOffset = Random.Range(0f, 100f);

        // Rotation speed (random direction)
        rotationSpeed = Random.Range(-50f, 50f); // degrees per second
    }

    void Update()
    {
        // Smooth horizontal motion
        float x = startX + Mathf.Sin((Time.time + timeOffset) * floatFrequency) * floatAmplitude;
        float y = transform.position.y + floatSpeed * Time.deltaTime;

        transform.position = new Vector3(x, y, transform.position.z);

        // Rotation
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);

        // Destroy when out of bounds
        if (y > deathY)
        {
            ObjectPool.Instance.ReturnToPool("decorative_note", gameObject);
            //Destroy(gameObject);
        }
    }
}
