//Create a concrete class called Track that implements the IPlayable interface. 

using AudioFile.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Playables;
using AudioFile.ObserverManager;

namespace AudioFile.Model
{
    public class Track : MediaLibraryComponent, IPlayable
    {
        AudioSource _audioSource;
        AudioPlayableOutput _audioPlayableOutput;
        AudioClipPlayable _audioPlayable;
        public TrackProperties TrackProperties;
        //TrackLibrary _trackLibrary;
        private bool _isCurrentTrack;
        private string _trackDuration;

        //TODO: reset-up Track since it is a MonoBehaviour item

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
            _isCurrentTrack = false;

            _trackDuration = FormatTime(GetDuration());
            TrackProperties.SetProperty("Duration", _trackDuration);

        }
        /*public Track(AudioClip loadedClip, string trackTitle = "Untitled Track", string trackArtist = "Unknown Artist", string trackAlbum = "Unknown Album", string name = "A track") : base(name)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.clip = loadedClip;
            TrackProperties = new TrackProperties();
            TrackProperties.SetProperty("Title", trackTitle);
            TrackProperties.SetProperty("Artist", trackArtist);
            TrackProperties.SetProperty("Album", trackAlbum);

            _trackDuration = this.FormatTime(this.GetDuration());
            TrackProperties.SetProperty("Duration", _trackDuration);

            _playableGraph = PlayableGraph.Create();

            // Create an AudioPlayableOutput linked to this track's AudioSource
            _audioPlayableOutput = AudioPlayableOutput.Create(_playableGraph, "Audio", _audioSource);

            // Create an AudioPlayable that this Track controls
            _audioPlayable = AudioClipPlayable.Create(_playableGraph, _audioSource.clip, false);

            // Link the PlayableOutput to the Playable
            _audioPlayableOutput.SetSourcePlayable(_audioPlayable);

            _playableHandle = _audioPlayable.GetHandle();
            _isCurrentTrack = false;
        }*/

        public override string ToString()
        {
            return $"{TrackProperties.GetProperty("Title")} - {TrackProperties.GetProperty("Artist")}";
        }
        #region Playback method implementations
        // Play the track
        string FormatTime(float seconds)
        {
            int minutes = Mathf.FloorToInt(seconds / 60);
            int secs = Mathf.FloorToInt(seconds % 60);
            return string.Format("{0:0}:{1:00}", minutes, secs);
        }

        public override void Play()
        {
            _isCurrentTrack = true;
            _playableGraph.Play();
            _audioSource.Play();
        }

        // Pause the track
        public override void Pause()
        {
            _isCurrentTrack = false;
            _playableGraph.Stop();
            _audioSource.Pause();
        }

        // Stop the track
        public override void Stop()
        {
            _isCurrentTrack = false;
            _playableGraph.Stop();
            _audioSource.Stop();
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
            return _isCurrentTrack && _audioPlayable.IsDone();
        }
        #endregion
    }
}


