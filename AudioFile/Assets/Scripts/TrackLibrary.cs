using AudioFile.Model;
using System.Collections;
using AudioFile.ObserverManager;
using System.IO;
using UnityEngine.Networking;
using SFB;
using TagLib;
using System.Linq;
using UnityEngine;
using System;
using System.Collections.Generic;
using AudioFile.Controller;

namespace AudioFile.Model
{
    /// <summary>
    /// Concrete composite class for the global Track Library, which holds all Track objects uploaded to AudioFile.
    /// <remarks>
    /// Part of a Composite design pattern implementation. TrackLibrary is a composite node. Track are the children leaf nodes TrackLibrary contains. 
    /// Not sure yet whether to implement Playlists as composite nodes in this tree or a detached filter type object so only TrackLibary holds Tracks and Playlists only hold TrackIDs
    /// Members: GetTrackAtIndex(), GetTrackIndex(). Has Play(), Pause(), Stop() that largely delegate down to individual Track objects. These methods inherit from MediaLibraryComponent. 
    /// Inherits NextItem(), PreviousItem(), AddItem(), RemoveItem(), RemoveItemAtIndex() methods form MediaLibaryComponent as well.
    /// Has default ToString() override implementations. Implements Initialize() from MonoBehaviour.
    /// </remarks>
    /// <see cref="MonoBehaviour"/>
    /// </summary>
    public class TrackLibrary : MediaLibraryComponent
    {
        // Lazy<T> ensures that the instance is created in a thread-safe manner
        private static readonly Lazy<TrackLibrary> _instance = new Lazy<TrackLibrary>(CreateSingleton);

        // Private constructor to prevent instantiation
        private TrackLibrary() { } //Unity objects should not use constructors with parameters as Unity uses its own lifecycle methods to manage these objects

        public static TrackLibrary Instance => _instance.Value;

        protected List<Track> trackList;
        private int currentTrackIndex;

        private static TrackLibrary CreateSingleton()
        {
            // Create a new GameObject to hold the singleton instance if it doesn't already exist
            GameObject singletonObject = new GameObject(typeof(TrackLibrary).Name);
            // Add the TrackListController component to the GameObject
            return singletonObject.AddComponent<TrackLibrary>();
        }

        /*public void Start()
        {
            throw new NotImplementedException();
        }*/

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
            //Debug.Log($"Track index = {trackList.IndexOf(track)}");
            return trackList.IndexOf(track);
        }
        public Track GetTrackAtID(string trackDisplayID)
        {
            return trackList
                .Where(track => track.TrackProperties.GetProperty("TrackID") == trackDisplayID)
                .FirstOrDefault();
        }

        private int GetTrackIndexAtID(string trackDisplayID)
        {
            //Debug.Log($"trackDisplayID passed = {trackDisplayID}.");
            return trackList
                .Where(track => track.TrackProperties.GetProperty("TrackID") == trackDisplayID)
                .Select(track => trackList.IndexOf(track))
                .FirstOrDefault();
        }

        public string GetTrackID(Track track)
        {
            return track.TrackProperties.GetProperty("TrackID");
        }
        public override void Play(string trackDisplayID)
        {
            currentTrackIndex = GetTrackIndexAtID(trackDisplayID);
            try
            {
                trackList[currentTrackIndex].Play();
            }
            catch (Exception ex)
            {
                Debug.Log($"Track skipped: {ex.GetType()} - {ex.Message}");
                Skip(currentTrackIndex);
            }
        }

        public override void Pause(string trackDisplayID)
        {
            currentTrackIndex = GetTrackIndexAtID(trackDisplayID);
            trackList[currentTrackIndex].Pause();
        }

