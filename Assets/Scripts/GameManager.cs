
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;


    //[Header("Timing")]
    //public float leadTime = 1.5f; // seconds tile takes to fall from spawn to hit line

    [Header("Gameplay")]
    public Transform hitLine;// y position reference where player should hit
    public float bottomLine = -6f; // y position reference where the note is missed
    public AudioSource audioSource;

    [HideInInspector]
    public float SongStartTime = 0f;
    [HideInInspector]
    public bool isTappable;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        isTappable = true;
    }

    private void Update()
    {
        //Debug.Log(GetSongTime());
    }

    public float GetSongTime()
    {
        return Time.timeSinceLevelLoad - SongStartTime;
    }

    public void StartSong(Song song)
    {
        audioSource.Play();
        SongStartTime = Time.timeSinceLevelLoad;
        
    }

    public void StopSong(Song song)
    {
        audioSource.Stop();

    }
}
