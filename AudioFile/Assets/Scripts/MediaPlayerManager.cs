using System.Collections.Generic;
using UnityEngine;

public enum PlayMode { Consecutive, RecommendedRandom, TrueRandom }

public class MediaPlayerManager : MonoBehaviour
{
    #region Variables
    public AudioSource audioSource; // Unity's AudioSource component for playing music
    public MediaLibrary mediaLibrary;
    public GameObject radialWaveformVisualizerPrefab;
    private int currentSongIndex = 0;

    // Events to communicate with other components
    public delegate void TrackChangeHandler(string trackName);
    public event TrackChangeHandler OnTrackChanged;

    public delegate void PlayStateChangeHandler(bool isPlaying);
    public event PlayStateChangeHandler OnPlayStateChanged;

    private PlayMode currentPlayMode = PlayMode.Consecutive;

    // List to track instances of RadialWaveformVisualizer
    public List<RadialWaveformVisualizer> visualizers = new List<RadialWaveformVisualizer>();
    public float smallestRingProportion = 0.1f;
    public int numberOfRings = 10;
    public float expansionDuration = 10f; // Duration over which the visualizers expand
    #endregion

    #region UnityEngine
    private void Start()
    {
        if (audioSource == null || mediaLibrary == null || mediaLibrary.songs.Length == 0)
        {
            Debug.LogError("MediaPlayerManager setup error: Ensure AudioSource and MediaLibrary are set and MediaLibrary is not empty.");
            this.enabled = false; // Disable the script if setup is incomplete
        }
        else
        {
            PlayCurrentSong();
        }

        // Call DuplicateVisualizer in the Start method
        DuplicateVisualizerInward();
        DuplicateVisualizerOutward();
    }
    #endregion

    #region TestingMethods
    public void PlayCurrentSong()
    {
        if (mediaLibrary.songs.Length > 0 && currentSongIndex < mediaLibrary.songs.Length)
        {
            audioSource.clip = mediaLibrary.songs[currentSongIndex].clip;
            audioSource.Play();
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
            OnTrackChanged?.Invoke(audioSource.clip.name);

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
            OnTrackChanged?.Invoke(audioSource.clip.name);
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
            Debug.Log("Audio was Played");
            OnPlayStateChanged?.Invoke(true);
        }
    }

    public void Pause()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Pause();
            Debug.Log("Audio was Paused");
            OnPlayStateChanged?.Invoke(false);
        }
    }

    public void Skip(bool forward)
    {
        // This is a placeholder for how you might handle track skipping (i.e., if tracks are missing or can't be loaded)
        if (forward)
        {
            // Move to the next track
            Debug.Log("Skipping to next track");
        }
        else
        {
            // Move to the previous track
            Debug.Log("Skipping to previous track");
        }

        // Trigger the track change event
        OnTrackChanged?.Invoke(audioSource.clip.name);
        Play(); // Play the new track
    }

    public void SetPlayMode(PlayMode mode)
    {
        currentPlayMode = mode;
        Debug.Log($"Play mode changed to: {mode}");
    }

    public void DuplicateVisualizerInward()
    {
        Vector3 startingPosition = new Vector3(960, 540, 0);
        float step = (1.0f - smallestRingProportion) / (numberOfRings - 1);

        for (int i = 0; i < numberOfRings; i++)
        {
            float scale = 1.0f - (step * i);
            GameObject newVisualizer = Instantiate(radialWaveformVisualizerPrefab);
            newVisualizer.transform.SetParent(transform);
            newVisualizer.transform.localPosition = startingPosition;
            newVisualizer.transform.localScale = new Vector3(scale, scale, scale);

            RadialWaveformVisualizer visualizerScript = newVisualizer.GetComponent<RadialWaveformVisualizer>();
            if (visualizerScript != null)
            {
                visualizerScript.Initialize(audioSource, this, scale);
                visualizers.Add(visualizerScript);
            }
        }
    }

    public void DuplicateVisualizerOutward()
    {
        Vector3 startingPosition = new Vector3(960, 540, 0);
        float step = (3.0f - 1.0f) / (numberOfRings - 1);

        for (int i = 0; i < numberOfRings; i++)
        {
            float scale = 1.0f + (step * i);
            GameObject newVisualizer = Instantiate(radialWaveformVisualizerPrefab);
            newVisualizer.transform.SetParent(transform);
            newVisualizer.transform.localPosition = startingPosition;
            newVisualizer.transform.localScale = new Vector3(scale, scale, scale);

            RadialWaveformVisualizer visualizerScript = newVisualizer.GetComponent<RadialWaveformVisualizer>();
            if (visualizerScript != null)
            {
                visualizerScript.Initialize(audioSource, this, scale);
                visualizers.Add(visualizerScript);
            }
        }
    }
}
