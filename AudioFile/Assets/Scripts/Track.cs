//Create a concrete class called Track that implements the IPlayable interface. 

using AudioFile.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Playables;

namespace AudioFile.Model
{
    public class TrackProperties
    {
        Dictionary<string, string> trackProperties = new Dictionary<string, string>()
        {
            {"Title", "Untitled Track"},
            {"Artist", "Unknown Artist"},
            {"Album", "Unknown Album"},
            {"Duration", "--:--"},
            {"BPM", "--" }
        };

        public string GetProperty(string key)
        {
            if (trackProperties.ContainsKey(key))
            {
                return trackProperties[key];
            }
            return null;
        }

        /*Method not needed can just call the trackProperties.Values or trackProperties.Keys to get all keys and values 
public Dictionary<string, string> GetAllProperties()
        {
            return new Dictionary<string, string>(trackProperties);
        }*/

        public void SetProperty(string key, string value)
        {
            if (trackProperties.ContainsKey(key))
            {
                trackProperties[key] = value;
            }
        }
    }
    public class Track : MediaLibraryComponent, IPlayable
    {
        //_playableGraph and _playableHandle are declared in the base class
        AudioSource _audioSource;
        AudioPlayableOutput _audioPlayableOutput;
        AudioClipPlayable _audioPlayable;
        TrackProperties _trackProperties;

        public Track(AudioSource source, string trackTitle, string trackArtist, string trackAlbum, string trackDuration, string name = "A track") : base(name)
        {
            _audioSource = source; // Set the AudioSource's clip and spaud
            _trackProperties = new TrackProperties();
            _trackProperties.SetProperty("Title", trackTitle);
            _trackProperties.SetProperty("Artist", trackArtist);
            _trackProperties.SetProperty("Album", trackAlbum);
            _trackProperties.SetProperty("Duration", trackDuration);

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
            return $"{_trackProperties.GetProperty("Title")} - {_trackProperties.GetProperty("Artist")}";
        }

        // Play the track
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

        public void SetTime(double time)
        {
            _audioPlayable.SetTime(time);
            _audioSource.time = (float)time;
        }

        // Check if the track is done
        public bool IsDone()
        {
            return _audioPlayable.IsDone();
        }

    }
}

/*public class Track : ITrack<Track>
{
    #region Variables
    public string Name { get; private set; } //Name is intrinsic to the Track
    public string UserDescription { get; set; }
    public string Artist { get; set; }
    public string Album { get; set; }
    public float Duration { get; private set; }
    public float CurrentPosition { get; set; }
    public float BPM { get; private set; }
    public AudioSource AudioSource { get; private set; }
    public AudioClip AudioClip { get; private set; }


public Track(string name, string userDescription, string artist, string album, float bpm, AudioClip clip, AudioSource audioSource)
    {
        Name = name;
        UserDescription = userDescription;
        Artist = artist;
        Album = album;
        BPM = bpm; //No get method exists for BPM on an AudioClip object
        AudioClip = clip; // Set the AudioSource's clip
        AudioSource = audioSource;
        //Duration = AudioSource.clip.length; // Set the Duration
    }
    #endregion

    #region IPlayable implementation
    public void Play(Track item)
    {
        throw new NotImplementedException();
    }

    public void Pause(Track item)
    {
        throw new NotImplementedException();
    }

    public void Stop(Track item)
    {
        throw new NotImplementedException();
    }

    public float GetDuration(Track item)
    {
        throw new NotImplementedException();
    }

    public float GetCurrentPosition(Track item)
    {
        throw new NotImplementedException();
    }
    #endregion

    #region ITrack implementation

    public void UpdateMetadata()
    {
       throw new NotImplementedException();
    }

    public void ClearMetadata()
    {
        throw new NotImplementedException();
    }

    #endregion
}
*/
