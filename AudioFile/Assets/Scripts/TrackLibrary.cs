using AudioFile.Model;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using AudioFile.ObserverManager;

namespace AudioFile.Model
{
    public class TrackLibrary : MediaLibraryComponent, IAudioFileObserver
    {
        //protected override string _name = "Track Library";
        protected List<Track> trackList = new List<Track>();
        private int currentTrackIndex = 0;

        public TrackLibrary(string name = "Track Library") : base(name)
        {
        }

        public override string ToString()
        {
            return $"{_name}";
        }

        #region Playback method implementations

        public override void Skip()
        {
            try
            {
                trackList[currentTrackIndex].Play();
            }
            catch (Exception)
            {
                NextItem();
            }
        }

        public override void NextItem()
        {
            if (currentTrackIndex < trackList.Count - 1)
            {
                currentTrackIndex++;
                trackList[currentTrackIndex].Play();
            }
        }

        public override void PreviousItem()
        {
            if (currentTrackIndex > 0)
            {
                currentTrackIndex--;
                trackList[currentTrackIndex].Play();
            }
        }

        public void AudioFileUpdate(string observationType, object data)
        {
            if (observationType == "OnCurrentTrackIsDone")
            {
                NextItem();
            }
        }
        #endregion
        #region Model control methods

        public override void LoadItem()
        {
            base.LoadItem();
        }
        public override void AddItem()
        { 
            base.AddItem(); 
        }
        public override void RemoveItem()
        {
            base.RemoveItem();
        }
        #endregion
    }
}
/*
public class TrackLibrary : ILibrary<Track>
{
    #region Singleton pattern with Lazy<T> implementation (thread-safe)
    private static readonly Lazy<TrackLibrary> lazy =
        new Lazy<TrackLibrary>(() => new TrackLibrary());

    public static TrackLibrary Instance { get { return lazy.Value; } }

    private List<Track> tracks;

    private TrackLibrary()
    {
        tracks = new List<Track>();
    }
    #endregion

    #region Variables
    public Track this[int index] { get => tracks[index]; set => tracks[index] = value; }

    public int Count => tracks.Count;

    public bool IsReadOnly => false;
    #endregion

    
    #region IList implementation

    public void Add(Track item)
    {
        tracks.Add(item);
    }

    public void Clear()
    {
        tracks.Clear();
    }

    public bool Contains(Track item)
    {
        return tracks.Contains(item);
    }

    public void CopyTo(Track[] array, int arrayIndex)
    {
        tracks.CopyTo(array, arrayIndex);
    }

    public int IndexOf(Track item)
    {
        return tracks.IndexOf(item);
    }

    public void Insert(int index, Track item)
    {
        tracks.Insert(index, item);
    }

    public bool Remove(Track item)
    {
        return tracks.Remove(item);
    }

    public void RemoveAt(int index)
    {
        tracks.RemoveAt(index);
    }

    public IEnumerator<Track> GetEnumerator()
    {
        return tracks.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return tracks.GetEnumerator();
    }
    //Codepilot seems to suggest this method should be implemented in the Track class
    // should I just have it return 0?

    public int CompareTo(Track other) //Add this to the Track class
    {
        throw new NotImplementedException();
    }
    
    #endregion

    #region ILibrary implementation

    public Track GetSelection()
    {
        throw new NotImplementedException();
    }

    public void ClearSelection()
    {
        throw new NotImplementedException();
    }

    public List<Track> GetSelectedItems()
    {
        throw new NotImplementedException();
    }

    public void SelectAll()
    {
        throw new NotImplementedException();
    }

    public void DeselectAll()
    {
        throw new NotImplementedException();
    }

    public Track GetSelectedItem(int index)
    {
        throw new NotImplementedException();
    }

    public void SelectItem(int index)
    {
        throw new NotImplementedException();
    }

    public void DeselectItem(int index)
    {
        throw new NotImplementedException();
    }

    public bool IsItemSelected(int index)
    {
        throw new NotImplementedException();
    }
    #endregion
}
*/
