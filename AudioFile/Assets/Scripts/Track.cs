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


        #region Setup/Unity methods
        // Static factory method to create and initialize Track
        public static Track CreateTrack(AudioClip loadedClip, string trackTitle = "Untitled Track",
                                        string trackArtist = "Unknown Artist", string trackAlbum = "Unknown Album", string name = "A track")
        {
            // Create a new GameObject and attach Track as a component
            GameObject trackObject = new GameObject("Track_" + trackTitle);
            Track track = trackObject.AddComponent<Track>();
            track.Initialize(loadedClip, trackTitle, trackArtist, trackAlbum);
            return track;
        }

        // Initialize method to set up properties
        private void Initialize(AudioClip loadedClip, string trackTitle, string trackArtist, string trackAlbum)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.clip = loadedClip;
            TrackProperties = new TrackProperties();
            TrackProperties.SetProperty("Title", trackTitle);
            TrackProperties.SetProperty("Artist", trackArtist);
            TrackProperties.SetProperty("Album", trackAlbum);

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
            if (_playableGraph.IsValid() && _playableGraph.IsPlaying())
            {
                double currentTime = _audioPlayable.GetTime();
                double clipLength = _audioPlayable.GetDuration();
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

        public override void Play(int index = 0)
        {
            //AudioFile.Controller.PlaybackController.Instance.SetCurrentTrack(this);
            _playableGraph.Play();
            _audioSource.Play();
            Debug.Log($"Track {this} has been played");
        }

        public override void Pause(int index = 0)
        {
            //Pausing does not affect which track is known as the current track by the PlaybackController
            _playableGraph.Stop();
            _audioSource.Pause();
            Debug.Log($"Track {this} has been paused");

        }
        public override void Stop(int index = 0)
        {
            //AudioFile.Controller.PlaybackController.Instance.SetCurrentTrack(null);
            _playableGraph.Stop();
            _audioSource.Stop();
            Debug.Log($"Track {this} has been stopped");
            AudioFile.ObserverManager.ObserverManager.Instance.NotifyObservers("OnTrackStopped");

        }

        // Get or set playback time
        public override float GetDuration()
        {
            return (float)_audioPlayable.GetDuration();
        }

        public override void SetTime(float time)
        {
            _audioPlayable.SetTime(time);
            _audioSource.time = (float)time;
        }

        // Check if the track is done
        public bool IsDone()
        {
            if (_audioPlayable.IsValid())
            {
                return _audioPlayable.IsDone();
            }
            else return false;
        }
        #endregion
    }
}


