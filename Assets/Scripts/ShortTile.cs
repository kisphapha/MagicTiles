using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.Types;

public class ShortTile : Tile
{
    public override void Update()
    {
        base.Update();

        if (transform.position.y + tileHeight / 2 < GameManager.Instance.bottomLine && gameObject.activeSelf)
        {
            if (tapped)
            {
                //Destroy(gameObject);
                ObjectPool.Instance.ReturnToPool("tile", gameObject);
                ResetData();
            }
            else
            {
                UIManager.Instance.GameOver(this);
                StartCoroutine(FlashTile(new Color(1f, 1f, 1f, 0.2f), 4f, 8));
            }
        }
    }

    public override void OnTap()
    {
        base.OnTap();
        mainSprite.color = new Color(0f, 0f, 0f, 0f);

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
}
