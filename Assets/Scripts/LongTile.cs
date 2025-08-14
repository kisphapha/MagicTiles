using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongTile : Tile
{
    [Header("Long Tile Components")]
    public Transform sliderLine;
    public Transform sliderTip;
    public float extraDuration = 1f; // How long the hold lasts (seconds)

    private bool isHolding = false;
    private float holdTime = 0f;

    public void InitLong(float speed, Vector3 target, float extraDuration)
    {
        Init(speed, target);
        this.extraDuration = extraDuration;

        // Calculate length based on speed and duration
        float lineLength = speed * extraDuration;

        // Since the sprite is sliced, we only scale the height
        var mainSR = mainSprite.GetComponent<SpriteRenderer>();
        var mainCollider = mainSprite.GetComponent<BoxCollider2D>();
        var sliderSR = sliderLine.GetComponent<SpriteRenderer>();
        var ghostSR = ghostSprite.GetComponent<SpriteRenderer>();

        // Scale vertically without touching the corners
        mainSprite.transform.localScale = new Vector3(mainSprite.transform.localScale.x, 1f, mainSprite.transform.localScale.z);
        ghostSprite.transform.localScale = new Vector3(ghostSprite.transform.localScale.x, 1f, ghostSprite.transform.localScale.z);
        mainSR.size = new Vector2(mainSR.size.x, lineLength);
        mainCollider.offset = new Vector2(0, -lineLength / 2);

        sliderLine.localScale = new Vector3(sliderLine.localScale.x, 1f, sliderLine.localScale.z);
        sliderSR.size = new Vector2(sliderSR.size.x, lineLength - 1f);

        transform.position = new Vector2(transform.position.x, transform.position.y + lineLength / 2);
        // Position slider tip at the end
        sliderTip.localPosition = new Vector3(0, -(lineLength - 1) / 2, 0);

        tileHeight = mainSprite.size.y;
    }

    public override void OnTap()
    {
        if (tapped) return;

        tapped = true;
        isHolding = true;
        holdTime = 0f;

        // Show ghost full alpha, hide tip
        ghostSprite.color = new Color(1, 1, 1, 1);
        sliderTip.gameObject.SetActive(false);

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

        // Start filling ghost
        StartCoroutine(HoldFillRoutine());
    }

    private IEnumerator HoldFillRoutine()
    {
        Vector2 originalSize = ghostSprite.size;
        Vector3 originalPos = ghostSprite.transform.localPosition;
        Vector3 particlePos = tapParticles.transform.localPosition;
        float originalHeight = originalSize.y;

        while (isHolding && holdTime < extraDuration)
        {
            holdTime += Time.deltaTime;

            float fillPercent = Mathf.Clamp01(holdTime / extraDuration);
            float currentHeight = fillPercent * tileHeight;

            ghostSprite.size = new Vector2(originalSize.x, currentHeight);

            ghostSprite.transform.localPosition = new Vector3(
                originalPos.x,
                originalPos.y - (tileHeight - currentHeight) / 2,
                originalPos.z
            );

            tapParticles.transform.localPosition = new Vector3(
                particlePos.x,
                particlePos.y - (tileHeight - currentHeight) / 2 + currentHeight/2,
                particlePos.z
            );

            if (!tapParticles.isPlaying)
                tapParticles.Play();

            yield return null;
        }

        EndHold();
    }

    private void EndHold()
    {
        isHolding = false;
        tapParticles.Stop();
        mainSprite.color = new Color(0f, 0f, 0f, 0f);
        StartCoroutine(PlayGhostEffect());
        UIManager.Instance.UpdateScore(3);
    }

    public override void Update()
    {
        base.Update();

#if UNITY_EDITOR || UNITY_STANDALONE
        if (isHolding && Input.GetMouseButtonUp(0))
        {
            EndHold();
        }
#endif
        if (Input.touchCount > 0 && isHolding)
        {
            bool released = true;
            foreach (Touch touch in Input.touches)
            {
                if (touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled)
                {
                    released = false;
                    break;
                }
            }
            if (released) EndHold();
        }

        if (transform.position.y + tileHeight/2 < GameManager.Instance.bottomLine && gameObject.activeSelf)
        {
            if (tapped)
            {
                //Destroy(gameObject);
                ObjectPool.Instance.ReturnToPool("long_tile", gameObject);
                ResetData();
            }
            else
            {
                UIManager.Instance.GameOver(this);
                StartCoroutine(FlashTile(new Color(1f, 1f, 1f, 0.2f), 4f, 8));
            }
        }
    }

    private IEnumerator PlayGhostEffect()
    {
        float duration = 0.3f;
        float time = 0f;
        Color startColor = ghostSprite.color;
        Vector3 ghostOriginalScale = ghostSprite.transform.localScale;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            // Scale up and fade out
            ghostSprite.transform.localScale = ghostOriginalScale * (1f + t * 0.5f);
            ghostSprite.color = Color.Lerp(startColor, new Color(1, 1, 1, 0), t);

            yield return null;
        }

        ghostSprite.enabled = false;
    }

    public override void ResetData()
    {
        base.ResetData();
        sliderTip.gameObject.SetActive(true);
        holdTime = 0;
    }
}
