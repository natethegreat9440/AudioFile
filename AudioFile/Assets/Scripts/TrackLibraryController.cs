using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AudioFile;
using AudioFile.Model;
using AudioFile.View;
using AudioFile.ObserverManager;
using System;
using System.Windows.Forms;

namespace AudioFile.Controller
{

    //Responsible for loading and removing tracks and (for now) instantiating the TrackLibrary
    public class TrackLibraryController : MonoBehaviour, IController
    {
        // Lazy<T> ensures that the instance is created in a thread-safe manner
        private static readonly Lazy<TrackLibraryController> _instance = new Lazy<TrackLibraryController>(CreateSingleton);

        // Private constructor to prevent instantiation
        private TrackLibraryController() { } //Unity objects should not use constructors with parameters as Unity uses its own lifecycle methods to manage these objects

        public static TrackLibraryController Instance => _instance.Value;

        private TrackLibrary trackLibrary;

        private static TrackLibraryController CreateSingleton()
        {
            // Create a new GameObject to hold the singleton instance if it doesn't already exist
            GameObject singletonObject = new GameObject(typeof(TrackLibraryController).Name);

            // Add the TrackListController component to the GameObject
            return singletonObject.AddComponent<TrackLibraryController>();
        }

        public void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
        }
        public void Start()
        {
            trackLibrary = gameObject.AddComponent<TrackLibrary>();
            trackLibrary.Initialize();
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public void HandleRequest(object request, bool isUndo)
        {
            if (isUndo == false)
            {
                LoadTrack();
                //Pass command to the Command Controller which manages a stack of commands
            }
            else
            {
                //Will need to remove the item that was added (once item is added) to library it will need to update the command controller 
                //which item was removed for that way it can pass to this method the correct track to remove
                //RemoveItem(MediaLibraryComponent mediaLibraryComponent);
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void LoadTrack()
        {
            trackLibrary.LoadItem();
        }

        public void RemoveTrack(Track track)
        {
            trackLibrary.RemoveItem(track);
        }

        //TODO: Move this method to a Playlist controller which will add the track to a playlist
        /*public void AddItem(Track track)
        {
            throw new System.NotImplementedException();
        }*/
    }
}


/*public enum PlayMode { Consecutive, RecommendedRandom, TrueRandom, }

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
#endregion*/