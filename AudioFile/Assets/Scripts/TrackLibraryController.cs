using System.Windows.Forms;
using AudioFile;
using AudioFile.ObserverManager;
using Unity.VisualScripting;
using System;
using UnityEngine;
using AudioFile.View;
using SFB;
using System.IO;
using UnityEngine.Networking;
using System.Collections;
using AudioFile.Model;
using System.Collections.Generic;
using TagLib;
//using TagLib.Matroska;

namespace AudioFile.Controller
{
    /// <summary>
    /// Singleton Track Library Controller in AudioFile used for controlling loading and removing tracks from the Track Library as well as Initializing the Track Library
    /// <remarks>
    /// Members: LoadTrack(), RemoveTrack(), RemoveTrackAtIndex(), OpenFileDialog(). Coroutine LoadAudioClipFromFile(). Implements Awake() and Start() from MonoBehaviour. Implements HandleRequest() from IController.
    /// This controller has no implementation for IController methods Initialize() or Dispose() (yet).
    /// </remarks>
    /// <see cref="MonoBehaviour"/>
    /// <seealso cref="IController"/>
    /// </summary>

    public class TrackLibraryController : MonoBehaviour, IController
    {
        // Lazy<T> ensures that the instance is created in a thread-safe manner
        private static readonly Lazy<TrackLibraryController> _instance = new Lazy<TrackLibraryController>(CreateSingleton);

        // Private constructor to prevent instantiation
        private TrackLibraryController() { } //Unity objects should not use constructors with parameters as Unity uses its own lifecycle methods to manage these objects

        public static TrackLibraryController Instance => _instance.Value;

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
            TrackLibrary.Instance.Initialize();
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public void HandleRequest(object request, bool isUndo)
        {
            //Add methods to log these commands with the UndoController
            string command = request.GetType().Name;

            if (isUndo == false)
            {
                switch (command)
                {
                    case "AddTrackCommand":
                        AddTrackCommand addTrackCommand = request as AddTrackCommand;
                        LoadTrack(addTrackCommand); 
                        //The completion of LoadAudioClipFromFile will log the AddTrackCommand with the CommandStackController with the newly created Track reference
                        //This way undoing the AddTrackCommand will know which track to remove from the Library
                        //This is why LoadItem() passes along the AddTrackCommand
                        break;

                    case "RemoveTrackCommand":
                        RemoveTrackCommand removeTrackCommand = request as RemoveTrackCommand;
                        string removedTrackPath = RemoveTrackAtIndex(removeTrackCommand.Index);
                        removeTrackCommand.Path = removedTrackPath;
                        //Add logic to log this command to the CommandStackController.
                        //Setting the Path property is necessary for being able to perform an undo operation on the RemoveTrackCommand later 
                        break;

                    default:
                        Debug.LogWarning($"Unhandled command: {request}");
                        break;
                }
            }
            else
            {
                switch (command)
                {
                    case "AddTrackCommand":
                        AddTrackCommand addTrackCommand = request as AddTrackCommand;
                        RemoveTrack(addTrackCommand.Track);
                        break;

                    case "RemoveTrackCommand":
                        RemoveTrackCommand removeTrackCommand = request as RemoveTrackCommand;
                        string path = removeTrackCommand.Path;
                        if (!string.IsNullOrEmpty(path) && System.IO.File.Exists(path))
                        {
                            StartCoroutine(LoadAudioClipFromFile(path, null));
                        }
                        else
                        {
                            Debug.LogError("Invalid file path or file does not exist. Action can not be undone");
                        }                        
                        break;

                    default:
                        Debug.LogWarning($"Unhandled command: {request}");
                        break;
                }
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
        public void RemoveTrack(Track track)
        {
            TrackLibrary.Instance.RemoveItem(track);
        }
        public string RemoveTrackAtIndex(int index)
        {
            Track trackToRemove = TrackLibrary.Instance.GetTrackAtIndex(index);
            string trackPath = trackToRemove.TrackProperties.GetProperty("Path");
            TrackLibrary.Instance.RemoveItemAtIndex(index);
            return trackPath; //Returns Track's path so Handle Request can log this path to the RemoveTrackCommand that initiates this method
        }

        public void LoadTrack(AddTrackCommand addTrackCommand)
        {
            string path = OpenFileDialog();
            if (!string.IsNullOrEmpty(path) && System.IO.File.Exists(path))
            {
                StartCoroutine(LoadAudioClipFromFile(path, addTrackCommand));
            }
            else
            {
                Debug.LogError("Invalid file path or file does not exist.");
            }
        }

        private List<string> ExtractFileMetadata(string filePath)
        {
            string trackTitle = "Untitled Track";
            string contributingArtists = "Unknown Artist";
            string trackAlbum = "Unknown Album";
            List<string> metadata = new List<string>() { trackTitle, contributingArtists, trackAlbum };

            try
            {
                var file = TagLib.File.Create(filePath);
                trackTitle = !string.IsNullOrEmpty(file.Tag.Title) ? file.Tag.Title : Path.GetFileName(filePath);
                trackAlbum = !string.IsNullOrEmpty(file.Tag.Album) ? file.Tag.Album : "Unknown Album";
                contributingArtists = !string.IsNullOrEmpty(string.Join(", ", file.Tag.Performers)) ? string.Join(", ", file.Tag.Performers) //Wrapping around next line since its so goddamn long
                    : !string.IsNullOrEmpty(string.Join(", ", file.Tag.AlbumArtists)) ? string.Join(", ", file.Tag.AlbumArtists) : "Unknown Artist";
                metadata[0] = trackTitle;
                metadata[1] = contributingArtists;
                metadata[2] = trackAlbum;
                return metadata;
                //Looks for Tag.Performers first (this translates to the Contributing Artists property in File Explorer), then AlbumArtists, then finally Unknown Artist if nothing found
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error reading metadata: " + e.Message);
                return metadata;
            }
        }
        // Coroutine to load the mp3 file as an AudioClip
        private IEnumerator LoadAudioClipFromFile(string filePath, AddTrackCommand addTrackCommand)
        {
            List<string> metadata = ExtractFileMetadata(filePath);

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.MPEG))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("Error loading audio file: " + www.error);
                }
                else
                {
                    AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);
                    if (audioClip != null)
                    {
                        Debug.Log("Successfully loaded audio clip!");
                        string trackTitle = metadata[0];
                        string contributingArtists = metadata[1];
                        string trackAlbum = metadata[2];
                        Track newTrack = Track.CreateTrack(audioClip, trackTitle, contributingArtists, trackAlbum, filePath);
                        TrackLibrary.Instance.AddItem(newTrack);

                        if (addTrackCommand != null)
                        {
                            addTrackCommand.Track = newTrack;
                        }
                        //TODO: Add logic to pass this AddTrackCommand reference to the CommandStackController
                    }
                }
            }
        }

        // Function to open a file dialog (example, would need a third-party library)
        private string OpenFileDialog()
        {
            //TODO: Move this method to controller class later
            //Uses the standalone file browser (SFB) library on Github and use that to open a file dialog for selecting MP3 files
            string[] paths = StandaloneFileBrowser.OpenFilePanel("Select an MP3 file", "", "mp3", false);
            if (paths.Length > 0)
            {
                string selectedFilePath = paths[0];
                Debug.Log("Selected file: " + selectedFilePath);
                // Use the selected file path in your project
                return selectedFilePath;
            }
            else
            {
                Debug.LogError("No file selected.");
                return string.Empty;
            }
        }
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