using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicSampleTrack : Track
{
   public Dictionary<string, SampleInfo> Samples { get; private set; }

    public MusicSampleTrack(string name, string userDescription, string artist, string album, float bpm, AudioClip clip, AudioSource audioSource)
        : base(name, userDescription, artist, album, bpm, clip, audioSource)
    {
        Samples = new Dictionary<string, SampleInfo>();
    }
#region MusicSampleTrack specific methods
    public void AddSample(string key, SampleInfo sampleInfo)
    {
        Samples[key] = sampleInfo;
    }

    public void RemoveSample(string key)
    {
        Samples.Remove(key);
    }
    #endregion
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

public class SampleInfo
{
    public string SampleTrackName { get; set; }
    public string SampleTrackArtist { get; set; }

    public SampleInfo(string sampleTrackName, string sampleTrackArtist)
    {
        SampleTrackName = sampleTrackName;
        SampleTrackArtist = sampleTrackArtist;
    }
}