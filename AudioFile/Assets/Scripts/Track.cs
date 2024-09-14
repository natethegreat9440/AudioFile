//Create a concrete class called Track that implements the IPlayable interface. 

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Track : ITrack<Track>
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


protected Track(string name, string userDescription, string artist, string album, float bpm, AudioClip clip, AudioSource audioSource)
    {
        Name = name;
        UserDescription = userDescription;
        Artist = artist;
        Album = album;
        BPM = bpm;
        AudioSource = audioSource;
        AudioSource.clip = clip; // Set the AudioSource's clip
        Duration = AudioSource.clip.length; // Set the Duration


    }
    #endregion

    public abstract void Play(Track item);
public abstract void Pause(Track item);
public abstract void Stop(Track item);
public abstract float GetDuration(Track item);
public abstract float GetCurrentPosition(Track item);
public abstract void UpdateMetadata();
public abstract void ClearMetadata();

    #region IPlayable implementation
    /*public void Play(Track item)
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
*/
    #endregion
}

