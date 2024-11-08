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
        // Lazy<T> ensures that the instance is created in a thread-safe manner
        private static readonly Lazy<TrackLibrary> _instance = new Lazy<TrackLibrary>(CreateSingleton);

        // Private constructor to prevent instantiation
        private TrackLibrary() { } //Unity objects should not use constructors with parameters as Unity uses its own lifecycle methods to manage these objects

        public static TrackLibrary Instance => _instance.Value;

        protected List<Track> trackList;
        private int currentTrackIndex;

        private TrackLibraryController _trackLibraryController;

        private static TrackLibrary CreateSingleton()
        {
            // Create a new GameObject to hold the singleton instance if it doesn't already exist
            GameObject singletonObject = new GameObject(typeof(TrackLibrary).Name);
            // Add the TrackListController component to the GameObject
            return singletonObject.AddComponent<TrackLibrary>();
        }

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Initialize()
        {
            //TODO: Change this method later so it initializes to what is in memory
            //and set the current track index to whatever the last played track is
            trackList = new List<Track>();
            currentTrackIndex = 0;
        }

        public override string ToString()
        {
            return $"{_name}";
        }

        #region Playback method implementations
        public Track GetTrackAtIndex(int index)
        {
            return trackList[index];
        }
        public int GetTrackIndex(Track track)
        {
            return trackList.IndexOf(track);
        }
        public override void Play(int index)
        {
            currentTrackIndex = index;
            trackList[currentTrackIndex].Play();
        }

        public override void Pause(int index)
        {
            currentTrackIndex = index;
            trackList[currentTrackIndex].Pause();
        }

        public override void Stop(int index)
        {
            currentTrackIndex = index;
            trackList[currentTrackIndex].Stop();
        }
        public override void Skip(int index)
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
                trackList[currentTrackIndex].Stop();

                currentTrackIndex++;
                AudioFile.Controller.PlaybackController.Instance.SetCurrentTrack(trackList[currentTrackIndex]);
                AudioFile.ObserverManager.ObserverManager.Instance.NotifyObservers("OnCurrentTrackCycled", currentTrackIndex);
                trackList[currentTrackIndex].Play();
            }
            else
            {
                Debug.Log("Reached the end of the playlist.");
                AudioFile.ObserverManager.ObserverManager.Instance.NotifyObservers("OnTrackListEnd", currentTrackIndex);
            }
        }

        public override void PreviousItem()
        {
            if (currentTrackIndex > 0)
            {
                trackList[currentTrackIndex].Stop();

                currentTrackIndex--;
                AudioFile.Controller.PlaybackController.Instance.SetCurrentTrack(trackList[currentTrackIndex]);
                AudioFile.ObserverManager.ObserverManager.Instance.NotifyObservers("OnCurrentTrackCycled", currentTrackIndex);
                trackList[currentTrackIndex].Play();
            }
            else
            {
                Debug.Log("Reached the front of the playlist.");
                //Same event type as NextItem(), may want to change this later
                AudioFile.ObserverManager.ObserverManager.Instance.NotifyObservers("OnTrackListEnd", currentTrackIndex);
            }
        }
        #endregion
        #region Model control methods

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

        public void RemoveItemAtIndex(int providedIndex)
        {
            Track removedTrack = trackList[providedIndex];
            Debug.Log($"Track '{removedTrack}' has been removed from the media library.");

            trackList.RemoveAt(providedIndex);

            AudioFile.ObserverManager.ObserverManager.Instance.NotifyObservers("OnTrackRemoved", removedTrack);
        }

        public void AudioFileUpdate(string observationType, object data)
        {
            /*switch (observationType)
            {
                case "":
                    break;
                // Add more cases here if needed
                default:
                    Debug.LogWarning($"Unhandled observation type: {observationType}");
                    break;
            }*/
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
