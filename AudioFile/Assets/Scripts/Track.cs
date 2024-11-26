using AudioFile.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using AudioFile.ObserverManager;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Playables;

namespace AudioFile.Model
{
    /// <summary>
    /// Concrete class for Track objects
    /// <remarks>
    /// Part of a Composite design pattern implementation. Track is a leaf node. Tracks are the children to the TrackLibrary composite node.
    /// Track Properties (i.e., Title, Artist, etc.) are stored in a TrackProperties object for better cohesion. Uses a static factory method to create and initialize Tracks.
    /// Members: IsPlaying, IsPaused, TrackProperties, _audioSource, _trackDuration, CreateTrack(), IsDone(), FormatTime(). 
    /// Inherits Play(), Pause(), Stop(), GetDuration(), SetTime() from MediaLibraryComponent and largely delegates to the playback methods of Unity AudioSource objects with minor additions to help facilitate program flow.
    /// Has _audioPlayableOutput and _audioPlayable properties that hold Unity AudioPlayableOutput and AudioClipPlayable objects for more advanced playback capabilities (so far I haven't really needed to use them, so could be candidates for removal later)
    /// Has default ToString() override implementations. Implements Initialize(), Update(), OnDestroy(), from MonoBehaviour.
    /// </remarks>
    /// <see cref="TrackProperties"/>
    /// <seealso cref="MediaLibraryComponent"/>
    /// <seealso cref="TrackLibrary"/>
    /// <seealso cref="IPlayable"/>
    /// </summary>

    [Serializable]
    public class Track : MediaLibraryComponent, IPlayable
    {
        AudioSource _audioSource;
        AudioPlayableOutput _audioPlayableOutput;
        AudioClipPlayable _audioPlayable;

        [SerializeField]
        public TrackProperties TrackProperties;

        private string _trackDuration;

        bool _isPlaying = false;
        public bool IsPlaying { get { return _isPlaying; } private set { _isPlaying = value; } }

        bool _isPaused = false;
        public bool IsPaused { get { return _isPaused; } private set { _isPaused = value; } }

        #region Setup/Unity methods
        // Static factory method to create and initialize Track
        public static Track CreateTrack(AudioClip loadedClip, string trackTitle = "Untitled Track",
                                        string trackArtist = "Unknown Artist", string trackAlbum = "Unknown Album", string loadedPath = "Unknown Path", string trackID = null)
        {
            // Create a new GameObject and attach Track as a component
            GameObject trackObject = new GameObject("Track_" + trackTitle);
            Track track = trackObject.AddComponent<Track>();
            if (trackID == null)
            { trackID = TrackIDRegistry.Instance.GenerateNewTrackID(); }
            else if (System.Text.RegularExpressions.Regex.IsMatch(trackID, @"^\d{6}$"))
            {
                TrackIDRegistry.Instance.AddExistingIDOnStart(trackID);
            }
            //Debug.Log($"The loaded path is: {loadedPath}");

            track.Initialize(trackID, loadedClip, trackTitle, trackArtist, trackAlbum, loadedPath);
            return track;
        }

        // Initialize method to set up properties
        private void Initialize(string trackID, AudioClip loadedClip, string trackTitle, string trackArtist, string trackAlbum, string loadedPath)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.clip = loadedClip;
            TrackProperties = new TrackProperties();
            TrackProperties.SetProperty("Title", trackTitle);
            TrackProperties.SetProperty("Artist", trackArtist);
            TrackProperties.SetProperty("Album", trackAlbum);
            TrackProperties.SetProperty("Path", loadedPath);
            //Debug.Log($"{this.TrackProperties.GetProperty("Path")} path added to {this}");
            //Debug.Log($"The loaded path is: {loadedPath}");
            TrackProperties.SetProperty("TrackID", trackID);

