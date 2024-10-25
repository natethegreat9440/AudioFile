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
        //_playableGraph and _playableHandle are declared in the base class
        AudioSource _audioSource;
        //AudioClip _audioClip;
        AudioPlayableOutput _audioPlayableOutput;
        AudioClipPlayable _audioPlayable;
        public TrackProperties TrackProperties;
        TrackLibrary _trackLibrary;

        public Track(AudioClip loadedClip, string trackTitle= "Untitled Track", string trackArtist = "Unknown Artist", string trackAlbum = "Unknown Album", string trackDuration = "--:--", string name = "A track") : base(name)
        {
            //_audioSource = source; // Set the AudioSource's clip and spaud
            _audioSource.clip = loadedClip;
            TrackProperties = new TrackProperties();
            TrackProperties.SetProperty("Title", trackTitle);
            TrackProperties.SetProperty("Artist", trackArtist);
            TrackProperties.SetProperty("Album", trackAlbum);
            TrackProperties.SetProperty("Duration", trackDuration);

            _playableGraph = PlayableGraph.Create();

            // Create an AudioPlayableOutput linked to this track's AudioSource
            AudioPlayableOutput _audioPlayableOutput = AudioPlayableOutput.Create(_playableGraph, "Audio", _audioSource);

            // Create an AudioPlayable that this Track controls
            AudioClipPlayable _audioPlayable = AudioClipPlayable.Create(_playableGraph, _audioSource.clip, false);

            // Link the PlayableOutput to the Playable
            _audioPlayableOutput.SetSourcePlayable(_audioPlayable);

            _playableHandle = _audioPlayable.GetHandle();
        }

        public override string ToString()
        {
            return $"{TrackProperties.GetProperty("Title")} - {TrackProperties.GetProperty("Artist")}";
        }
        #region Playback method implementations
        // Play the track

        private void Update()
        {
            // Assuming 'track' is an instance of the Track class
            if (this.IsDone())
            {
                Debug.Log("Track has finished playing.");
                // Access the singleton instance and call NotifyObservers
                AudioFile.ObserverManager.ObserverManager.Instance.NotifyObservers("OnCurrentTrackIsDone", null);
            }
        }
        public override void Play()
        {
             _playableGraph.Play();
            _audioSource.Play();
           
        }

        // Pause the track
        public override void Pause()
        {
            _playableGraph.Stop();
            _audioSource.Pause();
        }

        // Stop the track
        public override void Stop()
        {
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
            return _audioPlayable.IsDone();
        }
        #endregion
    }
}


