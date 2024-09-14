using System;
using UnityEngine;
using UnityEngineInternal;

//TODO: figure out how to implement Tablature and GuitarType as something that is a user-parameter for GuitarTrack

public static class TrackFactory
{
    /* public static GuitarTrack CreateGuitarTrack(string name, string userDescription, string artist, string album, float duration, float bpm, AudioClip clip, AudioSource audioSource, GuitarTrack.GuitarType type, Tablature tablature)
     {
         return new GuitarTrack(name, userDescription, artist, album, bpm, clip, audioSource, type, tablature);
     }

     public static MusicSampleTrack CreateMusicSampleTrack(string name, string userDescription, string artist, string album, float duration, float bpm, AudioClip clip, AudioSource audioSource)
     {
         return new MusicSampleTrack(name, userDescription, artist, album, bpm, clip, audioSource);
     }

     // Add methods to create other types of tracks if needed
    */
    public static Track CreateTrack(
        string type,
        string name,
        string userDescription = "",
        string artist = "",
        string album = "",
        float bpm = 120.0f,
        AudioClip clip = null,
        AudioSource audioSource = null)
        //GuitarTrack.GuitarTypes guitarType = GuitarTrack.GuitarTypes.Acoustic)
        //Tablature tablature = null)
    {
    switch (type)
    {
        case "GenericTrack":
            return new GenericTrack(name, userDescription, artist, album, bpm, clip, audioSource);
        case "GuitarTrack":
            return new GuitarTrack(name, userDescription, artist, album, bpm, clip, audioSource);
        case "MusicSampleTrack":
            return new MusicSampleTrack(name, userDescription, artist, album, bpm, clip, audioSource);
        default:
            throw new ArgumentException("Invalid track type");
        }
    }
}