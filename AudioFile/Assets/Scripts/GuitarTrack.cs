using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuitarTrack : Track
{
    public enum GuitarTypes
    {
        Acoustic,
        Electric,
        Bass,
        Classical,
        TwelveString,
        Unspecified
    }
    public GuitarTypes GuitarType { get; set; }
    //public Tablature Tablature { get; set; }

    public GuitarTrack(string name, string userDescription, string artist, string album, float bpm, AudioClip clip, AudioSource audioSource, GuitarTypes guitarType = GuitarTypes.Unspecified)
        : base(name, userDescription, artist, album, bpm, clip, audioSource)
    {
        GuitarType = guitarType;
        //Tablature = tablature;
    }

    public override void Play(Track item)
    {
        throw new NotImplementedException();
    }

    public override void Pause(Track item)
    {
        throw new NotImplementedException();
    }

    public override void Stop(Track item)
    {
        throw new NotImplementedException();
    }

    public override float GetDuration(Track item)
    {
        return Duration;
    }

    public override float GetCurrentPosition(Track item)
    {
        return CurrentPosition;
    }

    public override void UpdateMetadata()
    {
        throw new NotImplementedException();
    }

    public override void ClearMetadata()
    {
        throw new NotImplementedException();
    }
}