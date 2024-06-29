//Create a concrete class called Visualizer that implements the IVisualizer interface. 

using System;
using System.Collections.Generic;

public class Visualizer : IVisualizer
{
    #region Singleton pattern with Lazy<T> implementation (thread-safe)
    private static readonly Lazy<Visualizer> lazy =
        new Lazy<Visualizer>(() => new Visualizer());

    public static Visualizer Instance { get { return lazy.Value; } }

    private List<Visual> visuals;

    private Visualizer()
    {
        visuals = new List<Visualizer>();
    }
    #endregion

    #region Variables
    public Visualizer this[int index] { get => visuals[index]; set => visuals[index] = value; }

    public int Count => visuals.Count;

    public bool IsReadOnly => false;
    #endregion

    #region IList implementation (commented out because I'm not sure it's needed)
    /* Not sure how this got in here. I don't think it's needed, but we'll leave as comments for now.
    public void Add(Visual item)
    {
        visuals.Add(item);
    }

    public void Clear()
    {
        visuals.Clear();
    }

    public bool Contains(Visual item)
    {
        return visuals.Contains(item);
    }

    public void CopyTo(Visual[] array, int arrayIndex)
    {
        visuals.CopyTo(array, arrayIndex);
    }

    public int IndexOf(Visual item)
    {
        return visuals.IndexOf(item);
    }

    public void Insert(int index, Visual item)
    {
        visuals.Insert(index, item);
    }

    public bool Remove(Visual item)
    {
        return visuals.Remove(item);
    }

    public void RemoveAt(int index)
    {
        visuals.RemoveAt(index);
    }
    */
    #endregion

    #region IVisualizer implementation

    public void Initialize()
    {
        throw new NotImplementedException();
    }    

    public void UpdateVisualization()
    {
       throw new NotImplementedException();     
    }

    public void Reset()
    {
        throw new NotImplementedException();
    }
    #endregion

    #region IPlayable implementation
    public void Play()
    {
        throw new NotImplementedException();
    }

    public void Pause()
    {
        throw new NotImplementedException();
    }

    public void Stop()
    {
        throw new NotImplementedException();
    }

    public float GetDuration()
    {
        throw new NotImplementedException();
    }

    public float GetCurrentPosition()
    { 
        throw new NotImplementedException();
    }
    #endregion
}