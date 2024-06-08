using UnityEngine;

[System.Serializable]
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
    public Song[] songs; // Array to store songs

    public void AddSong(string songName, AudioClip clip)
    {
        //Create Song
        Song newSong = new Song(songName, clip);

        //Receate Array
        int currentLength = songs.Length;
        Song[] newSongs = new Song[currentLength + 1];
        songs.CopyTo(newSongs, 0);
        newSongs[currentLength] = newSong;

        //Update Array Reference
        songs = newSongs;
    }
}
