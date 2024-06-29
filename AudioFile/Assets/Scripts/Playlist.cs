//Create a concrete class called Playlist that implements the IPlaylist interface. 

using System;
using System.Collections;
using System.Collections.Generic;

public class Playlist : IPlaylist<T> where T : Track
{
    #region Singleton pattern with Lazy<T> implementation (thread-safe)
    private static readonly Lazy<Playlist> lazy =
        new Lazy<Playlist>(() => new Playlist());

    public static Playlist Instance { get { return lazy.Value; } }

    private List<Track> playlist;

    private Playlist()
    {
        playlist = new List<Track>();
    }
    #endregion

    #region Variables
    public Track this[int index] { get => playlist[index]; set => playlist[index] = value; }

    public int Count => playlist.Count;

    public bool IsReadOnly => false;
    #endregion

    #region IList implementation

    public void Add(Track item)
    {
        playlist.Add(item);
    }

    public void Clear()
    {
        playlist.Clear();
    }

    public bool Contains(Track item)
    {
        return playlist.Contains(item);
    }

    public void CopyTo(Track[] array, int arrayIndex)
    {
        playlist.CopyTo(array, arrayIndex);
    }

    public int IndexOf(Track item)
    {
        return playlist.IndexOf(item);
    }

    public void Insert(int index, Track item)
    {
        playlist.Insert(index, item);
    }
    #endregion 
    //Add the other methods from the IPlaylist interface

    public void Remove(Track item)
    {
        playlist.Remove(item);
    }

    public void RemoveAt(int index)
    {
        playlist.RemoveAt(index);
    }

    public IEnumerator<Track> GetEnumerator()
    {
        return playlist.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
    public void Shuffle()
    {
        Random rng = new Random();
        int n = playlist.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            Track value = playlist[k];
            playlist[k] = playlist[n];
            playlist[n] = value;
        }
    }

    public void Sort()
    {
        playlist.Sort();
    }

    public void Reverse()
    {
        playlist.Reverse();
    }


}