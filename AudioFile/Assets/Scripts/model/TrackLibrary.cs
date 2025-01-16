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
using System.Data;
using Mono.Data.Sqlite;

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

        public List<Track> TrackList;
        public int ActiveTrackIndex { get; set; }

        private static TrackLibrary CreateSingleton()
        {
            // Create a new GameObject to hold the singleton instance if it doesn't already exist
            GameObject singletonObject = new GameObject(typeof(TrackLibrary).Name);
            // Add the TrackListController component to the GameObject
            return singletonObject.AddComponent<TrackLibrary>();
        }

        public void Initialize()
        {
            //TODO: Change this method later so it initializes to what is in memory
            //and set the active and selected track index to whatever the last played track is
            TrackList = new List<Track>();
            ActiveTrackIndex = 0;
        }

        public override string ToString()
        {
            return $"{_name}";
        }

        #region Playback method implementations
        public Track GetTrackAtIndex(int index)
        {
            return TrackList[index];
        }
        public int GetTrackIndex(Track track)
        {
            //Debug.Log($"Track index = {TrackList.IndexOf(track)}");
            return TrackList.IndexOf(track);
        }
        public Track GetTrackAtID(string trackDisplayID)
        {
            return TrackList
                .Where(track => track.TrackProperties.GetProperty("TrackID") == trackDisplayID)
                .FirstOrDefault();
        }

        private int GetTrackIndexAtID(string trackDisplayID)
        {
            //Debug.Log($"trackDisplayID passed = {trackDisplayID}.");
            return TrackList
                .Where(track => track.TrackProperties.GetProperty("TrackID") == trackDisplayID)
                .Select(track => TrackList.IndexOf(track))
                .FirstOrDefault();
        }

        public string GetTrackID(Track track)
        {
            return track.TrackProperties.GetProperty("TrackID");
        }
        public override void Play(string trackDisplayID)
        {
            ActiveTrackIndex = GetTrackIndexAtID(trackDisplayID);
            try
            {
                TrackList[ActiveTrackIndex].Play();
            }
            catch (Exception ex)
            {
                Debug.Log($"Track skipped: {ex.GetType()} - {ex.Message}");
                Skip(ActiveTrackIndex);
            }
        }

        public override void Pause(string trackDisplayID)
        {
            ActiveTrackIndex = GetTrackIndexAtID(trackDisplayID);
            TrackList[ActiveTrackIndex].Pause();
        }

        public override void Stop(string trackDisplayID)
        {
            ActiveTrackIndex = GetTrackIndexAtID(trackDisplayID);
            TrackList[ActiveTrackIndex].Stop();
        }
        public override void Skip(int index) //Commenting this out for now as it seems to me this Skip() logic could just be implemented into the Play() method directly 
        //(and potentially other playback methods, however I'll want to test how Skip logic works inside Play before deciding if I want to add to other methods)
        {
            if (TrackList[ActiveTrackIndex] != null)
            {
                ObserverManager.ObserverManager.Instance.NotifyObservers("OnTrackSkipped", TrackList[ActiveTrackIndex]); //Passes the track name if possible when skipped
            }
            else
            {
                ObserverManager.ObserverManager.Instance.NotifyObservers("OnTrackSkipped", null); //Otherwise passes null (most likely to end up here I would imagine)
            }
            NextItem();
        }

        public override void NextItem()
        {
            ActiveTrackIndex = GetTrackIndex(PlaybackController.Instance.ActiveTrack);

            if (ActiveTrackIndex < TrackList.Count - 1)
            {
                TrackList[ActiveTrackIndex].Stop();

                ActiveTrackIndex++;
                PlaybackController.Instance.SetActiveTrack(TrackList[ActiveTrackIndex]);
                ObserverManager.ObserverManager.Instance.NotifyObservers("OnActiveTrackCycled", ActiveTrackIndex);
                ObserverManager.ObserverManager.Instance.NotifyObservers("OnActiveTrackChanged", null);
                TrackList[ActiveTrackIndex].Play();
            }
            else
            {
                Debug.Log("Reached the end of the playlist.");
                ObserverManager.ObserverManager.Instance.NotifyObservers("OnTrackListEnd", ActiveTrackIndex);
            }
        }

        public override void PreviousItem()
        {
            ActiveTrackIndex = GetTrackIndex(PlaybackController.Instance.ActiveTrack);
            Debug.Log($"Active track index = {ActiveTrackIndex}");

            if (ActiveTrackIndex > 0)
            {
                TrackList[ActiveTrackIndex].Stop();

                ActiveTrackIndex--;
                PlaybackController.Instance.SetActiveTrack(TrackList[ActiveTrackIndex]);
                ObserverManager.ObserverManager.Instance.NotifyObservers("OnActiveTrackCycled", ActiveTrackIndex);
                ObserverManager.ObserverManager.Instance.NotifyObservers("OnActiveTrackChanged", null);
                TrackList[ActiveTrackIndex].Play();
            }
            else
            {
                Debug.Log("Reached the front of the playlist.");
                //Same event type as NextItem(), may want to change this later
                ObserverManager.ObserverManager.Instance.NotifyObservers("OnTrackListEnd", ActiveTrackIndex);
            }
        }
        #endregion
        #region Model control methods
        public override void AddItem(MediaLibraryComponent trackToAdd, bool isTrackNew)
        {
            if (trackToAdd is Track track)
            { 
                if (isTrackNew)
                {
                    ObserverManager.ObserverManager.Instance.NotifyObservers("OnNewTrackAdded", track);
                    Debug.Log($"New track '{track}' has been added to the media library.");
                }

                TrackList.Add(track);
                Debug.Log($"Track '{track}' has been loaded");
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
            TrackList[trackIndex].Stop();

            Debug.Log($"trackToRemove = {TrackList[trackIndex]}");

            TrackList.Remove(trackToRemove);
            Debug.Log($"Track '{trackToRemove}' has been removed from the media library.");
            ObserverManager.ObserverManager.Instance.NotifyObservers("OnTrackRemoved", trackToRemove);
        }

        public override void RemoveItemAtIndex(int providedIndex)
        {
            var trackToRemove = TrackList[providedIndex];
            Debug.Log($"Track '{trackToRemove}' has been removed from the media library.");
            TrackList[providedIndex].Stop();

            //Move the selected and active track to the previous track
            ActiveTrackIndex--;
            PlaybackController.Instance.SetActiveTrack(TrackList[ActiveTrackIndex]);

            TrackList.RemoveAt(providedIndex);
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
