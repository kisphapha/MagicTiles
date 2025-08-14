using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;


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

        GameObject go = ObjectPool.Instance.SpawnFromPool("tile", pos, Quaternion.identity, transform); //Instantiate(tilePrefab, pos, Quaternion.identity, transform);

        //Configure tile movement
        Tile tile = go.GetComponent<Tile>();
        tile.Init(songData.DropSpeed, GameManager.Instance.hitLine.position);     
        tile.desiredTapTime =  data.HitTime - GameManager.Instance.GetSongTime();


        //Responsive tiles

        SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
        // Step 1: Get lane width in world space
        Vector3[] corners = new Vector3[4];
        lanes[lane].GetWorldCorners(corners);
        float laneWidthWorld = Vector3.Distance(corners[0], corners[3]); // left to right corners

        // Step 2: Get sprite's current world width
        float spriteWorldWidth = sr.bounds.size.x;

        // Step 3: Calculate new scale factor
        float scaleFactor = laneWidthWorld / spriteWorldWidth;

        // Step 4: Apply scale uniformly (only affecting width here)
        sr.transform.localScale = sr.transform.localScale * scaleFactor;
    }

    private IEnumerator Ending()
    {
        yield return new WaitForSeconds(3f);

        UIManager.Instance.FinishGame();
    }
}
