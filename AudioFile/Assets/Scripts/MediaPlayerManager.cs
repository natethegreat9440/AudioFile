using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayMode { Consecutive, RecommendedRandom, TrueRandom }

public class MediaPlayerManager : MonoBehaviour
{
    #region Variables
    [Header("Sound Components:")]
    public AudioSource audioSource;
    public MediaLibrary mediaLibrary;

    [Header("Visual Components:")]
    public int numberOfRings = 10;
    private float songDuration;
    public GameObject radialWaveformVisualizerPrefab;
    public List<RadialWaveformVisualizer> visualizers = new List<RadialWaveformVisualizer>();
    public float expansionSpeed = 1.5f; // Factor to make the rings expand faster

    [HideInInspector]
    public int currentSongIndex = 0;

    private PlayMode currentPlayMode = PlayMode.Consecutive;

    public delegate void TrackChangeHandler(string trackName);
    public event TrackChangeHandler OnTrackChanged;

    public delegate void PlayStateChangeHandler(bool isPlaying);
    public event PlayStateChangeHandler OnPlayStateChanged;
    #endregion

    #region UnityEngine
    private void Start()
    {
        if (audioSource == null || mediaLibrary == null || mediaLibrary.songs.Length == 0)
        {
            Debug.LogError("MediaPlayerManager setup error: Ensure AudioSource and MediaLibrary are set and MediaLibrary is not empty.");
            this.enabled = false;
        }
        else
        {
            PlayCurrentSong();
            CreateAndExpandVisualizers();
        }
    }

    private void Update()
    {
        CheckIfSongFinished();
    }
    #endregion

    #region TestingMethods
    public void PlayCurrentSong()
    {
        if (mediaLibrary.songs.Length > 0 && currentSongIndex < mediaLibrary.songs.Length)
        {
            audioSource.clip = mediaLibrary.songs[currentSongIndex].clip;
            songDuration = audioSource.clip.length; // Get the duration of the current song
            audioSource.Play();
            OnTrackChanged?.Invoke(mediaLibrary.songs[currentSongIndex].name); // Use Song name
            CreateAndExpandVisualizers();
        }
        else
        {
            Debug.LogError("No songs available or index out of range.");
        }
    }

    public void NextSong()
    {
        if (currentSongIndex < mediaLibrary.songs.Length - 1)
        {
            currentSongIndex++;
            PlayCurrentSong();
        }
        else
        {
            Debug.Log("Reached the end of the playlist.");
        }
    }
    #endregion

    public void PreviousSong()
    {
        if (currentSongIndex > 0)
        {
            currentSongIndex--;
            PlayCurrentSong();
        }
        else
        {
            Debug.Log("Already at the beginning of the playlist.");
        }
    }

    public void Play()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
            OnPlayStateChanged?.Invoke(true);
        }
    }

    public void Pause()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Pause();
            OnPlayStateChanged?.Invoke(false);
        }
    }

    public void Skip(bool forward)
    {
        if (forward)
        {
            NextSong();
        }
        else
        {
            PreviousSong();
        }
    }

    public void SetPlayMode(PlayMode mode)
    {
        currentPlayMode = mode;
        Debug.Log($"Play mode changed to: {mode}");
    }

    private void CheckIfSongFinished()
    {
        if (!audioSource.isPlaying && audioSource.time >= songDuration)
        {
            NextSong();
        }
    }

    public void CreateAndExpandVisualizers()
    {
        // Clear existing visualizers
        foreach (var visualizer in visualizers)
        {
            Destroy(visualizer.gameObject);
        }
        visualizers.Clear();

        Vector3 startingPosition = new Vector3(960, 540, 0);
        float timeOffsetStep = songDuration / numberOfRings;

        for (int i = 0; i < numberOfRings; i++)
        {
            GameObject newVisualizer = Instantiate(radialWaveformVisualizerPrefab);
            newVisualizer.transform.SetParent(transform);
            newVisualizer.transform.localPosition = startingPosition;

            float initialRadiusOffset = timeOffsetStep * i;

            RadialWaveformVisualizer visualizerScript = newVisualizer.GetComponent<RadialWaveformVisualizer>();
            if (visualizerScript != null)
            {
                visualizerScript.Initialize(audioSource, this, 0.1f, initialRadiusOffset);
                visualizers.Add(visualizerScript);
            }
        }

        StartCoroutine(ExpandVisualizersOneByOne());
    }

    private IEnumerator ExpandVisualizersOneByOne()
    {
        while (true)
        {
            float startTime = Time.time;

            while (Time.time - startTime < songDuration)
            {
                for (int i = 0; i < visualizers.Count; i++)
                {
                    float elapsedTime = (Time.time - startTime - (songDuration / visualizers.Count) * i * expansionSpeed) % songDuration;
                    float scale = Mathf.Lerp(0.0f, 3.0f, elapsedTime / songDuration);
                    visualizers[i].transform.localScale = new Vector3(scale, scale, scale);
                    visualizers[i].UpdateRadius(scale * 5f); // Adjust radius to match the new scale
                }

                yield return null;
            }
        }
    }
}
