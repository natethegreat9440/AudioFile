using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericTrack : Track
{
    public GenericTrack(string name, string userDescription, string artist, string album, float bpm, AudioClip clip, AudioSource audioSource)
        : base(name, userDescription, artist, album, bpm, clip, audioSource)
    { 
        //TODO: Add additional code here if needed
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