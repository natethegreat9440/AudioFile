using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public enum PlayMode { Consecutive, RecommendedRandom, TrueRandom }

public class MediaPlayerManager : MonoBehaviour
{
    #region Variables
    [Header("Sound Components:")]
    public AudioSource audioSource; // Unity's AudioSource component for playing music
    public MediaLibrary mediaLibrary;

    [Header("Visual Components:")]
    //TODO: Add a VisualizerManager class and have these methods refer to whatever selected Visualizer is passed
    public RadialWaveformVisualizer radialWaveformVisualizer;

    [HideInInspector]
    public int currentSongIndex = 0;
    private float songDuration;

    // Events to communicate with other components
    public delegate void TrackChangeHandler(string trackName);
    public event TrackChangeHandler OnTrackChanged;

    public delegate void PlayStateChangeHandler(bool isPlaying);
    public event PlayStateChangeHandler OnPlayStateChanged;

    private PlayMode currentPlayMode = PlayMode.Consecutive;
    #endregion

    #region UnityEngine
    private void Start()
    {
        if (audioSource == null || mediaLibrary == null || mediaLibrary.songs.Count == 0)
        {
            Debug.LogError("MediaPlayerManager setup error: Ensure AudioSource and MediaLibrary are set and MediaLibrary is not empty.");
            this.enabled = false; // Disable the script if setup is incomplete
        }
        else
        {
            PlayCurrentSong();
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
        if (mediaLibrary.songs.Count > 0 && currentSongIndex < mediaLibrary.songs.Count)
        {
            audioSource.clip = mediaLibrary.songs[currentSongIndex].clip;

            if (audioSource.clip != null)
            {
                songDuration = audioSource.clip.length; // Get the duration of the current song
                audioSource.Play();
                OnTrackChanged?.Invoke(audioSource.clip.name); // Trigger event here
                //CreateAndExpandVisualizers();
            }
            else
            {
                if (currentSongIndex == mediaLibrary.songs.Count - 1)
                {
                    Skip(false); // Skip to the previous song if the current clip is null and you are at the last song on a playlist
                }
                else
                {
                    Skip(true);  // Skip to the next song if the current clip is null and you are NOT at the last song on a playlist
                }

            }
        }
        else
        {
            Debug.LogError("No songs available or index out of range.");
        }
    }
    public void NextSong()
    {
        if (currentSongIndex < mediaLibrary.songs.Count - 1)
        {
            currentSongIndex++;
            PlayCurrentSong();
        }
        else
        {
            Debug.Log("Reached the end of the playlist.");
        }
    }
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
    private void CheckIfSongFinished()
    {
        if (!audioSource.isPlaying && audioSource.time >= songDuration)
        {
            NextSong();
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
        if (forward)
        {
            Debug.LogError($"Clip at index {currentSongIndex} is null. Skipping to the next song.");
            NextSong();
        }
        else
        {
            Debug.LogError($"Clip at index {currentSongIndex} is null. Skipping to the previous song.");
            PreviousSong();
        }
    }

    public void SetPlayMode(PlayMode mode)
    {
        currentPlayMode = mode;
        Debug.Log($"Play mode changed to: {mode}");
    }
}
#endregion