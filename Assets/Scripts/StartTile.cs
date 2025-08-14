using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StartTile : MonoBehaviour
{
    private SpriteRenderer headSprite;
    private bool tapped = false;

    [Header("Visuals")]
    public SpriteRenderer ghostSprite;
    public ParticleSystem tapParticles;
    public TextMeshProUGUI startText;

    private void Start()
    {
        headSprite = GetComponent<SpriteRenderer>();
    }

    void Update()
    {

#if UNITY_EDITOR || UNITY_STANDALONE
        // Mouse click for testing in editor/PC build
        if (Input.GetMouseButtonDown(0))
        {
            DetectTap(Input.mousePosition);
        }
#endif

        // Mobile touch input
        if (Input.touchCount > 0)
        {
            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Began)
                {
                    DetectTap(touch.position);
                }
            }
        }
    }

    void DetectTap(Vector2 screenPos)
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        if (hit.collider != null)
        {
            StartTile tile = hit.collider.GetComponent<StartTile>();
            if (tile != null)
            {
                tile.OnTap();
            }
        }
    }

    public void OnTap()
    {
        if (tapped) return;
        tapped = true;

        headSprite.color = new Color(0f, 0f, 0f, 0f);
        startText.enabled = false;

        StartCoroutine(PlayGhostEffect());

        tapParticles.Play();

        GetComponent<Collider2D>().enabled = false;

        TileSpawner.Instance.StartSpawning();
    }

    private IEnumerator PlayGhostEffect()
    {
        ghostSprite.color = new Color(1, 1, 1, 0.5f);
        ghostSprite.transform.localScale = Vector3.one;

        float duration = 0.3f;
        float time = 0f;
        Color startColor = ghostSprite.color;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            // Scale up and fade out
            ghostSprite.transform.localScale = Vector3.one * (1f + t * 0.5f);
            ghostSprite.color = Color.Lerp(startColor, new Color(1, 1, 1, 0), t);

            yield return null;
        }

        ghostSprite.enabled = false;

        yield return new WaitForSeconds(3f);

        Destroy(gameObject);
    }
}
