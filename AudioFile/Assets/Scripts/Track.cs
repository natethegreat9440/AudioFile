//Create a concrete class called Track that implements the IPlayable interface. 

using System;
using System.Collections;
using System.Collections.Generic;

public class Track : ITrack<Track>
{
    #region Variables
    public string Title { get; set; }
    public string Artist { get; set; }
    public string Album { get; set; }
    public float Duration { get; set; }
    public float CurrentPosition { get; set; }
    public float BPM { get; set; }

    public Track(string title, string artist, string album, float duration, float bpm)
    {
        Title = title;
        Artist = artist;
        Album = album;
        Duration = duration;
        BPM = bpm;
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