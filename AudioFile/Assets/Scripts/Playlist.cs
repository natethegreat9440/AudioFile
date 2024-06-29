//Create a concrete class called Playlist that implements the IPlaylist interface. 

using System;
using System.Collections;
using System.Collections.Generic;

public class Playlist : IPlaylist<MediaItem>
{
    #region Singleton pattern with Lazy<T> implementation (thread-safe)
    private static readonly Lazy<Playlist> lazy =
        new Lazy<Playlist>(() => new Playlist());

    public static Playlist Instance { get { return lazy.Value; } }

    private List<MediaItem> playlist;

    private Playlist()
    {
        playlist = new List<MediaItem>();
    }
    #endregion

    #region Variables
    public MediaItem this[int index] { get => playlist[index]; set => playlist[index] = value; }

    public int Count => playlist.Count;

    public bool IsReadOnly => false;
    #endregion

    #region IList implementation

    public void Add(MediaItem item)
    {
        playlist.Add(item);
    }

    public void Clear()
    {
        playlist.Clear();
    }

    public bool Contains(MediaItem item)
    {
        return playlist.Contains(item);
    }

    public void CopyTo(MediaItem[] array, int arrayIndex)
    {
        playlist.CopyTo(array, arrayIndex);
    }

    public int IndexOf(MediaItem item)
    {
        return playlist.IndexOf(item);
    }

    public void Insert(int index, MediaItem item)
    {
        playlist.Insert(index, item);
    }
    #endregion 
    //Add the other methods from the IPlaylist interface

    public void Remove(MediaItem item)
    {
        playlist.Remove(item);
    }

    public void RemoveAt(int index)
    {
        playlist.RemoveAt(index);
    }

    public IEnumerator<MediaItem> GetEnumerator()
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
            MediaItem value = playlist[k];
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