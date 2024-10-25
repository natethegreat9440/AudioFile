//Create a concrete class called Track that implements the IPlayable interface. 

using AudioFile.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Playables;

public class Track : MediaLibraryComponent, IPlayable
{
    AudioSource _audioSource;
    AudioPlayableOutput _audioPlayableOutput;
    AudioClipPlayable _audioPlayable;
    //PlayableGraph _playableGraph;
    //PlayableHandle _playableHandle;

    //Abstract these out into a TrackProperties Dictionary
    public string trackTitle { get; set; }
    public string trackArtist { get; set; }
    public string trackAlbum { get; set; }

    public string trackDuration { get; }

    public Track(AudioSource source, string trackTitle, string trackArtist, string trackAlbum, string trackDuration, string name="A track") : base(name)
    {
        _audioSource = source; // Set the AudioSource's clip and spaud
        this.trackTitle = trackTitle;
        this.trackArtist = trackArtist;
        this.trackAlbum = trackAlbum;
        this.trackDuration = trackDuration;

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
        return $"{trackTitle} - {trackArtist}";
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
