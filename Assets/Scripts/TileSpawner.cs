using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TileSpawner : MonoBehaviour
{
    public static TileSpawner Instance;

    public RectTransform[] lanes;
    public float spawnY = 6f;          
    public string chartJsonPath;
    public CanvasScaler scaler;

    private Song songData;
    private int nextIndex;
    private bool spawning = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Load chart
        songData = SongLoader.LoadFromJson(chartJsonPath);
        if (songData == null || songData.Tiles.Count == 0)
        {
            Debug.LogError("No chart data loaded.");
            return;
        }

        // Sort tiles by HitTime to ensure order
        songData.Tiles.Sort((a, b) => a.HitTime.CompareTo(b.HitTime));

        var audioClip = Resources.Load<AudioClip>(songData.AudioFilePath);
        if (audioClip != null)
        {
            GameManager.Instance.audioSource.clip = audioClip;
            Debug.Log("Found audio clip " + audioClip.name);
        }
        else
        {
            Debug.LogError("Audio not found");
        }
        
        spawning = false;
        UIManager.Instance.RunBanner(0.5f, 1.2f, songData.Name, songData.Author);
    }


    void FixedUpdate()
    {
        if (!spawning) return;

        if (nextIndex >= songData.Tiles.Count)
        {
            spawning = false;
            StartCoroutine(Ending());
            return;
        }

        float songTime = GameManager.Instance.GetSongTime();
        float distance = spawnY - GameManager.Instance.hitLine.position.y;
        float travelTime = distance / songData.DropSpeed;

        while (nextIndex < songData.Tiles.Count)
        {
            TileData nextTile = songData.Tiles[nextIndex];
            float spawnTime = nextTile.HitTime - travelTime;

            if (songTime >= spawnTime)
            {
                SpawnTile(nextTile);
                nextIndex++;
            }
            else
            {
                break;
            }
        }
    }

    public void StartSpawning()
    {
        GameManager.Instance.StartSong(songData);
        spawning = true;
    }
    public void StopSpawning()
    {
        GameManager.Instance.StopSong(songData);
        spawning = false;
    }
    private void SpawnTile(TileData data)
    {
        int lane = Mathf.Clamp((int)data.Lane, 0, lanes.Length - 1);
        Vector3 pos = new(lanes[lane].position.x, spawnY, transform.position.z);

        GameObject go = ObjectPool.Instance.SpawnFromPool(data.IsLong ? "long_tile" : "tile", pos, Quaternion.identity, transform); //Instantiate(tilePrefab, pos, Quaternion.identity, transform);
        
        //Configure tile movement
        Tile tile = go.GetComponent<Tile>();
        tile.Init(songData.DropSpeed, GameManager.Instance.hitLine.position);     
        tile.desiredTapTime =  data.HitTime - GameManager.Instance.GetSongTime();
        BoxCollider2D collider = tile.GetComponent<BoxCollider2D>();

        if (data.IsLong)
        {
            LongTile longTile = go.GetComponent<LongTile>();
            longTile.InitLong(songData.DropSpeed, GameManager.Instance.hitLine.position, data.Duration);
            collider = longTile.GetComponentInChildren<BoxCollider2D>();
        }


        // Responsive tiles
        SpriteRenderer sr = tile.mainSprite;
        SpriteRenderer ghostSr = tile.ghostSprite;

        // Step 1: Get lane width in world space
        Vector3[] corners = new Vector3[4];
        lanes[lane].GetWorldCorners(corners);
        float laneWidthWorld = Vector3.Distance(corners[0], corners[3]); // left to right

        // Step 2: Keep scale = 1 so borders stay correct
        sr.transform.localScale = new Vector3(1, sr.transform.localScale.y, sr.transform.localScale.z);
        ghostSr.transform.localScale = new Vector3(1, sr.transform.localScale.y, sr.transform.localScale.z);

        // Step 3: Set sliced sprite size to match lane width
        sr.size = new Vector2(laneWidthWorld, sr.size.y);
        collider.size = new Vector2(sr.size.x, collider.size.y);
        ghostSr.size = new Vector2(laneWidthWorld, ghostSr.size.y);
    }

    private IEnumerator Ending()
    {
        yield return new WaitForSeconds(3f);

        UIManager.Instance.FinishGame();
    }
}
