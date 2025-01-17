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
using System.Linq;
using Unity.Loading;
using Newtonsoft.Json;

namespace AudioFile.Controller
{
    /// <summary>
    /// Singleton Track Library Controller in AudioFile used for controlling loading and removing tracks from the Track Library as well as Initializing the Track Library
    /// <remarks>
    /// Members: LoadNewTrack(), RemoveTrack(), RemoveTrackAtIndex(), OpenFileDialog(). Coroutine LoadAudioClipFromFile(). Implements Awake() and Start() from MonoBehaviour. Implements HandleRequest() from IController.
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

        public List<Track> TrackList { get => TrackLibrary.Instance.TrackList; set => TrackLibrary.Instance.TrackList = value; }

        Track ActiveTrack { get => PlaybackController.Instance.ActiveTrack; }

        private int loadingCoroutinesCount = 0;
        public void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
        }
        public void Start()
        {
            TrackLibrary.Instance.Initialize();
            LoadTracksFromJSON();  //Method defaults to Documents/AudioFileTracks as the path to look for (just using this for initial development)      
        }

        void OnApplicationQuit()
        {
            Debug.Log($"Track Library count on exit is: {TrackLibrary.Instance.TrackList.Count}");
            Debug.Log("Saving tracks on application exit");
            SaveTracksToFile();  //Method defaults to Documents/AudioFileTracks as the path to look for (just using this for initial development)      
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public void HandleRequest(object request, bool isUndo)
        {
            string command = request.GetType().Name;

            Action action = (command, isUndo) switch
            {
                ("AddTrackCommand", false) => () =>
                {
                    AddTrackCommand addTrackCommand = request as AddTrackCommand;
                    LoadNewTrack(addTrackCommand);
                },
                ("RemoveTrackCommand", false) => () =>
                {
                    RemoveTrackCommand removeTrackCommand = request as RemoveTrackCommand;

                    //Behavior for setting the selected track based on whether the active playback track is within the tracks to be removed selection or not. If there is no active track do nothing
                    //var playOrPausedTrack = TrackList.Where(track => track.IsPlaying || track.IsPaused).FirstOrDefault();

                    if (ActiveTrack != null)
                    {
                        var activeTrackID = ActiveTrack.TrackProperties.GetProperty("TrackID");

                        if (removeTrackCommand.TrackDisplayIDs.Contains(activeTrackID))
                        {
                            if (PlaybackController.Instance.ActiveTrackIndex > 0)
                            {
                                PlaybackController.Instance.ActiveTrackIndex--;
                                PlaybackController.Instance.SetActiveTrack(TrackList[PlaybackController.Instance.ActiveTrackIndex]);
                                PlaybackController.Instance.SetSelectedTrack(TrackList[PlaybackController.Instance.ActiveTrackIndex]);
                            }
                            else
                            {
                                PlaybackController.Instance.ActiveTrackIndex++;
                                PlaybackController.Instance.SetActiveTrack(TrackList[PlaybackController.Instance.ActiveTrackIndex]);
                                PlaybackController.Instance.SetSelectedTrack(TrackList[PlaybackController.Instance.ActiveTrackIndex]);
                            }
                        }
                        else
                        {
                            //PlaybackController.Instance.SetSelectedTrack(ActiveTrack);
                        }
                    }
                    //RemoveTrackCommand.TrackProperties has the ability to hold Track Properties for multiple tracks if a bulk remove was performed
                    foreach (var trackDisplayID in removeTrackCommand.TrackDisplayIDs)
                    {
                        var trackProperties = TrackList
                        .Where(track => track.TrackProperties.GetProperty("TrackID") == trackDisplayID)
                        .Select(track => new Dictionary<string, string>(track.TrackProperties.GetAllProperties()))
                        .FirstOrDefault();

                        removeTrackCommand.TrackProperties.Add(trackProperties);

                        Track trackToRemove = TrackLibrary.Instance.GetTrackAtID(trackDisplayID);
                        string removedTrackPath = trackToRemove.TrackProperties.GetProperty("Path");
                        removeTrackCommand.Paths.Add(removedTrackPath);

                        RemoveTrack(trackDisplayID);
                    }
                },
                //Add more switch arms here as needed

                //Start of Undo actions
                ("AddTrackCommand", true) => () =>
                {
                    AddTrackCommand addTrackCommand = request as AddTrackCommand;
                    foreach (Track track in addTrackCommand.Tracks)
                    {
                        RemoveTrack(track.TrackProperties.GetProperty("TrackID"));
                    }
                },
                ("RemoveTrackCommand", true) => () =>
                {
                    RemoveTrackCommand removeTrackCommand = request as RemoveTrackCommand;


                    //RemoveTrackCommand.TrackProperties has the ability to hold Track Properties for multiple tracks if a bulk remove was performed
                    foreach (var trackDisplayID in removeTrackCommand.TrackDisplayIDs)
                    {
                        var path = removeTrackCommand.TrackProperties
                            .Where(properties => properties.ContainsValue(trackDisplayID))
                            .Select(properties => properties.ContainsKey("Path") ? properties["Path"] : null)
                            .FirstOrDefault();

                        var trackProperties = removeTrackCommand.TrackProperties
                            .Where (properties => properties.ContainsValue(trackDisplayID))
                            .Select(properties => properties)
                            .FirstOrDefault();

                        if (!string.IsNullOrEmpty(path) && System.IO.File.Exists(path))
                        {
                            StartCoroutine(LoadAudioClipFromFile(path, trackDisplayID, trackProperties));
                        }
                        else
                        {
                            Debug.LogError("Invalid file path or file does not exist. Action can not be undone");
                        }
                    }
                },
                //Add more switch arms here as needed
                _ => () => Debug.LogWarning($"Unhandled command: {request}")
            };

            action();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
        public void RemoveTrack(string trackDisplayID)
        {
            TrackLibrary.Instance.RemoveItem(trackDisplayID);
        }
        public string RemoveTrackAtIndex(int index)
        {
            Track trackToRemove = TrackLibrary.Instance.GetTrackAtIndex(index);
            string trackPath = trackToRemove.TrackProperties.GetProperty("Path");
            TrackLibrary.Instance.RemoveItemAtIndex(index);
            return trackPath; //Returns Track's path so Handle Request can log this path to the RemoveTrackCommand that initiates this method
        }

        public void LoadNewTrack(AddTrackCommand addTrackCommand)
        {
            string[] paths = OpenFileDialog();

            for (int i = 0; i < paths.Length; i++)
            {
                if (!string.IsNullOrEmpty(paths[i]))
                {
                    if (System.IO.File.Exists(paths[i]))
                    {
                        StartCoroutine(LoadAudioClipFromFile(paths[i], null, null, newTrack =>
                        {
                            //This lambda expression is passed as a callback function to the LoadAudioClipFromFile coroutine
                            if (addTrackCommand != null)
                            {
                                Debug.Log($"trackToAdd for addTrackCommand:{newTrack}");
                                addTrackCommand.Tracks.Add(newTrack);
                                Debug.Log($"addTrackCommand.Tracks: {addTrackCommand.Tracks.Count}");
                            }
                            //TODO: Add logic to pass this AddTrackCommand reference to the CommandStackController
                        }));
                    }
                    else
                    {
                        Debug.LogError("Invalid file path or file does not exist.");
                    }
                }
                else
                {
                    Debug.LogError("No file selected.");
                }
            }
        }

        // Coroutine to load the mp3 file as an AudioClip
        private IEnumerator LoadAudioClipFromFile(string filePath, string trackID = null, Dictionary<string, string> otherProperties = null, Action<Track> onTrackLoaded = null)
        {
            List<string> metadata = ExtractFileMetadata(filePath); //Metadata is always extracted even when loading clips that have already been added to the Library before.
                                                                   //This is so if the user updates/fixes any mistakes in the local file themselves these will be automatically propagated on load

            // Escape any '#' characters in the path for UnityWebRequest
            string escapedPath = filePath.Replace("#", "%23"); 

            //Debug.Log("Escaped path: " + escapedPath);

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + escapedPath, AudioType.MPEG))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("Error loading audio file: " + www.error);
                    ObserverManager.ObserverManager.Instance.NotifyObservers("AudioFileError", "Error loading audio file at path (check file path): " + "file://" + escapedPath);
                }
                else
                {
                    AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);
                    if (audioClip != null && audioClip.samples > 0)
                    {
                        Debug.Log("Successfully loaded audio clip!");

                        string trackTitle = metadata[0];
                        string contributingArtists = metadata[1];
                        string trackAlbum = metadata[2];
                        string albumTrackNumber = metadata[3];

                        Track trackToAdd = Track.CreateTrack(audioClip, trackTitle, contributingArtists, trackAlbum, filePath, trackID, albumTrackNumber);

                        bool isTrackNew = true;
                        // Set the TrackProperties from the properties dictionary. Only happens when loading deserialized tracks
                        if (otherProperties != null)
                        {
                            isTrackNew = false;

                            foreach (var property in otherProperties)
                            {
                                //Skips setting any properties that are not already required/set by the CreateTrack method so only "other properties" get set
                                //if (property.Key != "Title" || property.Key != "Artist" || property.Key != "Album" || property.Key != "Duration" || property.Key != "Path" || property.Key != "TrackID" || property.Key != "AlbumTrackNumber")
                                    if (!new[] { "Title", "Artist", "Album", "Duration", "Path", "TrackID", "AlbumTrackNumber" }.Contains(property.Key))
                                    {
                                        trackToAdd.TrackProperties.SetProperty(property.Key, property.Value);
                                    }
                                    //trackToAdd.TrackProperties.SetProperty(property.Key, property.Value);
                            }
                        }

                        TrackLibrary.Instance.AddItem(trackToAdd, isTrackNew);

                        // Invoke the callback with the new track.
                        // If an existing track is being loaded then trackToAdd will not be used in the callback function OnAllTracksDeserialized
                        //However trackToAdd will be used from within LoadNewTrack to set the Tracks associated with the AddTrackCommand (for undo operations later)
                        onTrackLoaded?.Invoke(trackToAdd);
                    }
                    else
                    {
                        Debug.Log($"Unknown error: Audio clip is either null or has no samples. audioClip: {audioClip} samples: {audioClip.samples}");
                        ObserverManager.ObserverManager.Instance.NotifyObservers("AudioFileError", $"Unknown error loading/creating audio file: {metadata[0]} - {metadata[1]}");
                    }
                }
            }
        }

        private List<string> ExtractFileMetadata(string filePath)
        {
            string trackTitle = "Untitled Track";
            string contributingArtists = "Unknown Artist";
            string trackAlbum = "Unknown Album";
            var albumTrackNumber = 0;
            List<string> metadata = new List<string>() { trackTitle, contributingArtists, trackAlbum, albumTrackNumber.ToString() };

            try
            {
                var file = TagLib.File.Create(filePath);
                trackTitle = !string.IsNullOrEmpty(file.Tag.Title) ? file.Tag.Title : Path.GetFileName(filePath);
                trackAlbum = !string.IsNullOrEmpty(file.Tag.Album) ? file.Tag.Album : "Unknown Album";
                contributingArtists = !string.IsNullOrEmpty(string.Join(", ", file.Tag.Performers)) ? string.Join(", ", file.Tag.Performers) //Wrapping around next line since its so goddamn long
                    : !string.IsNullOrEmpty(string.Join(", ", file.Tag.AlbumArtists)) ? string.Join(", ", file.Tag.AlbumArtists) : "Unknown Artist";
                albumTrackNumber = file.Tag.Track != null ? (int)file.Tag.Track : 0;

                metadata[0] = trackTitle;
                metadata[1] = contributingArtists;
                metadata[2] = trackAlbum;
                metadata[3] = albumTrackNumber.ToString();

                return metadata;
                //Looks for Tag.Performers first (this translates to the Contributing Artists property in File Explorer), then AlbumArtists, then finally Unknown Artist if nothing found
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error reading metadata: " + e.Message);
                return metadata;
            }
        }

        // Function to open a file dialog (example, would need a third-party library)
        private string[] OpenFileDialog()
        {
            //TODO: Move this method to controller class later
            //Uses the standalone file browser (SFB) library on Github and use that to open a file dialog for selecting MP3 files
            string[] paths = StandaloneFileBrowser.OpenFilePanel("Select an MP3 file", "", "mp3", true);
            for (int i = 0; i < paths.Length; i++)
            {
                if (paths.Length > 0)
                {
                    string selectedFilePath = paths[i];
                    Debug.Log("Selected file: " + selectedFilePath);
                }
                else
                {
                    Debug.LogError("No file selected.");
                    return Array.Empty<string>();
                }
            }
            return paths;
        }

        private void SaveTracksToFile(string filePath = null)
        {
            try
            {
                //For initial testing/programming purposes the filePath essentially defaults to Documents/AudioFileTracks/tracks.json
                if (string.IsNullOrEmpty(filePath))
                {
                    string defaultDirectory = SetTracksDirectory();
                    string tracksDirectory = Path.Combine(defaultDirectory, "AudioFileTracks");
                    if (!Directory.Exists(tracksDirectory))
                    {
                        Debug.Log($"Directory does not exist: {tracksDirectory}. Creating Directory.");
                        Directory.CreateDirectory(tracksDirectory);
                        Debug.Log(tracksDirectory + " created");
                        // Ensure the directory exists
                        // Handle the case where the directory does not exist,
                        // which would occur whenever the program is loaded and the user has not attempted to load any files into the program
                    }
                    filePath = Path.Combine(tracksDirectory, "tracks.json");
                }

                var trackData = TrackList.Select(track => new TrackData
                {
                    TrackProperties = track.TrackProperties.GetAllProperties()
                }).ToList();

                var trackLibraryData = new TrackLibraryData { Tracks = trackData };

                var json = JsonConvert.SerializeObject(trackLibraryData, Formatting.Indented);
                System.IO.File.WriteAllText(filePath, json);
                Debug.Log($"Tracks saved to {filePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error saving tracks: {ex.Message}");
            }
        }

        private void LoadTracksFromJSON(string filePath = null)
        {
            try
            {
                //For initial testing/programming purposes the filePath essentially defaults to Documents/AudioFileTracks/tracks.json
                if (string.IsNullOrEmpty(filePath))
                {
                    string defaultDirectory = SetTracksDirectory();
                    string tracksDirectory = Path.Combine(defaultDirectory, "AudioFileTracks");
                    if (!Directory.Exists(tracksDirectory))
                    {
                        Debug.LogWarning($"Directory does not exist: {tracksDirectory}");
                        return;
                        // Handle the case where the directory does not exist,
                        // which would occur whenever the program is loaded and the user has not attempted to load any files into the program
                    }
                    filePath = Path.Combine(tracksDirectory, "tracks.json");
                    Debug.Log("Loading tracks from " + filePath);
                }

                if (!System.IO.File.Exists(filePath))
                {
                    Debug.LogWarning($"File not found: {filePath}");
                    return;
                }

                var json = System.IO.File.ReadAllText(filePath);

                var trackLibraryData = JsonConvert.DeserializeObject<TrackLibraryData>(json);

                loadingCoroutinesCount = trackLibraryData.Tracks.Count;

                Debug.Log("Calling PopulateOnStart in TrackLibraryController");
                ObserverManager.ObserverManager.Instance.NotifyObservers("TrackDisplayPopulateStart");

                //This is the spot where things start to hang up on load.
                //Used to think it was in TrackListDisplayManager.PopulateOnStart due to Unity's way of Instatiating game objects (i.e., the TrackDisplays)
                //But it must be in the loading process of the TrackLibraryController from JSON or metadata extraction or new Track creation

                foreach (var data in trackLibraryData.Tracks)
                {
                    StartCoroutine(LoadAudioClipFromFile(data.TrackProperties["Path"], data.TrackProperties["TrackID"], data.TrackProperties, OnTracksDeserialized)); //OnTracksDeserialized is passed as a callback function
                    //Debug.Log($"Track {data.TrackProperties["Title"]}  loaded successfully.");
                }

            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading tracks: {ex.Message}");
            }
        }
        private void OnTracksDeserialized(Track unusedTrackParameter)
        {
            //This function gets called whenever LoadAudioClipFromFile coroutine gets called from LoadTracksFromJSON (each time).
            //Doesn't do anything but decrement loadingCoroutinesCount until all loading coroutines have completed
            //Doesn't need to do anything with the unusedTrackParameter parameter, but it is required in the callback function so thats why it is there

            loadingCoroutinesCount--;

            if (loadingCoroutinesCount == 0)
            {
                // All coroutines have completed
                SortController.Instance.RestoreDefaultOrder(UITrackListDisplayManager.Instance.TracksDisplayed);
                ObserverManager.ObserverManager.Instance.NotifyObservers("TracksDeserialized", TrackList);
            }
        }

        private static string SetTracksDirectory()
        {
            try
            {
                Debug.Log($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}");
                #if UNITY_EDITOR //This if statement only occurs while using the Unity Editor which defaults to the One Drive/MyDocuments folder for testing. Unfortunately Environment.SpecialFolder.MyDocuments defaults to the OneDrive/MyDocuments instead of the actual Documents folder on my PC. Maybe I'll fix that later        
                    return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                #else //This else statement only occurs when the program is run in the standalone build and sets appDirectory to whichever directory the program was installed in by the installer
                    //return AppDomain.CurrentDomain.BaseDirectory; //Saves to same location as the executable, but this could result in permissions issues depending on if the default install location is selected on a non-admin user profile (ProgramFiles typically requires admin privileges when writing files)
                    return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData); //Saves to User/AppData/Roaming. This directory does not usually cause permissions issues when it comes to writing files.
                #endif
            }
            catch (UnauthorizedAccessException)
            {
                Debug.LogError($"Error setting tracks directory. Write access denied.");
                ObserverManager.ObserverManager.Instance.NotifyObservers("AudioFileError", "Error writing tracks to memory. Write access denied. Please check with your system administrator.");
                return string.Empty;
            }
        }

        [Serializable]
        private class TrackLibraryData //Helper class for LoadTracksFromJSON
        {
            [SerializeField]
            public List<TrackData> Tracks = new List<TrackData>();
        }

        [Serializable]
        private class TrackData //Helper class for LoadTracksFromJSON
        {
            [SerializeField]
            public Dictionary<string, string> TrackProperties = new Dictionary<string, string>
            {
                { "Title", string.Empty },
                { "Artist", string.Empty },
                { "Album", string.Empty },
                { "Duration", string.Empty },
                { "BPM", string.Empty },
                { "Path", string.Empty },
                { "TrackID", string.Empty },
                { "AlbumTrackNumber", string.Empty },
            };
        }
    }
}


/*public enum PlayMode { Consecutive, RecommendedRandom, TrueRandom, }

    private PlayMode currentPlayMode = PlayMode.Consecutive;

    public void PlayCurrentTrack()
    {
        if (mediaLibrary.tracks.Count > 0 && CurrentTrackIndex < mediaLibrary.tracks.Count)
        {
            AudioClip currentTrackClip = mediaLibrary.tracks[CurrentTrackIndex].AudioSource.clip;
            audioSource = mediaLibrary.tracks[CurrentTrackIndex].AudioSource;

            if (audioSource != null)
            {
                trackDuration = mediaLibrary.tracks[CurrentTrackIndex].Duration; // Get the duration of the current track
                audioSource.Play();
                OnTrackChanged?.Invoke(currentTrackClip.name); // Trigger event here
                //CreateAndExpandVisualizers();
            }
            else
            {
                if (CurrentTrackIndex == mediaLibrary.tracks.Count - 1)
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


    public void SetPlayMode(PlayMode mode)
    {
        currentPlayMode = mode;
        Debug.Log($"Play mode changed to: {mode}");
    }
}
#endregion*/