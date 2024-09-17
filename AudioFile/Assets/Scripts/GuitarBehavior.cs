using System;
using UnityEngine;

public class GuitarBehavior
{
    private readonly Track _track;

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
    public Tablature Tablature { get; set; }

    public GuitarBehavior(Track track, GuitarTypes guitarType = GuitarTypes.Unspecified, Tablature tablature = null)
    {
        _track = track ?? throw new ArgumentNullException(nameof(track));
        GuitarType = guitarType;
        Tablature = tablature;
    }
    /*
    public void Play()
    {
        _track.Play(_track);
        Debug.Log("Playing guitar track with type: " + GuitarType);
    }

    public void Pause()
    {
        _track.Pause(_track);
        Debug.Log("Pausing guitar track with type: " + GuitarType);
    }

    public void Stop()
    {
        _track.Stop(_track);
        Debug.Log("Stopping guitar track with type: " + GuitarType);
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
        Debug.Log("Updating metadata for guitar track with type: " + GuitarType);
    }

    public void ClearMetadata()
    {
        _track.ClearMetadata();
        Debug.Log("Clearing metadata for guitar track with type: " + GuitarType);
    }
*/
    public void DisplayTablature()
    {
        if (Tablature != null)
        {
            Tablature.Display();
        }
        else
        {
            Debug.Log("No tablature available for this guitar track.");
        }
    }
}