            _playableGraph = PlayableGraph.Create();
            _audioPlayableOutput = AudioPlayableOutput.Create(_playableGraph, "Audio", _audioSource);
            _audioPlayable = AudioClipPlayable.Create(_playableGraph, _audioSource.clip, false);
            _audioPlayableOutput.SetSourcePlayable(_audioPlayable);
            _playableHandle = _audioPlayable.GetHandle();

            _trackDuration = FormatTime(GetDuration());
            TrackProperties.SetProperty("Duration", _trackDuration);

        }

        void Update()
        {
            if (_audioSource != null && _audioSource.isPlaying)
            {
                double currentTime = _audioSource.time;
                double clipLength = _audioSource.clip.length;
                float progress = (float)(currentTime / clipLength);
                ObserverManager.ObserverManager.Instance.NotifyObservers("OnTrackFrameUpdate", progress);
            }
        }
        void OnDestroy()
        {
            if (_playableGraph.IsValid())
            {
                _playableGraph.Destroy();
            }

            if (_audioSource != null)
            {
                Destroy(_audioSource);
            }
        }
        public override string ToString()
        {
            return $"{TrackProperties.GetProperty("Title")} - {TrackProperties.GetProperty("Artist")}";
        }
        #endregion
        #region Playback method implementations

        //Make this a Utilities method
        public string FormatTime(float seconds)
        {
            int minutes = Mathf.FloorToInt(seconds / 60);
            int secs = Mathf.FloorToInt(seconds % 60);
            return string.Format("{0:0}:{1:00}", minutes, secs);
        }

        public override void Play(string trackDisplayID = "")
        {
            //_playableGraph.Play(); Doesn't affect playback. This kind of object just offers more advanced 
            // functionality than AudioSource does, but for now it might be overkill
            Debug.Log($"{this}_audioSource.time: {FormatTime((float)_audioSource.time)}");

            if (_audioSource.time > 0)
            {
                _audioSource.UnPause();
                Debug.Log($"Track {this} has been resumed at {FormatTime((float)_audioSource.time)}");
            }
            else
            {
                _audioSource.Play();
                Debug.Log($"Track {this} has been played");
            }
            IsPlaying = true;
            IsPaused = false;
        }

        public override void Pause(string trackDisplayID="")
        {
            //Pausing does not affect which track is known as the current track by the PlaybackController
            //_playableGraph.Stop();
            _audioSource.Pause();
            Debug.Log($"Track {this} has been paused at {FormatTime((float)_audioSource.time)}");
            IsPaused = true;
            IsPlaying = false;
        }
        public override void Stop(string trackDisplayID = "")
        {
            //Stopping a PlayableGraph essentially just pauses it. Need to manually set to zero
            //using AudioPlayable.SetTime(0.0) if using PlayableGraph
            //_playableGraph.Stop();
            _audioSource.Stop();
            Debug.Log($"Track {this} has been stopped. Time is now {FormatTime((float)_audioSource.time)}");
            IsPlaying = false;
            IsPaused = false;
            //Debug.Log($"AudioPlayable time: { FormatTime((float)_audioPlayable.GetTime())}");
            //Debug.Log($"AudioSource time: {FormatTime((float)_audioSource.time)}");

            ObserverManager.ObserverManager.Instance.NotifyObservers("OnTrackStopped");

        }

        public override float GetDuration()
        {
            return (float)_audioSource.clip.length;
        }

        public override void SetTime(float time)
        {
            //_audioPlayable.SetTime(time);
            _audioSource.time = (float)time;
            Debug.Log($"Track {this} has been set to {FormatTime(_audioSource.time)}");
            //_audioSource.Play();
            IsPlaying = true;
            IsPaused = false;
        }

        public float GetTime()
        {
            return (float)_audioSource.time;
        }

        // Check if the track is done
        public bool IsDone()
        {
            if (_audioSource.time >= _audioSource.clip.length)
            {
                Debug.Log($"{this} is done");
                return !_audioSource.isPlaying;
            }
            else
            {
                return false;
            }
        }
        #endregion
    }
}


