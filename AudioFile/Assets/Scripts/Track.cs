//Create a concrete class called Track that implements the IPlayable interface. 

using AudioFile.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Playables;
using AudioFile.ObserverManager;
using UnityEditor;

namespace AudioFile.Model
{
    public class Track : MediaLibraryComponent, IPlayable
    {
        AudioSource _audioSource;
        AudioPlayableOutput _audioPlayableOutput;
        AudioClipPlayable _audioPlayable;
        public TrackProperties TrackProperties;
        private string _trackDuration;

        bool _isPlaying = false;
        public bool IsPlaying { get { return _isPlaying; } private set { _isPlaying = value; } }

        bool _isPaused = false;
        public bool IsPaused { get { return _isPaused; } private set { _isPaused = value; } }

        #region Setup/Unity methods
        // Static factory method to create and initialize Track
        public static Track CreateTrack(AudioClip loadedClip, string trackTitle = "Untitled Track",
                                        string trackArtist = "Unknown Artist", string trackAlbum = "Unknown Album", string name = "A track", string loadedPath = "Unknown Path")
        {
            // Create a new GameObject and attach Track as a component
            GameObject trackObject = new GameObject("Track_" + trackTitle);
            Track track = trackObject.AddComponent<Track>();
            track.Initialize(loadedClip, trackTitle, trackArtist, trackAlbum, loadedPath);
            return track;
        }

        // Initialize method to set up properties
        private void Initialize(AudioClip loadedClip, string trackTitle, string trackArtist, string trackAlbum, string loadedPath)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.clip = loadedClip;
            TrackProperties = new TrackProperties();
            TrackProperties.SetProperty("Title", trackTitle);
            TrackProperties.SetProperty("Artist", trackArtist);
            TrackProperties.SetProperty("Album", trackAlbum);
            TrackProperties.SetProperty("Path", loadedPath);

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
                AudioFile.ObserverManager.ObserverManager.Instance.NotifyObservers("OnTrackFrameUpdate", progress);
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
        string FormatTime(float seconds)
        {
            int minutes = Mathf.FloorToInt(seconds / 60);
            int secs = Mathf.FloorToInt(seconds % 60);
            return string.Format("{0:0}:{1:00}", minutes, secs);
        }

        public override void Play(int index = -1)
        {
            //AudioFile.Controller.PlaybackController.Instance.SetCurrentTrack(this);
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

        public override void Pause(int index = -1)
        {
            //Pausing does not affect which track is known as the current track by the PlaybackController
            //_playableGraph.Stop();
            _audioSource.Pause();
            Debug.Log($"Track {this} has been paused at {FormatTime((float)_audioSource.time)}");
            IsPaused = true;
            IsPlaying = false;
        }
        public override void Stop(int index = -1)
        {
            //AudioFile.Controller.PlaybackController.Instance.SetCurrentTrack(null);

            //Stopping a PlayableGraph essentially just pauses it. Need to manually set to zero
            //using AudioPlayable.SetTime(0.0) if using PlayableGraph
            //_playableGraph.Stop();
            _audioSource.Stop();
            Debug.Log($"Track {this} has been stopped. Time is now {FormatTime((float)_audioSource.time)}");
            IsPlaying = false;
            IsPaused = false;
            //Debug.Log($"AudioPlayable time: { FormatTime((float)_audioPlayable.GetTime())}");
            //Debug.Log($"AudioSource time: {FormatTime((float)_audioSource.time)}");

            AudioFile.ObserverManager.ObserverManager.Instance.NotifyObservers("OnTrackStopped");

        }

        // Get or set playback time
        public override float GetDuration()
        {
            //return (float)_audioPlayable.GetDuration();
            return (float)_audioSource.clip.length;
        }

        public override void SetTime(float time)
        {
            //_audioPlayable.SetTime(time);
            _audioSource.time = (float)time;
        }

        // Check if the track is done
        public bool IsDone()
        {
            /*if (_audioSource != null && this.IsPlaying && !this.IsPaused)
            {
                Debug.Log($"{this} is done");
                return !_audioSource.isPlaying;
            }*/
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


