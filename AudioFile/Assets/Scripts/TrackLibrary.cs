using AudioFile.Model;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using AudioFile.ObserverManager;
using System.IO;
using UnityEngine.Networking;
using SFB;
using TagLib;
using AudioFile.Controller;


namespace AudioFile.Model
{
    public class TrackLibrary : MediaLibraryComponent, IAudioFileObserver
    {
        protected List<Track> trackList;
        private int currentTrackIndex;

        private TrackLibraryController _trackLibraryController;

        public TrackLibrary(TrackLibraryController trackLibraryController, string name = "Track Library") : base(name)
        {
            _trackLibraryController = trackLibraryController;
        }

        public void Start()
        {
            AudioFile.ObserverManager.ObserverManager.Instance.RegisterObserver("OnCurrentTrackIsDone", this);
        }

        public void Initialize()
        {
            trackList = new List<Track>();
            currentTrackIndex = 0;
        }

        public override string ToString()
        {
            return $"{_name}";
        }

        #region Playback method implementations

        public override void Skip()
        {
            try
            {
                trackList[currentTrackIndex].Play();
            }
            catch (Exception)
            {
                NextItem();
            }
        }

        public override void NextItem()
        {
            if (currentTrackIndex < trackList.Count - 1)
            {
                currentTrackIndex++;
                trackList[currentTrackIndex].Play();
            }
        }

        public override void PreviousItem()
        {
            if (currentTrackIndex > 0)
            {
                currentTrackIndex--;
                trackList[currentTrackIndex].Play();
            }
        }

        public void AudioFileUpdate(string observationType, object data)
        {
            if (observationType == "OnCurrentTrackIsDone")
            {
                NextItem();
            }
        }
        #endregion
        #region Model control methods

        public override void LoadItem()
        {
            string path = OpenFileDialog(); 
            if (!string.IsNullOrEmpty(path) && System.IO.File.Exists(path))
            {
                StartCoroutine(LoadAudioClipFromFile(path));
            }
            else
            {
                Debug.LogError("Invalid file path or file does not exist.");
            }
        }

        // Coroutine to load the mp3 file as an AudioClip
        private IEnumerator LoadAudioClipFromFile(string filePath)
        {
            //TODO: Move metadata extraction to a separate method that can be delegated by this method or called seperately with
            //different arguments for whether we want full extraction or just basic extraction
            string trackTitle = "Untitled Track";
            string trackAlbum = "Unknown Album";
            string contributingArtists = "Unknown Artist";

            try
            {
                var file = TagLib.File.Create(filePath);
                trackTitle = !string.IsNullOrEmpty(file.Tag.Title) ? file.Tag.Title : Path.GetFileName(filePath);
                trackAlbum = !string.IsNullOrEmpty(file.Tag.Album) ? file.Tag.Album : "Unknown Album";
                contributingArtists = !string.IsNullOrEmpty(string.Join(", ", file.Tag.Performers)) ? string.Join(", ", file.Tag.Performers) //Wrapping around next line since its so goddamn long
                    : !string.IsNullOrEmpty(string.Join(", ", file.Tag.AlbumArtists)) ? string.Join(", ", file.Tag.AlbumArtists) : "Unknown Artist";
                //Looks for Tag.Performers first (this translates to the Contributing Artists property in File Explorer), then AlbumArtists, then finally Unknown Artist if nothing found
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error reading metadata: " + e.Message);
            }
            
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
                        Track newTrack = Track.CreateTrack(audioClip, trackTitle, contributingArtists, trackAlbum);
                        AddItem(newTrack);
                    }
                }
            }
        }

        /* Redundant method. Might keep it around for now in case we need it later for a specific situation
        private Track CreateTrack(AudioClip audioClip) // Function to create a track with the loaded AudioClip
        {
            Track newTrack = new Track(audioClip);
            return newTrack;
            // Add newTrack to your media library collection, etc.
        }*/

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
        public override void AddItem(MediaLibraryComponent newTrack)
        {
            
            if (newTrack is Track track)
            {
                trackList.Add(track);
                Debug.Log($"Track '{track}' has been added to the media library.");
                AudioFile.ObserverManager.ObserverManager.Instance.NotifyObservers("OnTrackAdded", track);
            }
            else
            {
                Debug.LogError("The provided item is not a Track.");
            }

        }
        public override void RemoveItem(MediaLibraryComponent providedTrack)
        {
            trackList.Remove((Track)providedTrack);
            Debug.Log($"Track '{providedTrack}' has been removed from the media library.");
            AudioFile.ObserverManager.ObserverManager.Instance.NotifyObservers("OnTrackRemoved", providedTrack);

        }
        #endregion
    }
}
/*
public class TrackLibrary : ILibrary<Track>
{
    #region Singleton pattern with Lazy<T> implementation (thread-safe)
    private static readonly Lazy<TrackLibrary> lazy =
        new Lazy<TrackLibrary>(() => new TrackLibrary());

    public static TrackLibrary Instance { get { return lazy.Value; } }

    private List<Track> tracks;

    private TrackLibrary()
    {
        tracks = new List<Track>();
    }
    #endregion

    #region Variables
    public Track this[int index] { get => tracks[index]; set => tracks[index] = value; }

    public int Count => tracks.Count;

    public bool IsReadOnly => false;
    #endregion

    
    #region IList implementation

    public void Add(Track item)
    {
        tracks.Add(item);
    }

    public void Clear()
    {
        tracks.Clear();
    }

    public bool Contains(Track item)
    {
        return tracks.Contains(item);
    }

    public void CopyTo(Track[] array, int arrayIndex)
    {
        tracks.CopyTo(array, arrayIndex);
    }

    public int IndexOf(Track item)
    {
        return tracks.IndexOf(item);
    }

    public void Insert(int index, Track item)
    {
        tracks.Insert(index, item);
    }

    public bool Remove(Track item)
    {
        return tracks.Remove(item);
    }

    public void RemoveAt(int index)
    {
        tracks.RemoveAt(index);
    }

    public IEnumerator<Track> GetEnumerator()
    {
        return tracks.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return tracks.GetEnumerator();
    }
    //Codepilot seems to suggest this method should be implemented in the Track class
    // should I just have it return 0?

    public int CompareTo(Track other) //Add this to the Track class
    {
        throw new NotImplementedException();
    }
    
    #endregion

    #region ILibrary implementation

    public Track GetSelection()
    {
        throw new NotImplementedException();
    }

    public void ClearSelection()
    {
        throw new NotImplementedException();
    }

    public List<Track> GetSelectedItems()
    {
        throw new NotImplementedException();
    }

    public void SelectAll()
    {
        throw new NotImplementedException();
    }

    public void DeselectAll()
    {
        throw new NotImplementedException();
    }

    public Track GetSelectedItem(int index)
    {
        throw new NotImplementedException();
    }

    public void SelectItem(int index)
    {
        throw new NotImplementedException();
    }

    public void DeselectItem(int index)
    {
        throw new NotImplementedException();
    }

    public bool IsItemSelected(int index)
    {
        throw new NotImplementedException();
    }
    #endregion
}
*/
