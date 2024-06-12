using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
//Song should be broken out into a new Track.cs object
public class Song
{
    public string name;
    public AudioClip clip;

    public Song(string name, AudioClip clip)
    {
        this.name = name;
        this.clip = clip;
    }
}

public class MediaLibrary : MonoBehaviour
{
    public List<Song> songs = new List<Song>(); // List to store songs. List is dynamically resizable at runtime whereas an array is not

    public void AddSong(string songName, AudioClip clip)
    {
        if (string.IsNullOrEmpty(songName) || clip == null)
        {
            Debug.LogError("Invalid song name or clip.");
            return;
        }

        if (songs.Exists(song => song.name == songName))
        {
            Debug.LogWarning("Song already exists in the library.");
            return;
        }

        songs.Add(new Song(songName, clip));
    }
    public bool RemoveSongByName(string songName)
    {
        Song songToRemove = songs.Find(song => song.name == songName);
        if (songToRemove != null)
        {
            songs.Remove(songToRemove);
            return true;
        }
        return false;
    }
    public void RemoveSongAtIndex(int index)
    {
        if (index >= 0 && index < songs.Count)
        {
            songs.RemoveAt(index);
        }
    }
}
