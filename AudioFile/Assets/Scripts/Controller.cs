using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public enum PlayMode { Consecutive, RecommendedRandom, TrueRandom, }

public class MediaPlayerManager : MonoBehaviour
{
    #region Variables
    //TODO: See how to reimplement MediaLibrary as either/or a TrackLibrary or VisualLibrary
    //or Playlist
    [Header("Sound Components:")]
    public AudioSource audioSource; // Unity's AudioSource component for playing music
    public MediaLibrary mediaLibrary;

    [Header("Visual Components:")]
    //TODO: Add a VisualizerManager class and have these methods refer to whatever selected Visualizer is passed
    //public RadialWaveformVisualizer radialWaveformVisualizer;

    [HideInInspector]
    public int currentTrackIndex = 0;
    private float trackDuration;

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
        if (audioSource == null || mediaLibrary == null || mediaLibrary.tracks.Count == 0)
        {
            Debug.LogError("MediaPlayerManager setup error: Ensure AudioSource and MediaLibrary are set and MediaLibrary is not empty.");
            this.enabled = false; // Disable the script if setup is incomplete
        }
        else
        {
            PlayCurrentTrack();
        }
    }
    private void Update()
    {
        CheckIfTrackFinished();
    }
    #endregion

    #region TestingMethods
    //TODO: Refactor PlayCurrentTrack() to use Track instead of audiSource.clip
    public void PlayCurrentTrack()
    {
        if (mediaLibrary.tracks.Count > 0 && currentTrackIndex < mediaLibrary.tracks.Count)
        {
            AudioClip currentTrackClip = mediaLibrary.tracks[currentTrackIndex].AudioSource.clip;
            audioSource = mediaLibrary.tracks[currentTrackIndex].AudioSource;

            if (audioSource != null)
            {
                trackDuration = mediaLibrary.tracks[currentTrackIndex].Duration; // Get the duration of the current track
                audioSource.Play();
                OnTrackChanged?.Invoke(currentTrackClip.name); // Trigger event here
                //CreateAndExpandVisualizers();
            }
            else
            {
                if (currentTrackIndex == mediaLibrary.tracks.Count - 1)
                {
                    Skip(false); // Skip to the previous track if the current clip is null and you are at the last track on a playlist
                }
                else
                {
                    Skip(true);  // Skip to the next track if the current clip is null and you are NOT at the last track on a playlist
                }

            }
        }
        else
        {
            Debug.LogError("No tracks available or index out of range.");
        }
    }
    public void NextTrack()
    {
        if (currentTrackIndex < mediaLibrary.tracks.Count - 1)
        {
            currentTrackIndex++;
            PlayCurrentTrack();
        }
        else
        {
            Debug.Log("Reached the end of the playlist.");
        }
    }
    public void PreviousTrack()
    {
        if (currentTrackIndex > 0)
        {
            currentTrackIndex--;
            PlayCurrentTrack();
        }
        else
        {
            Debug.Log("Already at the beginning of the playlist.");
        }
    }
    private void CheckIfTrackFinished()
    {
        if (!audioSource.isPlaying && audioSource.time >= trackDuration)
        {
            NextTrack();
        }
    }

 /*   public void Play()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
            Debug.Log("Audio was Played");

            OnPlayStateChanged?.Invoke(true);

        }
    }
 */

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
            Debug.LogError($"Clip at index {currentTrackIndex} is null. Skipping to the next track.");
            NextTrack();
        }
        else
        {
            Debug.LogError($"Clip at index {currentTrackIndex} is null. Skipping to the previous track.");
            PreviousTrack();
        }
    }

    public void SetPlayMode(PlayMode mode)
    {
        currentPlayMode = mode;
        Debug.Log($"Play mode changed to: {mode}");
    }
}
#endregion