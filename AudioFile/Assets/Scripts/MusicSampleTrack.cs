using System;
using System.Collections.Generic;
using UnityEngine;

public class MusicSampleBehavior
{
    private readonly Track _track;

    public Dictionary<string, SampleInfo> MusicSamples { get; private set; }

    public MusicSampleBehavior(Track track)
    {
        _track = track ?? throw new ArgumentNullException(nameof(track));
        MusicSamples = new Dictionary<string, SampleInfo>();
    }
    public void DisplaySampleInfo()
    {
        if (MusicSamples != null)
        {
            //MusicSamples.Display();
            throw new NotImplementedException();
        }
        else
        {
            Debug.Log("No sample info available for this track.");
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


        #region MusicSampleBehavior specific methods
        /*
        public void AddSample(string key, SampleInfo sampleInfo)
        {
            Samples[key] = sampleInfo;
        }

        public void RemoveSample(string key)
        {
            Samples.Remove(key);
        }
        #endregion

        public void Play()
        {
            _track.Play(_track);
            Debug.Log("Playing music sample track.");
        }

        public void Pause()
        {
            _track.Pause(_track);
            Debug.Log("Pausing music sample track.");
        }

        public void Stop()
        {
            _track.Stop(_track);
            Debug.Log("Stopping music sample track.");
        }

        public float GetDuration()
        {
            return _track.GetDuration(_track);
        }

        public float GetCurrentPosition()
        {
            return _track.GetCurrentPosition(_track);
        }

        public void UpdateMetadata()
        {
            _track.UpdateMetadata();
            Debug.Log("Updating metadata for music sample track.");
        }

        public void ClearMetadata()
        {
            _track.ClearMetadata();
            Debug.Log("Clearing metadata for music sample track.");
        }
    }
    */

    }