        public override void Stop(string trackDisplayID)
        {
            currentTrackIndex = GetTrackIndexAtID(trackDisplayID);
            trackList[currentTrackIndex].Stop();
        }
        public override void Skip(int index) //Commenting this out for now as it seems to me this Skip() logic could just be implemented into the Play() method directly 
        //(and potentially other playback methods, however I'll want to test how Skip logic works inside Play before deciding if I want to add to other methods)
        {
            if (trackList[currentTrackIndex] != null)
            {
                ObserverManager.ObserverManager.Instance.NotifyObservers("OnTrackSkipped", trackList[currentTrackIndex]); //Passes the track name if possible when skipped
            }
            else
            {
                ObserverManager.ObserverManager.Instance.NotifyObservers("OnTrackSkipped", null); //Otherwise passes null (most likely to end up here I would imagine)
            }
            NextItem();
        }

        public override void NextItem()
        {
            currentTrackIndex = PlaybackController.Instance.GetCurrentTrackIndex();

            if (currentTrackIndex < trackList.Count - 1)
            {
                trackList[currentTrackIndex].Stop();

                currentTrackIndex++;
                PlaybackController.Instance.SetCurrentTrack(trackList[currentTrackIndex]);
                ObserverManager.ObserverManager.Instance.NotifyObservers("OnCurrentTrackCycled", currentTrackIndex);
                trackList[currentTrackIndex].Play();
            }
            else
            {
                Debug.Log("Reached the end of the playlist.");
                ObserverManager.ObserverManager.Instance.NotifyObservers("OnTrackListEnd", currentTrackIndex);
            }
        }

        public override void PreviousItem()
        {
            currentTrackIndex = PlaybackController.Instance.GetCurrentTrackIndex();
            Debug.Log($"Current track index = {PlaybackController.Instance.GetCurrentTrackIndex()}");

            if (currentTrackIndex > 0)
            {
                trackList[currentTrackIndex].Stop();

                currentTrackIndex--;
                PlaybackController.Instance.SetCurrentTrack(trackList[currentTrackIndex]);
                ObserverManager.ObserverManager.Instance.NotifyObservers("OnCurrentTrackCycled", currentTrackIndex);
                trackList[currentTrackIndex].Play();
            }
            else
            {
                Debug.Log("Reached the front of the playlist.");
                //Same event type as NextItem(), may want to change this later
                ObserverManager.ObserverManager.Instance.NotifyObservers("OnTrackListEnd", currentTrackIndex);
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
                ObserverManager.ObserverManager.Instance.NotifyObservers("OnTrackAdded", track);
            }
            else
            {
                Debug.LogError("The provided item is not a Track.");
            }
        }

        public override void RemoveItem(string trackDisplayID)
        {
            var trackToRemove = GetTrackAtID(trackDisplayID);

            var trackIndex = GetTrackIndex(trackToRemove);
            trackList[trackIndex].Stop();

            Debug.Log($"trackToRemove = {PlaybackController.Instance.CurrentTrack}");

            PlaybackController.Instance.CurrentTrack.TrackProperties.GetProperty("TrackID");
            currentTrackIndex = GetTrackIndexAtID(trackDisplayID);

            //Move the current track to the previous track
            currentTrackIndex--;
            PlaybackController.Instance.SetCurrentTrack(trackList[currentTrackIndex]);

            trackList.Remove(trackToRemove);
            Debug.Log($"Track '{trackToRemove}' has been removed from the media library.");
            ObserverManager.ObserverManager.Instance.NotifyObservers("OnTrackRemoved", trackToRemove);
        }

        public override void RemoveItemAtIndex(int providedIndex)
        {
            var trackToRemove = trackList[providedIndex];
            Debug.Log($"Track '{trackToRemove}' has been removed from the media library.");
            trackList[providedIndex].Stop();

            //Move the current track to the previous track
            currentTrackIndex--;
            PlaybackController.Instance.SetCurrentTrack(trackList[currentTrackIndex]);

            trackList.RemoveAt(providedIndex);
            Debug.Log($"Track '{trackToRemove}' has been removed from the media library.");
            ObserverManager.ObserverManager.Instance.NotifyObservers("OnTrackRemoved", trackToRemove);
        }
        #endregion
    }
}
/*
 //Methods I may want to add later (logic below is generic)

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

    public int CompareTo(Track other) //Add this to the Track class
    {
        throw new NotImplementedException();
    }
    
}
*/
