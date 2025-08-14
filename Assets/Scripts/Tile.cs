using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.Networking.Types;

public class Tile : MonoBehaviour
{
    
    public SpriteRenderer mainSprite;

    [Header("Visuals")]
    public SpriteRenderer ghostSprite;
    public ParticleSystem tapParticles;

    [HideInInspector]
    public float desiredTapTime = 0f;

    protected float timer = 0f;
    protected float tileHeight = 0;
    protected float speed;
    protected Vector3 target;
    protected bool tapped = false;

    public void Init(float speed, Vector3 target)
    {
        this.speed = speed;
        this.target = target;

        tileHeight = mainSprite.size.y;
    }

    public virtual void Update()
    {
        timer += Time.deltaTime;

        if (!GameManager.Instance.isTappable) return;

        transform.position = new Vector3(
                transform.position.x,
                transform.position.y - speed * Time.deltaTime,
                transform.position.z
            );


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
            if (tile == null) tile = hit.collider.GetComponentInParent<Tile>();
            if (tile != null)
            {
                tile.OnTap();
            }
        }
    }

    public virtual void ResetData()
    {
        tapped = false;

        mainSprite.color = new Color(1f, 1f, 1f, 1f);

        var collider = GetComponent<Collider2D>();
        if (collider == null) collider = GetComponentInChildren<Collider2D>();
        collider.enabled = true;

        ghostSprite.enabled = true;
        ghostSprite.color = new Color(1, 1, 1, 0);
        ghostSprite.transform.localScale = Vector3.one;

        timer = 0;
    }

    public virtual void OnTap()
    {
        if (tapped) return;
        tapped = true;   
    }

    protected IEnumerator FlashTile(Color flashColor, float flashDuration, int flashCount)
    {
        Color originalColor = mainSprite.color;

        for (int i = 0; i < flashCount; i++)
        {
            mainSprite.color = flashColor;
            yield return new WaitForSeconds(flashDuration / (flashCount * 2));

            mainSprite.color = originalColor;
            yield return new WaitForSeconds(flashDuration / (flashCount * 2));
        }

        mainSprite.color = originalColor; // Make sure it stays normal after flashing
    }

}
