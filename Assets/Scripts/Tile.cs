using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.Types;

public class Tile : MonoBehaviour
{
    private float speed;
    private Vector3 target;
    private SpriteRenderer headSprite;
    private bool tapped = false;

    [Header("Visuals")]
    public SpriteRenderer ghostSprite;
    public ParticleSystem tapParticles;

    [HideInInspector]
    public float desiredTapTime = 0f;

    private float timer = 0f;
    public void Init(float speed, Vector3 target)
    {
        this.speed = speed;
        this.target = target;
    }

    private void Start()
    {
        headSprite = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (!GameManager.Instance.isTappable) return;

        transform.position = new Vector3(
                transform.position.x,
                transform.position.y - speed * Time.deltaTime,
                transform.position.z
            );

        if (transform.position.y < GameManager.Instance.bottomLine && gameObject.activeSelf)
        {
            if (tapped)
            {
                //Destroy(gameObject);
                ObjectPool.Instance.ReturnToPool("tile", gameObject);
                ResetData();
            }
            else
            {
                UIManager.Instance.GameOver(gameObject);
                StartCoroutine(FlashTile(new Color(1f, 1f, 1f, 0.2f), 4f, 8));
            }
        }


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
            Tile tile = hit.collider.GetComponent<Tile>();
            if (tile != null)
            {
                tile.OnTap();
            }
        }
    }

    public void ResetData()
    {
        tapped = false;

        headSprite.color = new Color(1f, 1f, 1f, 1f);

        GetComponent<Collider2D>().enabled = true;

        ghostSprite.enabled = true;
        ghostSprite.color = new Color(1, 1, 1, 0);
        ghostSprite.transform.localScale = Vector3.one;

        timer = 0;
    }
    public void OnTap()
    {
        if (tapped) return;
        tapped = true;
 
        headSprite.color = new Color(0f,0f,0f,0f);
        
        StartCoroutine(PlayGhostEffect());

        tapParticles.Play();

        GetComponent<Collider2D>().enabled = false;

        UIManager.Instance.FlashDecorations();

        var difference = Mathf.Abs(timer - desiredTapTime);
        var accuracy = UIManager.AccuracyText.Miss;

        if (difference < 0.1f)
        {
            accuracy = UIManager.AccuracyText.Perfect;
            UIManager.Instance.perfectCombo++;
            UIManager.Instance.UpdateScore(3 + UIManager.Instance.perfectCombo - 1);
        }
        else if (difference < 0.3f) 
        {
            accuracy = UIManager.AccuracyText.Good;
            UIManager.Instance.UpdateScore(2);
            UIManager.Instance.perfectCombo = 0;
        }
        else
        {
            UIManager.Instance.UpdateScore(1);
            UIManager.Instance.perfectCombo = 0;
        }

        UIManager.Instance.DisplayText(accuracy);
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
    }

    private IEnumerator FlashTile(Color flashColor, float flashDuration, int flashCount)
    {
        Color originalColor = headSprite.color;

        for (int i = 0; i < flashCount; i++)
        {
            headSprite.color = flashColor;
            yield return new WaitForSeconds(flashDuration / (flashCount * 2));

            headSprite.color = originalColor;
            yield return new WaitForSeconds(flashDuration / (flashCount * 2));
        }

        headSprite.color = originalColor; // Make sure it stays normal after flashing
    }

}
