using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
//Song should be broken out into a new Track.cs object
/*public class Song
{
    public string name;
    public AudioClip clip;

    public Song(string name, AudioClip clip)
    {
        this.name = name;
        this.clip = clip;
    }
}
*/

//TODO: Add method to change track type
public class MediaLibrary : MonoBehaviour
{
    public List<Track> tracks = new List<Track>(); // List to store tracks. List is dynamically resizable at runtime whereas an array is not

    public void AddTrack(string type, string trackName, AudioClip clip)
    {
        if (string.IsNullOrEmpty(trackName) || clip == null)
        {
            Debug.LogError("Invalid track name or clip.");
            return;
        }

        if (tracks.Exists(track => track.Name == trackName))
        {
            Debug.LogWarning("Track already exists in the library.");
            return;
        }

        Track newTrack = TrackFactory.CreateTrack("GenericTrack", trackName, "", "", "", 120, clip);
        tracks.Add(newTrack);
    }
    public bool RemoveTrackByName(string trackName)
    {
        /* Track trackToRemove = tracks.Find(t => t.name == trackName);
         if (trackToRemove != null)
         {
             tracks.Remove(trackToRemove);
             return true;
         }
         return false;
        */
        Track trackToRemove = null;
        foreach (Track track in tracks)
        {
            if (track.Name == trackName)
            {
                trackToRemove = track;
                break;
            }
        }
        if (trackToRemove != null)
        {
            tracks.Remove(trackToRemove);
            return true;
        }
        return false;
    }
    public void RemoveTrackAtIndex(int index)
    {
        if (index >= 0 && index < tracks.Count)
        {
            tracks.RemoveAt(index);
        }
    }
}
