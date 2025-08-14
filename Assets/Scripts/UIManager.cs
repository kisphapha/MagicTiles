using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Texts")]
    public TextMeshProUGUI goodText;
    public TextMeshProUGUI missText;
    public TextMeshProUGUI perfectText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI comboText;
    public GameObject hitParticles;

    [Header("Game Ending")]
    public Transform finishPoint;
    public GameObject winBlast;
    public Button replayButton;

    [Header("Middle Section")]
    public GameObject middleSection;
    public TextMeshProUGUI middleText;
    public TextMeshProUGUI middleSubText;
    public Image middleBackground;

    private int score = 0;
    private Dictionary<TMP_Text, Coroutine> activeCoroutines = new();
    private Coroutine scoreTextCoroutine;
    private Coroutine comboTextCoroutine;
    private Coroutine runBackgroundCoroutine;
    private Vector3 scoreTextOriginalPos;
    private Vector3 originalBackgroundPos;

    [HideInInspector]
    public int perfectCombo = 0;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        goodText.text = "";
        missText.text = "";
        perfectText.text = "";
        replayButton.gameObject.SetActive(false);
        middleSection.SetActive(false);
        originalBackgroundPos = middleBackground.transform.position;
        comboText.gameObject.SetActive(false);
    }

    public void UpdateScore(int score)
    {
        this.score += score;
        scoreText.text = this.score.ToString();
        if (scoreTextCoroutine != null)
        {
            StopCoroutine(scoreTextCoroutine);
        }
        StartCoroutine(PopTextScore());
    }

    public void FlashDecorations()
    {
        //decorBackgroundFlash.PlayFlash();
        //hitLineFlash.PlayFlash();
        var allFlash = FindObjectsOfType<Flash>();
        foreach (var flash in allFlash)
        {
            flash.PlayFlash();
        }
    }

    public void DisplayText(AccuracyText accuracyText)
    {
        TMP_Text targetText = accuracyText switch
        {
            AccuracyText.Good => goodText,
            AccuracyText.Perfect => perfectText,
            AccuracyText.Miss => missText,
            _ => null
        };

        if (targetText == null) return;

        // Set text
        goodText.text = accuracyText == AccuracyText.Good ? "Good" : "";
        perfectText.text = accuracyText == AccuracyText.Perfect ? "Perfect" : "";
        missText.text = accuracyText == AccuracyText.Miss ? "Miss" : "";

        // Reset visuals
        targetText.color = new Color(targetText.color.r, targetText.color.g, targetText.color.b, 1f);
        targetText.transform.localScale = Vector3.one;

        // Play particles
        if (accuracyText != AccuracyText.Miss)
        {
            var hitBlast = Instantiate(hitParticles, targetText.transform);
            hitBlast.GetComponent<ParticleSystem>().Play();
        }
        

        // Stop old coroutine if it exists
        if (activeCoroutines.TryGetValue(targetText, out Coroutine running))
        {
            StopCoroutine(running);
        }

        // Start a new coroutine for this specific text
        activeCoroutines[targetText] = StartCoroutine(PopText(targetText));

        //if combo
        if (perfectCombo > 1)
        {
            if (comboTextCoroutine != null)
            {
                StopCoroutine (comboTextCoroutine);
            }
            comboTextCoroutine = StartCoroutine(PopComboText());
        }
    }

    private IEnumerator PopComboText()
    {
        float duration = 0.3f;
        float time = 0f;

        comboText.gameObject.SetActive(true);
        comboText.text = "x" + perfectCombo;
        comboText.transform.localScale = Vector3.one * 0.5f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            comboText.transform.localScale = Vector3.one * (0.5f + t * 0.5f);
            yield return null;
        }

        yield return new WaitForSeconds(duration);

        time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            comboText.transform.localScale = Vector3.one * (1 - t);
            yield return null;
        }

        comboText.gameObject.SetActive(false);
        comboTextCoroutine = null;
    }

    private IEnumerator PopText(TMP_Text text)
    {
        float duration = 0.3f;
        float time = 0f;
        Color originalColor = text.color;

        // Scale out
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            text.transform.localScale = Vector3.one * (1f + t * 0.5f);
            if (perfectCombo > 1) comboText.transform.localScale = Vector3.one * (0.5f + t * 0.5f);
            yield return null;
        }

        // Scale in
        time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            text.transform.localScale = Vector3.one * (1.5f - t * 0.5f);
            yield return null;
        }

        // Fade out
        Color fadedColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            text.color = Color.Lerp(originalColor, fadedColor, t);
            if (perfectCombo > 1) comboText.transform.localScale = Vector3.one * (1 - t);
            yield return null;
        }

        if (activeCoroutines.ContainsKey(text))
        {
            activeCoroutines.Remove(text);
        }

    }

    private IEnumerator PopTextScore()
    {
        float duration = 0.3f;
        float time = 0f;

        // Scale out
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            scoreText.transform.localScale = Vector3.one * (1f + t * 0.25f);
            yield return null;
        }

        // Scale in
        time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            scoreText.transform.localScale = Vector3.one * (1.25f - t * 0.25f);
            yield return null;
        }

        scoreTextCoroutine = null;
    }

    private IEnumerator ShowMiddleText(float duration, string text, bool isSubText = false)
    {
        float time = 0f;
        TMP_Text targetText = isSubText ? middleSubText : middleText;
        Color color = targetText.color;
        targetText.text = text;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            targetText.color = new Color(color.r, color.g, color.b, t);
            yield return null;
        }
    }

    private IEnumerator HideMiddleText(float duration, string text, bool isSubText = false)
    {
        float time = 0f;
        TMP_Text targetText = isSubText ? middleSubText : middleText;
        Color color = targetText.color;
        targetText.text = text;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            targetText.color = new Color(color.r, color.g, color.b, 1f - t);
            yield return null;
        }
    }

    private IEnumerator RunMiddleBackground(float runDuration, float appearDuration, string text, string subText = null)
    {
        middleSection.SetActive(true);
        middleText.color = new Color(1f, 1f, 1f, 0f);
        middleSubText.color = new Color(1f, 1f, 1f, 0f);
        float time = 0f;
        
        //Run background from the right
        float distance = middleBackground.rectTransform.rect.width * middleBackground.rectTransform.lossyScale.x;
        middleBackground.transform.position = new Vector3(
                originalBackgroundPos.x + distance,
                middleBackground.transform.position.y
            );
        float runSpeed = distance / runDuration; 
        while (time <= runDuration)
        {
            time += Time.deltaTime;
            float t = time / runDuration;
            middleBackground.transform.position = new Vector3(
                Mathf.Lerp(originalBackgroundPos.x + distance, originalBackgroundPos.x, t),
                middleBackground.transform.position.y
            );

            yield return null;
        }

        //Show texts
        if (!string.IsNullOrEmpty(subText))
        {
            StartCoroutine(ShowMiddleText(1f, subText, true));
        }
        yield return StartCoroutine(ShowMiddleText(1f,text));

        if (appearDuration <= 0f)
        {
            yield break; //Infinite showing
        }
        yield return new WaitForSeconds(appearDuration);

        //Hide texts
        if (!string.IsNullOrEmpty(subText))
        {
            StartCoroutine(HideMiddleText(1f, subText, true));
        }
        yield return StartCoroutine(HideMiddleText(1f, text));

        //Run background to the left
        time = 0f;
        distance = middleBackground.rectTransform.rect.width * middleBackground.rectTransform.lossyScale.x;
        runSpeed = distance / runDuration;
        while (time <= runDuration)
        {
            time += Time.deltaTime;
            float t = time / runDuration;
            middleBackground.transform.position = new Vector3(
                Mathf.Lerp(originalBackgroundPos.x, originalBackgroundPos.x - distance, t),
                middleBackground.transform.position.y
            );

            yield return null;
        }
        middleSection.SetActive(false);
        middleBackground.transform.position = originalBackgroundPos;
        runBackgroundCoroutine = null;
    }

    public void RunBanner(float runDuration, float appearDuration, string text, string subText = null)
    {
        if (runBackgroundCoroutine != null)
        {
            StopCoroutine(runBackgroundCoroutine);
        }
        runBackgroundCoroutine = StartCoroutine(RunMiddleBackground(runDuration, appearDuration, text, subText));
    }

    public void GameOver(Tile missedTile)
    {
        GameManager.Instance.isTappable = false;
        TileSpawner.Instance.StopSpawning();
        StartCoroutine(PlayGameOver(missedTile));
    }

    private IEnumerator PlayGameOver(Tile missedTile)
    {
        //All notes fall up
        var tileSprite = missedTile.mainSprite;
        float spriteHeight = tileSprite.size.y;
        float time = 0f;
        float duration = 1f;

        var allTiles = FindObjectsOfType<Tile>()
            .Where(t => t.gameObject != null && t.gameObject.activeInHierarchy)
            .ToList();

        var startPositions = allTiles.Select(t => t.transform.position).ToList();

        while (time <= duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);

            for (int i = 0; i < allTiles.Count; i++)
            {
                if (allTiles[i] == null) continue;

                var startPos = startPositions[i];
                allTiles[i].transform.position = new Vector3(
                    startPos.x,
                    Mathf.Lerp(startPos.y, startPos.y + spriteHeight, t),
                    startPos.z
                );
            }

            yield return null;
        }

        RunBanner(0.5f, 0f, "Game Over");
        replayButton.gameObject.SetActive(true);
    }

    public void FinishGame()
    {
        scoreTextOriginalPos = scoreText.transform.position;
        StartCoroutine(MoveTextScoreToFinishPoint());
    }

    private IEnumerator MoveTextScoreToFinishPoint()
    {
        float duration = 1f;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            var moveDistance = scoreTextOriginalPos.y - finishPoint.position.y;

            scoreText.transform.position = new Vector3(
                    transform.position.x,
                    scoreTextOriginalPos.y - t * moveDistance,
                    transform.position.z
                );

            scoreText.transform.localScale = Vector3.one * (1 + t);

            yield return null;
        }

        var winParticles = Instantiate(winBlast,finishPoint).GetComponent<ParticleSystem>();
        winParticles.Play();

        replayButton.gameObject.SetActive(true);
    }

    public void Replay()
    {
        SceneManager.LoadScene(0);
    }

    public enum AccuracyText
    {
        Perfect = 0,
        Good = 1,
        Miss = 2
    }

}
